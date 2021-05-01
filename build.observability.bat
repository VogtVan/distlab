REM Prometheus
cd .observability/prometheus
call init.bat
call restore.bat
REM Zipkin
docker container rm distlab-zipkin --force > NUL
docker container create ^
    --name distlab-zipkin ^
    -p 9411:9411 openzipkin/zipkin-slim
REM Grafana
cd ../grafana
call init.bat
call restore.bat
cd ../../
REM network
docker network rm distlab-network > NUL
docker network create distlab-network
docker network connect distlab-network distlab-prometheus
docker network connect distlab-network distlab-grafana
