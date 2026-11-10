# Creates the Lucide icons of the application (Source/NETworkManager.Controls/LucideIcons.cs) from the SVG files of the
# npm package lucide-static and copies its license (ISC, incl. the MIT license of the icons derived from Feather) to
# Source/NETworkManager.Documentation/Licenses/Lucide.txt.
#
# Only the icons in $Icons (and the custom icons) are included. To add an icon: add its name (https://lucide.dev/icons)
# to $Icons, run the script and use it as {controls:Lucide Kind=<Name>} or LucideIconKind.<Name>.
#
# The elements of an icon (path, circle, rect, line, polyline, polygon, ellipse) are converted to one path geometry
# that is drawn with a pen (24 x 24 units, round caps and joins, see LucideIcon). Elements with fill="currentColor"
# (e.g. the dot of key-round) are also filled.

param(
    # Version of lucide-static (https://www.npmjs.com/package/lucide-static)
    [string]$Version = "1.71.0"
)

$ErrorActionPreference = "Stop"

$RepositoryRoot = Split-Path $PSScriptRoot -Parent
$OutFilePath = Join-Path -Path $RepositoryRoot -ChildPath "Source/NETworkManager.Controls/LucideIcons.cs"
$LicenseFilePath = Join-Path -Path $RepositoryRoot -ChildPath "Source/NETworkManager.Documentation/Licenses/Lucide.txt"

# Icons of Lucide used in the application
$Icons = @(
    "activity", "app-window", "arrow-big-up", "arrow-down", "arrow-left", "arrow-left-right", "arrow-up", "binary",
    "book-open", "box", "brick-wall-fire", "bug", "cable", "calculator", "check", "check-check", "chevron-down",
    "chevron-left", "chevron-right", "chevron-up", "chevrons-right", "circle-alert", "circle-arrow-left",
    "circle-arrow-right", "circle-check", "circle-question-mark", "clipboard-paste", "clock-check", "cloud-download",
    "cloud-upload", "copy", "copy-plus", "database-search", "download", "ellipsis-vertical", "ethernet-port",
    "external-link", "eye", "eye-off", "file", "file-output", "file-pen-line", "file-search", "files", "folder-input",
    "folder-open", "folder-search", "frown", "funnel", "funnel-x", "gauge", "globe", "grip-horizontal", "grip-vertical",
    "heart", "history", "id-card", "info", "key-round", "keyboard", "languages", "layers", "layout-dashboard", "list",
    "list-chevrons-down-up", "list-chevrons-up-down", "list-filter", "list-plus", "list-x", "lock", "map-pin",
    "maximize", "maximize-2", "minimize", "minus", "monitor", "monitor-down", "network", "octagon-alert", "palette",
    "pause", "pencil", "pin", "play", "plug-zap", "plus", "power", "radar", "radio-tower", "refresh-cw", "rocket",
    "rotate-ccw", "route", "router", "save", "scan", "scan-line", "scissors", "search", "server", "settings", "shield",
    "shield-lock", "shield-off", "sliders-horizontal", "square-chevron-right", "table", "tag", "terminal", "timer",
    "toggle-left", "toggle-right", "triangle-alert", "undo-2", "unplug", "user", "user-lock", "users", "wifi",
    "wifi-high", "wifi-lock", "wifi-low", "wifi-off", "wifi-zero", "x"
)

# Lock of the icon wifi-lock (bottom right), for the Wi-Fi signal levels with lock
$WifiLock = '<path d="M20 16v-2a2 2 0 0 0-4 0v2" /><rect x="14" y="16" width="8" height="5" rx="1" />'

# Custom icons: SVG elements (24 x 24 like Lucide), optionally scaled and moved (Scale, then Offset)
$CustomIcons = [ordered]@{
    # Wi-Fi signal levels with lock, from the parts of wifi-lock
    "wifi-zero-lock" = @{ Svg = '<path d="M12 20h.01" />' + $WifiLock }
    "wifi-low-lock"  = @{ Svg = '<path d="M8.5 16.429a5 5 0 0 1 1.794-1.13" />' + $WifiLock }
    "wifi-high-lock" = @{ Svg = '<path d="M5 12.859a10 10 0 0 1 8.436-2.756" /><path d="M8.5 16.429a5 5 0 0 1 1.794-1.13" />' + $WifiLock }

    # Brand logos (Lucide has no brand icons) from Simple Icons (https://simpleicons.org, CC0 1.0), filled. Scaled from
    # 24 to 20 units to get the padding of the Lucide icons.
    "github"         = @{
        Svg    = '<path fill="currentColor" stroke="none" d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12" />'
        Scale  = 20 / 24
        Offset = 2
    }
    "x-logo"         = @{
        Svg    = '<path fill="currentColor" stroke="none" d="M14.234 10.162 22.977 0h-2.072l-7.591 8.824L7.251 0H.258l9.168 13.343L.258 24H2.33l8.016-9.318L16.749 24h6.993zm-2.837 3.299-.929-1.329L3.076 1.56h3.182l5.965 8.532.929 1.329 7.754 11.09h-3.182z" />'
        Scale  = 20 / 24
        Offset = 2
    }
}

