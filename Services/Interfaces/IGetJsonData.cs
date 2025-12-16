namespace _12_fukotai.Services.Interfaces;

using _12_fukotai.Models;

/// <summary>
/// JSONデータ読み取りサービスのインターフェース
/// </summary>
public interface IGetJsonData
{
    /// <summary>
    /// JSONファイルからPLCデータを非同期で読み込む
    /// </summary>
    /// <returns>
    /// 成功時: PlcDataオブジェクト
    /// 失敗時: null
    /// </returns>
    Task<PlcData?> LoadAsync();

    /// <summary>
    /// 最後のエラーメッセージを取得
    /// </summary>
    string? LastError { get; }

    /// <summary>
    /// 最後に読み込みに成功した日時を取得
    /// </summary>
    DateTime? LastSuccessTime { get; }
}
