$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Import-Module "$root\Saritasa.WebDeploy.psd1"

Task package-zerg -depends build-zerg `
{
    $packagePath = "$samples\ZergRushCo.Todosya\Docker\Zerg.zip"
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration
}

Task docker-zerg -depends package-zerg `
{
    $dockerContext = "$samples\ZergRushCo.Todosya\Docker"
    $version = (gitversion /showvariable SemVer)

    Exec { docker build -t zerg/web:latest -t "zerg/web:$version" -f "$dockerContext\Dockerfile.web" $dockerContext }
    Exec { docker build -t zerg/db:latest -t "zerg/db:$version" -f "$dockerContext\Dockerfile.db" $dockerContext }
}

Task publish-bw -depends build-bw `
{
    $packagePath = "$samples\Saritasa.BoringWarehouse\Docker\BW.zip"
    Invoke-PackageBuild -ProjectPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.Web\Saritasa.BoringWarehouse.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration -Precompile $false
}

Task docker-bw -depends build-bw `
{
    $dockerContext = "$samples\Saritasa.BoringWarehouse\Docker"
    $version = (gitversion /showvariable SemVer)

    Exec { docker build -t bw/web:latest -t "bw:$version" $dockerContext }
}
