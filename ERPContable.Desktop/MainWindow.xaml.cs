using System.IO;
using System.ComponentModel;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace ERPContable.Desktop;

public partial class MainWindow : Window
{
    private readonly DesktopApiService _api = new();
    private bool _starting;
    private bool _allowClose;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await StartAsync();
    }

    private async Task StartAsync()
    {
        if (_starting) return;
        _starting = true;
        RetryButton.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Visible;
        StatusText.Text = "Preparando la base de datos local…";

        try
        {
            var address = await _api.StartAsync();
            StatusText.Text = "Iniciando la interfaz…";

            var webViewData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ERPContable", "WebView2");
            Directory.CreateDirectory(webViewData);
            var webViewEnvironment = await CoreWebView2Environment.CreateAsync(null, webViewData);
            await Browser.EnsureCoreWebView2Async(webViewEnvironment);
            Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
            Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            Browser.NavigationCompleted += Browser_NavigationCompleted;
            Browser.Source = address;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            _starting = false;
        }
    }

    private void Browser_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            return;
        }

        ShowError(new InvalidOperationException($"No se pudo abrir la interfaz ({e.WebErrorStatus})."));
    }

    private void ShowError(Exception ex)
    {
        StatusText.Text = "No se pudo iniciar ERP Contable. " + ex.Message;
        RetryButton.Visibility = Visibility.Visible;
        LoadingPanel.Visibility = Visibility.Visible;
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e) => await StartAsync();

    protected override async void OnClosing(CancelEventArgs e)
    {
        if (_allowClose)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        IsEnabled = false;
        Browser.Dispose();
        await _api.DisposeAsync();
        _allowClose = true;
        Close();
    }
}
