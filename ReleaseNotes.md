# Release Notes

## 2.2.4
* Added TestNupkg.ToByteArray and TestNupkg.Save(Stream) to create packages in memory
* Added TestNupkg.AddTextFile to add UTF-8 text files
* Added TestNupkg.FileName and TestNupkg.OverwriteExisting to control the saved file
* Added TestNupkg.NuspecEntryName to change the path of the nuspec in the package
* Added TestNupkg.EntryLastWriteTime to create packages with the same bytes each time
* Added TestNuspec license, repository, and framework reference metadata
* Added package type versions with TestNuspec.AddPackageType and TestNuspec.PackageTypeVersions

## 2.2.2
* Fixed TestNuspec ignoring ContentFiles
* Fixed TestNuspec dropping dependency include and exclude flags when no target framework is used
* Fixed TestNuspec failing when Dependencies has more than one group without a target framework
* Fixed TestNupkg.Save returning a FileInfo that reports the file as missing

## 2.2.1
* Added a package readme

## 2.1.54
* Update NuGet.* packages to 7.6.0

## 2.1.40
* net10.0 support

## 2.1.38
* Readme support

## 2.1.37
* Update NuGet.* packages to 6.12.1
* net9.0 support

## 2.1.36
* Updated NuGet.* packages to 6.9.1 to address CVE-2024-0057

## 2.1.35
* Update NuGet.* packages to 6.8.0
* net8.0 support

## 2.1.24
* Update NuGet.* packages to 6.2.1

## 2.1.0
* netstandard2.1 support

## 1.1.0
* NETStandard2.0 support, NETStandard1.3 has been removed
* NuGet 4.3.0 dependencies

## 1.0.0
* Shortened test directory path
* NuGet.Test.Helpers.dll is now signed

## 0.1.0-beta-2025
* First beta release