dotnet publish -c Release --nologo

Copy-Item "./Setup/*" -Destination "./Deploy"

Compress-Archive -Path "./Deploy/*" -Destination "./SinExecSQLQuery.zip" -Force