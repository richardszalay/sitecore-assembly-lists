Import-Module "$PSScriptRoot\..\src\SitecoreAssemblyList.psd1" -Scope Local -Force | Out-Null

# Just a few high level acceptance tests
Describe "Acceptance fixture" {
    Context "when testing the acceptance assembly list" {
        $result = Test-SitecoreAssemblyList -AssemblyList "$PSScriptRoot\fixtures\acceptance-list.txt" -AssemblyFolder "$PSScriptRoot\fixtures\acceptance"
         
        It "successfully validates" {
            $result | Should Be $true
        }
    }

    Context "when generating the acceptance assembly list" {
        $assemblyList = New-TemporaryFile
        $assemblyListFile = New-SitecoreAssemblyList -FilePath $assemblyList -AssemblyFolder "$PSScriptRoot\fixtures\acceptance" -PassThru
         
        It "generates the expected output" {
            Get-Content $assemblyListFile -Raw | Should Be "Filename,FileVersion,Version
Test1.dll,1.2.3.4,1.0.0.0
Test2.dll,2.1.0.0,2.1.0.0
Test3.dll,1.2.3.4,3.0.1.0
Test4.dll,1.2.3.0,4.0.0.1
"
        }
    }

    Context "when attempting a round trip validation" {
        $assemblyList = New-TemporaryFile
        $assemblyListFile = New-SitecoreAssemblyList -FilePath $assemblyList -AssemblyFolder "$PSScriptRoot\fixtures\acceptance" -PassThru
        $result = Test-SitecoreAssemblyList -AssemblyList $assemblyListFile -AssemblyFolder "$PSScriptRoot\fixtures\acceptance"
         
        It "successfully validates" {
            $result | Should Be $true
        }
    }
}

#$asm = New-SitecoreAssemblyList -FilePath (New-TemporaryFile) -AssemblyFolder $folder -PassThru

#Test-SitecoreAssemblyList -AssemblyList $asm -AssemblyFolder $folder