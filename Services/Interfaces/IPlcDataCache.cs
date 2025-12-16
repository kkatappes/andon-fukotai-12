namespace _12_fukotai.Services.Interfaces;

using _12_fukotai.Models;

/// <summary>
/// PLCデータキャッシュサービスのインターフェース
/// </summary>
public interface IPlcDataCache
{
    /// <summary>
    /// 現在のPLCデータ（スレッドセーフ）
    /// </summary>
    PlcData? CurrentData { get; }

    /// <summary>
    /// 通信状態（60秒以内に成功 = true）
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 最終更新日時
    /// </summary>
    DateTime? LastUpdateTime { get; }

    /// <summary>
    /// 連続失敗回数
    /// </summary>
    int ConsecutiveFailures { get; }

    /// <summary>
    /// 最終エラーメッセージ
    /// </summary>
    string? LastError { get; }

    /// <summary>
    /// デバイス番号で値を取得
    /// </summary>
    /// <param name="deviceNumber">デバイス番号（例：D8924）</param>
    /// <returns>見つかった場合は値、見つからない場合はnull</returns>
    int? GetValueByDeviceNumber(string deviceNumber);
}
