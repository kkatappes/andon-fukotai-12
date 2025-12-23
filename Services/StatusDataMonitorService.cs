namespace _12_fukotai.Services;

using _12_fukotai.Services.Interfaces;

/// <summary>
/// D_STATUSデータ監視サービス（5秒間隔でログ出力）
/// </summary>
public class StatusDataMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StatusDataMonitorService> _logger;
    private const int UpdateIntervalSeconds = 5;

    public StatusDataMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<StatusDataMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StatusDataMonitorService started");

        // 起動時に1回実行
        await RefreshStatusData();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 5秒待機
                await Task.Delay(TimeSpan.FromSeconds(UpdateIntervalSeconds), stoppingToken);

                // D_STATUSデータ取得
                await RefreshStatusData();
            }
            catch (OperationCanceledException)
            {
                // アプリケーション終了時の正常なキャンセル
                _logger.LogInformation("StatusDataMonitorService キャンセル要求受信");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "D_STATUSデータ取得エラー");
                // エラーが発生しても継続
            }
        }

        _logger.LogInformation("StatusDataMonitorService stopped");
    }

    private async Task RefreshStatusData()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var andonDataService = scope.ServiceProvider.GetRequiredService<IAndonDataService>();

            var statusList = await andonDataService.GetMachineStatusListAsync();

            // ログは AndonDataService 内で出力される
            // ここでは特に何もしない（必要ならサマリーを出力）
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshStatusDataでエラー発生");
        }
    }
}
