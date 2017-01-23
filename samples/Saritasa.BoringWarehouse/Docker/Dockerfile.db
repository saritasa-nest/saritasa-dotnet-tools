# escape=`
FROM microsoft/mssql-server-windows-express
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]

ENV sa_password 29hHzGtB
ENV ACCEPT_EULA Y

ADD CreateDB.sql C:\Temp\CreateDB.sql

# TODO: Wait for SQL Server start here.

RUN sqlcmd -i C:\Temp\CreateDB.sql
