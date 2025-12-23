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
    private readonly IPlcDataCache _plcCache;
    private readonly ILogger<AndonDataService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _linkedServerQuery;

    public AndonDataService(
        AndonDbContext dbContext,
        IMasterDataCache masterCache,
        IPlcDataCache plcCache,
        ILogger<AndonDataService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _masterCache = masterCache;
        _plcCache = plcCache;
        _logger = logger;
        _configuration = configuration;

        // UseLinkedServerフラグを読み取り
        var useLinkedServer = _configuration.GetValue<bool>("DatabaseConnectionSettings:UseLinkedServer");

        if (useLinkedServer)
        {
            // リンクサーバーモード: [LinkedServer].[Database].[dbo]
            var useDatabaseName = _configuration.GetValue<string>("AndonSettings:UseDatabaseName") ?? "AndonDatabase_himeji1";
            var prefix = useDatabaseName == "AndonDatabase_himeji2" ? "Himeji2" : "Himeji1";

            var linkedServer = _configuration.GetValue<string>($"LinkedServerSettings:{prefix}_LinkedServerName") ?? "10.60.40.14";
            var database = _configuration.GetValue<string>($"LinkedServerSettings:{prefix}_DatabaseName") ?? "KadouMoni3_144";

            _linkedServerQuery = $"[{linkedServer}].[{database}].[dbo]";
            _logger.LogInformation("リンクサーバーモード: {Query} (UseDatabaseName: {UseDatabaseName})", _linkedServerQuery, useDatabaseName);
        }
        else
        {
            // 直接接続モード: [dbo] のみ
            _linkedServerQuery = "[dbo]";
            _logger.LogInformation("直接接続モード: {Query}", _linkedServerQuery);
        }
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

            var jstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            _logger.LogInformation("[{JstTime:yyyy-MM-dd HH:mm:ss}] D_STATUSデータ取得: {Count}件", jstTime, statusList.Count);

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
            ArrangeStatus = status.ArrangeStatus,
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

        // 材料アラームチェック（JSONのXデバイスから取得）
        CheckMaterialAlarm(dto);

        // StatusMessage組み立て
        dto.StatusMessage = BuildStatusMessage(dto);

        return dto;
    }

    /// <summary>
    /// 材料アラームをチェック
    /// </summary>
    private void CheckMaterialAlarm(MachineStatusDto dto)
    {
        // 装置番号とXデバイスの対応マッピング
        var materialAlarmMapping = new Dictionary<byte, (string DeviceNumber, string AlarmName)>
        {
            { 1, ("X576", "完成品トレー不足") },  // FAW-A
            { 2, ("X376", "完成品トレー不足") },  // FAW-B
            { 3, ("X2AC", "完成品トレー不足") },  // FAW-CD
            { 4, ("X24D", "完成品トレー不足") },  // FAW-E
            { 5, ("X1BA", "完成品トレー満杯") }   // HMA
        };

        if (materialAlarmMapping.TryGetValue(dto.MachineNo, out var mapping))
        {
            var deviceValue = _plcCache.GetValueByDeviceNumber(mapping.DeviceNumber);

            if (deviceValue == 1)
            {
                dto.MaterialAlarm = true;
                dto.MaterialAlarmName = mapping.AlarmName;

                _logger.LogDebug(
                    "材料アラーム検出: Machine={MachineNo}, Device={Device}, Name={Name}",
                    dto.MachineNo, mapping.DeviceNumber, mapping.AlarmName);
            }
            else
            {
                dto.MaterialAlarm = false;
                dto.MaterialAlarmName = null;
            }
        }
        else
        {
            dto.MaterialAlarm = false;
            dto.MaterialAlarmName = null;
        }
    }

    /// <summary>
    /// 表示用状態メッセージを組み立て
    /// </summary>
    private string BuildStatusMessage(MachineStatusDto dto)
    {
        // 全ステータスが0の場合はERR_STATUSとして扱う
        bool allStatusZero = dto.ReadyStatus != true
                          && dto.RunStatus != true
                          && dto.WaitStatus != true
                          && dto.ArrangeStatus != true
                          && dto.StopStatus != true
                          && dto.ErrStatus != true;

        if (allStatusZero)
        {
            // 全て0の場合はエラーとして扱う
            return !string.IsNullOrEmpty(dto.ErrName) ? dto.ErrName : "異常";
        }

        // 優先順位: ERR_STATUS > STOP_STATUS > 材料アラーム > ARRANGE_STATUS > WAIT_STATUS = RUN_STATUS = READY_STATUS
        if (dto.ErrStatus == true)
            return !string.IsNullOrEmpty(dto.ErrName) ? dto.ErrName : "異常";

        if (dto.StopStatus == true)
            return "停止中";

        // 材料アラーム
        if (dto.MaterialAlarm)
            return !string.IsNullOrEmpty(dto.MaterialAlarmName) ? dto.MaterialAlarmName : "材料アラーム";

        if (dto.ArrangeStatus == true)
            return "段取り中";

        if (dto.WaitStatus == true)
            return !string.IsNullOrEmpty(dto.WaitName) ? dto.WaitName : "待機中";

        if (dto.RunStatus == true)
            return "稼働中";

        if (dto.ReadyStatus == true)
            return "準備中";

        return "--";
    }
}
