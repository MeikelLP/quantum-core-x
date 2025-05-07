echo 'Attention! This does not check if the migration has been applied to the db. You will have to reset your db manually'

$providers = @{
    'Sqlite' = 'Data Source=game.sqlite';
    'Mysql' = 'Server=localhost;Database=game;Uid=metin2;Pwd=metin2;';
    'Postgresql' = 'Server=localhost;Database=game;User Id=metin2;Password=metin2;'
}

dotnet build $PSScriptRoot/../../Executables/Game > $null

foreach ($provider in $providers.GetEnumerator())
{
  dotnet dotnet-ef migrations remove `
        --no-build `
        --context "$( $provider.Name )GameDbContext" `
        --startup-project $PSScriptRoot/../../Executables/Game/ `
        --force `
        -- `
        --Database:Provider $provider.Name `
        --Database:ConnectionString $provider.Value
}
