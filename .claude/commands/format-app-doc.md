# Format Application Documentation

Reformat an application documentation page in `Website/docs/application/` to match the project's standard structure. Also fix consistency issues and improve prose throughout.

Target file: $ARGUMENTS

If no file path is given, ask the user which file to reformat.

## Standard page structure (top to bottom)

Apply this order strictly. Only include sections that have actual content.

### 1. Frontmatter
Keep `sidebar_position` and `keywords` unchanged. The `description` field must start with a verb in the present tense and describe the feature's purpose concisely (e.g. `"Scan for active devices..."`, `"Monitor host reachability..."`, `"View and manage ARP cache entries..."`). Fix it if it does not follow this pattern.

### 2. Intro paragraph
One concise paragraph describing what the feature does. Start with `The **Feature Name** ...` or `With **Feature Name** ...`. Fix grammar, passive voice, or vague phrasing, but do not add or remove facts.

### 3. Supporting content (optional)
Tables, bullet lists, or paragraphs that belong to the intro (e.g. supported input formats, example inputs). Keep these before any admonitions. Fix typos and prose quality within this content.

### 4. `:::info` block
**If present:** Improve readability if the prose is dense or run-on, but do not change facts or remove content. This block must contain only background information about the underlying technology (e.g. what ARP is, what the hosts file is) — never application-specific behavior.

**If absent:** Check whether the feature is built on a well-known technology or protocol (e.g. ICMP for Ping, DNS, RDP, ARP, SNMP, WoL). If so, add a `:::info` block with a concise factual explanation of that technology. Do not invent details — only include what is commonly known and accurate. If the feature is application-specific with no clear underlying technology to explain, omit the block.

### 5. `:::note` for prerequisite software (optional)
If the feature requires third-party software to be installed (e.g. PuTTY, TigerVNC), keep a single `:::note` for that here.

### 6. `:::warning[Administrator privileges required]` (only if admin rights are needed)
Use this exact admonition title. One or two sentences: state that the view is read-only without elevation, and how to restart as administrator. Do not mix other information into this block.

### 7. Screenshot
Keep the existing `![Feature Name](../img/...)` line unchanged.

### 8. `:::note` for non-obvious app behavior (optional)
Only for standalone application behavior facts that don't fit elsewhere (e.g. automatic backup behavior, Windows API limitations). One note max. Do not use for actions or admin rights.

### 9. `## Actions`
Replace all scattered `:::note` blocks that describe UI interactions with a single `## Actions` section. Include only the subsections that apply to this page. Omit subsections with no content.

**Important:** Context menus or keyboard shortcuts that appear **inside a `## Settings` or `## Profile` field** (e.g. right-clicking a settings list entry) must stay where they are — they are field-level constraints, not top-level actions. Do not move them here.

#### `### Toolbar`
Use when there are buttons in a toolbar (typically below or above the main list/table):

| Button | Description |
|--------|-------------|
| **Button label** | What it does |

#### `### Tab context menu`
Use when right-clicking on a **session tab header** opens a context menu (e.g. PuTTY, Remote Desktop, TigerVNC). Lead with one sentence explaining where the menu appears, then a table:

| Action | Description |
|--------|-------------|
| **Action label** | What it does |

#### `### Context menu`
Use when right-clicking on a **row or result** in the main view opens a context menu:

| Action | Description |
|--------|-------------|
| **Action label** | What it does |

If the feature has **multiple tabs** with different context menus or toolbars per tab, use named subsections instead:

##### `#### Tab: WiFi` / `#### Tab: Channels`
One subsection per tab, each containing its own Toolbar / Context menu / Keyboard shortcuts tables as needed.

If a column or tab header has its own separate context menu distinct from the row context menu, add a second table with a short lead-in sentence.

#### `### Keyboard shortcuts`

| Key | Action |
|-----|--------|
| `F5` | Refresh |
| `F2` | Edit selected entry |
| `Del` | Delete selected entry |

### 10. Remaining sections
Keep all `## Connect`, `## Add entry`, `## Profile`, `## Group`, `## Settings`, and similar sections. Preserve their structure, field definitions, and any `:::note` / `:::warning` blocks that describe field-level constraints or conditions (e.g. "Only available if ..."). Lightly improve field description prose for clarity.

## Consistency fixes to apply throughout

Apply these fixes to all prose, field descriptions, and admonitions:

### Protocol and technology capitalization
Always capitalize: `ICMP`, `TCP`, `UDP`, `DNS`, `ARP`, `SSH`, `RDP`, `SNMP`, `HTTP`, `HTTPS`, `SNTP`, `NDP`, `NetBIOS`, `IPv4`, `IPv6`. Never write them in lowercase (e.g. `icmp`, `tcp`, `dns`).

### Boolean defaults
Always use `Enabled` / `Disabled` for boolean field defaults — never `true` / `false`.

### Cross-links between tools
When a field description or note references another NETworkManager feature by name (e.g. "Use the Port Scanner for a detailed scan"), link it using the existing relative path pattern: `[Port Scanner](./port-scanner)`. Do not add links that are not already implied by the text.

### Typos and grammar
Fix obvious typos (e.g. `ipadress` → `IP address`, `Multipe` → `Multiple`) and lowercase protocol names in prose.

## Rules

- **Never remove** an existing `:::info` block.
- **Never invent** application-specific content (context menu entries, field options, behavior) that is not already present in the source.
- Use `:::warning[Administrator privileges required]` only for the admin banner, with that exact title.
- Use `:::note` only for genuinely non-obvious side facts — not for actions or admin rights.
- **Omit** any `## Actions` subsection that has no content for this page.
- Preserve all existing links, anchors (`#section-name`), and image references exactly.
- Do not change field types, examples, or possible values.
- Do not add new top-level sections (`##`) that do not exist in the source, except `## Actions`.
