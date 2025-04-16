<#
    Convert 4x3 country flags from SVG to PNG using ImageMagick.

    Flags source: https://github.com/lipis/flag-icon-css
#>

$ConvertPath = "C:\Tools\ImageMagick-7.1.0-portable-Q16-x64\convert.exe"

$SourcePath = "C:\Temp\jp.svg"
$DestinationPath = "C:\Temp\ja-JP.png"

Start-Process -FilePath $ConvertPath -ArgumentList "-antialias -density 600 -background transparent -resize x48 ""$SourcePath"" ""$DestinationPath""" -NoNewWindow -Wait
