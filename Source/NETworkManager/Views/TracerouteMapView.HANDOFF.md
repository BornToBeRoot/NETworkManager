# Traceroute World Map — Implementation Handoff

Status as of 2026-07-10: feature-complete prototype, all previously-reported bugs
fixed and confirmed by the user. This file exists so another AI agent (or the
author, later) can resume work without re-deriving the reasoning below purely
from the diffs. **This file is not committed project documentation** — it's a
working note. Delete it (or fold anything still relevant into code comments)
once the feature has settled and this history stops being useful.

## What this feature is

An offline, abstract world map embedded in the Traceroute tool
(`TracerouteView`), below the hop `DataGrid`, behind a collapsible `Expander`.
It plots each resolved hop's geolocation as a marker on a vector world map
(country outlines + reference city dots), connects consecutive hops with
curved directional arrows, and supports pan/zoom. Hovering a marker or arrow
shows details (location, ISP/ASN, hostname, IP, average RTT) in a fixed
top-left info panel, and cross-highlights the adjacent arrow(s)/markers.

All map data is embedded, offline, no network calls at runtime — generated
once via a PowerShell script from a public dataset.

## Key files

| File | Role |
|---|---|
| `NETworkManager/Views/TracerouteMapView.xaml(.cs)` | The map control itself. Nearly all of the interesting logic lives here. |
| `NETworkManager/Views/TracerouteView.xaml(.cs)` | Hosts the map below the DataGrid inside a `GridSplitter`-resizable, `Expander`-collapsible row. |
| `NETworkManager/Resources/Maps/world-map.json` | Embedded country outline polygons (simplified Natural Earth 50m, outer rings only, 2-decimal rounding). |
| `NETworkManager/Resources/Maps/world-cities.json` | Embedded reference city dots (admin capitals + pop ≥ 500k, ~660 cities). |
| `Scripts/Create-WorldMapFromWeb.ps1` | Regenerates both JSON files from `martynafford/natural-earth-geojson`. Re-run this if you need more/fewer cities or higher-resolution country outlines. |
| `NETworkManager.Settings/GlobalStaticConfiguration.cs`, `SettingsInfo.cs` | `Traceroute_ExpandMapView` setting (default `true`). |
| `NETworkManager.Documentation/ResourceManager.cs`, `Strings.resx` | Attribution entry for the Natural Earth data (CC0-1.0). |
| `NETworkManager.Models/Network/Ping.cs` (read-only reference) | `TimeToString` — confirms both `IPStatus.Success` and `TtlExpired` count as a valid reply; informed `IsValidPingStatus` in the map code. |

## Architecture notes worth knowing before touching this code

- **Coordinate system**: everything in `RootCanvas`/`HopsCanvas`/etc. is in
  fixed *map units* (`MapWidth=1000 × MapHeight=500`, equirectangular
  projection via `Project(lat, lon)`). A `TransformGroup` (`ScaleTransform` +
  `TranslateTransform`) on `RootCanvas` handles pan/zoom — `_scale` is applied
  to the *whole* canvas, not per-element.
- **Screen-constant sizing pattern**: anything that should render at a fixed
  *screen* pixel size regardless of zoom (marker dots, city dots, label text,
  stroke thickness, arrowheads) multiplies its map-unit size by
  `inverseScale = 1 / _scale`, recomputed in `ApplyTransform()` every time
  `_scale` changes. Most elements use a **shared** `_inverseScaleTransform`
  (a `ScaleTransform` with `RenderTransformOrigin=(0.5,0.5)`) since they don't
  need a custom pivot. Arrows are the exception — see below.
- **`ApplyTransform()` skip-optimization**: panning alone (no scale change)
  skips the whole scale-dependent recompute block (`_lastScaleDependentUpdateScale`
  guard) for performance. Every `RedrawHops()` call resets that guard to `-1`
  first — otherwise brand-new elements created by a redraw that happens to
  land on the same `_scale` as before would never get positioned correctly.
- **`RedrawHops()` fits the view *before* drawing anything** (`FitToHops` runs
  first), so `_scale` is already correct for *this* route when markers/labels/
  arrows compute their initial screen-constant sizing. Get this ordering wrong
  and everything silently uses stale scale from the previous route.
- **WPF `Visibility.Collapsed` gotcha**: an element's `DesiredSize` from
  `Measure()` resets to `(0,0)` — including retroactively, when WPF's own
  layout system re-measures it later while `Collapsed`, even if it measured
  fine earlier while `Visible`. Fix pattern used throughout this file: cache
  the measured `Size` into `.Tag` right after the first `Measure()`, and never
  trust live `DesiredSize` again. This bit us hard once (see Bugs below) —
  don't reintroduce a `DesiredSize` read on a label that might be collapsed.
