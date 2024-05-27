$ErrorActionPreference = 'Stop'

$packageName = 'NETworkManager'
$softwareName = 'NETworkManager*'
$installerType = 'MSI'
$silentArgs = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiUninstall.log`""
# https://msdn.microsoft.com/en-us/library/aa376931(v=vs.85).aspx
$validExitCodes = @(0, 3010, 1605, 1614, 1641)

$uninstalled = $false

[array]$key = Get-UninstallRegistryKey -SoftwareName $softwareName

if ($key.Count -eq 1) {
  $key | ForEach-Object {
    $file = "$($_.UninstallString)"

    if ($installerType -eq 'MSI') {
      # The Product Code GUID is all that should be passed for MSI, and very
      # FIRST, because it comes directly after /x, which is already set in the
      # Uninstall-ChocolateyPackage msiargs (facepalm).
      $silentArgs = "$($_.PSChildName) $silentArgs"

      # Don't pass anything for file, it is ignored for msi (facepalm number 2)
      # Alternatively if you need to pass a path to an msi, determine that and
      # use it instead of the above in silentArgs, still very first
      $file = ''
    }

    Uninstall-ChocolateyPackage -PackageName $packageName `
      -FileType $installerType `
      -SilentArgs "$silentArgs" `
      -ValidExitCodes $validExitCodes `
      -File "$file"
  }
}
elseif ($key.Count -eq 0) {
  Write-Warning "$packageName has already been uninstalled by other means."
}
elseif ($key.Count -gt 1) {
  Write-Warning "$key.Count matches found!"
  Write-Warning "To prevent accidental data loss, no programs will be uninstalled."
  Write-Warning "Please alert package maintainer the following keys were matched:"
  $key | ForEach-Object { Write-Warning "- $_.DisplayName" }
}
