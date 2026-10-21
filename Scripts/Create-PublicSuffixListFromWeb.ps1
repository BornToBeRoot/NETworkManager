# Filepath in the resources
[string]$OutFilePath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager.Models\Resources\PublicSuffixList.dat"

# Download the public suffix list (MPL 2.0) from publicsuffix.org
$List = (Invoke-WebRequest -Uri "https://publicsuffix.org/list/public_suffix_list.dat").Content -split "[`r`n]+"

# Keep the license notice, version and commit of the list
$Header = $List | Where-Object { $_ -match "^// (This Source Code Form|License, v\. 2\.0|file, You can obtain|VERSION:|COMMIT:)" }

# Only the ICANN section is used, since the private section (e.g. github.io) doesn't contain registered domains
$IsICANN = $false
$IdnMapping = New-Object System.Globalization.IdnMapping

$Rules = foreach ($Line in $List) {
    if ($Line -eq "// ===BEGIN ICANN DOMAINS===") {
        $IsICANN = $true
        continue
    }

    if ($Line -eq "// ===END ICANN DOMAINS===") {
        break
    }

    $Rule = $Line.Trim()

    if (-not $IsICANN -or [string]::IsNullOrEmpty($Rule) -or $Rule.StartsWith("//")) {
        continue
    }

    # Rules are converted to A-labels (punycode), like the queries. Wildcard (*.) and exception (!) prefixes are kept.
    $Prefix = if ($Rule.StartsWith("*.")) { "*." } elseif ($Rule.StartsWith("!")) { "!" } else { "" }

    $Prefix + $IdnMapping.GetAscii($Rule.Substring($Prefix.Length)).ToLowerInvariant()
}

$Description = @"
$($Header -join "`n")
//
// ICANN section of the public suffix list from https://publicsuffix.org/list/public_suffix_list.dat, with the
// rules converted to A-labels (punycode).
// Generated $(Get-Date) by Scripts/Create-PublicSuffixListFromWeb.ps1
"@

Set-Content -Path $OutFilePath -Value (@($Description) + $Rules) -Encoding utf8NoBOM
