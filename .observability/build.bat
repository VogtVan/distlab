REM Prometheus
docker container rm distlab-prometheus --force
docker container create ^
    --name distlab-prometheus ^
    --volume "%cd%/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml" ^
    --volume "%cd%/prometheus/data:/prometheus" ^
    -p 9090:9090 prom/prometheus
REM Zipkin
docker container rm distlab-zipkin --force
docker container create ^
    --name distlab-zipkin ^
    -p 9411:9411 openzipkin/zipkin-slim
REM Grafana
docker container rm distlab-grafana --force
docker container create ^
    --name distlab-grafana ^
    --volume "%cd%/grafana/grafana.ini:/etc/grafana/grafana.ini" ^
    --volume "%cd%/grafana/data:/var/lib/grafana" ^
    --volume "%cd%/grafana/provisioning:/etc/grafana/provisioning" ^
    -p 9030:3000 grafana/grafana
REM network
docker network rm distlab-network
docker network create distlab-network
docker network connect distlab-network distlab-prometheus
docker network connect distlab-network distlab-grafana
