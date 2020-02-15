outputFile=bin/release/netcoreapp2.1/DynamoDbServerless.zip
rm -f $outputFile
dotnet lambda package --configuration release --framework netcoreapp2.1 --output-package $outputFile --msbuild-parameters DynamoDbServerless.csproj
