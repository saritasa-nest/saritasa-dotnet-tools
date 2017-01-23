# escape=`
FROM microsoft/aspnet
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]

RUN `
    # Disable DNS cache.
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord; `
    # Install Chocolatey.
    $env:chocolateyUseWindowsCompression = 'false'; `
    iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex; `
    # Install WebDeploy.
    choco install webdeploy -y

COPY Run.ps1 Temp\Run.ps1
COPY BoringWarehouse.zip C:\Temp\BoringWarehouse.zip

RUN C:\Temp\Run.ps1

EXPOSE 80
