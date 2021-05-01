#!/bin/bash
docker run --rm -v $(pwd)/backup:/backup -v distlab-grafana-data:/dest ubuntu bash -c "cd /dest && tar cvf /backup/backup.tar ."