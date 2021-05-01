docker container rm distlab-prometheus --force > NUL
docker volume rm distlab-prometheus-data > NUL
docker container create ^
    --name distlab-prometheus ^
    -v distlab-prometheus-data:/prometheus ^
    -p 9090:9090 prom/prometheus --config.file "/prometheus/prometheus-win.yml"