# Legacy

While we try to support as much of the previous functionality not everything can be replicated due to different
constraints. If you think something should be supported please make a PR.

QuantumCoreX tries to implement Metin2 in a modern way. Some things may not match the new workflow. However we want to
collect some scripts here to help migrating or importing legacy data into QuantumCoreX.

## Database

### Shops

```shell
docker run --rm mysql:latest mysql -h host.docker.internal -P 3306 -u root -pREPLACE_ME -e "SELECT * FROM player.shop;" > shops.tsv
docker run --rm mysql:latest mysql -h host.docker.internal -P 3306 -u root -pREPLACE_ME -e "SELECT * FROM player.shop_item;" > shop_items.tsv
```