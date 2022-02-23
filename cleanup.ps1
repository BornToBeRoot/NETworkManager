foreach($path in Get-ChildItem -Path "$PSScriptRoot\Source\" -Directory)
{
    "bin","obj" | ForEach-Object {
        if(Test-Path -Path "$path\$_" -PathType Container) {
            Remove-Item -Path "$path\$_" -Recurse -Force
        }
    }
}