#!/bin/bash
docker container stop distlab-prometheus &&
docker container stop distlab-zipkin &&
docker container stop distlab-grafana