- **Shoelace centroid vs. bounding-box center**: simplified/rounded Natural
  Earth polygon rings can self-intersect slightly (esp. complex coastlines).
  A shoelace-formula centroid isn't guaranteed to stay inside a
  self-intersecting ring. `BuildCountryLabels` uses bounding-box center
  instead, which can't escape the ring's extent.
- **Hit-testing "donut hole"**: every visible shape (`Ellipse`, `Path`,
  `Polygon`) needs `IsHitTestVisible = false`, otherwise it shadows the wider
  invisible hit-target sibling drawn for easier hovering, and only the ring
  *around* the visible shape (not the shape itself) responds to hover.

## Arrow geometry — the part most recently rewritten, read this before touching `DrawArrow`

This was the site of the two bugs fixed most recently (2026-07-09/10) and is
the most subtle part of the file. Current design, post-fix:

- Each arrow is represented by a small `ArrowVisual` record
  (`FromPoint`/`ToPoint`/`Length`/`ControlPoint`/`StartAngle`/`EndAngle` +
  references to the mutable `PathFigure`/`QuadraticBezierSegment`/`Path`
  (visible)/`Path` (hit-test)/`Polygon` (arrowhead)/`ScaleTransform`
  (arrowhead scale+pivot)). Tracked in a single `_arrows` list (previously
  three parallel lists — collapsed into one when this was fixed).
- The curve's **shape** (control point, tangent angles) is scale-independent
  and computed once in `DrawArrow`.
