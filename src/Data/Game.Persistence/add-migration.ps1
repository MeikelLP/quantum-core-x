param (
    [Parameter(Position = 0, Mandatory = $true)]
    [String]
    $migrationName
)

$providers = 'Sqlite', 'Mysql', 'Postgresql'

dotnet build $PSScriptRoot/../../Executables/Game > $null

$providers | foreach {
    dotnet ef migrations add $migrationName `
        --no-build `
        --context "$( $_ )GameDbContext" `
        --output-dir "$PSScriptRoot/Migrations/$_" `
        --startup-project $PSScriptRoot/../../Executables/Game/ `
        -- `
        --Database:Provider $_ `
        --Database:ConnectionString "Server=localhost;Database=game;Uid=metin2;Pwd=metin2;"
}
