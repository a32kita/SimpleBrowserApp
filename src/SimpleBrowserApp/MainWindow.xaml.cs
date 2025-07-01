using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace SimpleBrowserApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Config class for JSON deserialization
        private class AppConfig
        {
            public string? title { get; set; }
            public bool? topmost { get; set; }
            public bool? menu { get; set; }
            public int? window_width { get; set; }
            public int? window_height { get; set; }
            public string? html_path { get; set; }
            // Add window_icon property for icon path (.ico only)
            public string? window_icon { get; set; }
            // Add window_state_autosave property for window state autosave
            public bool? window_state_autosave { get; set; }
            // Add use_page_title property for using the page title as window title
            public bool? use_page_title { get; set; }
            // Add use_browser_context_menu property for controlling WebView2 context menu
            public bool? use_browser_context_menu { get; set; }
        }

        // Class for saving/restoring window state
        private class WindowStateInfo
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public WindowState WindowState { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            // Load config
            string exePath = System.Reflection.Assembly.GetEntryAssembly()?.Location ?? "";
            string exeDir = Path.GetDirectoryName(exePath) ?? "";
            string exeName = Path.GetFileNameWithoutExtension(exePath);
            string configPath = Path.Combine(exeDir, $"{exeName}-config.json");

            // Set defaults
            string? title = null;
            bool topmost = false;
            bool menuVisible = false;
            int windowWidth = 300;
            int windowHeight = 300;
            string htmlRelPath = "app.html";
            string? windowIconPath = null;
            bool windowStateAutosave = true; // default true
            bool usePageTitle = false; // default false
            bool useBrowserContextMenu = true; // default true

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);

                    title = config?.title ?? title;
                    topmost = config?.topmost ?? topmost;
                    menuVisible = config?.menu ?? menuVisible;
                    windowWidth = config?.window_width ?? windowWidth;
                    windowHeight = config?.window_height ?? windowHeight;
                    htmlRelPath = config?.html_path ?? htmlRelPath;
                    windowIconPath = config?.window_icon ?? windowIconPath;
                    windowStateAutosave = config?.window_state_autosave ?? true; // default true
                    usePageTitle = config?.use_page_title ?? false; // default false
                    useBrowserContextMenu = config?.use_browser_context_menu ?? true; // default true
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read config: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Apply config to window
            this.Title = title ?? this.Title;
            this.Topmost = topmost;
            this.Width = windowWidth;
            this.Height = windowHeight;

            // Restore window state if autosave enabled
            string windowStatePath = Path.Combine(exeDir, $"{exeName}-windowstate.json");
            if (windowStateAutosave && File.Exists(windowStatePath))
            {
                try
                {
                    string stateJson = File.ReadAllText(windowStatePath);
                    var state = JsonSerializer.Deserialize<WindowStateInfo>(stateJson);
                    if (state != null)
                    {
                        // Restore position and size
                        this.Left = state.Left;
                        this.Top = state.Top;
                        this.Width = state.Width;
                        this.Height = state.Height;
                        // Restore window state (Normal/Maximized only)
                        if (state.WindowState == WindowState.Maximized || state.WindowState == WindowState.Normal)
                        {
                            this.WindowState = state.WindowState;
                        }
                    }
                }
                catch
                {
                    // Ignore errors and use default position/size
                }
            }

            // Menu visibility
            if (this.FindName("AlwaysOnTopMenuItem") is MenuItem alwaysOnTopMenuItem)
            {
                alwaysOnTopMenuItem.IsChecked = topmost;
            }
            if (this.FindName("Menu") is Menu menuControl)
            {
                menuControl.Visibility = menuVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // Fallback: try to find the Menu in the visual tree
                foreach (var child in LogicalTreeHelper.GetChildren(this))
                {
                    if (child is Grid grid)
                    {
                        foreach (var gridChild in grid.Children)
                        {
                            if (gridChild is Menu menu)
                            {
                                menu.Visibility = menuVisible ? Visibility.Visible : Visibility.Collapsed;
                                break;
                            }
                        }
                    }
                }
            }

            // Register event handlers for menu items
            AlwaysOnTopMenuItem.Checked += AlwaysOnTopMenuItem_Checked;
            AlwaysOnTopMenuItem.Unchecked += AlwaysOnTopMenuItem_Unchecked;
            ExitMenuItem.Click += ExitMenuItem_Click;

            // Register window closing event for state autosave
            if (windowStateAutosave)
            {
                this.Closing += (s, e) =>
                {
                    try
                    {
                        var state = new WindowStateInfo
                        {
                            Left = this.RestoreBounds.Left,
                            Top = this.RestoreBounds.Top,
                            Width = this.RestoreBounds.Width,
                            Height = this.RestoreBounds.Height,
                            WindowState = this.WindowState
                        };
                        string stateJson = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(windowStatePath, stateJson);
                    }
                    catch
                    {
                        // Ignore errors on save
                    }
                };
            }

            // Set window icon if window_icon is specified and valid (.ico, exists)
            if (!string.IsNullOrEmpty(windowIconPath))
            {
                string resolvedIconPath = windowIconPath;
                if (!Path.IsPathRooted(resolvedIconPath))
                {
                    resolvedIconPath = Path.Combine(exeDir, resolvedIconPath);
                }
                if (File.Exists(resolvedIconPath) && Path.GetExtension(resolvedIconPath).Equals(".ico", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(resolvedIconPath, UriKind.Absolute));
                    }
                    catch
                    {
                        // If icon loading fails, fallback to default icon (do nothing)
                    }
                }
                // else: fallback to default icon (do nothing)
            }

            // Set WebView2 source to html_path (supports http(s), absolute, and relative paths)
            Uri? browserUri = null;
            if (!string.IsNullOrEmpty(htmlRelPath))
            {
                if (htmlRelPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    htmlRelPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // Web URL
                    browserUri = new Uri(htmlRelPath, UriKind.Absolute);
                }
                else if (Path.IsPathRooted(htmlRelPath))
                {
                    // Absolute file path
                    if (File.Exists(htmlRelPath))
                        browserUri = new Uri(htmlRelPath, UriKind.Absolute);
                }
                else
                {
                    // Relative path from exeDir
                    string htmlFullPath = Path.Combine(exeDir, htmlRelPath);
                    if (File.Exists(htmlFullPath))
                        browserUri = new Uri(htmlFullPath, UriKind.Absolute);
                }
            }
            Browser.Source = browserUri ?? new Uri("about:blank");

            // --- use_page_title / use_browser_context_menu: WebView2 event hooks ---
            if (usePageTitle || !useBrowserContextMenu)
            {
                // WebView2 の CoreWebView2 準備完了時にイベント購読
                Browser.CoreWebView2InitializationCompleted += (s, e) =>
                {
                    if (Browser.CoreWebView2 != null)
                    {
                        // use_page_title: ページタイトルをウィンドウタイトルに反映
                        if (usePageTitle)
                        {
                            // 初回タイトル反映
                            if (!string.IsNullOrEmpty(Browser.CoreWebView2.DocumentTitle))
                            {
                                this.Title = Browser.CoreWebView2.DocumentTitle;
                            }
                            // タイトル変更時に MainWindow のタイトルを更新
                            Browser.CoreWebView2.DocumentTitleChanged += (sender, args) =>
                            {
                                this.Title = Browser.CoreWebView2.DocumentTitle;
                            };
                        }
                        // use_browser_context_menu: 標準コンテキストメニュー抑止
                        if (!useBrowserContextMenu)
                        {
                            Browser.CoreWebView2.ContextMenuRequested += (sender, args) =>
                            {
                                args.Handled = true;
                            };
                        }
                    }
                };
            }
        }

        // Toggle Topmost property when the menu item is checked/unchecked
        private void AlwaysOnTopMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }

        private void AlwaysOnTopMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }

        // Exit the application when the menu item is clicked
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
