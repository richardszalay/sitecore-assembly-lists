param(
    [string]$AssemblyListIndex,
    [string]$ProductName,
    [string]$AssemblyList,
    [string]$ContextAssembly
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $AssemblyListIndex)) {
    Add-Content -Path $AssemblyListIndex -Value "ProductName,AssemblyList,Assembly,AssemblyVersion,FileVersion"
}

if (".dll" -ne [System.IO.Path]::GetExtension($ContextAssembly)) {
    $ContextAssembly = "$ContextAssembly.dll"
}

function ParseAssemblyList($file)
{
    $lines = Get-Content $file

    $lines | Select-Object -Skip 1 | Foreach-Object {
        $parts = $_.Split(@(',',' '))
        @{ 
            Assembly = ($parts[0]);
            FileVersion = $parts[1];
            AssemblyVersion = $parts[2]
        }
    }
}

$assemblyListEntries = ParseAssemblyList $AssemblyList

$contextAssemblyEntry = $assemblyListEntries | Where-Object { $_.Assembly -eq $ContextAssembly }

if (-not $contextAssemblyEntry) {
    throw "$ContextAssembly not found in $AssemblyList"
}

$assemblyListFilename = Split-Path $AssemblyList -Leaf

Add-Content -Path $AssemblyListIndex -Value "$ProductName,$assemblyListFilename,$ContextAssembly,$($contextAssemblyEntry.AssemblyVersion),$($contextAssemblyEntry.FileVersion)"
