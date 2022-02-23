$ConvertPath =  "C:\Tools\ImageMagick-7.1.0-portable-Q16-x64\convert.exe"

$sizes = "16","24","32","48","64","96","128","256","512"

foreach($size in $sizes)
{
    Start-Process -FilePath $ConvertPath -ArgumentList "-density 1200 -background transparent $PSScriptRoot\NETworkManager.svg -resize $($size)x$($size) $PSScriptRoot\NETworkManager_$($size)x$($size).png" -NoNewWindow -Wait
}

Start-Process -FilePath $ConvertPath -ArgumentList "-density 1200 -background transparent $PSScriptRoot\NETworkManager.svg -define icon:auto-resize $PSScriptRoot\NETworkManager.ico" -NoNewWindow -Wait