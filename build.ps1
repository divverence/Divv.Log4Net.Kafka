function New-Package($version) {
    New-SharedAssemblyInfo $version
    dotnet pack /p:PackageVersion="$($version.FullSemVer)" /p:NoPackageAnalysis=true /p:Configuration=Release
} 

function New-SharedAssemblyInfo($version) {
    $assemblyInfoContent = @"
// <auto-generated/>
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompanyAttribute("Divverence B.V.")]
[assembly: AssemblyProductAttribute("Divverence log4net Kafka & JSON")]
[assembly: AssemblyCopyrightAttribute("Copyright 2019 Divverence B.V.")]
[assembly: AssemblyVersionAttribute("$($version.AssemblyVersion)")]
[assembly: AssemblyFileVersionAttribute("$($version.AssemblyVersion)")]
[assembly: AssemblyInformationalVersionAttribute("$($version.FullSemVer)")]
"@

    if (-not (Test-Path ".build")) {
        New-Item -ItemType Directory ".build"
    }
    $assemblyInfoContent | Out-File -Encoding utf8 (Join-Path ".build" "SharedAssemblyInfo.cs") -Force
}

Remove-Item .build -Force -Recurse -ErrorAction SilentlyContinue
Remove-Item built -Force -Recurse -ErrorAction SilentlyContinue

$version = git-flow-version | ConvertFrom-Json
Write-Host "calculated version:"
$version | Format-List
New-Package $version

dotnet clean 
dotnet restore
dotnet build --no-restore -c Release

if (-not (Test-Path "built")) {
	New-Item -ItemType Directory "built"
}

dotnet test --no-restore --no-build -c Release /p:CollectCoverage=true /p:Exclude=[xunit.*]* /p:CoverletOutput='../../built/DivvLog4Net.xml' /p:CoverletOutputFormat=cobertura

$Net452ZipPath = (Join-Path "built" "Divv.Log4Net.Kafka_$($version.SemVer)_net452.zip")
gci src\*\bin\Release\net452\Divv*.dll | compress-archive -DestinationPath $Net452ZipPath
gci -Directory src\*\bin\Release\net452\* | compress-archive -DestinationPath $Net452ZipPath -Update
