dotnet pack -c Release -o .\bin\Release\nuget\

Copy-Item .\bin\Release\nuget\* $HOME\AppData\Roaming\NuGet\bin\Release\nuget -Recurse

dotnet nuget update source LocalPackages -s $HOME\AppData\Roaming\NuGet\bin\Release\nuget\