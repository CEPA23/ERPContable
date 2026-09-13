using ERPContable.API;

var app = await ApiHost.BuildAsync(args);
await app.RunAsync();
