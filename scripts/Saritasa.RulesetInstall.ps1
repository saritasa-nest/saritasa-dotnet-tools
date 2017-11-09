#
# Script installs Saritasa stylecop analyzer ruleset for every project in solution.
# Examples:
#   ps Saritasa.RulesetInstall.ps1
#   ps Saritasa.RulesetInstall.ps1 -remove 1 -exclude MyProject.*
#

param (
    # remove CodeAnalysisRuleSet values
    [boolean] $remove = $false,
    # exclude specified files using file mask
    [string] $exclude = $null
)

# get all project files first
$projectFiles = Get-ChildItem -Path .\ -Filter *.csproj -Recurse -Name -Exclude $exclude
$projectFiles += Get-ChildItem -Path .\ -Filter *.vbproj -Recurse -Name -Exclude $exclude
$currentLocation = Get-Location

# determine ruleset path
$rulesetPath = "Saritasa.ruleset"
$rulesetLocal = $false
if (Test-Path "./tools/Saritasa.ruleset")
{
    $rulesetPath = Resolve-Path "./tools/Saritasa.ruleset"
    $rulesetLocal = $true
}

# add code analysis ruleset to every project
foreach ($projectFile in $projectFiles)
{
    [xml] $projectXml = Get-Content $projectFile

    # is .NET Core project
    $isCoreProject = $false
    if ($projectXml.Sdk)
    {
        $isCoreProject = $true
    }

    # update projects
    foreach ($propertyGroup in $projectXml.Project.PropertyGroup)
    {
        [string] $target = $rulesetPath
        if ($rulesetLocal)
        {
            Set-Location ([System.IO.Path]::GetDirectoryName($projectFile))
            $target = Resolve-Path $rulesetPath -Relative
            Set-Location $currentLocation
        }

        if ($propertyGroup.CodeAnalysisRuleSet -eq $null)
        {
            # for standard .NET project we also should check Condition block existance
            if ($isCoreProject -eq $false -and $propertyGroup.Condition -eq $null -or $remove -eq $true)
            {
                continue
            }
            $el = $projectXml.CreateElement("CodeAnalysisRuleSet", "http://schemas.microsoft.com/developer/msbuild/2003")
            $el.InnerText = $target
            Write-Output "Add to file $projectFile"
            $propertyGroup.AppendChild($el) | out-null
        }
        else
        {
            if ($remove -eq $false)
            {
                $propertyGroup.CodeAnalysisRuleSet = $target
            }
            else
            {
                $propertyGroup.CodeAnalysisRuleSet = ''
            }
            Write-Output "Update file $projectFile"
        }
    }
    $projectXml.Save((Resolve-Path $projectFile))

    # check that StyleCop is install for project
    $stylecopNodes = $projectXml.SelectNodes("//*[contains(@Include, 'StyleCop.Analyzers')]")
    if ($stylecopNodes.Count -eq 0)
    {
        Write-Warning "StyleCop seems not installed for project $projectFile"
    }
}
