namespace _12_fukotai.Services;

using Microsoft.EntityFrameworkCore;
using _12_fukotai.Models;
using _12_fukotai.Models.Database;
using _12_fukotai.Services.Database;
using _12_fukotai.Services.Interfaces;

/// <summary>
/// アンドンデータ取得サービス
/// </summary>
public class AndonDataService : IAndonDataService
{
    private readonly AndonDbContext _dbContext;
    private readonly IMasterDataCache _masterCache;
    private readonly ILogger<AndonDataService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _linkedServerQuery;

    public AndonDataService(
        AndonDbContext dbContext,
        IMasterDataCache masterCache,
        ILogger<AndonDataService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _masterCache = masterCache;
        _logger = logger;
        _configuration = configuration;

        // リンクサーバー設定を読み込み
        var linkedServer = _configuration.GetValue<string>("LinkedServerSettings:Himeji1_LinkedServerName") ?? "10.60.40.14";
        var database = _configuration.GetValue<string>("LinkedServerSettings:Himeji1_DatabaseName") ?? "KadouMoni3_144";

        _linkedServerQuery = $"[{linkedServer}].[{database}].[dbo]";
        _logger.LogInformation("リンクサーバークエリ: {Query}", _linkedServerQuery);
    }

    /// <summary>
    /// 全装置の状態リストを取得
    /// </summary>
    public async Task<List<MachineStatusDto>> GetMachineStatusListAsync()
    {
        try
        {
            // リンクサーバー経由で生SQLクエリを実行
            var sql = $@"
                SELECT
                    [MACHINE_NO], [MODEL_NO], [READY_STATUS], [RUN_STATUS],
                    [WAIT_STATUS], [WAIT_NO], [ARRANGE_STATUS], [STOP_STATUS],
                    [ERR_STATUS], [ERR_NO], [INTO_SUM], [PRODUCT_SUM], [TARGET_NUM],
                    [INTO_NUM], [PRODUCT_NUM], [OUT_NUM], [UPDATE_TIME], [NOTE],
                    [LOT_NO], [KISHU_NAME], [PRODUCT_KBN], [COIL_NUM]
                FROM {_linkedServerQuery}.[D_STATUS]
                ORDER BY [MACHINE_NO]";

            var statusList = await _dbContext.DStatus
                .FromSqlRaw(sql)
                .ToListAsync();

            _logger.LogDebug("D_STATUSデータ取得: {Count}件", statusList.Count);

            return statusList.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "装置状態リスト取得エラー");
            return new List<MachineStatusDto>();
        }
    }

    /// <summary>
    /// 指定した装置番号の状態を取得
    /// </summary>
    public async Task<MachineStatusDto?> GetMachineStatusAsync(int machineNo)
    {
        try
        {
            var sql = $@"
                SELECT
                    [MACHINE_NO], [MODEL_NO], [READY_STATUS], [RUN_STATUS],
                    [WAIT_STATUS], [WAIT_NO], [ARRANGE_STATUS], [STOP_STATUS],
                    [ERR_STATUS], [ERR_NO], [INTO_SUM], [PRODUCT_SUM], [TARGET_NUM],
                    [INTO_NUM], [PRODUCT_NUM], [OUT_NUM], [UPDATE_TIME], [NOTE],
                    [LOT_NO], [KISHU_NAME], [PRODUCT_KBN], [COIL_NUM]
                FROM {_linkedServerQuery}.[D_STATUS]
                WHERE [MACHINE_NO] = {{0}}";

            var status = await _dbContext.DStatus
                .FromSqlRaw(sql, machineNo)
                .FirstOrDefaultAsync();

            if (status == null)
            {
                _logger.LogWarning("装置番号 {MachineNo} のデータが見つかりません", machineNo);
                return null;
            }

            return MapToDto(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "装置状態取得エラー (MachineNo={MachineNo})", machineNo);
            return null;
        }
    }

    /// <summary>
    /// D_STATUSエンティティをDTOにマッピング
    /// </summary>
    private MachineStatusDto MapToDto(DStatus status)
    {
        var dto = new MachineStatusDto
        {
            MachineNo = status.MachineNo,
            ProductNum = status.ProductNum,
            ReadyStatus = status.ReadyStatus,
            RunStatus = status.RunStatus,
            WaitStatus = status.WaitStatus,
            StopStatus = status.StopStatus,
            ErrStatus = status.ErrStatus
        };

        // ERR名取得
        if (status.ErrStatus == true && status.ErrNo.HasValue && status.ErrNo.Value != 0)
        {
            if (_masterCache.ErrMaster.TryGetValue(
                (status.MachineNo, status.ErrNo.Value),
                out var errName))
            {
                dto.ErrName = errName;
                _logger.LogDebug(
                    "エラー名取得: Machine={MachineNo}, ErrNo={ErrNo}, Name={ErrName}",
                    status.MachineNo, status.ErrNo, errName);
            }
            else
            {
                _logger.LogWarning(
                    "エラーマスターにデータなし: Machine={MachineNo}, ErrNo={ErrNo}",
                    status.MachineNo, status.ErrNo);
            }
        }

        // WAIT名取得
        if (status.WaitStatus == true && status.WaitNo.HasValue && status.WaitNo.Value != 0)
        {
            if (_masterCache.WaitMaster.TryGetValue(
                (status.MachineNo, status.WaitNo.Value),
                out var waitName))
            {
                dto.WaitName = waitName;
                _logger.LogDebug(
                    "待機名取得: Machine={MachineNo}, WaitNo={WaitNo}, Name={WaitName}",
                    status.MachineNo, status.WaitNo, waitName);
            }
            else
            {
                _logger.LogWarning(
                    "待機マスターにデータなし: Machine={MachineNo}, WaitNo={WaitNo}",
                    status.MachineNo, status.WaitNo);
            }
        }

        // STOP名取得（将来の拡張用）
        if (status.StopStatus == true && status.ErrNo.HasValue && status.ErrNo.Value != 0)
        {
            if (_masterCache.StopMaster.TryGetValue(
                (status.MachineNo, status.ErrNo.Value),
                out var stopName))
            {
                dto.StopName = stopName;
            }
        }

        // StatusMessage組み立て
        dto.StatusMessage = BuildStatusMessage(dto);

        return dto;
    }

    /// <summary>
    /// 表示用状態メッセージを組み立て
    /// </summary>
    private string BuildStatusMessage(MachineStatusDto dto)
    {
        // 優先順位: エラー > 待機 > 稼働 > 準備
        if (!string.IsNullOrEmpty(dto.ErrName))
            return dto.ErrName;

        if (!string.IsNullOrEmpty(dto.WaitName))
            return dto.WaitName;

        if (dto.RunStatus == true)
            return "稼働中";

        if (dto.ReadyStatus == true)
            return "準備";

        if (dto.StopStatus == true)
            return "停止";

        return "--";
    }
}
