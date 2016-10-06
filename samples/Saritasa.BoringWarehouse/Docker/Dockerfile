FROM microsoft/aspnet
SHELL ["powershell"]

RUN $env:chocolateyUseWindowsCompression = 'false'; \
    iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex

RUN choco install webdeploy -y

COPY BW.zip Temp/BW.zip
COPY Run.ps1 Temp/Run.ps1

RUN C:/Temp/Run.ps1

EXPOSE 80
