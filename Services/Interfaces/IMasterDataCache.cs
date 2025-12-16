namespace _12_fukotai.Services.Interfaces;

/// <summary>
/// マスターデータキャッシュサービスのインターフェース
/// </summary>
public interface IMasterDataCache
{
    /// <summary>
    /// エラーマスターデータ
    /// Key: (MachineNo, ErrNo), Value: ErrName
    /// </summary>
    Dictionary<(byte MachineNo, short ErrNo), string?> ErrMaster { get; }

    /// <summary>
    /// 待機マスターデータ
    /// Key: (MachineNo, WaitNo), Value: WaitName
    /// </summary>
    Dictionary<(byte MachineNo, short WaitNo), string?> WaitMaster { get; }

    /// <summary>
    /// 停止マスターデータ
    /// Key: (MachineNo, StopNo), Value: StopName
    /// </summary>
    Dictionary<(byte MachineNo, short StopNo), string?> StopMaster { get; }

    /// <summary>
    /// マスターデータを再読み込み
    /// </summary>
    Task RefreshAsync();
}
