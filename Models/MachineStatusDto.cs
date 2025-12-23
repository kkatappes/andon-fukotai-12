namespace _12_fukotai.Models;

/// <summary>
/// 装置状態表示用DTOモデル
/// </summary>
public class MachineStatusDto
{
    /// <summary>
    /// 装置番号
    /// </summary>
    public byte MachineNo { get; set; }

    /// <summary>
    /// 生産数
    /// </summary>
    public int? ProductNum { get; set; }

    /// <summary>
    /// 状態メッセージ（画面に表示する異常名・待機名など）
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    // 状態フラグ

    /// <summary>
    /// 準備状態フラグ
    /// </summary>
    public bool? ReadyStatus { get; set; }

    /// <summary>
    /// 稼働状態フラグ
    /// </summary>
    public bool? RunStatus { get; set; }

    /// <summary>
    /// 待機状態フラグ
    /// </summary>
    public bool? WaitStatus { get; set; }

    /// <summary>
    /// 段取り状態フラグ
    /// </summary>
    public bool? ArrangeStatus { get; set; }

    /// <summary>
    /// 停止状態フラグ
    /// </summary>
    public bool? StopStatus { get; set; }

    /// <summary>
    /// 異常状態フラグ
    /// </summary>
    public bool? ErrStatus { get; set; }

    // マスターとJOINした結果

    /// <summary>
    /// エラー名（M_ERRから取得）
    /// </summary>
    public string? ErrName { get; set; }

    /// <summary>
    /// 待機名（M_WAITから取得）
    /// </summary>
    public string? WaitName { get; set; }

    /// <summary>
    /// 停止名（M_STOPから取得）
    /// </summary>
    public string? StopName { get; set; }

    /// <summary>
    /// 材料アラーム状態フラグ（JSONのXデバイスから取得）
    /// </summary>
    public bool MaterialAlarm { get; set; }

    /// <summary>
    /// 材料アラーム名（JSONのXデバイスから取得）
    /// </summary>
    public string? MaterialAlarmName { get; set; }
}
