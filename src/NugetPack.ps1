dotnet pack -c Release -o .\bin\Release\nuget\

Copy-Item .\bin\Release\nuget\* C:\Users\z163z\AppData\Roaming\NuGet\bin\Release\nuget -Recurse

dotnet nuget update source LocalPackages -s C:\Users\z163z\AppData\Roaming\NuGet\bin\Release\nuget\