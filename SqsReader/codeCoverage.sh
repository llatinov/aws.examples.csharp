#!/bin/sh

if [ ! -z $1 ]; then
  if [ $1 -lt 0 ] || [ $1 -gt 100 ]; then
    echo "Threshold should be between 0 and 100"
    threshold=80
  fi
  threshold=$1
else
  threshold=80
fi

dotnet restore
dotnet build

cd tools
dotnet restore

# Instrument assemblies inside 'test' folder to detect hits for source files inside 'src' folder
dotnet minicover instrument --workdir ../ --assemblies test/**/bin/**/*.dll --sources src/**/*.cs 

# Reset hits count in case minicover was run for this project
dotnet minicover reset

cd ..

for project in test/**/*.csproj; do dotnet test --no-build $project; done

cd tools

# Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
dotnet minicover uninstrument --workdir ../

# Create HTML reports inside folder coverage-html
# This command returns failure if the coverage is lower than the threshold
dotnet minicover htmlreport --workdir ../ --threshold $threshold

# Print console report
# This command returns failure if the coverage is lower than the threshold
dotnet minicover report --workdir ../ --threshold $threshold

# Create NCover report
dotnet minicover xmlreport --workdir ../ --threshold $threshold

cd ..