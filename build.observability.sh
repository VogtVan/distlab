#!/bin/bash
# Prometheus
cd .observability/prometheus
./init.sh
./restore.sh
# Zipkin
docker container rm distlab-zipkin --force > /dev/null
docker container create \
    --name distlab-zipkin \
    -p 9411:9411 openzipkin/zipkin-slim
# Grafana
cd ../grafana
./init.sh
./restore.sh
cd ..
# network
# docker network rm distlab-network
docker network rm distlab-network > /dev/null
docker network create distlab-network &&
docker network connect distlab-network distlab-prometheus &&
docker network connect distlab-network distlab-grafana
