#
# Script installs Saritasa stylecop analyzer ruleset for every project in solution.
# Examples:
#   ps Saritasa.RulesetInstall.ps1
#   ps Saritasa.RulesetInstall.ps1 -remove 1 -exclude MyProject.*
#
# If you put ruleset file into ./tools directory it will be used instead global one.
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

    # remove case
    if ($remove -eq $false)
    {
        # is .NET Core project
        $isCoreProject = $false
        if ($projectXml.Project.Sdk -ne $null)
        {
            $isCoreProject = $true
        }

        # get first PropertyGroup without Condition attribute
        $propertyGroup = $projectXml.Project.PropertyGroup | Where-Object { $_.Condition -eq $null } `
            | Select-Object -First 1
        if ($propertyGroup -eq $null)
        {
            Write-Error "Cannot find default PropertyGroup"
            continue
        }

        # update project
        [string] $target = $rulesetPath
        if ($rulesetLocal)
        {
            Set-Location ([System.IO.Path]::GetDirectoryName($projectFile))
            $target = Resolve-Path $rulesetPath -Relative
            Set-Location $currentLocation
        }

        if ($propertyGroup.CodeAnalysisRuleSet -eq $null)
        {
            if ($propertyGroup.Condition -ne $null)
            {
                continue
            }
            if ($isCoreProject -eq $true)
            {
                $el = $projectXml.CreateElement("CodeAnalysisRuleSet")
            }
            else
            {
                $el = $projectXml.CreateElement("CodeAnalysisRuleSet", "http://schemas.microsoft.com/developer/msbuild/2003")
            }
            $el.InnerText = $target
            Write-Output "Add to file $projectFile"
            $propertyGroup.AppendChild($el) | Out-Null
        }
        else
        {
            $propertyGroup.CodeAnalysisRuleSet = $target
            Write-Output "Update file $projectFile"
        }

        # check that StyleCop is installed for project
        $stylecopNodes = $projectXml.SelectNodes("//*[contains(@Include, 'StyleCop.Analyzers')]")
        if ($stylecopNodes.Count -eq 0)
        {
            Write-Warning "StyleCop seems not installed for project $projectFile"
        }
    }
    else
    {
        foreach ($propertyGroup in $projectXml.Project.PropertyGroup)
        {
            if ($propertyGroup.SelectNodes)
            {
                $codeAnalysisNode = $propertyGroup.SelectNodes('*') | Where-Object { $_.Name -eq "CodeAnalysisRuleSet" } `
                    | Select-Object -First 1
                if ($codeAnalysisNode -ne $null)
                {
                    Write-Output "Remove CodeAnalysisRuleSet from $projectFile"
                    $codeAnalysisNode.ParentNode.RemoveChild($codeAnalysisNode) | Out-Null
                }
            }
        }
    }

    $projectXml.Save((Resolve-Path $projectFile))
}
