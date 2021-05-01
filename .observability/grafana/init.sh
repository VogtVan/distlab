#!/bin/bash
# remove existing container and volume
docker container rm distlab-grafana --force > /dev/null
docker volume rm distlab-grafana-data > /dev/null
# first create grafana container which will create the volume for permissions issues
docker container create \
    --name distlab-grafana \
    -v distlab-grafana-data:/var/lib/grafana \
    -e GF_PATHS_CONFIG=/var/lib/grafana/grafana.ini \
    -p 9030:3000 grafana/grafana > /dev/null