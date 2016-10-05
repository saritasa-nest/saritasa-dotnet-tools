FROM microsoft/iis
SHELL ["powershell"]

RUN Install-WindowsFeature NET-Framework-45-ASPNET; \  
    Install-WindowsFeature Web-Asp-Net45

COPY Zerg.zip Temp/Zerg.zip

EXPOSE 80

CMD ["dir"]

ENTRYPOINT ["cmd"]
