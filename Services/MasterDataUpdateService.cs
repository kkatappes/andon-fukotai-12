namespace _12_fukotai.Services;

using _12_fukotai.Services.Interfaces;

/// <summary>
/// マスターデータ自動更新サービス（15分間隔）
/// </summary>
public class MasterDataUpdateService : BackgroundService
{
    private readonly IMasterDataCache _masterCache;
    private readonly ILogger<MasterDataUpdateService> _logger;
    private const int UpdateIntervalMinutes = 15;

    public MasterDataUpdateService(
        IMasterDataCache masterCache,
        ILogger<MasterDataUpdateService> logger)
    {
        _masterCache = masterCache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MasterDataUpdateService started");

        // 起動時に1回実行
        try
        {
            await _masterCache.RefreshAsync();
            _logger.LogInformation("初回マスターデータ読み込み完了");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初回マスターデータ読み込みエラー");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 15分待機
                _logger.LogDebug("次回更新まで{Minutes}分待機", UpdateIntervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(UpdateIntervalMinutes), stoppingToken);

                // マスターデータ更新
                await _masterCache.RefreshAsync();
                _logger.LogInformation("マスターデータ定期更新完了");
            }
            catch (OperationCanceledException)
            {
                // アプリケーション終了時の正常なキャンセル
                _logger.LogInformation("MasterDataUpdateService キャンセル要求受信");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "マスターデータ定期更新エラー");
                // エラーが発生しても継続
            }
        }

        _logger.LogInformation("MasterDataUpdateService stopped");
    }
}
