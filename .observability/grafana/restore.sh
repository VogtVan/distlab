#!/bin/bash
docker run --rm -v $(pwd)/backup:/backup -v distlab-grafana-data:/temp ubuntu bash -c "cd /backup && cp grafana.ini /temp && cd /temp && tar xvf /backup/backup.tar --strip 1 > /dev/null"