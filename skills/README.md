# PMCRO Custom DocFX Template

**Author:** Shawn Bellazan · **ThoughtLock:** 2026-05-05 · **Target:** DocFX 2.78.5 + modern template

I am the visual and prose identity layer of the PMCRO Substrate documentation.  
I override the DocFX `modern` template with a dark-first design, PMCRO phase colours,  
and a TTS listening mode so the documentation can be heard as naturally as it is read.

---

## What Is in This Deliverable

```
templates/
└── pmcro/
    └── public/
        ├── main.css    ← All visual overrides. Bootstrap 5 variable overrides + PMCRO design
        └── main.js     ← Dark default, GitHub icon link, TTS mode toggle button

docs/
└── architecture/
    ├── what-pmcro-is.md   ← Rewritten in first-person TTS-friendly voice
    ├── pmcro-loop.md      ← Rewritten in first-person TTS-friendly voice
    └── cognitive-trail.md ← Rewritten in first-person TTS-friendly voice

docfx.json   ← Updated: adds "templates/pmcro" to the template array, fixes markdigExtensions
```

---

## Install — 3 Steps

### Step 1: Copy the template folder

Copy the `templates/pmcro/` folder into the root of your ProjectName repo,
so the structure becomes:

```
ProjectName/
├── docfx.json
├── templates/
│   └── pmcro/
│       └── public/
│           ├── main.css
│           └── main.js
├── docs/
├── src/
└── ...
```

### Step 2: Replace docfx.json

Replace your existing `docfx.json` with the one provided here.  
The only new line is the third entry in the `template` array:

```json
"template": [
  "default",
  "modern",
  "templates/pmcro"
]
```

DocFX merges templates in order. `default` provides the base. `modern` adds Bootstrap 5.  
`templates/pmcro` overrides colours, typography, and adds the TTS button.  
**Nothing in `default` or `modern` is removed — only overridden.**

### Step 3: Replace the rewritten docs (optional but recommended)

Copy the three rewritten architecture files into `docs/architecture/`:

- `what-pmcro-is.md` — replaces the existing file
- `pmcro-loop.md` — replaces the existing file
- `cognitive-trail.md` — replaces the existing file

These files use a first-person, present-tense voice designed for natural TTS playback.  
Every heading reads as a spoken statement. Every paragraph flows at a natural sentence pace.

---

## Build and Verify

```powershell
dotnet tool restore
docfx metadata docfx.json
docfx build docfx.json
docfx serve _site
```

Open `http://localhost:8080` and confirm:

- Background is `#0d1117` (dark substrate)
- Headings use Space Grotesk
- H1 has a cyan left-border accent
- A **LISTEN** button appears in the bottom-right corner
- Clicking LISTEN widens line-height and letter-spacing for listening mode
- LISTEN button turns cyan and label changes to **READING** when active

---

## Design System

The colour system is named after the PMCRO phases:

| Token | Value | Used for |
|---|---|---|
| `--pmcro-substrate` | `#0d1117` | Page background |
| `--pmcro-surface` | `#161b22` | Sidebar, cards |
| `--pmcro-cyan` | `#39d0d8` | Links, accents, active states, H1 border |
| `--pmcro-plan` | `#58a6ff` | Plan phase, first-column monospace text |
| `--pmcro-make` | `#3fb950` | Make phase, inline code |
| `--pmcro-check` | `#f0883e` | Check phase, warnings |
| `--pmcro-reflect` | `#bc8cff` | Reflect phase, emphasis, return types |
| `--pmcro-orchestrate` | `#39d0d8` | Orchestrate phase (same as cyan) |

Fonts:
- **Display / Headings:** Space Grotesk — sharp, technical, modern
- **Body:** DM Sans — open tracking, comfortable at length, TTS-friendly
- **Mono / Code:** JetBrains Mono — first-class developer readability

---

## TTS Mode

The TTS button in the bottom-right corner toggles `body.tts-mode`.  
When active:

- Font size increases to `1.1rem`
- Line height increases to `1.95` — more time between lines for the voice to settle
- Letter spacing opens slightly — `0.012em` — natural spoken word spacing
- Inline code loses its coloured background — code tokens in prose won't interrupt listening flow
- The preference is saved to `localStorage` and restored on next load

To use with your phone or a browser TTS extension, activate listening mode first,  
then trigger your TTS reader. The wider spacing makes sentence boundaries clearer  
for both synthetic voices and human listening.

---

## Constraint Earned

**DFX-012:** When delivering documentation intended for TTS consumption, I ALWAYS write  
headings as spoken statements, paragraphs at natural sentence cadence, and strip  
decorative inline code from prose sections so TTS readers scan cleanly.

---

*© 2026 Tooensure LLC — Behavioral Intent Programming*