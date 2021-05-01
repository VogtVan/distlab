REM remove existing container and volume
docker container rm distlab-grafana --force > NUL
docker volume rm distlab-grafana-data > NUL
REM first create grafana container which will create the volume for permissions issues
docker container create ^
    --name distlab-grafana ^
    -v distlab-grafana-data:/var/lib/grafana ^
    -e GF_PATHS_CONFIG=/var/lib/grafana/grafana.ini ^
    -p 9030:3000 grafana/grafana > NUL