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

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);

                    title = config?.title ?? null;
                    topmost = config?.topmost ?? false;
                    menuVisible = config?.menu ?? false;
                    windowWidth = config?.window_width ?? 300;
                    windowHeight = config?.window_height ?? 300;
                    htmlRelPath = config?.html_path ?? "app.html";
                    windowIconPath = config?.window_icon ?? null;
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
