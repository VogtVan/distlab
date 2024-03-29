# ABOUT

Distlab is about simulating and observing distributed components. The main purpose of Distlab is to provide a ready to use infrastructure handling the life cycle of different processes and the communication between them, with an observability stack helping in monitoring those processes working together.

It is not an infrastructure meant to host production distributed applications, event though it is potentially able to manage the life cycle and the communication of processes among different computers. It is a tool box which allows you to simulate distributed applications, making proof of concepts, focusing more on the algorithm and the semantics of an application rather than on the complexity of deployment and communication handling.

Distlab is built on top of open source components. It runs either on Linux or Windows systems. Key components are the following one:

- A distlab C# library (.Net 6.0)
- Prometheus for the collection of metrics
- Zipkin for the collection of traces (Jaeger is also supported)
- Grafana for the dashboards

Even though you can use Distlab with any kind of editor, even with notepad, it is strongly recommended to use it with VS Code because it will simplify the installation of the .Net 5.0 platform and bring you the ability to easily debug your simulation.\
Currently the implementation has to be done in C#, but it is not necessary to have a in depth knowledge of C# to be able to do interesting things, as the coding part strives to be as close to pseudo code as possible. 

# CONCEPTS
## COMPONENTS
The simplified view below shows some of the key components making the simulation runtime:

![Distlab](./img/DistlabConcepts.jpg "Distlab concepts")
### <b>The controller</b>

This is the entry point of the simulation. It is launched with a single argument carrying the data plan name the controller needs to orchestrate.

The controller and the container wrapper are shiped with Distlab under the .controller and the .container folders.

Depending on the data plan, the controller will instanciate replicaset controllers in order to manage the life cycle of the services you want to deploy.

### <b>The replicaset controller</b>

A replicaset controller is in charge of the life cycle of a set of instances of the same service. The desired number of services to bring to life is defined in the data plan. Each service is assigned a specific index by the replicaset controller and a specific configuration can be bound to each of them through the dataplan.

### <b>The container controller</b>

This component is wrapping and monitoring the process container. It has a liveness probe which allows it to restart unresponsive processes. It handles one end of the communication link to a service instance. Actually, processes are hosted in a process container host which is an abstration on top of the service which allows the simulation to run your application components inside real processes or just as different threads on the controller process.

### <b>The container</b>
The container is the base of the service you want to implement. It holds the other end of the communication link which allows the service to communicate with other services. Among other things, it offers base methods to allow the service to manage specific metrics, call other services either synchronously or asynchronously and access the service configurations.

## SERVICES
These are the components of the distributed application you want to simulate. This is where Distlab works ends and yours starts. Each service needs to define a set of methods which corresponds to the semantic you want to put in place and you want to explore. The service is defined by a C# class deriving from the base Container class given by the distlab.dll library. It exposes a main() method which is invoked when the service is ready.\
You need to keep things simple and just focus on the semantics of the messages and the dynamics of the application.

## DATA PLAN
The data plan is defined with a yaml file and describes all the services you want to deploy and monitor. Main features are the following one:
- ability to describe a set of services to  orchestrate 
- specify the number of instances per service
- launch the services inside threads or processes
- specify the log level for the controller and for each service instance
- enable or disable the traces export
- export traces to zipkin or jaeger
- export prometheus metrics for the controller and for each service instance
- introduce communication delays for a specific service instance
- define custom configuration for a specific service instance
- define custom metrics for a specific service instance


# GETTING STARTED

## PRE REQUISITE

- Docker https://www.docker.com/
    - On Windows: WSL2 https://docs.microsoft.com/fr-fr/windows/wsl/install-win10
    - On Linux: https://docs.docker.com/engine/install/ then create a docker security group https://docs.docker.com/engine/install/linux-postinstall/
- .Net 7.0 SDK (not only the runtime)
     - On Windows: https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net70
     - On Linux: https://docs.microsoft.com/en-us/dotnet/core/install/linux
- VS Code https://code.visualstudio.com/ + OmniSharp extension


## SETUP

The first time you need to create the observability stack.\
The stack will be installed in Docker with Prometheus, Zipklin and Grafana.\
Basic configuration ready to use with Distlab will be setup. If the configuration does not suit your need, you can modify it and reinstall the stack.

