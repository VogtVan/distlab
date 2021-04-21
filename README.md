

# OBSERVABILITY


## CONCEPTS

In DISTLAB a metric agent holds a trace provider created from a metric service with a Prometheus server and Zipkin or Jaeger exporter depending on the configuration.


Activities from System.Diagnostics are used to track time; some metrics are sent based on this time.\
Traces activities are used to track the correlation between services through opentelemetry library.

One fgets metrics and correlation through Prometheus and Zipkin, dashboard are provided with Grafana.

## GETTING STARTED

The first time you need to create the observability stack.\
The stack will be installed in Docker with Prometheus, Zipklin and Grafana.\
Basic configuration ready to use with DISTLAB will be setup. If the configuration does not suit your need, you can modify it and reinstall the stack.

- Windows

    You need Docker for Desktop installed on your machine.

```cmd
distlab/.observability> build.bat
```
- Linux (TODO)

Once the stack created, next time just use:
- Windows

```cmd
distlab/.observability> start.bat
```

```cmd
distlab/.observability> stop.bat
```

- Linux (TODO)

Once the stack is launched, a simulation can be run; metrics and traces will be collected.


# PROMETHEUS

- A [default configuration](/.observability/prometheus/prometheus.yml) is provided which scrapes four different URLs. Those URLs are to be used in the dataplan you want to launch.

    The four metrics servers are:

    ```cmd
    '127.0.0.1:7777', '127.0.0.1:7778', '127.0.0.1:7779', '127.0.0.1:7780'
    ```

    The local storage is used and is collected [here](/.observability/prometheus/data).

- In DISTLAB, for prometheus metrics, each metric agent defines an http server exposing the /metrics endpoint.\
    For a simulation launched in thread mode, only one server should be used for the controller. Other prometheus configuration at service level should be disabled in the data plan.

    server 1: http://127.0.0.1:7777/metrics\
    server 2: http://127.0.0.1:7778/metrics\
    server 3: http://127.0.0.1:7779/metrics\
    server 4: http://127.0.0.1:7780/metrics

- Prometheus UI is available here after launch: [http://127.0.0.1:9090/](http://127.0.0.1:9090/)    


# ZIPKIN

Zipkin is used by the obervability strack to collected correlated traces.\
No persistence is used between simulation runs. You may export them from Zipkin UI after a simulation run.
- Zipkin UI is available here after launch: [http://127.0.0.1:9411/](http://127.0.0.1:9411/)   

# GRAFANA
Grafana is used to provide dashboards to DISTLAB. Dashboards are connected to the Prometheus instance.\
Default dashboards are provided to help monitoring a simulation status.\
Specific dashboards should be developped depending on the different aspects of your simulation you want to track.
- Grafan UI is available here after launch: [http://127.0.0.1:9030/](http://127.0.0.1:9030/) 

## controller

there is a metric agent for the controller which is responsible of collecting the metrics for controller and replicas

- MetricType.gauge, "replicaset", "Number of replicasets.", "dataPlan"

## replicaset controller

- MetricType.gauge, "replicas", "Number of replicas.", "dataPlan", "service"

## container controller

- MetricType.counter, "msg", "Number of messages.", "direction", "service", "index"
- MetricType.counter, "restart", "Number of service restart.", "service"
- MetricType.gauge, "start", "Time to start container in ms.", "service"
- (MetricType.gauge, "stop", "Time to stop container in ms.", "service"

## container

- MetricType.gauge, "latency", "Network latency.", "service", "index"
- MetricType.gauge, "execution", "Execution duration.", "service", "operation", "index"
- MetricType.counter, "msg", "Number of messages.", "direction", "service", "index"
- MetricType.counter, "timeout", "Number of timeouts.", "service", "operation", "index"
- specific metrics


there is also one metric agent per container, so for each service instance, if the configuration specifies prometheus or trace enabled


# zipkin

service name: zipkin

http://localhost:30411/


# distlab

docker build . distlab:1.0.0
metrics exposed on 7777-80 port
service name: distlab

http://localhost:30777/metrics

started by kubectl apply. Modifying the template would need a restart.

# grafana

service name: grafana

http://localhost:30000/

# prometheus traces

## process

per process, there is a clean separation between prometheus instances.

### controller 

- HELP THESIS_restart Number of service restart.
- TYPE THESIS_restart counter
- HELP THESIS_stop Time to stop container in ms.
- TYPE THESIS_stop gauge
- HELP THESIS_replicaset Number of replicasets.
- TYPE THESIS_replicaset gauge
THESIS_replicaset{dataPlan="inMemoryDBEventual"} 2
- HELP THESIS_start Time to start container in ms.
- TYPE THESIS_start gauge
THESIS_start{service="inmemorydbclient"} 1319.0975
THESIS_start{service="inmemorydb"} 1166.6638
- HELP THESIS_replicas Number of replicas.
- TYPE THESIS_replicas gauge
THESIS_replicas{dataPlan="inMemoryDBEventual",service="inmemorydbclient"} 1
THESIS_replicas{dataPlan="inMemoryDBEventual",service="inmemorydb"} 3
- HELP THESIS_msg Number of messages.
- TYPE THESIS_msg counter
THESIS_msg{direction="out",service="inmemorydb",index="1"} 430
THESIS_msg{direction="out",service="inmemorydb",index="0"} 831
THESIS_msg{direction="out",service="inmemorydbclient",index="0"} 667
THESIS_msg{direction="out",service="inmemorydb",index="2"} 428

### service 

- HELP THESIS_latency Network latency.
- TYPE THESIS_latency gauge
- HELP THESIS_execution Execution duration.
- TYPE THESIS_execution gauge
THESIS_execution{service="inmemorydb",operation="get",index="0"} 0.2902
THESIS_execution{service="inmemorydb",operation="replicate",index="0"} 0.1916
THESIS_execution{service="inmemorydb",operation="set",index="0"} 7.1815
- HELP THESIS_timeout Number of timeouts.
- TYPE THESIS_timeout counter
- HELP THESIS_msg Number of messages.
- TYPE THESIS_msg counter
THESIS_msg{direction="in",service="inmemorydb",index="0"} 229

## thread

when launch per thread, the prometheus server listens to all metrics because they are static. 
=> the configuration should define prometheus on controller only.