foreach($path in Get-ChildItem -Path "$PSScriptRoot\Source\" -Directory)
{
    "bin","obj" | ForEach-Object {        
        Remove-Item "$path\$_" -Recurse -Force
    }
}