using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
using ERPContable.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ERPContable.Desktop;

internal sealed class DesktopApiService : IAsyncDisposable
{
    private WebApplication? _app;

    public async Task<Uri> StartAsync(CancellationToken cancellationToken = default)
    {
        if (_app is not null)
            return new Uri(_app.Urls.Single());

        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ERPContable");
        var dataDirectory = Path.Combine(appData, "Data");
        Directory.CreateDirectory(dataDirectory);

        var databasePath = Path.Combine(dataDirectory, "erpcontable.db");
        var jwtKey = LoadOrCreateJwtKey(Path.Combine(appData, "desktop.key"));
        var address = new Uri($"http://127.0.0.1:{FindAvailablePort()}");

        var app = await ApiHost.BuildAsync([], "Desktop", builder =>
        {
            builder.WebHost.UseUrls(address.ToString());
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "Sqlite",
                ["ConnectionStrings:Sqlite"] = $"Data Source={databasePath};Cache=Shared;Foreign Keys=True",
                ["Jwt:Key"] = jwtKey,
                ["Jwt:Issuer"] = "ERPContable",
                ["Jwt:Audience"] = "ERPContable.Desktop",
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "14",
                ["Auth:RequireConfirmedEmail"] = "false",
                ["DemoData:Enabled"] = "true",
                ["App:FrontendUrl"] = address.ToString().TrimEnd('/')
            });
        });

        try
        {
            await app.StartAsync(cancellationToken);
            _app = app;
            return address;
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is null) return;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await _app.StopAsync(timeout.Token);
        await _app.DisposeAsync();
        _app = null;
    }

    private static int FindAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string LoadOrCreateJwtKey(string path)
    {
        if (File.Exists(path)) return File.ReadAllText(path).Trim();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        File.WriteAllText(path, key);
        File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
        return key;
    }
}
