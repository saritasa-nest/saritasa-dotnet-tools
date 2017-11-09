#
# Script copies Saritasa stylecop analyzer ruleset into Visual Studio directory.
#

# check that script is running under admin rights
$windowsId = [System.Security.Principal.WindowsIdentity]::GetCurrent()
$windowsPrincipal = new-object System.Security.Principal.WindowsPrincipal($windowsId)
$adminRole = [System.Security.Principal.WindowsBuiltInRole]::Administrator
if (!$windowsPrincipal.IsInRole($adminRole))
{
    Write-Error 'Script requires administrator rights to run'
    exit 1
}

# get Visual Studio directory using VS*COMNTOOLS environment variable
$comntools = ('VS90COMNTOOLS', 'VS100COMNTOOLS', 'VS110COMNTOOLS', 'VS120COMNTOOLS', 'VS130COMNTOOLS',
    'VS140COMNTOOLS', 'VS150COMNTOOLS', 'VS160COMNTOOLS', 'VS170COMNTOOLS')
foreach ($comntool in $comntools)
{
    $vspath = [environment]::GetEnvironmentVariable($comntool)
    if ($vspath.length -gt 0)
    {
        break
    }
}
if ($vspath.length -lt 1)
{
    Write-Error 'Cannot find Visual Studio directory'
    exit 2
}

# install ruleset
try
{
    Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/Saritasa/SaritasaTools/develop/tools/Saritasa.ruleset' `
        -OutFile "${vspath}/../../Team Tools/Static Analysis Tools/Rule Sets/Saritasa.ruleset"
}
catch
{
    Write-Error "Cannot download Saritasa ruleset from GitHub ($_)"
    exit 3
}

# everything is good
Write-Output 'The Saritasa C# ruleset has been successfully installed!'
