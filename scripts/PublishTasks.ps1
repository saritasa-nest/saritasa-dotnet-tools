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