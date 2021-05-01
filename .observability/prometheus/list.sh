#!/bin/bash
docker run --rm -v distlab-prometheus-data:/dest ubuntu bash -c "cd /dest && ls -a"