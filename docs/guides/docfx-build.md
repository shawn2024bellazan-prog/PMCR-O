---
title: "DocFX Build (CL-007)"
---

# DocFX Build (CL-007)

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchors: DFX-001 through DFX-010*

---

## Prerequisites

- .NET 10 SDK installed
- DocFX 2.78.5 installed as a global tool:

```bash
dotnet tool restore
dotnet tool list -g | grep docfx   # should show 2.78.5
```

- All source projects compile: `dotnet build` exits 0 before running DocFX.

## What the End of This Guide Looks Like

All three stages of CL-007 exit 0. The `_site/` directory is produced and valid. No errors in `docfx metadata` or `docfx build` output.

---

## Steps

### 1. Restore Tools

```powershell
dotnet tool restore
```

This reads `.config/dotnet-tools.json` and installs DocFX 2.78.5. If the tool is already installed at the correct version, this is a no-op.

### 2. Run Metadata Generation

```bash
docfx metadata docfx.json
```

This step reads all `.csproj` files specified in `docfx.json` and generates `api/*.yml` from XML doc comments. **The source must compile before this step.** If `docfx metadata` fails:

| Error | Root cause | Fix |
|---|---|---|
| `CS0246: IChatClient not found` | Missing `Microsoft.Extensions.AI` reference | Add `PackageReference` in `Directory.Packages.props` and the `.csproj` |
| `CS0246: [Type] not found` | Missing namespace or unimplemented type | Add `using` directive or implement the type |
| `Missing <summary>` warning | Public member lacks XML doc comment | Add `I AM the...` summary — EC-005, DFX-004 |

### 3. Run Site Build

```bash
docfx build docfx.json
```

This step converts `api/*.yml` and `docs/**/*.md` into `_site/`. If this step fails:

| Error | Root cause | Fix |
|---|---|---|
| Broken xref | Type renamed or namespace moved | Update the `@"TypeName"` xref in the `.md` file |
| Missing TOC entry | New `.md` added without `toc.yml` entry | Add the entry — DFX-005, DFX-008 |
| `_site/` already committed | `.gitignore` gap | `git rm -r --cached _site/` |

### 4. Preview (Optional)

```bash
docfx serve _site
```

Navigate to `http://localhost:8080`. Confirm:
- Navigation renders the four-section TOC correctly.
- Search is functional.
- API reference pages show XML doc comment summaries.
- All internal links resolve without 404.

---

## Verification

CL-007 passes when all three commands exit 0 with no errors (warnings are acceptable):

```
dotnet tool restore   → exit 0
docfx metadata        → exit 0
docfx build           → exit 0
```

---

## What Must Never Be Committed

| Path | Why |
|---|---|
| `_site/` | Build output only. Generated on every build. |
| `api/*.yml` | Generated from source. Hand-editing is a fracture (DFX-001). |

Both paths must be in `.gitignore`. Verify:

```bash
git check-ignore _site api
```

Both lines should be printed. If either is absent, add the missing entry to `.gitignore` and run `git rm -r --cached [path]`.

---

## See Also

- [XML Doc Comment Standard](xml-doc-comments.md) — required format for every public type
- [Architecture → Earned Laws Registry](../architecture/laws.md) — EC-005 (documentation law)
