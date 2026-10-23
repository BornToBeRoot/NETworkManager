---
name: feature-review
description: In-depth review of one NETworkManager feature/tool (e.g. Port Scanner, Hosts File Editor, Firewall), one settings page, or one cross-cutting area (profiles, export, status window, dashboard). First builds an understanding of how the feature works and how it integrates (models, view models, views, settings, profiles, docs), then looks for bugs and logic errors, inconsistencies within the feature and against the rest of the application (UI, icons, spacing, context menus, load/refresh/scan logic, texts), performance problems (scan logic and UI responsiveness), and missing essential functions. Use this whenever the user wants to review, audit, check, or go through a NETworkManager tool, feature, settings page or area, or asks what is wrong/missing/inconsistent in a specific part of the app, even if they don't say "review". Not for reviewing a diff/PR (use code-review for that).
---

# NETworkManager feature review

Review **one** feature/tool, settings page or area of NETworkManager at a time, the way a developer
would who reads this code for the first time: understand it first, then judge it. The scope is the
feature as it is now (not a diff), so the review covers code that was written long ago as well.

The user names the scope (e.g. `/feature-review Port Scanner`, `/feature-review Settings > Profiles`).
If no scope is given, ask which one, and offer the list of review units from
[references/integration-map.md](references/integration-map.md).

The goal is a short, trustworthy list of findings the user can act on — not a long list of maybes.
The application deliberately keeps its code simple and similar across tools; fixes should follow the
existing patterns and not introduce new abstractions unless the same problem exists in several places.

## Workflow

### 1. Map the feature

Find every file that belongs to the feature. Don't stop at the obvious View/ViewModel — most bugs and
inconsistencies hide in the integration points (settings, profiles, group settings, export, command line,
run command, "open in other tool" navigation, documentation). [references/integration-map.md](references/integration-map.md) lists where a tool lives and
how to find all touch points (`git grep -l "<ApplicationName>" -- Source Website` is a good start).

### 2. Understand how it works

Read the code in this order: model (business logic) → view model (state, commands, threading) → view
(XAML, code-behind) → integrations (settings, profiles, export, docs). Then write a short
**"How it works"** section for the report (5–15 bullets), covering:

- What the user can do with it (main actions, context menu, keyboard shortcuts, toolbar buttons)
- The data flow of the main action (e.g. scan: input → validation → model → events → UI update → result/export)
- State and lifecycle: running/refreshing/modifying flags, cancellation, what happens on tab switch,
  view hide/visible, app close
- Integrations: which settings it reads, which profile/group fields it uses, history lists, export formats,
  admin rights requirement, external dependencies (PowerShell, WebView2, external processes, APIs)
- Entry points: how the tool is opened with a target or profile (run command `Ctrl+Shift+P`, `NavigationManager.Open`
  from other tools, profile panel, command line) and which other tools it sends data to

This section matters on its own: it lets the user verify that the reviewer understood the feature, and
it is the basis for spotting what is missing. If something is unclear after reading the code, say so
instead of guessing.

### 3. Pick comparison features

Choose 2–3 features that are closest in shape (e.g. IP Scanner ↔ Port Scanner ↔ Ping Monitor for scanners;
Hosts File Editor ↔ Firewall ↔ Neighbor Table for "edit system state with admin rights"; Remote Desktop ↔
PuTTY ↔ PowerShell for tabbed remote connections). Consistency findings come from comparing against
these, not from personal taste. When the features differ, decide which one is the better pattern and
say which one should be aligned — sometimes it's the reviewed feature that is right.

[references/conventions.md](references/conventions.md) lists the patterns the application already follows
(commands and `CanExecute`, refresh/modify guards, admin rights, context menus, dialogs, tooltips, export,
status messages, layout). Read it before judging consistency.

### 4. Review

Go through these areas. Skip an area if it doesn't apply, but say so in one line in the report.

1. **Bugs and logic errors** — wrong results, unhandled exceptions, race conditions (async commands that can
   overlap, refresh vs. modify), cancellation that doesn't stop work, state flags that can get stuck
   (set without `try/finally`), stale data after an action, edge cases (empty input, IPv6, very large
   ranges, invalid/duplicate/overlapping entries, no network adapter, no admin rights), wrong disposal / event handlers
   that are never unsubscribed (static events!), status messages overwritten by a later event (e.g. a completion
   event after cancel or after an input error), exceptions in fire-and-forget `async Task` methods (`_ = Start()`)
   that leave flags set.
2. **Consistency within the feature** — the same action behaving differently depending on where it is
   triggered (button vs. context menu vs. keyboard shortcut vs. profile vs. run command / other tool), single vs. multi selection,
   texts/terms that differ inside the feature.
