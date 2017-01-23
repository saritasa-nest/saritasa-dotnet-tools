# escape=`
FROM microsoft/iis
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]

RUN `
    # Disable DNS cache.
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord; `
    # Install ASP.NET.
    Install-WindowsFeature NET-Framework-45-ASPNET; `
    Install-WindowsFeature Web-Asp-Net45; `
    # Install Chocolatey.
    $env:chocolateyUseWindowsCompression = 'false'; `
    iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex; `
    # Install WebDeploy.
    choco install webdeploy -y

COPY Run.ps1 Temp\Run.ps1
COPY ZergRushCo.zip Temp\ZergRushCo.zip

RUN C:\Temp\Run.ps1

EXPOSE 80
