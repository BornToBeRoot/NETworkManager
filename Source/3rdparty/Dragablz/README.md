# Dragablz

Tearable tab control for WPF (tabs, drag out to a new window, merge, split layouts), used for the tabs of the
NETworkManager tools.

| | |
|---|---|
| Original | [ButchersBoy/Dragablz](https://github.com/ButchersBoy/Dragablz) by James Willock, last upstream commit `e493913` (2022-09-09) |
| Fork | [BornToBeRoot/Dragablz](https://github.com/BornToBeRoot/Dragablz), commit `5415e71` (2026-11-14) - was the git submodule `Source/3rdparty/Dragablz` until it was copied here |
| License | MIT, see [LICENSE](LICENSE) - keep this file and the copyright notice with the code |
| Changes | See [CHANGELOG.md](CHANGELOG.md) (all changes compared to the original) |

Only the library is included (no demos, tests or NuGet package) and it's stripped to what NETworkManager uses.
The project, assembly and namespaces are still called `Dragablz` / `Dragablz.Dockablz`, so the `xmlns` and pack URIs
(`pack://application:,,,/Dragablz;component/Themes/MahApps.xaml`) in the application stay unchanged.

## Maintenance

- This folder is the source of truth now; the fork is kept for history only.
- Change the code only when a feature or fix of NETworkManager needs it, keep the changes small and add an entry to
  [CHANGELOG.md](CHANGELOG.md).
- Don't reformat or restyle the code (`Source/.editorconfig` marks it as generated code, so analyzers and formatting
  rules ignore it).
- The file version in `Properties/AssemblyInfo.cs` is set to the NETworkManager version by AppVeyor (`appveyor.yml`),
  otherwise the MSI doesn't replace `Dragablz.dll` on an upgrade - keep the `AssemblyFileVersion` attribute.
- The license is also shipped with the application (`Source/NETworkManager.Documentation/Licenses/Dragablz.txt`, shown
  in the about / license view via `LibraryManager`).
