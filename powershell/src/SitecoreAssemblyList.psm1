function NormalizeVersion([string]$versionString) {
    $normalizedVerson = ($versionString.Split('.') | Select -First 4) -join "."
    while (([version]$normalizedVerson).Revision -eq -1) { # yes, this is dubious
        $normalizedVerson = "$normalizedVerson.0"
    }
    $normalizedVerson
}

function ParseAssemblyList($file)
{
    $lines = Get-Content $file

    $lines | Select-Object -Skip 1 | Foreach-Object {
        $parts = $_ -split ","
        @{ 
            Assembly = ($parts[0]);
            FileVersion = $parts[1];
            AssemblyVersion = $parts[2]
        }
    }
}


function GetAssemblyInfo {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True,ValueFromPipeline=$True)]
        [string[]]$InputObject
    )

    $jobParams = @{ assemblyFiles = @($Input) }

    Start-Job -InputObject $jobParams -ScriptBlock {
        function NormalizeVersion([string]$versionString) {
            $normalizedVerson = ($versionString.Split('.') | Select -First 4) -join "."
            while (([version]$normalizedVerson).Revision -eq -1) { # yes, this is dubious
                $normalizedVerson = "$normalizedVerson.0"
            }
            $normalizedVerson
        }

        $assemblyFiles = $Input.assemblyFiles

        foreach ($assemblyFile in $assemblyFiles) {
            $assembly = [System.Reflection.Assembly]::LoadFrom($assemblyFile)
            $assemblyVersion = $assembly.GetName().Version.ToString()
            $fileVersion = $assembly.CustomAttributes | `
                Where-Object { $_.AttributeType.FullName -eq "System.Reflection.AssemblyFileVersionAttribute" } | `
                ForEach-Object { 
                    $rawVersion = $_.ConstructorArguments[0].Value
                    $normalizedVerson = NormalizeVersion $rawVersion

                    $v = ([version]$normalizedVerson)
                    "$($v.Major).$($v.Minor).$($v.Build).$($v.Revision)"
                }

            @{
                Assembly = Split-Path -Leaf $assemblyFile;
                FileVersion = [string]$fileVersion;
                AssemblyVersion = [string]$assemblyVersion
            }
        }            
    } | Receive-Job -Wait -AutoRemoveJob
}

filter IndexAssemblyInfo {
    begin { $hash = @{} }
    process { $hash[$_.Assembly] = $_ }
    end { return $hash }
}

function TestAssembly($definition, $actual)
{
    if (-not $actual)
    {
        Write-Warning "Assembly $($definition.Assembly) not found"
        return $false
    }

    if ($definition.AssemblyVersion -ne $actual.AssemblyVersion)
    {
        Write-Warning "Assembly $($definition.Assembly) version mismatch. Expected $($definition.AssemblyVersion) but found $($actual.AssemblyVersion)"
        return $false
    }

    if ($actual.FileVersion -and $actual.FileVersion -ne $definition.FileVersion)
    {
        Write-Warning "Assembly $($definition.Assembly) file version mismatch. Expected $($definition.FileVersion) but found $($actual.FileVersion)"
        return $false
    }

    return $true
}

function Test-SitecoreAssemblyList {
    #[CmdletBinding()]
    param(
        [string]$AssemblyList,
        [string]$AssemblyFolder,
        [switch]$PassThru
    )

    $assemblyDefinitions = ParseAssemblyList $AssemblyList

    $indexedAssemblyList = @($assemblyDefinitions.Assembly | %{ Join-Path $AssemblyFolder $_ }) | GetAssemblyInfo | IndexAssemblyInfo

    $passedValidations = ($assemblyDefinitions | Where-Object { TestAssembly $_ $indexedAssemblyList[$_.Assembly] })

    if ($PassThru) {
        New-Object psobject -Property @{
            Total = $assemblyDefinitions.Length;
            Failed = $assemblyDefinitions.Length - $passedValidations.Length
        }
    } else {
        $assemblyDefinitions.Length -eq $passedValidations.Length
    }
}

function New-SitecoreAssemblyList {
    [CmdletBinding()]
    param(
        [string]$AssemblyFolder,
        [string]$FilePath,
        [switch]$PassThru
    )

    $assemblies = (Get-ChildItem $AssemblyFolder -Filter *.dll).FullName

    $assemblyInfo = $assemblies | GetAssemblyInfo

    $header = @("Filename,FileVersion,Version")

    $lines = $assemblyInfo | ForEach-Object {
        $fileVersion = if ($_.FileVersion) { $_.FileVersion } else { $_.AssemblyVersion }
        "$($_.Assembly),$fileVersion,$($_.AssemblyVersion)"
    }

    $header+$lines | Set-Content $FilePath

    if ($PassThru) {
        Get-Item $FilePath
    }
}

Export-ModuleMember Test-SitecoreAssemblyList
Export-ModuleMember New-SitecoreAssemblyList


