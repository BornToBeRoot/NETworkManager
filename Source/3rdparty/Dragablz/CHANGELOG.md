# Changelog

All changes compared to the original [ButchersBoy/Dragablz](https://github.com/ButchersBoy/Dragablz) (`e493913`,
2022-09-09). Newest first. Add an entry for every change made in this folder.

## NETworkManager (`Source/3rdparty/Dragablz`)

- `Themes/MahApps.xaml` reduced to the styles NETworkManager uses: `MahAppsFocusVisual`, `MahAppsInvisibleThumbStyle`
  and `MahAppsTabablzControlStyle` (without `ItemContainerStyle` and the trapezoid offset
  `AdjacentHeaderItemOffset=-12`, NETworkManager sets its own). Removed the converters,
  button, trapezoid tab and tool window styles.
- Removed unused code: `HeaderedItemViewModel`, `TabablzItemStyleSelector`,
  `HorizontalPositionMonitor` / `VerticalPositionMonitor`, `Core/HitTest.cs` and the empty project template files
  `Properties/Resources.*` / `Properties/Settings.*`.
- Removed the custom window `DragablzWindow` (NETworkManager uses MahApps windows) with `DragablzIcon`, the internal
  `SystemCommand` / `WindowMessage` enums, the window-only natives in `Core/Native.cs`
  (`SendMessage`, `PostMessage`, `SetWindowPlacement`, `WINDOWPLACEMENT`, `RECT`, `GetRawCursorPos`) and their
  styles in `Generic.xaml` (`DragablzIcon`, `WindowTemplateKey`, `DragablzWindow`, `CloseMenuCommandButtonStyle`).
- Removed the MaterialDesign theme (`Themes/MaterialDesign.xaml`, `MaterialDesignAssist`,
  `BrushToRadialGradientBrushConverter`), the `Ripple` control (`Ripple`, `RippleAssist`, its style in `Generic.xaml`)
  used only by that theme, and the empty `Themes/Dockablz.xaml`.
- Copied the library from the fork (`5415e71`) into the repository (replaces the git submodule). Not included: demos
  (`DragablzDemo`, `DragablzModernUIDemo`), tests (`Dragablz.Test`), solution, NuGet package (`Dragablz.nuspec`),
  `Resources/` (logos). The project is flattened (`Dragablz.csproj` in this folder), the NuGet metadata, the output path
  `..\bin` and `InternalsVisibleTo("Dragablz.Test")` are removed.

## Fork ([BornToBeRoot/Dragablz](https://github.com/BornToBeRoot/Dragablz), 2022-11 to 2026-11)

- Merge hint and larger merge area for tabs dragged as a window (opt-in, defaults unchanged):
  - `MergeTargetArea` (`HeaderItems` (default) / `HeaderPanel`): `HeaderPanel` also accepts a tab over the empty area of
    the tab bar (optional template part `PART_HeaderPanel`, renamed from `HeaderContainerGrid`).
  - `ShowMergeHint`, `MergeHintProximity` (default 48 DIPs) and `MergeHintTemplate` show a hint in the tab bar of every
    tab control the dragged tab can be merged into; read-only `IsMergeTargetParticipating` / `IsMergeTargetOffered`
    for custom templates.
  - Vertical tab strips with fixed headers are respected when searching a merge target; a tab dropped behind the last
    tab is inserted after it.
- Fix `NullReferenceException` when a tab drag starts while `InterTabController.Partition` is `null`
  (`TabablzControl.ItemDragStarted`).
- Fix `NullReferenceException` when dragging a maximized `DragablzWindow` (`WindowInteropHelper` instead of reading a
  private `Window` property via reflection).
- Fix MahApps theme (`MahApps.xaml`): `MahApps.Brushes.*` / `MahApps.Colors.*` resource keys of MahApps.Metro 2.x
  (`MahApps.Colors.Accent2`, `MahApps.Brushes.IdealForeground` instead of `WhiteBrush`). Most of these styles were
  removed later (see the `MahApps.xaml` reduction above).
- Wider hit area for the grid splitters between layout branches (`Generic.xaml`).
- `DisableBranchConsolidation` (`TabablzControl`): don't consolidate a branch when its tab control is emptied (e.g. to
  keep the main tab control that creates new tabs).
- Fix closing a tab in a layout branch with `TabEmptiedResponse.DoNothing` (`TabEmptiedResponse`, `TabablzControl`).
- `InterTabController.Partition` and `Layout.Partition` are dependency properties (bindable), from
  [Dragablz#275](https://github.com/ButchersBoy/Dragablz/pull/275).
- Fix [right click and drag "locks" the tab item](https://github.com/ButchersBoy/Dragablz/issues/132)
  (`DragablzItem`), from [Dragablz#275](https://github.com/ButchersBoy/Dragablz/pull/275).
- .NET 6 → 8 → 10: SDK-style project, target framework `net10.0-windows` (no Windows SDK version, no
  `RuntimeIdentifier` / `PlatformTarget`, so it builds for x64 and arm64).