3. **Consistency across the application** — compare with the features from step 3 and the conventions:
   layout and spacing, icons, button placement, header/status texts, context menu items and order,
   load/refresh/scan/apply logic, confirmation dialogs, export, history, settings structure, tooltips,
   keyboard shortcuts, documentation structure.
4. **Performance** — in the logic (concurrency limits, blocking calls, per-item work that could be batched,
   allocations in hot paths, repeated PowerShell/process calls) *and* in the UI (UI thread blocked, too many
   `Dispatcher.Invoke` calls per result, large collections updated item by item, `CanExecute` doing
   expensive work, missing virtualization in large grids, timers that keep running when the view is hidden).
   The UI has to stay smooth while a scan runs. Estimate the worst-case duration with the default settings for a
   typical input (e.g. an empty /24): rounds per item (items ÷ concurrency) × timeout, for every probe — the slowest
   probe decides when a result and the progress appear. Also check synchronous work on the UI thread after the action
   (export, sorting, large `Clear`/`Add` loops).
5. **Missing essentials** — functions a user would expect for this kind of tool, especially ones that
   similar tools in the app already have (export, copy, history, cancel, profile support, search/filter,
   multi-select actions, refresh, keyboard shortcuts). Separate small gaps (fix directly) from larger
   features (write an issue draft, see below).
6. **Robustness and security** — input passed to PowerShell/command lines/processes without escaping (also values
   received from the network, e.g. hostnames in custom commands),
   credentials in logs or plain text, unvalidated input, behaviour without admin rights, behaviour when
   an external dependency (PuTTY, WebView2, API) is missing or fails.
7. **Localization and texts** — hard-coded user-visible strings, strings used for multiple meanings,
   placeholders (`{0}`) that translations may break, missing `...` on items that open a dialog.
8. **Documentation and changelog** — does `Website/docs/application/<feature>.md` (and settings/profile docs)
   match the actual behaviour, options, defaults, context menu and shortcuts?

### 5. Verify every finding

Before a finding goes into the report, confirm it in the code: read the exact lines, follow the call chain,
and check whether another part of the code already prevents the problem (e.g. a `CanExecute` guard, a
lock, a validator). Write down the concrete scenario (input/state → wrong result). Drop findings you
can't confirm, or list them separately as "unverified" with what would be needed to confirm them.

The application only builds and runs on Windows. If you can't build or run it, the review is static —
say so in the report, and mark findings that need a manual check on Windows.

### 6. Report

Use this structure (keep it compact; tables for findings):

```markdown
# Review: <Feature>

## How it works
<5–15 bullets from step 2, incl. files: model, view model, view, settings, profiles, docs>

Compared with: <features from step 3>

## Findings

### Bugs and logic
| # | Finding | Scenario | Proposed fix |
|---|---|---|---|

### Consistency (feature / application)
| # | Finding | Compared with | Proposed fix |

### Performance
| # | Finding | Impact | Proposed fix |

### Missing essentials
| # | Gap | Why it's essential | Size (small fix / issue) |

### Other (security, localization, docs)
| # | Finding | Proposed fix |

## Not reviewed / unverified
<areas skipped, findings that need a Windows test>

## Suggested order
<what to fix first and what to group into one PR>
```

Reference code as `path:line`. Rate each finding (High / Medium / Low) in the finding text when it helps
prioritize. Proposed fixes describe the smallest change that follows the existing patterns.

For larger missing features, add an issue draft at the end that follows the repository's feature request
template (`.github/ISSUE_TEMPLATE/Feature_request.md`): a short title, "Describe your feature request",
the motivation, a proposed behaviour (UI + logic), and which existing feature it could be modelled on.

### 7. After the report

Don't change code during the review. Ask the user which findings to fix.

When the user wants the work handed over (to another agent or for later), write the result to
`.claude/reviews/<feature>.md` (kebab-case, e.g. `ip-scanner.md`; the folder is not committed). Write it as an
implementation plan, not as a report: verified findings with `path:line`, the concrete fix, the work packages / PR
order, decisions taken and still open, and what must be tested on Windows. Read existing files in `.claude/reviews/`
and `.claude/plans/` for the area first — they record earlier decisions.

When fixing:

- Follow the conventions and keep the change minimal; extract shared helpers only when the same code
  already exists in two or more places.
- Add strings to `Strings.resx` and, in alphabetical order, to `Strings.Designer.cs`.
- Update the feature documentation and add a changelog entry in `Website/docs/changelog/next-release.md`.
- Never modify `Source/3rdparty/`.
