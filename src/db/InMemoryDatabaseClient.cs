using System.Threading.Tasks;
using distlab.core;
using Microsoft.Extensions.Logging;
using distlab.metrics;
using System;
using System.Collections.Generic;
using distlab.runtime.container;

namespace msbd.thesis.consistency.db{
    public class InMemoryDatabaseClient : Container{

        Random rnd = new Random();
        protected override async Task main() {
            while(Status == ContainerStatus.started){
                await Task.Delay(rnd.Next(1000, 2000));
                await this.call<object>(ServiceName, "set", Index); // calling this way to allow tracing
            }                
        }
        
        int lastValue = 0;
        
        public async Task set(){
            lastValue = rnd.Next(0, 100);
            logger.LogInformation("Client setting {lastValue}", lastValue);
            try{
                await this.call<object>("inmemorydb", "set", null, "key", lastValue);
            }
            catch(Exception e){
                logger.LogError("Unable to set the value in the db: {exception}.", e.Message);
            }
            try{
            logger.LogInformation("Client reading {lastValue}", lastValue);
                int readValue = await this.call<int>("inmemorydb", "get", null, "key");
                if(readValue!=lastValue){
                    logger.LogCritical("Inconsistency between set value {set} and read value {read} !", lastValue, readValue);
                    // add a count to read your own writes consistency
                    this.incCounter("readYourWrites", 1, this.Index.ToString());
                }
                else
                    logger.LogInformation("set and read value are the same : {value}", lastValue);
            }
            catch(Exception e){
                logger.LogError("Unable to get the value from the db: {exception}.", e.Message);
            }
        }

    }
}