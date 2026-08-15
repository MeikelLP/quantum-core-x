# Shops

The shop files are `shops.tsv` and `shop_items.tsv` which are the legacy sql tables but as TSV

## Generating tsv

```shell
docker run --rm mysql:latest mysql -h host.docker.internal -P 3306 -u root -pREPLACE_ME -e "SELECT * FROM player.shop;" > shops.tsv
docker run --rm mysql:latest mysql -h host.docker.internal -P 3306 -u root -pREPLACE_ME -e "SELECT * FROM player.shop_item;" > shop_items.tsv
```