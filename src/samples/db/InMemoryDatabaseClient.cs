using System.Threading.Tasks;
using distlab.core;
using distlab.model;
using Microsoft.Extensions.Logging;
using distlab.metrics;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using distlab.runtime.container;

namespace distlab.samples.db{

    public class ClientConfig: ContainerConfigDefinition{
        public int MinDelayMs{get;set;}
        public int MaxDelayMs{get;set;}
    }

    public class InMemoryDatabaseClient : Container<ClientConfig>{

        Random rnd = new Random();
        protected override async Task main() {
            while(Status == ContainerStatus.started){
                await Task.Delay(rnd.Next(this.Definition[0].MinDelayMs, this.Definition[0].MaxDelayMs));
                int value = rnd.Next(0, 100);
                // starting the root activity for tracing
                Activity activity = this.startActivity($"set {value} from client");
                try{
                    await set(value);
                }
                finally{
                    activity?.Dispose();
                }
            }                
        }
                
        public async Task set(int value){
            logger.LogInformation("Client setting {value}", value);
            try{
                await this.call<object>("inmemorydb", "set", null, "key", value);
            }
            catch(Exception e){
                logger.LogError("Unable to set the value in the db: {exception}.", e.Message);
            }
            try{
            logger.LogInformation("Client reading {lastValue}", value);
                int readValue = await this.call<int>("inmemorydb", "get", null, "key");
                if(readValue!=value){
                    logger.LogCritical("Inconsistency between set value {set} and read value {read} !", value, readValue);
                    // add a count to read your own writes consistency
                    this.incCounter("readYourWrites", 1, this.Index.ToString());
                }
                else
                    logger.LogInformation("set and read value are the same : {value}", value);
            }
            catch(Exception e){
                logger.LogError("Unable to get the value from the db: {exception}.", e.Message);
            }
        }

    }
}