$Culture = [System.Globalization.CultureInfo]::InvariantCulture

function Format-Number([double]$Value) {
    $Value.ToString("0.####", $Culture)
}

# Number of parameters per path command and which of them are x / y coordinates (moved by the offset if absolute) or
# lengths (only scaled); arc flags and the rotation are kept.
$PathCommands = @{
    "M" = "xy"; "L" = "xy"; "T" = "xy"; "H" = "x"; "V" = "y"; "C" = "xyxyxy"; "S" = "xyxy"; "Q" = "xyxy";
    "A" = "llrffxy"; "Z" = ""
}

# Normalizes the path data of an SVG path (numbers separated by spaces, arc flags without separator like "0120 0"
# are split) and applies the scale / offset. The path mini-language of WPF supports the same commands.
function Convert-PathData([string]$Data, [double]$Scale = 1, [double]$Offset = 0) {
    $Result = [System.Text.StringBuilder]::new()
    $Pos = 0
    $Command = $null
    $IsFirst = $true

    while ($true) {
        while ($Pos -lt $Data.Length -and ([char]::IsWhiteSpace($Data[$Pos]) -or $Data[$Pos] -eq ",")) {
            $Pos++
        }

        if ($Pos -ge $Data.Length) {
            break
        }

        if ([char]::IsLetter($Data[$Pos])) {
            $Command = [string]$Data[$Pos]
            $Pos++

            if (-not $PathCommands.ContainsKey($Command.ToUpperInvariant())) {
                throw "Unknown path command '$Command' in '$Data'."
            }
        }
        elseif ($null -eq $Command) {
            throw "Path data doesn't start with a command: '$Data'."
        }

        # A relative moveto at the start of a path is absolute (the paths of an icon are joined to one path)
        $Implicit = $null

        if ($IsFirst -and $Command -ceq "m") {
            $Command = "M"
            $Implicit = "l"
        }

        $IsFirst = $false

        $Upper = $Command.ToUpperInvariant()
        $IsRelative = $Command -cne $Upper
        $Roles = $PathCommands[$Upper]

        [void]$Result.Append($Command)

        foreach ($Role in $Roles.ToCharArray()) {
            while ($Pos -lt $Data.Length -and ([char]::IsWhiteSpace($Data[$Pos]) -or $Data[$Pos] -eq ",")) {
                $Pos++
            }

            if ($Role -eq "f") {
                # Arc flag: a single 0 or 1 (may be followed by the next number without separator)
                $Value = [double]::Parse([string]$Data[$Pos], $Culture)
                $Pos++
            }
            else {
                $Match = [regex]::Match($Data.Substring($Pos), "^[+-]?(\d+\.?\d*|\.\d+)([eE][+-]?\d+)?")

                if (-not $Match.Success) {
                    throw "Invalid number at position $Pos in '$Data'."
                }

                $Value = [double]::Parse($Match.Value, $Culture)
                $Pos += $Match.Length

                if ($Role -in "x", "y", "l") {
                    $Value *= $Scale
                }

                if ($Role -in "x", "y" -and -not $IsRelative) {
                    $Value += $Offset
                }
            }

            [void]$Result.Append((Format-Number $Value)).Append(" ")
        }

        if ($Roles.Length -eq 0) {
            [void]$Result.Append(" ")
        }

        # Implicit commands after M / m are lines, Z has no parameters
        if ($Upper -eq "M") {
            $Command = if ($Implicit) { $Implicit } elseif ($IsRelative) { "l" } else { "L" }
        }
        elseif ($Upper -eq "Z") {
            $Command = $null
        }
    }

    $Result.ToString().Trim()
}

