dotnet restore DynamoDbServerless.csproj
dotnet lambda package --configuration release --framework netcoreapp2.1 --output-package bin/release/netcoreapp2.1/DynamoDbServerless.zip --msbuild-parameters DynamoDbServerless.csproj
