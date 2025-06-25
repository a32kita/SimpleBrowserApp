![Simple Browser App Window](README-images/simplebrwapp-window.png)

# Simple Browser App
A customizable WPF desktop application featuring a built-in browser (WebView2) and flexible UI/behavior controlled by a JSON configuration file.

## Features
- Displays a local HTML file or a web page (http/https URL) in a browser view (WebView2).
- Application window title, size, menu visibility, and "Always on Top" state are configurable via JSON.
- The HTML file or URL to display can be specified in the config file (relative path, absolute path, or URL).
- Menu includes:
  - "Always on Top" (checkable)
  - "Exit"
- All settings are loaded at startup from a config file named `[ExecutableName]-config.json` placed in the same directory as the executable.

## Configuration File (`[ExecutableName]-config.json`)
The application reads its settings from a JSON file named after the executable (e.g., `SimpleBrowserApp-config.json`).  
All properties are optional; defaults are used if not specified.

| Property      | Type    | Description                                                                                                   | Default      |
|---------------|---------|---------------------------------------------------------------------------------------------------------------|--------------|
| title         | string  | Window title.                                                                                                 | null (uses default window title) |
| topmost       | bool    | Whether the window stays always on top.                                                                       | false        |
| menu          | bool    | Whether the application menu is visible.                                                                      | false        |
| window_width  | number  | Window width in pixels.                                                                                       | 300          |
| window_height | number  | Window height in pixels.                                                                                      | 300          |
| html_path     | string  | Path or URL to display in the browser. Supports:<br> - Relative path from executable<br> - Absolute file path<br> - http/https URL | "app.html"   |

### Example `SimpleBrowserApp-config.json`
```json
{
  "title": "My Custom Browser",
  "topmost": true,
  "menu": true,
  "window_width": 900,
  "window_height": 600,
  "html_path": "app.html"
}
```
#### Example for absolute path
```json
{
  "html_path": "C:/Users/YourName/Documents/sample.html"
}
```
#### Example for web URL
```json
{
  "html_path": "https://example.com"
}
```

## How It Works
- On startup, the app loads `[ExecutableName]-config.json` from the executable's directory.
- All settings are applied immediately:
  - Window title, size, and "Always on Top" state.
  - Menu visibility and "Always on Top" menu check state.
  - The specified HTML file or URL is loaded into the browser view.
    - If `html_path` starts with `http://` or `https://`, it is loaded as a web page.
    - If `html_path` is an absolute path, that file is loaded.
    - Otherwise, it is treated as a relative path from the executable directory.
- If a property is missing, the default value (see table above) is used.

## Requirements
- .NET 8.0 or later
- [Microsoft.Web.WebView2](https://www.nuget.org/packages/Microsoft.Web.WebView2/) NuGet package

## Build & Run

1. Restore NuGet packages.
2. Build the solution.
3. Place your HTML file (e.g., `app.html`) and the config file in the same directory as the executable, or specify an absolute path or URL in the config.
4. Run the application.
