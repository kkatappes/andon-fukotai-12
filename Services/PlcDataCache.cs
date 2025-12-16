namespace _12_fukotai.Services;

using System.Text.Json;
using System.Threading;
using _12_fukotai.Models;
using _12_fukotai.Services.Interfaces;

/// <summary>
/// PLCデータキャッシュサービス（Singleton）
/// </summary>
public class PlcDataCache : IPlcDataCache
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<PlcDataCache> _logger;

    private PlcData? _currentData;
    private DateTime? _lastUpdateTime;
    private int _consecutiveFailures;
    private string? _lastError;


    public PlcData? CurrentData
    {
        get
        {
            _lock.Wait();
            try
            {
                return _currentData;
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    public bool IsConnected =>
        _lastUpdateTime.HasValue &&
        (DateTime.Now - _lastUpdateTime.Value).TotalSeconds <= 60;

    public DateTime? LastUpdateTime => _lastUpdateTime;
    public int ConsecutiveFailures => _consecutiveFailures;
    public string? LastError => _lastError;

    public PlcDataCache(ILogger<PlcDataCache> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// データを更新（IHostedService から呼び出し）
    /// </summary>
    internal async Task UpdateAsync(IGetJsonData jsonReader)
    {
        var data = await jsonReader.LoadAsync();

        await _lock.WaitAsync();
        try
        {
            if (data != null)
            {
                _currentData = data;
                _lastUpdateTime = DateTime.Now;
                _consecutiveFailures = 0;
                _lastError = null;

                _logger.LogDebug(
                    "PlcDataCache updated successfully. Items: {Count}",
                    data.Items.Count);
            }
            else
            {
                _consecutiveFailures++;
                _lastError = jsonReader.LastError ?? "Unknown error";

                _logger.LogWarning(
                    "PlcDataCache update failed ({Count} consecutive). Error: {Error}",
                    _consecutiveFailures,
                    _lastError);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// デバイス番号で値を取得
    /// </summary>
    public int? GetValueByDeviceNumber(string deviceNumber)
    {
        var data = CurrentData;
        if (data == null) return null;

        // デバイス番号で検索（Code + Number を結合して比較）
        var item = data.Items.FirstOrDefault(
            i => (i.Device.Code + i.Device.Number) == deviceNumber);

        if (item == null) return null;

        try
        {
            // JsonElementから整数値を取得
            if (item.Value.ValueKind == JsonValueKind.Number)
            {
                return item.Value.GetInt32();
            }

            // 文字列として数値が格納されている場合も対応
            if (item.Value.ValueKind == JsonValueKind.String)
            {
                var strValue = item.Value.GetString();
                if (int.TryParse(strValue, out int result))
                {
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to get value for device {Device}",
                deviceNumber);
        }

        return null;
    }
}
