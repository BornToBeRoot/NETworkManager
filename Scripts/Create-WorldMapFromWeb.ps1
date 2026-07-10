# Filepath in the resources
[string]$OutFilePath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager\Resources\Maps\world-map.json"
[string]$CitiesOutFilePath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "Source\NETworkManager\Resources\Maps\world-cities.json"

# Resolution of the Natural Earth admin-0 countries / populated places dataset (110m, 50m or 10m - higher detail = larger file)
[string]$Resolution = "50m"

# A city is included if it's a national capital, or its population is at least this high
[int]$CityMinPopulation = 500000

# Download countries as plain GeoJSON (pre-converted from Natural Earth shapefiles, no TopoJSON decoding required)
$GeoJson = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/martynafford/natural-earth-geojson/master/$Resolution/cultural/ne_${Resolution}_admin_0_countries.json").Content | ConvertFrom-Json

# Rounds [lon, lat] pairs to 2 decimal places (~1.1 km), which is enough detail for an abstract, non-navigational map
function Get-RoundedRing {
    param($Ring)

    $Points = [System.Collections.Generic.List[object]]::new()

    foreach ($Point in $Ring) {
        $Points.Add(@([Math]::Round([double]$Point[0], 2), [Math]::Round([double]$Point[1], 2)))
    }

    # Unary comma prevents PowerShell from unrolling the list when there is only a single ring/point
    , $Points
}

# Keeps only the outer ring of every polygon part (holes like inland lakes are not relevant for an abstract map)
function Get-OuterRings {
    param($Geometry)

    $Rings = [System.Collections.Generic.List[object]]::new()

    switch ($Geometry.type) {
        "Polygon" {
            $Rings.Add((Get-RoundedRing -Ring $Geometry.coordinates[0]))
        }
        "MultiPolygon" {
            foreach ($Part in $Geometry.coordinates) {
                $Rings.Add((Get-RoundedRing -Ring $Part[0]))
            }
        }
    }

    , $Rings
}

$Countries = [System.Collections.Generic.List[object]]::new()

foreach ($Feature in $GeoJson.features) {
    $Countries.Add([PSCustomObject]@{
        n = $Feature.properties.ADMIN
        r = Get-OuterRings -Geometry $Feature.geometry
    })
}

ConvertTo-Json -InputObject $Countries -Depth 10 -Compress | Set-Content -Path $OutFilePath -Encoding utf8NoBOM

# Download populated places (cities) as plain GeoJSON
$PlacesGeoJson = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/martynafford/natural-earth-geojson/master/$Resolution/cultural/ne_${Resolution}_populated_places_simple.json").Content | ConvertFrom-Json

$Cities = [System.Collections.Generic.List[object]]::new()

foreach ($Feature in $PlacesGeoJson.features) {
    $Properties = $Feature.properties

    # Keep national capitals regardless of population, plus every other city above the threshold
    if ($Properties.adm0cap -ne 1 -and $Properties.pop_max -lt $CityMinPopulation) {
        continue
    }

    $Cities.Add([PSCustomObject]@{
        n   = $Properties.name
        lat = [Math]::Round([double]$Properties.latitude, 2)
        lon = [Math]::Round([double]$Properties.longitude, 2)
    })
}

ConvertTo-Json -InputObject $Cities -Depth 5 -Compress | Set-Content -Path $CitiesOutFilePath -Encoding utf8NoBOM