#!/bin/bash
# Prometheus
mkdir -p prometheus/data &&
docker container rm distlab-prometheus --force &&
docker container create \
    --name distlab-prometheus \
    --volume "$PWD/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml" \
    --volume "$PWD/prometheus/data:/prometheus" \
    -p 9090:9090 prom/prometheus &&
# Zipkin
docker container rm distlab-zipkin --force &&
docker container create \
    --name distlab-zipkin \
    -p 9411:9411 openzipkin/zipkin-slim &&
# Grafana
docker container rm distlab-grafana --force &&
docker container create \
    --name distlab-grafana \
    --volume "$PWD/grafana/grafana.ini:/etc/grafana/grafana.ini" \
    --volume "$PWD/grafana/data:/var/lib/grafana" \
    --volume "$PWD/grafana/provisioning:/etc/grafana/provisioning" \
    -p 9030:3000 grafana/grafana &&
# network
docker network rm distlab-network &&
docker network create distlab-network &&
docker network connect distlab-network distlab-prometheus &&
docker network connect distlab-network distlab-grafana
