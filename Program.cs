using _12_fukotai.Components;
using _12_fukotai.Services;
using _12_fukotai.Services.Interfaces;
using _12_fukotai.Services.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// コンソールログを明示的に設定（単一実行ファイルでも表示されるようにする）
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = false;
    options.SingleLine = false;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// ログレベルを明示的に設定
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
// _12_fukotai名前空間全体をInformationレベルで出力
builder.Logging.AddFilter("_12_fukotai", LogLevel.Information);

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
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // 一時的な接続エラーに対して自動リトライを有効化
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);

        // コマンドタイムアウトを60秒に設定
        sqlOptions.CommandTimeout(60);
    }));

builder.Services.AddScoped<IAndonDataService, AndonDataService>();
builder.Services.AddSingleton<IMasterDataCache, MasterDataCache>();
builder.Services.AddHostedService<MasterDataUpdateService>();

// D_STATUSデータ監視サービス（5秒間隔でログ出力）
builder.Services.AddHostedService<StatusDataMonitorService>();

var app = builder.Build();

// ログシステムが動作しているか確認
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== アプリケーション起動 ===");
logger.LogInformation("使用データベース: {DbName}", databaseName);
logger.LogInformation("接続文字列: {ConnectionString}", connectionString?.Substring(0, Math.Min(50, connectionString.Length)) + "...");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
