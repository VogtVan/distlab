#!/bin/bash
docker container rm distlab-prometheus --force > /dev/null
docker volume rm distlab-prometheus-data > /dev/null
docker container create \
    --name distlab-prometheus \
    -v distlab-prometheus-data:/prometheus \
    -p 9090:9090 prom/prometheus --config.file "/prometheus/prometheus.yml"