# Simple Browser App
A customizable WPF desktop application featuring a built-in browser (WebView2) and flexible UI/behavior controlled by a JSON configuration file.

## Features
- Displays a local HTML file in a browser view (WebView2).
- Application window title, size, menu visibility, and "Always on Top" state are configurable via JSON.
- The HTML file to display can be specified in the config file (relative to the executable).
- Menu includes:
  - "Always on Top" (checkable)
  - "Exit"
- All settings are loaded at startup from a config file named `[ExecutableName]-config.json` placed in the same directory as the executable.

## Configuration File (`[ExecutableName]-config.json`)
The application reads its settings from a JSON file named after the executable (e.g., `SimpleBrowserApp-config.json`).  
All properties are optional; defaults are used if not specified.

| Property      | Type    | Description                                                                 | Default      |
|---------------|---------|-----------------------------------------------------------------------------|--------------|
| title         | string  | Window title.                                                               | null (uses default window title) |
| topmost       | bool    | Whether the window stays always on top.                                     | false        |
| menu          | bool    | Whether the application menu is visible.                                    | false        |
| window_width  | number  | Window width in pixels.                                                     | 300          |
| window_height | number  | Window height in pixels.                                                    | 300          |
| html_path     | string  | Relative path (from executable) to the HTML file to display in the browser. | "app.html"   |

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

## How It Works
- On startup, the app loads `[ExecutableName]-config.json` from the executable's directory.
- All settings are applied immediately:
  - Window title, size, and "Always on Top" state.
  - Menu visibility and "Always on Top" menu check state.
  - The specified HTML file is loaded into the browser view.
- If a property is missing, the default value (see table above) is used.

## Requirements
- .NET 8.0 or later
- [Microsoft.Web.WebView2](https://www.nuget.org/packages/Microsoft.Web.WebView2/) NuGet package

## Build & Run

1. Restore NuGet packages.
2. Build the solution.
3. Place your HTML file (e.g., `app.html`) and the config file in the same directory as the executable.
4. Run the application.
