# Playwright MCP — Tool Schemas

All tools are TYPE 2. Any phase agent may call them directly.

## NavigateTo

```json
{
  "name": "NavigateTo",
  "inputSchema": {
    "type": "object",
    "properties": {
      "url": { "type": "string", "description": "HTTP/HTTPS URL to navigate to." },
      "waitUntil": { "type": "string", "enum": ["load", "domcontentloaded", "networkidle"], "default": "load" },
      "timeoutMs": { "type": "integer", "default": 30000 }
    },
    "required": ["url"]
  },
  "returns": "SUCCESS: {finalUrl} — HTTP {status} | ERROR: {message}"
}
```

## GetPageContent

```json
{
  "name": "GetPageContent",
  "inputSchema": { "type": "object", "properties": {} },
  "returns": "SUCCESS: {full outerHTML of current page} | ERROR: {message}"
}
```

## GetElementText

```json
{
  "name": "GetElementText",
  "inputSchema": {
    "type": "object",
    "properties": {
      "selector": { "type": "string", "description": "CSS selector for the target element." }
    },
    "required": ["selector"]
  },
  "returns": "SUCCESS: {innerText} | ERROR: element not found"
}
```

## TakeScreenshot

```json
{
  "name": "TakeScreenshot",
  "inputSchema": {
    "type": "object",
    "properties": {
      "fileName": { "type": "string", "description": "Optional filename without extension. Defaults to timestamp." },
      "fullPage": { "type": "boolean", "default": false }
    }
  },
  "returns": "SUCCESS: .pmcro/screenshots/{fileName}.png | ERROR: {message}"
}
```

## ClickElement

```json
{
  "name": "ClickElement",
  "inputSchema": {
    "type": "object",
    "properties": {
      "selector": { "type": "string" },
      "timeoutMs": { "type": "integer", "default": 5000 }
    },
    "required": ["selector"]
  },
  "returns": "SUCCESS: clicked {selector} | ERROR: {message}"
}
```

## FillInput

```json
{
  "name": "FillInput",
  "inputSchema": {
    "type": "object",
    "properties": {
      "selector": { "type": "string" },
      "value": { "type": "string" }
    },
    "required": ["selector", "value"]
  },
  "returns": "SUCCESS: filled {selector} | ERROR: {message}"
}
```

## WaitForSelector

```json
{
  "name": "WaitForSelector",
  "inputSchema": {
    "type": "object",
    "properties": {
      "selector": { "type": "string" },
      "timeoutMs": { "type": "integer", "default": 10000 }
    },
    "required": ["selector"]
  },
  "returns": "SUCCESS: selector appeared | ERROR: timeout or not found"
}
```

## EvaluateScript

```json
{
  "name": "EvaluateScript",
  "inputSchema": {
    "type": "object",
    "properties": {
      "expression": {
        "type": "string",
        "description": "JavaScript expression evaluated in page context. Must not mutate external state."
      }
    },
    "required": ["expression"]
  },
  "returns": "SUCCESS: {JSON serialized result} | ERROR: {message}"
}
```