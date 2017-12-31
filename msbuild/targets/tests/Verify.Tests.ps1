. "$PSScriptRoot/utils/MSBuild.ps1"

$fixtures = @{
    default = @{
        Project = "$PSScriptRoot/fixtures/default/SitecoreAssemblyList.TestFixture.csproj";
    }    
}

Describe "VerifySitecoreAssemblyVersions" {
    Context "when assembly versions do not match the assembly list" {
        $projectPath = $fixtures.default.Project
        
        $out = Invoke-MSBuild -Project $projectPath -Properties @{
            "Configuration" = "Debug";
            "SitecoreAssemblyRefrenceSuffix" = "-alt";
        }

        It "should emit a warning" {
            $out | Should Not BeNullOrEmpty
        }
    }

    Context "when disabled" {
        $projectPath = $fixtures.default.Project
        
        $out = Invoke-MSBuild -Project $projectPath -Properties @{
            "Configuration" = "Debug";
            "SitecoreAssemblyRefrenceSuffix" = "-alt";
            "VerifySitecoreAssemblyVersions" = "false"
        }

        It "should not emit a warning" {
            $out | Should BeNullOrEmpty
        }
    }

    Context "when assembly versions match the assembly list" {
        $projectPath = $fixtures.default.Project
        
        $out = Invoke-MSBuild -Project $projectPath -Properties @{
            "Configuration" = "Debug";
            "SitecoreAssemblyRefrenceSuffix" = "";
        }

        It "should not emit a warning" {
            $out | Should BeNullOrEmpty
        }
    }

}

