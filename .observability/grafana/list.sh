#!/bin/bash
docker run --rm -v distlab-grafana-data:/dest ubuntu bash -c "cd /dest && ls -a"