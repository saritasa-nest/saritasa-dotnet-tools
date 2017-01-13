Task docker-zerg -depends package-zerg `
{
    $dockerContext = "$samples\ZergRushCo.Todosya\Docker"
    $version = (gitversion /showvariable SemVer)

    Exec { docker build -t zerg/web:latest -t "zerg/web:$version" -f "$dockerContext\Dockerfile.web" $dockerContext }
    Exec { docker build -t zerg/db:latest -t "zerg/db:$version" -f "$dockerContext\Dockerfile.db" $dockerContext }
}

Task docker-bw -depends package-bw `
{
    $dockerContext = "$samples\Saritasa.BoringWarehouse\Docker"
    $version = (gitversion /showvariable SemVer)

    Exec { docker build -t bw/web:latest -t "bw/web:$version" -f "$dockerContext\Dockerfile.web" $dockerContext }
    Exec { docker build -t bw/db:latest -t "bw/db:$version" -f "$dockerContext\Dockerfile.db" $dockerContext }
}

# Docker images should be built before run (docker-bw task).
Task run-bw-tests `
{
    # Recreate and containers.
    Exec { docker-compose -f "$samples\Saritasa.BoringWarehouse\docker-compose.yml" up --force-recreate -d }

    # Get IP address of DB container.
    $ipAddress = Exec { docker inspect saritasaboringwarehouse_db_1 -f '{{ .NetworkSettings.Networks.nat.IPAddress }}' }
    Write-Information "DB container address: $ipAddress"

    # Replace connection string.
    $appConfigPath = "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.IntegrationTests\bin\$Configuration\Saritasa.BoringWarehouse.IntegrationTests.dll.config"
    $lines = Get-Content $appConfigPath
    $lines | % `
        {
            $_ -replace '(^\s*)<add name="AppDbContext" connectionString="(.*)" .*$',
                "$1<add name=`"AppDbContext`" connectionString=`"Data Source=$ipAddress;Initial Catalog=BoringWarehouse;User ID=TestUser;Password=gHJT2SCm`" providerName=`"System.Data.SqlClient`" />"
        } | Set-Content $appConfigPath
    Write-Information "Updated $appConfigPath."

    Invoke-NUnit3Runner "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.IntegrationTests\bin\$Configuration\Saritasa.BoringWarehouse.IntegrationTests.dll"
}

Task docker-run-all -depends docker-bw, docker-zerg `
{
    Exec { docker-compose -f "$samples\Saritasa.BoringWarehouse\docker-compose.yml" up -d }
    Exec { docker-compose -f "$samples\ZergRushCo.Todosya\docker-compose.yml" up -d }
}
