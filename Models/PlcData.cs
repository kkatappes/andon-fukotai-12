namespace _12_fukotai.Models;

/// <summary>
/// JSONファイルのルート構造を表すモデル
/// </summary>
public class PlcData
{
    /// <summary>
    /// PLCデータ項目のリスト
    /// </summary>
    public List<PlcDataItem> Items { get; set; } = new();
}