# Converts one SVG element to path data (in SVG syntax, normalized by Convert-PathData)
function Convert-Element([System.Xml.XmlElement]$Element) {
    $Get = { param($Name, $Default = 0) $Attribute = $Element.GetAttribute($Name); if ($Attribute) { [double]::Parse($Attribute, $Culture) } else { $Default } }
    $N = { param($Value) Format-Number $Value }

    if ($Element.HasAttribute("transform")) {
        throw "Transforms are not supported: $($Element.OuterXml)"
    }

    switch ($Element.LocalName) {
        "path" {
            $Element.GetAttribute("d")
        }
        "line" {
            "M$(& $N (& $Get x1)) $(& $N (& $Get y1))L$(& $N (& $Get x2)) $(& $N (& $Get y2))"
        }
        { $_ -in "polyline", "polygon" } {
            $Points = $Element.GetAttribute("points").Trim() -split "[\s,]+"
            $Data = "M$($Points[0]) $($Points[1])"

            for ($i = 2; $i -lt $Points.Length; $i += 2) {
                $Data += "L$($Points[$i]) $($Points[$i + 1])"
            }

            if ($_ -eq "polygon") { $Data + "Z" } else { $Data }
        }
        { $_ -in "circle", "ellipse" } {
            $Cx = & $Get cx; $Cy = & $Get cy

            if ($_ -eq "circle") { $Rx = & $Get r; $Ry = $Rx } else { $Rx = & $Get rx; $Ry = & $Get ry }

            "M$(& $N ($Cx - $Rx)) $(& $N $Cy)A$(& $N $Rx) $(& $N $Ry) 0 1 0 $(& $N ($Cx + $Rx)) $(& $N $Cy)" +
            "A$(& $N $Rx) $(& $N $Ry) 0 1 0 $(& $N ($Cx - $Rx)) $(& $N $Cy)Z"
        }
        "rect" {
            $X = & $Get x; $Y = & $Get y; $W = & $Get width; $H = & $Get height
            $Rx = & $Get rx -1; $Ry = & $Get ry -1

            if ($Rx -lt 0) { $Rx = if ($Ry -lt 0) { 0 } else { $Ry } }
            if ($Ry -lt 0) { $Ry = $Rx }

            $Rx = [Math]::Min($Rx, $W / 2); $Ry = [Math]::Min($Ry, $H / 2)

            if ($Rx -eq 0 -or $Ry -eq 0) {
                "M$(& $N $X) $(& $N $Y)H$(& $N ($X + $W))V$(& $N ($Y + $H))H$(& $N $X)Z"
            }
            else {
                $Arc = "A$(& $N $Rx) $(& $N $Ry) 0 0 1"

                "M$(& $N ($X + $Rx)) $(& $N $Y)H$(& $N ($X + $W - $Rx))$Arc $(& $N ($X + $W)) $(& $N ($Y + $Ry))" +
                "V$(& $N ($Y + $H - $Ry))$Arc $(& $N ($X + $W - $Rx)) $(& $N ($Y + $H))" +
                "H$(& $N ($X + $Rx))$Arc $(& $N $X) $(& $N ($Y + $H - $Ry))" +
                "V$(& $N ($Y + $Ry))$Arc $(& $N ($X + $Rx)) $(& $N $Y)Z"
            }
        }
        default {
            throw "Unsupported element <$($Element.LocalName)>: $($Element.OuterXml)"
        }
    }
}

# Converts the SVG elements of an icon to the stroke and fill geometry (WPF path markup, "F1" = fill rule non-zero)
function Convert-Icon([string]$Svg, [double]$Scale = 1, [double]$Offset = 0) {
    $Xml = [xml]"<svg xmlns=`"http://www.w3.org/2000/svg`">$Svg</svg>"
    $Stroke = @()
    $Fill = @()

    foreach ($Element in $Xml.DocumentElement.ChildNodes) {
        if ($Element -isnot [System.Xml.XmlElement]) {
            continue
        }

        $Data = Convert-PathData -Data (Convert-Element $Element) -Scale $Scale -Offset $Offset

        if ($Element.GetAttribute("stroke") -ne "none") {
            $Stroke += $Data
        }

        if ($Element.GetAttribute("fill") -eq "currentColor") {
            $Fill += $Data
        }
    }

    [pscustomobject]@{
        Stroke = if ($Stroke) { "F1 " + ($Stroke -join " ") } else { $null }
        Fill   = if ($Fill) { "F1 " + ($Fill -join " ") } else { $null }
    }
}

