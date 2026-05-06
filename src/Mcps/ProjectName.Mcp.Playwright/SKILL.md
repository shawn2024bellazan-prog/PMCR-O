---
name: playwright-mcp
description: >-
  I AM the Playwright MCP Actuator for the PMCRO Substrate. Use me when a phase
  agent needs to automate a browser: navigate pages, click elements, fill forms,
  take screenshots, scrape HTML content, wait for selectors, evaluate JavaScript,
  or run multi-step browser sequences. I am TYPE 2 — I inform cognition, I do
  not change the world. All HTTP/HTTPS URLs only. Trigger on: open browser,
  navigate to URL, click button, fill form, screenshot, scrape page, get HTML,
  wait for element, run JavaScript in browser, browser automation, web scraping.
license: Proprietary
compatibility: .NET 10 | ModelContextProtocol.AspNetCore 1.0.0 | Microsoft.Playwright 1.50.0 | PMCRO Substrate
metadata:
  author: Tooensure LLC
  version: "1.0.0"
  pmcro-tier: infrastructure
  thought-lock: "2026-05-05"
  service-name: projectname-mcp-playwright
  mcp-endpoint: /mcp
  arch-law: ARCH-013, PW-LAW-001, PW-LAW-002, PW-LAW-003
---

# Playwright MCP Actuator

## Identity

I AM the Playwright MCP Actuator — the PMCRO substrate's browser automation layer.
I am a PURE TYPE 2 MCP — I inform cognition, I never change the world in the PMCRO sense.
I NEVER navigate to `file://` or non-HTTP(S) URLs.
I NEVER store credentials, cookies, or PII between agent cycles.
I ALWAYS route through the MAF `PlaywrightMicroWorkflow` — no direct Playwright calls in tools.
I ALWAYS return results prefixed with `SUCCESS:` or `ERROR:`.

## Tools (All TYPE 2)

| Tool              | Description |
|-------------------|-------------|
| `NavigateTo`      | Navigate to a URL and wait for page load. Returns final URL and HTTP status. |
| `GetPageUrl`      | Returns the current URL of the active page. |
| `GetPageTitle`    | Returns the `<title>` of the current page. |
| `GetPageContent`  | Returns full outer HTML of the current page. |
| `TakeScreenshot`  | Captures screenshot to `.pmcro/screenshots/`. Returns file path. |
| `ClickElement`    | Clicks the first DOM element matching a CSS selector. |
| `FillInput`       | Clears and fills a text input or textarea. |
| `HoverElement`    | Moves mouse pointer over a DOM element. |
| `PressKey`        | Sends keyboard key/chord to page (e.g. `Enter`, `Control+A`). |
| `SelectOption`    | Selects an option from a `<select>` by value or label. |
| `WaitForElement`  | Waits until a CSS selector is visible. Max 30 000 ms (PW-LAW-003). |
| `EvaluateScript`  | Evaluates a JavaScript expression. Returns JSON-serialized result. |

## Usage Rules

- Always call `NavigateTo` before any other interaction tool.
- Check response prefix (`SUCCESS:` / `ERROR:`) before proceeding.
- `WaitForElement` before interacting with dynamically-loaded elements.
- `EvaluateScript` blocks `document.cookie` and `localStorage.getItem` (PW-LAW-002).
- All calls pass through `BrowserPlanExecutor` → `BrowserActuatorExecutor` → `BrowserResultExecutor`.

## Typical Sequences

### Scrape a web page
```
1. NavigateTo("https://example.com/data")
2. WaitForElement("table.results")
3. GetPageContent()                         → parse HTML for data
```

### Fill and submit a form
```
1. NavigateTo("https://app.example.com/login")
2. WaitForElement("#username")
3. FillInput("#username", "user@example.com")
4. FillInput("#password", "***")
5. ClickElement("button[type=submit]")
6. WaitForElement(".dashboard")
7. TakeScreenshot()                         → confirm success
```

## Resources

See [tool-schemas.md](references/tool-schemas.md) for full input/output schemas.

## Law Anchors

| Law           | Statement |
|---------------|-----------|
| `ARCH-013`    | All browser access flows through this MCP exclusively. |
| `PW-LAW-001`  | `NavigateTo` accepts `http://` and `https://` only. |
| `PW-LAW-002`  | `EvaluateScript` blocks `document.cookie` and `localStorage.getItem`. |
| `PW-LAW-003`  | `WaitForElement` is capped at 30 000 ms. |
| `PW-LAW-005`  | The only place Playwright APIs are called is `BrowserActuatorExecutor`. |
| `FRAC-PW-001` | Safety decisions are deterministic code — AI output cannot approve an unsafe operation. |