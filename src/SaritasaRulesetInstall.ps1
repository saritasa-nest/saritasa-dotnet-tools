# Installs Saritasa stylecop analyzer ruleset

Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/saritasa/SaritasaTools/develop/src/tools/Saritasa.ruleset' `
    -OutFile "${env:ProgramFiles(x86)}\Microsoft Visual Studio 14.0\Team Tools\Static Analysis Tools\Rule Sets\Saritasa.ruleset"
Write-Output 'The Saritasa C# ruleset is successfully installed!'
