param(
    [Parameter(
        Position=0,
        Mandatory=$true)]
    [String]$OutPath
)

# VS prebuild event call: PowerShell.exe -ExecutionPolicy Bypass -NoProfile -File "$(ProjectDir)..\..\Scripts\PreBuildEventCommandLine.ps1" "$(TargetDir)"

# Fix wrong path
# Quotation marks are required if a blank is in the path... If there is no blank space in the path, a quote will be add to the end...
if(-not($OutPath.StartsWith('"')))
{
    $OutPath = $OutPath.TrimEnd('"')
}

# Test if files are already there...
if((Test-Path -Path "$OutPath\MSTSCLib.dll") -and (Test-Path -Path "$OutPath\AxMSTSCLib.dll"))
{
    Write-Host "MSTSCLib.dll and AxMSTSCLib.dll exist! Continue..."
    return
}

# Detect x86 or x64
$ProgramFiles_Path = ${Env:ProgramFiles(x86)}

if([String]::IsNullOrEmpty($ProgramFiles_Path))
{
    $ProgramFiles_Path = $Env:ProgramFiles    
}

# Get aximp.exe, ildasm.exe and ilasm.exe from sdk
$AximpPath = ((Get-ChildItem -Path "$ProgramFiles_Path\Microsoft SDKs\Windows" -Recurse -Filter "aximp.exe" -File) | Sort-Object CreationTime -Descending | Select-Object -First 1).FullName
$IldasmPath = ((Get-ChildItem -Path "$ProgramFiles_Path\Microsoft SDKs\Windows" -Recurse -Filter "ildasm.exe" -File) | Sort-Object CreationTime | Select-Object -First 1).FullName
$IlasmPath = ((Get-ChildItem -Path "$($Env:windir)\Microsoft.NET\Framework\" -Recurse -Filter "ilasm.exe" -File) | Sort-Object CreationTime | Select-Object -First 1).FullName

if([String]::IsNullOrEmpty($AximpPath) -or [String]::IsNullOrEmpty($IldasmPath) -or [String]::IsNullOrEmpty($IlasmPath))
{
    Write-Host "Could not find sdk tools:`naximp.exe`t$AximpPath`nildasm.exe`t$IldasmPath`nilasm.exe`t$IlasmPath"

    return
}

# Change location to output folder...
Write-Host "Change location to: $OutPath"
Set-Location -Path $OutPath 

# Create MSTSCLib.dll and AxMSTSCLib.dll
Write-Host "Build MSTSCLib.dll and AxMSTSCLib.dll..."
Write-Host "Using aximp.exe from: $AximpPath"
Start-Process -FilePath $AximpPath -ArgumentList "$($Env:windir)\system32\mstscax.dll" -Wait -NoNewWindow

# Modify MSTSCLib.ddl to fix an issue with SendKeys (See: https://social.msdn.microsoft.com/Forums/windowsdesktop/en-US/9095625c-4361-4e0b-bfcf-be15550b60a8/imsrdpclientnonscriptablesendkeys?forum=windowsgeneraldevelopmentissues&prof=required)
Write-Host "Modify MSTSCLib.dll..."
Write-Host "Using ildasm.exe from: $IldasmPath"

$MSTSCLibDLLPath = "$OutPath\MSTSCLib.dll"
$MSTSCLibILPath = "$OutPath\MSTSCLib.il"

Start-Process -FilePath $IldasmPath -ArgumentList "$MSTSCLibDLLPath /out=$MSTSCLibILPath" -Wait -NoNewWindow

Write-Host "Replace ""[in] bool& pbArrayKeyUp"" with ""[in] bool[] marshal([+0]) pbArrayKeyUp"""
(Get-Content -Path $MSTSCLibILPath).Replace("[in] bool& pbArrayKeyUp", "[in] bool[] marshal([+0]) pbArrayKeyUp") | Set-Content -Path $MSTSCLibILPath

Write-Host "Replace ""[in] int32& plKeyData"" with ""[in] int32[] marshal([+0]) plKeyData"""
(Get-Content -Path $MSTSCLibILPath).Replace("[in] int32& plKeyData", "[in] int32[] marshal([+0]) plKeyData") | Set-Content -Path $MSTSCLibILPath

Start-Process -FilePath $IlasmPath -ArgumentList "/dll $MSTSCLibILPath /output:$MSTSCLibDllPath" -Wait -NoNewWindow

Remove-Item -Path "$MSTSCLibILPath"
Remove-Item -Path "$OutPath\MSTSCLib.res"