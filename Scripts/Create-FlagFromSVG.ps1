<#
    Convert 4x3 country flags from SVG to PNG using ImageMagick.

    Flags source: https://github.com/lipis/flag-icon-css
#>

$ConvertPath = "C:\Tools\ImageMagick-7.1.0-portable-Q16-x64\convert.exe"

$SourcePath = "C:\Temp\ua.svg"
$DestinationPath = "C:\Temp\uk-UA.png"

Start-Process -FilePath $ConvertPath -ArgumentList "-antialias -density 600 -background transparent -resize x48 ""$SourcePath"" ""$DestinationPath""" -NoNewWindow -Wait
