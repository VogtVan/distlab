#!/bin/bash
docker run --rm -v $(pwd)/backup:/backup -v distlab-prometheus-data:/dest ubuntu bash -c "cd /dest && tar cvf /backup/backup.tar ."