- Windows

    On Windows we recommend to install a WSL2 distro like Ubuntu and to [work with VS Code inside the distro](https://code.visualstudio.com/docs/remote/wsl). If you do so please follow the Linux section below.

    Install the observability stack <b>at root</b> with:

    ```cmd
    distlab> build.observability.bat
    ```

    You need also to enable these urls for the metrics:

    ```cmd
    netsh http add urlacl url=http://+:7777/ user=[DOMAIN]\user
    netsh http add urlacl url=http://+:7778/ user=[DOMAIN]\user
    netsh http add urlacl url=http://+:7779/ user=[DOMAIN]\user
    netsh http add urlacl url=http://+:7780/ user=[DOMAIN]\user
    ```
    Later you may need to define other ports depending on the number of Prometheus exporters you will define in the data plan.

- Linux

  Install the observability stack <b>at root</b> with:

    ```cmd
    distlab> ./build.observability.sh
    ```

### Warning
- The build.observability may output some error messages. This is a known bug but it does not prevent it from working.
- The [default configuration](/.observability/prometheus/backup/prometheus.yml) provided for prometheus assumes the docker network bridge IP address is 172.17.0.1. It may be necessary to change it to the actual one. To get the IP address, please run:

    ```cmd
    distlab> docker network inspect bridge 
    ```
If the gateway ip address differs from 172.17.0.1, please update the prometheus.yml file accordingly, then build the observability stack.
## QUICK START

Please ensure to setup the observability stack in the section above. From now on, we do not specify which type of system you're on. Please run the appropriate command depending on you're working on Windows or Linux.

<b>Observing "read you writes" consistency</b>

The sample will start an in memory database (key/value) with a leader and two replicas. A client will be launched as well, and will perform random values set against the DB for the same key at random intervals. Right after a set, it will perform a read on the same key and compare both values. If the values does not match, an inconsistency counter will be incremented.

All set and read commands are targeted to an instance of the database following a round robin load balancing. A set command will always be redirected to the leader if it targets a replica. All get orders can be served by any DB instance.

The "read your write" counter will be available on a graph.

The default mode is set to asynchronous replication which will alow you to observe inconsistencies.

You may then play with the data plan parameters to change the replication mode to synchronous and check that inconsistencies are gone.

    
- Build the sample simulation:
    ```cmd
    distlab> build
    ```
- Run the sample simulation with the observability:
    ```cmd
    distlab> run o
    ```

- Open [Grafana](http://127.0.0.1:9030/) and select a dashboard

    ![Dashboards](./img/dashboards.jpg "Select dashboard")
    - Overview with replicaset instances

    ![Distlab](./img/distlab_dash.jpg "Distlab overview dashboard")
    - In memory DB sample

    ![In memoryDB](./img/inmemory_dash.jpg "In memoryDB dashboard")

- Open [Zipkin](http://127.0.0.1:9411/) and search for traces

    You can see on the sequence below that the client has set the 16 value to the key, and has read the value from the replica #2 before the value was replicated, resulting in an inconsistency.

    ![Zipkin](./img/zipkin-stale.jpg "Zipkin traces")

- Stop the sample simulation
    ```cmd
    CTRL+C
    ```
- Stop the observability stack
    ```cmd
    distlab> stop
    ```

- Database code: 
    - [Database](./src/samples/db/InMemoryDatabase.cs)
    - [Client](./src/samples/db/InMemoryDatabaseClient.cs)
- Configuration file: [inMemoryDBEventual.yaml](.dataplan/inMemoryDBEventual.yaml) 

    - Database section

        You may change there the IsSynchronous value.
        ```yaml
            Services: # services to launch
            -   Name: "inmemorydb" # public service name used to communicate
                AssemblyName: "services.dll" # assembly where the service is defined
                TypeName: "distlab.samples.db.InMemoryDatabase" # type of the service (derived from dislab.runtime.container.Container)
                Instances: 3 # number of instances to maintain - each instance will be given a spepcific index
                Config: # configuration section for a replicaset - use index to target a specific instance - not mandatory
                -   Index: 0 # target service for the configuration
                    Log:
                        LogLevelName: "Information"
                        LogToConsole: true
                        IsLogJsonFormat: false
                    IsSynchronous: false
        ```
    - Client section

        You may change there the MinDelayMs and MaxDelayMs values to change the rate at which a set command is performed.
        ```yaml
        -   Name: "inmemorydbclient"
            AssemblyName: "services.dll"
            TypeName: "distlab.samples.db.InMemoryDatabaseClient"
            Instances: 1
            Config: # configuration section for a replicaset - use index to target a specific instance - not mandatory
            -   Index: 0
                Log:
                    LogLevelName: "Information"
                    LogToConsole: true
                    IsLogJsonFormat: false
                MinDelayMs: 100 # min delay before doing another set
                MaxDelayMs: 200 # max delay before doing another set
        ```


# OBSERVABILITY STACK


## PROMETHEUS

- A [default configuration](/.observability/prometheus/backup/prometheus.yml) is provided which scrapes four different URLs. Those URLs are to be used in the data plan you want to launch.

    The four metrics servers are:

    ```cmd
    '127.0.0.1:7777', '127.0.0.1:7778', '127.0.0.1:7779', '127.0.0.1:7780'
    ```

    The local storage is used and is collected [here](/.observability/prometheus/data).

- In DISTLAB, for prometheus metrics, each metric agent defines an http server exposing the /metrics endpoint.\
    For a simulation launched in thread mode, only one server should be used for the controller. Other prometheus configuration at service level should be disabled in the data plan.

    server 1: http://127.0.0.1:7777/metrics \
    server 2: http://127.0.0.1:7778/metrics \
    server 3: http://127.0.0.1:7779/metrics \
    server 4: http://127.0.0.1:7780/metrics

- Prometheus UI is available here after launch: [http://127.0.0.1:9090/](http://127.0.0.1:9090/)    

### DEFAULT METRICS 

- controller

    there is a metric agent for the controller which is responsible of collecting the metrics for controller and replicas

    - MetricType.gauge, "replicaset", "Number of replicasets.", "dataPlan"

- replicaset controller

    - MetricType.gauge, "replicas", "Number of replicas.", "dataPlan", "service"

- container controller

    - MetricType.counter, "msg", "Number of messages.", "direction", "service", "index"
    - MetricType.counter, "restart", "Number of service restart.", "service"
    - MetricType.gauge, "start", "Time to start container in ms.", "service"
    - MetricType.gauge, "stop", "Time to stop container in ms.", "service"

- container

    - MetricType.gauge, "latency", "Network latency.", "service", "index"
    - MetricType.gauge, "execution", "Execution duration.", "service", "operation", "index"
    - MetricType.counter, "msg", "Number of messages.", "direction", "service", "index"
    - MetricType.counter, "timeout", "Number of timeouts.", "service", "operation", "index"
    - specific metrics

### THREAD OR PROCESS

There is one metric agent per container, so for each service instance, if the configuration specifies prometheus or trace enabled. So, per process, there is a clean separation between prometheus exporter instances.\
When launch per thread, all prometheus exporters listens to all metrics because they are static. The configuration should enable the prometheus on controller only in that case.
## ZIPKIN

Zipkin is used by the obervability strack to collected correlated traces.\
No persistence is used between simulation runs. You may export them from Zipkin UI after a simulation run.
- Zipkin UI is available here after launch: [http://127.0.0.1:9411/](http://127.0.0.1:9411/)   

## GRAFANA
Grafana is used to provide dashboards to DISTLAB. Dashboards are connected to the Prometheus instance.\
Default dashboards are provided to help monitoring a simulation status.\
Specific dashboards should be developped depending on the different aspects of your simulation you want to track.
- Grafana UI is available here after launch: [http://127.0.0.1:9030/](http://127.0.0.1:9030/) 

# DEVELOPING SERVICES (TODO)

To develop a service you need to define it from a base Container class.

A sample is available [here](./src/samples/db).

To enter the debug mode you need define the lauch mode to "thread" in the data plan and you need to launch the simulation from VSCode.

To run the simulation you need to pass the data plan name to the controller program:

```cmd
dotnet distlab.controller.dll <dataplan.yaml>
```

# DATAPLAN REFERENCE (TODO)
In order to launch a set of services you need to define a data plan in the .dataplan folder. A sample is defined  [here](.dataplan/inMemoryDBEventual.yaml).


