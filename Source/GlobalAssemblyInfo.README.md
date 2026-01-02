# Dynamic Copyright Year in GlobalAssemblyInfo

## Overview
The copyright year in `GlobalAssemblyInfo.cs` is now automatically generated at build time, so it doesn't need to be manually updated every year.

## How It Works

1. **Template File**: `GlobalAssemblyInfo.cs.template` contains a placeholder `{CURRENT_YEAR}` in the copyright line
2. **Build Process**: `Directory.Build.props` defines an MSBuild target that runs before compilation
3. **Generation Script**: `GenerateGlobalAssemblyInfo.ps1` handles the file generation with thread-safe mutex locking for parallel builds
4. **Thread Safety**: The script uses a global mutex to prevent concurrent file access issues when building multiple projects in parallel
5. **Git Ignore**: `GlobalAssemblyInfo.cs` is added to `.gitignore` as it's a generated file

## Files

- **Source/GlobalAssemblyInfo.cs.template** - Template with `{CURRENT_YEAR}` placeholder
- **Source/GenerateGlobalAssemblyInfo.ps1** - PowerShell script that generates the file with thread safety
- **Source/Directory.Build.props** - MSBuild configuration that invokes the generation script
- **Source/GlobalAssemblyInfo.cs** - Generated file (not in source control)

## Parallel Build Support

The generation script uses a named mutex (`Global\NETworkManager_GlobalAssemblyInfo_Mutex`) to ensure thread-safe file generation when Visual Studio builds multiple projects in parallel. This prevents file access conflicts and "stream not readable" errors.

## Manual Generation

If you need to generate the file manually:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File Source/GenerateGlobalAssemblyInfo.ps1 -TemplatePath "Source/GlobalAssemblyInfo.cs.template" -OutputPath "Source/GlobalAssemblyInfo.cs" -Year (Get-Date).Year
```

## Benefits

- No need to remember to update the year annually
- Consistent copyright year across all builds
- Year reflects when the build was created, not when the source was last modified
- Thread-safe for parallel builds in Visual Studio
