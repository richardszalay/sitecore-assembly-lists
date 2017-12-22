function New-MockAssemblyInfo {
    param(
        [string]$Name, 
        [string]$Version, 
        [string]$FileVersion = $null
    )

    $tempFile = New-TemporaryFile

    Add-Content -Path $tempFile -Value "using System.Reflection;"
    Add-Content -Path $tempFile -Value "[assembly:AssemblyTitle(`"$Name`")]"
    Add-Content -Path $tempFile -Value "[assembly:AssemblyVersion(`"$Version`")]"
    if ($FileVersion) {
        Add-Content -Path $tempFile -Value "[assembly:AssemblyFileVersion(`"$FileVersion`")]"
    }

    $tempFile
}

function New-MockAssembly {
    param(
        [string]$FilePath,
        [string]$Version, 
        [string]$FileVersion = $null
    )

    $Name = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
    
    $assemblyInfo = New-MockAssemblyInfo $Name $Version $FileVersion
    
    c:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /nologo /target:library /out:$FilePath $assemblyInfo.FullName
}

$ErrorActionPreference = "Stop"

$fixturesRoot = Resolve-Path "$PSScriptRoot\..\fixtures"

$acceptanceFixturesRoot = Join-Path $fixturesRoot "acceptance"

mkdir -Path $acceptanceFixturesRoot -ErrorAction SilentlyContinue

New-MockAssembly -FilePath "$acceptanceFixturesRoot\Test1.dll" -Version "1.0.0.0" -FileVersion "1.2.3.4"
New-MockAssembly -FilePath "$acceptanceFixturesRoot\Test2.dll" -Version "2.1.0.0"
New-MockAssembly -FilePath "$acceptanceFixturesRoot\Test3.dll" -Version "3.0.1.0" -FileVersion "1.2.3.04"
New-MockAssembly -FilePath "$acceptanceFixturesRoot\Test4.dll" -Version "4.0.0.1" -FileVersion "1.2.3"