function ConvertTo-PascalCase([string]$Name) {
    (($Name -split "-") | ForEach-Object { $_.Substring(0, 1).ToUpperInvariant() + $_.Substring(1) }) -join ""
}

# Download and extract the package
$TempPath = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "lucide-static-$Version-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $TempPath | Out-Null

try {
    $PackagePath = Join-Path -Path $TempPath -ChildPath "lucide-static.tgz"
    Invoke-WebRequest -Uri "https://registry.npmjs.org/lucide-static/-/lucide-static-$Version.tgz" -OutFile $PackagePath
    tar -xzf $PackagePath -C $TempPath

    if ($LASTEXITCODE -ne 0) {
        throw "Could not extract $PackagePath."
    }

    $IconsPath = Join-Path -Path $TempPath -ChildPath "package/icons"

    $Converted = [ordered]@{}

    foreach ($Icon in $Icons) {
        $SvgPath = Join-Path -Path $IconsPath -ChildPath "$Icon.svg"

        if (-not (Test-Path $SvgPath)) {
            throw "Icon '$Icon' not found in lucide-static $Version."
        }

        $Svg = [xml](Get-Content -Path $SvgPath -Raw)
        $Converted[$Icon] = Convert-Icon -Svg $Svg.DocumentElement.InnerXml.Replace(' xmlns="http://www.w3.org/2000/svg"', "")
    }

    foreach ($Icon in $CustomIcons.Keys) {
        $Custom = $CustomIcons[$Icon]
        $Scale = if ($Custom.Scale) { $Custom.Scale } else { 1 }
        $Offset = if ($Custom.Offset) { $Custom.Offset } else { 0 }

        $Converted[$Icon] = Convert-Icon -Svg $Custom.Svg -Scale $Scale -Offset $Offset
    }

    # License of Lucide (ISC, incl. the MIT license of the icons derived from Feather)
    $License = Get-Content -Path (Join-Path -Path $TempPath -ChildPath "package/LICENSE") -Raw
    Set-Content -Path $LicenseFilePath -Value (($License.TrimEnd() -replace "`r?`n", "`r`n") + "`r`n") -Encoding utf8NoBOM -NoNewline

    $Names = $Converted.Keys | Sort-Object { ConvertTo-PascalCase $_ }

    $Kinds = $Names | ForEach-Object { "    $(ConvertTo-PascalCase $_)," }
    $Data = $Names | ForEach-Object {
        $Value = $Converted[$_]
        $Stroke = if ($Value.Stroke) { "`"$($Value.Stroke)`"" } else { "null" }
        $Fill = if ($Value.Fill) { "`"$($Value.Fill)`"" } else { "null" }

        "        [LucideIconKind.$(ConvertTo-PascalCase $_)] = ($Stroke, $Fill),"
    }

    $LicenseComment = ($License.TrimEnd() -split "`r?`n" | ForEach-Object { "// $_".TrimEnd() }) -join "`n"

    $Content = @"
// <auto-generated>
// Generated $(Get-Date -Format "yyyy-MM-dd") by Scripts/Create-LucideIcons.ps1 from lucide-static $Version
// (https://lucide.dev). Don't edit this file, add the icon to the script and run it again.
//
// Custom icons: wifi-*-lock from the parts of wifi-lock (Lucide), github and x-logo from Simple Icons
// (https://simpleicons.org, CC0 1.0).
// </auto-generated>
//
$LicenseComment

using System.Collections.Generic;

namespace NETworkManager.Controls;

/// <summary>
///     Lucide icons of the application (see <see cref="LucideIcon" />).
/// </summary>
public enum LucideIconKind
{
    None,
$($Kinds -join "`n")
}

/// <summary>
///     Geometries of the Lucide icons (24 x 24 units): drawn with a pen, and filled (e.g. dots, logos).
/// </summary>
internal static class LucideIconData
{
    public static readonly IReadOnlyDictionary<LucideIconKind, (string Stroke, string Fill)> Icons =
        new Dictionary<LucideIconKind, (string Stroke, string Fill)>
        {
$($Data -join "`n")
        };
}
"@

    # LF -> CRLF like the other C# files
    Set-Content -Path $OutFilePath -Value ($Content -replace "`r?`n", "`r`n") -Encoding utf8NoBOM -NoNewline

    Write-Host "Created $OutFilePath with $($Converted.Count) icons."
}
finally {
    Remove-Item -Path $TempPath -Recurse -Force
}
