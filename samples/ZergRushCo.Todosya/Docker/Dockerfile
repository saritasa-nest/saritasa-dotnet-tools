FROM microsoft/iis
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]

RUN Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord

RUN Install-WindowsFeature NET-Framework-45-ASPNET; \
    Install-WindowsFeature Web-Asp-Net45

RUN $env:chocolateyUseWindowsCompression = 'false'; \
    iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex

RUN choco install webdeploy -y

COPY Zerg.zip Temp/Zerg.zip
COPY Run.ps1 Temp/Run.ps1

RUN C:/Temp/Run.ps1

EXPOSE 80
