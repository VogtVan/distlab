# Description

## backup

Contains backup data from grafana container

# Life cycle

- ./init.sh removes and recreates the grafana container
- ./restore.sh repopulates config and db
- ./backup.sh performs grafana container backup into the backup directory

