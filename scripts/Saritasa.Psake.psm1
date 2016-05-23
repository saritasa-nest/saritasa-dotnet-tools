<#
.SYNOPSIS

.DESCRIPTION
Tasks with description starting with * are main. They are shown on the help screen. Other tasks are auxiliary.
#>
function Register-HelpTask
{
    param
    (
        [string] $Name = 'help',
        [bool] $Default = $true
    )

    Task $Name -description 'Display description of main tasks.' `
    {
        Write-Host 'Main Tasks' -ForegroundColor DarkMagenta -NoNewline
        Get-PSakeScriptTasks | ? { $_.Description -Like '`**' } | Format-Table -Property Name, @{ Label = 'Description'; Expression = { $_.Description -Replace '^\* ', '' } }
        
        Write-Host 'Execute ' -NoNewline -ForegroundColor DarkMagenta
        Write-Host 'psake -docs' -ForegroundColor Black -BackgroundColor DarkMagenta -NoNewline
        Write-Host ' to see all tasks.' -ForegroundColor DarkMagenta
    }
    
    if ($Default)
    {
        Task default -depends $Name -description 'Show automatically generated help.'
    }
}
