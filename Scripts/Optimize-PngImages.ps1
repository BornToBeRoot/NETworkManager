<#
    Reduce the size of PNG images (e.g. screenshots of the documentation) using ImageMagick.

    - Lossless by default: removes metadata and uses the best PNG compression.
    - Optional: reduce the colors (lossy, e.g. -Colors 256), much smaller for screenshots.
    - A file is only replaced if the result is smaller, and the colors are only reduced if the image has more colors,
      so running the script again doesn't change anything.

    ImageMagick 7: https://imagemagick.org/script/download.php#windows (the portable version is enough)

    Examples:
        .\Optimize-PngImages.ps1
        .\Optimize-PngImages.ps1 -Path ..\Website\docs\img\dashboard.png
        .\Optimize-PngImages.ps1 -Colors 256 -WhatIf
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    # Files or folders (with subfolders) to optimize
    [string[]]$Path = @(
        "$PSScriptRoot\..\Website\docs\img",
        "$PSScriptRoot\..\Website\static\img",
        "$PSScriptRoot\..\Website\blog"
    ),

    # Path to magick.exe of ImageMagick 7 (if not found, magick from the PATH is used)
    [string]$MagickPath = "C:\Tools\ImageMagick-7.1.0-portable-Q16-x64\magick.exe",

    # Maximum number of colors (lossy, e.g. 256). 0 = lossless
    [ValidateRange(0, 65536)]
    [int]$Colors = 0
)

if (-not (Test-Path -LiteralPath $MagickPath)) {
    $magickCommand = Get-Command -Name "magick" -ErrorAction SilentlyContinue

    if (-not $magickCommand) {
        throw "ImageMagick not found: $MagickPath - set -MagickPath to magick.exe."
    }

    $MagickPath = $magickCommand.Source
}

$files = foreach ($item in $Path) {
    Get-ChildItem -Path $item -Filter "*.png" -File -Recurse
}

$totalBefore = 0
$totalAfter = 0

foreach ($file in $files) {
    $tempFile = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "$([guid]::NewGuid()).png"

    $arguments = @($file.FullName, "-strip")

    # Reduce the colors without dithering (dithering adds noise and makes screenshots larger). Only if the image
    # has more colors - reducing an already reduced image again would merge colors and lose quality with every run.
    if ($Colors -gt 0) {
        $uniqueColors = [int](& $MagickPath identify -format "%k" $file.FullName)

        if ($uniqueColors -gt $Colors) {
            $arguments += @("+dither", "-colors", $Colors)
        }
    }

    # Best compression; no date / time chunks, so an unchanged image stays byte-identical
    $arguments += @(
        "-define", "png:compression-level=9",
        "-define", "png:compression-filter=5",
        "-define", "png:compression-strategy=1",
        "-define", "png:exclude-chunks=date,time",
        $tempFile
    )

    & $MagickPath @arguments

    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $tempFile)) {
        Write-Warning "Failed to optimize: $($file.FullName)"
        Remove-Item -LiteralPath $tempFile -ErrorAction SilentlyContinue -WhatIf:$false
        continue
    }

    $sizeBefore = $file.Length
    $sizeAfter = (Get-Item -LiteralPath $tempFile).Length

    $totalBefore += $sizeBefore

    if ($sizeAfter -ge $sizeBefore) {
        Remove-Item -LiteralPath $tempFile -WhatIf:$false
        $totalAfter += $sizeBefore
        continue
    }

    $totalAfter += $sizeAfter
    $result = "{0,8:N0} KB -> {1,8:N0} KB ({2:N1} %)" -f ($sizeBefore / 1KB), ($sizeAfter / 1KB), (100 - $sizeAfter * 100 / $sizeBefore)

    # -WhatIf: only show the result
    if ($PSCmdlet.ShouldProcess($file.FullName, "Replace: $result")) {
        Move-Item -LiteralPath $tempFile -Destination $file.FullName -Force
        Write-Host ("{0,-60} {1}" -f $file.Name, $result)
    }
    else {
        Remove-Item -LiteralPath $tempFile -WhatIf:$false
    }
}

if ($totalBefore -gt 0) {
    Write-Host ("`nTotal: {0:N0} KB -> {1:N0} KB ({2:N1} %)" -f ($totalBefore / 1KB), ($totalAfter / 1KB), (100 - $totalAfter * 100 / $totalBefore))
}
else {
    Write-Host "No PNG files found."
}
