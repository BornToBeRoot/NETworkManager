param(
    [Parameter(
        Position=0,
        Mandatory=$true)]
    [String]$OutPath
)

# Test if files are already there...
if((Test-Path -Path "$OutPath\MSTSCLib.dll") -and (Test-Path -Path "$OutPath\AxMSTSCLib.dll"))
{
    Write-Host "MSTSCLib.dll and AxMSTSCLib.dll already created!"
    return
}

# x86 or x64
$ProgramFiles_Path = ${Env:ProgramFiles(x86)}

if([String]::IsNullOrEmpty($ProgramFiles_Path))
{
    $ProgramFiles_Path = $Env:ProgramFiles    
}

# Get aximp from sdk
$files = (Get-ChildItem -Path "$ProgramFiles_Path\Microsoft SDKs\Windows" -Recurse -Filter "aximp.exe" -File)

if($files.Count -eq 0)
{
    Write-Host "Aximp.exe not found on this system!"
    exit 2
}

# Change location
Write-Host "Change location to: $OutPath"
Set-Location -Path $OutPath 

# Create MSTSCLib.dll and AxMSTSCLib.dll
Write-Host "Creating MSTSCLib.dll and AxMSTSCLib.dll ..."
Start-Process -FilePath $files[$files.Length -1].FullName -ArgumentList "$($Env:windir)\system32\mstscax.dll" -Wait -NoNewWindow