using System.Threading.Tasks;
using distlab.core;
using distlab.model;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using distlab.runtime.container;

namespace msbd.thesis.consistency.db{

    public class Config: ContainerConfigDefinition{
        public bool IsSynchronous{get;set;}

    }

    public class InMemoryDatabase : Container<Config>{

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
                    Task<bool>[]  tasks = Enumerable.Range(0, this.Definition.Instances).Select(i => call<bool>(ServiceName, "replicate", i, key, value)).ToArray();
                    if(this.Definition[0].IsSynchronous){
                        Task.WaitAll(tasks);
                        return tasks.All(t => t.Result);
                    }
                    else{
                        await Task.WhenAll(tasks);
                    }
                    return true;
                }
                catch(Exception e){
                    logger.LogError("{name} unable to replicate: {exception}.",Name, e.Message);
                    return false;
                }
            else
                try{
                    // a replica forward to the leader. could use call to check the result.
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