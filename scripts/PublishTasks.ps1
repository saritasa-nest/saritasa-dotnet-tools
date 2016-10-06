$root = $PSScriptRoot
$src = "$root\..\src"
$samples = "$root\..\samples"

Import-Module "$root\Saritasa.WebDeploy.psd1"

Task package-zerg -depends build-zerg `
{
    $packagePath = "$samples\ZergRushCo.Todosya\Zerg.zip"
    Invoke-PackageBuild -ProjectPath "$samples\ZergRushCo.Todosya\ZergRushCo.Todosya.Web\ZergRushCo.Todosya.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration

    Set-Location "$samples\ZergRushCo.Todosya\Docker"
    Copy-Item $packagePath .

    $version = (gitversion /showvariable SemVer)
    docker build -t zerg:latest -t "zerg:$version" .
}

Task publish-bw -depends build-bw `
{
    $packagePath = "$samples\Saritasa.BoringWarehouse\BW.zip"
    Invoke-PackageBuild -ProjectPath "$samples\Saritasa.BoringWarehouse\Saritasa.BoringWarehouse.Web\Saritasa.BoringWarehouse.Web.csproj" `
        -PackagePath $packagePath -Configuration $Configuration -Precompile $false

    Set-Location "$samples\Saritasa.BoringWarehouse\Docker"
    Copy-Item $packagePath .

    $version = (gitversion /showvariable SemVer)
    docker build -t zerg:latest -t "bw:$version" .
}
