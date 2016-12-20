mkdir tools
if not exist tools\nuget.exe powershell -command "&Invoke-WebRequest -Uri \"https://dist.nuget.org/win-x86-commandline/latest/nuget.exe\" -OutFile \"tools\nuget.exe\""
if not exist tools\OpenCover.4.6.519\tools\OpenCover.Console.exe tools\nuget.exe install OpenCover -Version 4.6.519 -OutputDirectory tools
tools\nuget.exe install ReportGenerator -Version 2.5.1 -OutputDirectory tools
tools\OpenCover.4.6.519\tools\OpenCover.Console.exe -target:"%ProgramFiles%\dotnet\dotnet.exe" -targetargs:"test test/Unosquare.Swan.Test" -output:coverage.xml -register:user
tools\ReportGenerator.2.5.1\tools\ReportGenerator.exe "-reports:coverage.xml" -targetdir:coverageReport