Start-Process -Wait -NoNewWindow -PassThru 'C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe' `
    @('-verb:sync',
    "-source:package='C:\Temp\BW.zip'",
    '-dest:auto',
    "-setParam:name='IIS Web Application Name',value='Default Web Site/'",
    "-setParam:name='AppDbContext-Web.config Connection String',value='Data Source=bwdb;Initial Catalog=BoringWarehouse;User ID=TestUser;Password=gHJT2SCm'")
