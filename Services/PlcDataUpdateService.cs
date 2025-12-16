namespace _12_fukotai.Services;

using _12_fukotai.Services.Interfaces;

/// <summary>
/// PLC データ自動更新サービス（1秒間隔）
/// </summary>
public class PlcDataUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PlcDataCache _cache;
    private readonly ILogger<PlcDataUpdateService> _logger;

    public PlcDataUpdateService(
        IServiceScopeFactory scopeFactory,
        PlcDataCache cache,
        ILogger<PlcDataUpdateService> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PlcDataUpdateService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Scoped サービス用のスコープ作成
                using var scope = _scopeFactory.CreateScope();
                var jsonReader = scope.ServiceProvider
                    .GetRequiredService<IGetJsonData>();

                // データ更新
                await _cache.UpdateAsync(jsonReader);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PlcDataUpdateService");
            }

            // 1秒待機
            try
            {
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // アプリケーション終了時の正常なキャンセル
                break;
            }
        }

        _logger.LogInformation("PlcDataUpdateService stopped");
    }
}
