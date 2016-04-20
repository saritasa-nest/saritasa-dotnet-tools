# Requires psake to run, see README.md for more details.

if ($PSVersionTable.PSVersion.Major -lt 3)
{
    throw "PowerShell 3 is required.`nhttp://www.microsoft.com/en-us/download/details.aspx?id=40855"
}

task 'build' `
    -description "Build project and prepare nuget package" `
{
    & python .\scripts\make.py pack
}

task 'build-doc' `
    -description "Build project documentation" `
{
    cd .\docs
    & .\make.cmd html
    & .\_build\html\index.html
}
