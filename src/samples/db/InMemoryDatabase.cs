using System.Threading.Tasks;
using distlab.core;
using distlab.model;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using distlab.runtime.container;

namespace distlab.samples.db{

    public class DBConfig: ContainerConfigDefinition{
        public bool IsSynchronous{get;set;}

    }

    public class InMemoryDatabase : Container<DBConfig>{

        protected override async Task main() {
        }
        
        protected bool IsLeader => Index==0;
        Dictionary<string,object> db = new Dictionary<string, object>();
        
        public bool replicate(string key, object value){
            if(!IsLeader)
                logger.LogInformation("{name} replicating: {key} ({value}).",Name, key, value);
            else
                logger.LogInformation("{name} setting: {key} ({value}).",Name, key, value);
            db[key]=value;
            return true;
        }
        public async Task<bool> set(string key, object value){
            if(IsLeader)
                try{
                    Task[]  tasks = this.Definition[0].IsSynchronous ? 
                        // reliable broadcast replicate
                        Enumerable.Range(0, this.Definition.Instances).Select(i => call<bool>(ServiceName, "replicate", i, key, value)).ToArray() : 
                        Enumerable.Range(0, this.Definition.Instances).Select(i => invoke(ServiceName, "replicate", i, key, value)).ToArray();
                    await Task.WhenAll(tasks);
                    return this.Definition[0].IsSynchronous ? tasks.All(t => ((Task<bool>)t).Result) : true;
                }
                catch(Exception e){
                    logger.LogError("{name} unable to replicate: {exception}.",Name, e.Message);
                    return false;
                }
            else
                try{
                    // a replica forward to the leader. could use call to check the result.
                    if(this.Definition[0].IsSynchronous)
                        // need to wait for completion here
                        await call<bool>(ServiceName, "set", 0, key, value);
                    else
                        await invoke(ServiceName, "set", 0, key, value);
                    return true;
                }
                catch(Exception e){
                    logger.LogError("{name} unable to forward to leader: {exception}.",Name, e.Message);
                    return false;
                }
        }

        public object get(string key){
            // returns local value
            object value = db[key];
            logger.LogInformation("{name} returning: {key} ({value}).",Name, key, value);
            return value;
        }

    }
}