- The curve's **trim** (pulling the visible start/end back from the marker's
  *center* to its *outer edge*) is **not** scale-independent — it depends on
  `inverseScale` (marker screen-radius converted to current map units) — so it
  is recomputed by `UpdateArrowGeometry(arrow, inverseScale)`, called both
  once at draw time *and* again from `ApplyTransform()` every time `_scale`
  changes. The `PathGeometry` is therefore **not** `Freeze()`d anymore (it was,
  originally — that's exactly what made the trim impossible to keep live).

### Bug #1 (fixed): frozen trim became a huge visible gap after zooming past auto-fit

Original code computed `markerRadius`/trim **once**, in `DrawArrow`, using
whatever `_scale` was current when the route was first drawn (the auto-fit
scale for the *whole* route). For a route spanning continents (e.g.
Germany → USA), that auto-fit scale is very zoomed-out, so the trim (in fixed
map units) is small. Once the user zoomed in manually on just one part of the
route (e.g. Germany), that *same* frozen map-unit trim now represented a much
larger number of *screen* pixels — arrows visibly stopped short of / started
away from their markers. This is what the user described as "long arrows no
longer start at the data points."

Fix: trim is no longer frozen — `UpdateArrowGeometry` re-derives it from the
*current* `inverseScale` every time `ApplyTransform` runs, exactly like stroke
thickness and label offsets already did.

### Bug #2 (fixed): fixed curve-offset floor distorted very short segments into loops

`ArrowCurveMinOffset = 3` (map units) is an absolute floor on how much the
curve bows to one side (so a segment and its reverse, e.g. Frankfurt → Cologne
→ Frankfurt, don't draw on top of each other). For two markers only ~0.1–0.2
map units apart (e.g. two nearby cities in the same metro area), that fixed
floor dwarfed the segment length and bowed it into a near-loop instead of a
slight curve.

Fix: `curveOffset` is now also capped at `length * 0.5`, so short segments get
a proportionally small bow regardless of the floor.

### If a new "arrow doesn't line up" bug shows up

1. First suspect whether something computed inside `DrawArrow` (i.e. once,
   at draw time) should actually live in `UpdateArrowGeometry` (recomputed
   per zoom) instead — that was the root cause both times.
2. `markerRadius` is capped at `length * 0.3` specifically so a short segment
   in a route that *also* contains a far-away hop doesn't get eaten alive by
   a trim sized for the far-away hop's zoomed-out scale. If you see a short
   segment distorted specifically in a route with one very long leg, check
   this cap first.
3. Diagnostic logging via `Log.Info` (log4net → `%LocalAppData%\NETworkManager\NETworkManager.log`)
   was the single most effective debugging technique in this whole session —
   more effective than reasoning from screenshots alone. Don't hesitate to
   temporarily add `Log.Info` calls, rebuild, have the user reproduce, then
   read the log directly, for anything where the on-screen symptom doesn't
   obviously map to one specific code path.

## Bugs fixed this session (chronological, for context / "don't reintroduce this")

- Chevron flush against Expander edge → reused `PingMonitorExpander` style.
- Map bleeding into the adjacent Profiles sidebar on narrow windows → root
  cause was a negative-margin hack meant to escape an outer `Margin="10"` on
  `TracerouteView`'s root Grid; fixed by removing that outer margin entirely
  and giving each content piece its own margin instead.
- All reference labels visible at full zoom-out ("wall of text") → default
  `Visibility.Collapsed` at build time, not just after the first
  `ApplyTransform`.
- Hop labels stuck at wrong position until manual zoom → `BorderHost.ActualWidth`
  was still 0 right after `Loaded` even with `UpdateLayout()` called explicitly
  (ancestor layout not settled yet) → deferred initial `RedrawHops()` via
  `Dispatcher.BeginInvoke(RedrawHops, DispatcherPriority.Loaded)`.
- **"UK is in Germany" / countries and cities badly mislabeled** → root cause
  was the `Visibility.Collapsed` + `Measure()` gotcha described above (WPF
  silently reset `DesiredSize` to `(0,0)` on a second internal re-measure).
  Fixed by caching measured `Size` in `.Tag` immediately after the first
  `Measure()`. (A real-but-insufficient contributing fix along the way:
  switched country label positioning from shoelace centroid to bounding-box
  center — kept, but wasn't the actual root cause.)
- `_lastScaleDependentUpdateScale` skip-optimization silently breaking
  brand-new hop labels when a redraw happened to land on the same `_scale` as
  before → reset the guard to `-1` at the top of every `RedrawHops()`.
- Frankfurt-area markers never actually stacking despite visibly overlapping
  → `SpreadOverlappingPoints` originally keyed on exact `Point` equality, but
  different IPs in the same city commonly resolve to slightly different
  lat/lon → re-keyed on a `City␟Country` string instead (case-insensitive,
  matches `IsSameLocation`'s semantics).
- Average RTT only ever showing for the last hop → `GetAverageRoundTripTime`
  checked only `IPStatus.Success`, but intermediate traceroute hops normally
  reply `TtlExpired` (only the true destination replies `Success`) → added
  `IsValidPingStatus` accepting both, matching `Ping.TimeToString`'s own
  convention.
- Tooltip popup stealing focus / undoing hover highlight → WPF `ToolTip`
  popups could trigger a spurious `MouseLeave` on the element underneath when
  opening. Replaced entirely with a fixed, semi-transparent `InfoPanel` in the
  top-left corner, shown/hidden directly from code-behind — not a partial
  workaround, the popup mechanism was removed outright.
- "Donut hole" hit-testing (small visible shape blocking the larger invisible
  hit-target beneath it) → added `IsHitTestVisible=false` to all visible
  shapes.
- Arrow trim/curve bugs → see the dedicated section above.

## Known remaining / not done

- Nothing currently outstanding from explicit user requests. The "make arrows
  look more modern" exploratory question was addressed with rounded dash
  caps (`StrokeStartLineCap`/`StrokeEndLineCap`/`StrokeDashCap = PenLineCap.Round`)
  — implemented and accepted, not left open.
- If picking this back up: check whether the `ArrowCurveMinOffset`/`ArrowCurveMaxOffset`/
  `ArrowCurveFraction` constants (top of `TracerouteMapView.xaml.cs`) still
  feel right now that the floor is also capped at `length * 0.5` — they were
  tuned before that cap existed and might warrant revisiting together.

## Environment quirks encountered (not code bugs, don't waste time chasing these as if they were)

- `dotnet build` does **not** regenerate `*.Designer.cs` from `.resx` (that's
  a Visual-Studio-only "Custom Tool" step) — new resx keys must be added to
  `Strings.Designer.cs` by hand too, or the CLI build fails with `CS0117`.
- Intermittent `CS2001` (missing generated `.g.cs`) / `CS0115`
  (`OnSourceInitialized` override) build failures that resolve on a bare
  retry with no code changes — `obj` cache flakiness in this environment, not
  a real error. Retry once before investigating further.
- File-lock build failures (`MSB3027`/`MSB3021`) are the previously-launched
  `NETworkManager.exe` test instance holding the DLL open. Standing fix:
  `taskkill //F //IM NETworkManager.exe` (ignore "process not found") before
  rebuilding.
- Build output for manual testing: `NETworkManager/bin/Debug/net10.0-windows10.0.22621.0/win-x64/NETworkManager.exe`.
