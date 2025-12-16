using _12_fukotai.Components;
using _12_fukotai.Services;
using _12_fukotai.Services.Interfaces;
using _12_fukotai.Services.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// JSON読み取りサービスをScopedで登録
builder.Services.AddScoped<IGetJsonData, GetJsonData>();

// PLCデータキャッシュサービスをSingletonで登録
builder.Services.AddSingleton<PlcDataCache>();
builder.Services.AddSingleton<IPlcDataCache>(sp => sp.GetRequiredService<PlcDataCache>());

// 1秒間隔の自動更新サービスを登録
builder.Services.AddHostedService<PlcDataUpdateService>();

// SQLデータベース関連サービス
var databaseName = builder.Configuration.GetValue<string>("AndonSettings:UseDatabaseName")
    ?? "AndonDatabase_himeji1";
var connectionString = builder.Configuration.GetConnectionString(databaseName);

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        $"接続文字列 '{databaseName}' が appsettings.json に設定されていません。");
}

builder.Services.AddDbContext<AndonDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAndonDataService, AndonDataService>();
builder.Services.AddSingleton<IMasterDataCache, MasterDataCache>();
builder.Services.AddHostedService<MasterDataUpdateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
