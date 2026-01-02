# Dynamic Copyright Year in GlobalAssemblyInfo

## Overview
The copyright year in `GlobalAssemblyInfo.cs` is now automatically generated at build time, so it doesn't need to be manually updated every year.

## How It Works

1. **Template File**: `GlobalAssemblyInfo.cs.template` contains a placeholder `{CURRENT_YEAR}` in the copyright line
2. **Build Process**: `Directory.Build.props` defines an MSBuild target that runs before compilation
3. **Generation**: The target uses PowerShell to replace `{CURRENT_YEAR}` with the current year and generates `GlobalAssemblyInfo.cs`
4. **Git Ignore**: `GlobalAssemblyInfo.cs` is added to `.gitignore` as it's a generated file

## Files

- **Source/GlobalAssemblyInfo.cs.template** - Template with `{CURRENT_YEAR}` placeholder
- **Source/Directory.Build.props** - MSBuild configuration that generates the file
- **Source/GlobalAssemblyInfo.cs** - Generated file (not in source control)

## Manual Generation

If you need to generate the file manually:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "(Get-Content 'Source/GlobalAssemblyInfo.cs.template') -replace '\{CURRENT_YEAR\}', (Get-Date).Year | Set-Content 'Source/GlobalAssemblyInfo.cs'"
```

## Benefits

- No need to remember to update the year annually
- Consistent copyright year across all builds
- Year reflects when the build was created, not when the source was last modified
