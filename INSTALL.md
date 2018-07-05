Project Setup
=============

Here are steps you need to do to setup environment to be able to develop. You need following software installed:

- Visual Studio 2017 (https://www.visualstudio.com/downloads/download-visual-studio-vs.aspx) or JetBrains Rider (https://www.jetbrains.com/rider/)
- psake (https://github.com/psake/psake)
- PowerShell 5
- .NET Core (https://www.microsoft.com/net/core)

For documentation:

- python 3 (`choco install python`)
- sphinx (`pip install sphinx`)
- read the docs theme (`pip install sphinx_rtd_theme`)

Code Style Setup
----------------

We are using StyleCop.Analyzers project for extended code style check. It should be installed for every project in solution:

    ```
    Install-Package StyleCop.Analyzers
    ```

Then you need to install our codestyle ruleset. To do that just run the following cmd command under administrator:

    ```
    cmd:
    @powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/saritasa/SaritasaTools/develop/scripts/Saritasa.RulesetCopy.ps1'))" && SET PATH=%PATH%;

    PowerShell.exe (Ensure Get-ExecutionPolicy is at least RemoteSigned):
    iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/saritasa/SaritasaTools/develop/scripts/Saritasa.RulesetCopy.ps1'))

    PowerShell v3+ (Ensure Get-ExecutionPolicy is at least RemoteSigned):
    iwr https://raw.githubusercontent.com/saritasa/SaritasaTools/develop/scripts/Saritasa.RulesetCopy.ps1 -UseBasicParsing | iex
    ```

After that the "Saritasa Code Rules" will be available in "Code Analysis" tab in project properties. All Saritasa.Tools project uses relative path to ruleset within repository.

There is also `scripts\Saritasa.RulesetInstall.ps1` script that allows to setup ruleset for any project within current directory. It should be run within project repository root. If `/tools/Saritasa.ruleset` is found it ruleset file will be used. Example:

    ```
    PS W:\crm> W:\SaritasaTools\scripts\Saritasa.RulesetInstall.ps1 -exclude OldOrmProject.*
    ```

To install StyleCop analyzers for all projects within solution run following command in package manager console:

    ```
    Get-Project -All | Install-Package StyleCop.Analyzers
    ```
