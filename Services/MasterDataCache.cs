namespace _12_fukotai.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using _12_fukotai.Models.Database;
using _12_fukotai.Services.Database;
using _12_fukotai.Services.Interfaces;

/// <summary>
/// マスターデータキャッシュサービス（Singleton）
/// </summary>
public class MasterDataCache : IMasterDataCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MasterDataCache> _logger;
    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _linkedServerQuery;

    public Dictionary<(byte, short), string?> ErrMaster { get; private set; } = new();
    public Dictionary<(byte, short), string?> WaitMaster { get; private set; } = new();
    public Dictionary<(byte, short), string?> StopMaster { get; private set; } = new();

    public MasterDataCache(
        IServiceScopeFactory scopeFactory,
        ILogger<MasterDataCache> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;

        // リンクサーバー設定を読み込み
        var useDatabaseName = _configuration.GetValue<string>("AndonSettings:UseDatabaseName") ?? "AndonDatabase_himeji1";
        var prefix = useDatabaseName == "AndonDatabase_himeji2" ? "Himeji2" : "Himeji1";

        var linkedServer = _configuration.GetValue<string>($"LinkedServerSettings:{prefix}_LinkedServerName") ?? "10.60.40.14";
        var database = _configuration.GetValue<string>($"LinkedServerSettings:{prefix}_DatabaseName") ?? "KadouMoni3_144";

        _linkedServerQuery = $"[{linkedServer}].[{database}].[dbo]";
    }

    /// <summary>
    /// マスターデータを再読み込み
    /// </summary>
    public async Task RefreshAsync()
    {
        await _lock.WaitAsync();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AndonDbContext>();

            // M_ERR読み込み
            _logger.LogInformation("M_ERRマスター読み込み開始");
            var errSql = $@"
                SELECT [ENABLE_FLG], [MACHINE_NO], [ERR_NO], [ERR_NAME], [ERR_TYPE], [UPDATE_TIME], [NOTE]
                FROM {_linkedServerQuery}.[M_ERR]
                WHERE [ENABLE_FLG] = 1";
            _logger.LogDebug("M_ERR SQL: {Sql}", errSql);

            List<MErr> errList;
            try
            {
                errList = await dbContext.MErr.FromSqlRaw(errSql).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "M_ERR読み込み中にエラー発生。SQL: {Sql}", errSql);
                throw;
            }
            var errDict = new Dictionary<(byte, short), string?>();
            foreach (var e in errList)
            {
                errDict[(e.MachineNo, e.ErrNo)] = e.ErrName;
            }
            ErrMaster = errDict;

            _logger.LogDebug("エラーマスター読み込み完了: {Count}件", ErrMaster.Count);

            // M_WAIT読み込み
            _logger.LogInformation("M_WAITマスター読み込み開始");
            var waitSql = $@"
                SELECT [ENABLE_FLG], [MACHINE_NO], [WAIT_NO], [WAIT_NAME], [UPDATE_TIME], [NOTE]
                FROM {_linkedServerQuery}.[M_WAIT]
                WHERE [ENABLE_FLG] = 1 AND [WAIT_NAME] IS NOT NULL";
            _logger.LogDebug("M_WAIT SQL: {Sql}", waitSql);

            List<MWait> waitList;
            try
            {
                waitList = await dbContext.MWait.FromSqlRaw(waitSql).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "M_WAIT読み込み中にエラー発生。SQL: {Sql}", waitSql);
                throw;
            }
            var waitDict = new Dictionary<(byte, short), string?>();
            foreach (var w in waitList)
            {
                waitDict[(w.MachineNo, w.WaitNo)] = w.WaitName;
            }
            WaitMaster = waitDict;

            _logger.LogDebug("待機マスター読み込み完了: {Count}件", WaitMaster.Count);

            // M_STOP読み込み
            _logger.LogInformation("M_STOPマスター読み込み開始");
            var stopSql = $@"
                SELECT [ENABLE_FLG], [MACHINE_NO], [STOP_NO], [STOP_NAME], [UPDATE_TIME], [NOTE]
                FROM {_linkedServerQuery}.[M_STOP]
                WHERE [ENABLE_FLG] = 1";
            _logger.LogDebug("M_STOP SQL: {Sql}", stopSql);

            List<MStop> stopList;
            try
            {
                stopList = await dbContext.MStop.FromSqlRaw(stopSql).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "M_STOP読み込み中にエラー発生。SQL: {Sql}", stopSql);
                throw;
            }
            var stopDict = new Dictionary<(byte, short), string?>();
            foreach (var s in stopList)
            {
                stopDict[(s.MachineNo, s.StopNo)] = s.StopName;
            }
            StopMaster = stopDict;

            _logger.LogDebug("停止マスター読み込み完了: {Count}件", StopMaster.Count);

            _logger.LogInformation(
                "マスターデータ更新完了（ERR:{ErrCount}, WAIT:{WaitCount}, STOP:{StopCount}）",
                ErrMaster.Count, WaitMaster.Count, StopMaster.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "マスターデータ読み込みエラー");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}
