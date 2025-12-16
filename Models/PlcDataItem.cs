namespace _12_fukotai.Models;

/// <summary>
/// PLCデバイスデータ項目を表すモデル
/// </summary>
public class PlcDataItem
{
    /// <summary>
    /// デバイス情報
    /// </summary>
    public DeviceInfo Device { get; set; } = new();

    /// <summary>
    /// 桁数（1以上）
    /// </summary>
    public int Digits { get; set; }

    /// <summary>
    /// データ単位（bit, word, dword）
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// データ値（JsonElementで動的型を扱う）
    /// </summary>
    public System.Text.Json.JsonElement Value { get; set; }
}

/// <summary>
/// デバイス番号情報
/// </summary>
public class DeviceInfo
{
    /// <summary>
    /// デバイスコード（例: D, M）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// デバイス番号（例: 8924, 100）
    /// </summary>
    public string Number { get; set; } = string.Empty;
}
