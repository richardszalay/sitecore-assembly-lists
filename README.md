Sitecore started providing assembly list information for Sitecore releases starting at 8.2 Update 5. This project provides tools for validating Sitecore assembly lists, both against an installation and during the development process.

# PowerShell Module

## Installation

```
PS> Install-Module SitecoreAssemblyLists
```

## Usage

```
Test-SitecoreAssemblyList `
    -AssemblyList ".\Sitecore.Platform.Assemblies 9.0.0 rev. 171002.txt" `
    -AssemblyFolder c:\inetpub\wwwroot\Sitecore\bin
```

Returns `$true` if all assemblies in the list were found in the directory and had the correct assembly and file version.

If the `-PassThru` switch is supplied, returns an object with properties containing the `Total` and `Failed` number of assemblies.

In both cases, missing assemblies and version mismatches emitted as warnings.

```
New-SitecoreAssemblyList `
    -AssemblyFolder c:\inetpub\wwwroot\Sitecore\bin `
    -FilePath ".\Sitecore.Platform.Assemblies 7.2.0 rev. 1501021.txt"
```

Generates a new Sitecore assembly list for all assemblies in a folder. Useful when working with versions of Sitecore before 8.2u5, when Sitecore began making the lists available for download.

# MSBuild Targets

Coming soon