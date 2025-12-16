namespace _12_fukotai.Models.Database;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// D_STATUSテーブル（装置状態）のモデル
/// </summary>
[Table("D_STATUS")]
public class DStatus
{
    /// <summary>
    /// 装置番号（主キー）
    /// </summary>
    [Key]
    [Column("MACHINE_NO")]
    public byte MachineNo { get; set; }

    /// <summary>
    /// モデル番号
    /// </summary>
    [Column("MODEL_NO")]
    public short? ModelNo { get; set; }

    /// <summary>
    /// 準備状態フラグ
    /// </summary>
    [Column("READY_STATUS")]
    public bool? ReadyStatus { get; set; }

    /// <summary>
    /// 稼働状態フラグ
    /// </summary>
    [Column("RUN_STATUS")]
    public bool? RunStatus { get; set; }

    /// <summary>
    /// 待機状態フラグ
    /// </summary>
    [Column("WAIT_STATUS")]
    public bool? WaitStatus { get; set; }

    /// <summary>
    /// 待機番号
    /// </summary>
    [Column("WAIT_NO")]
    public short? WaitNo { get; set; }

    /// <summary>
    /// 段取状態フラグ
    /// </summary>
    [Column("ARRANGE_STATUS")]
    public bool? ArrangeStatus { get; set; }

    /// <summary>
    /// 停止状態フラグ
    /// </summary>
    [Column("STOP_STATUS")]
    public bool? StopStatus { get; set; }

    /// <summary>
    /// 異常状態フラグ
    /// </summary>
    [Column("ERR_STATUS")]
    public bool? ErrStatus { get; set; }

    /// <summary>
    /// 異常番号
    /// </summary>
    [Column("ERR_NO")]
    public short? ErrNo { get; set; }

    /// <summary>
    /// 投入合計
    /// </summary>
    [Column("INTO_SUM")]
    public int? IntoSum { get; set; }

    /// <summary>
    /// 生産合計
    /// </summary>
    [Column("PRODUCT_SUM")]
    public int? ProductSum { get; set; }

    /// <summary>
    /// 目標数
    /// </summary>
    [Column("TARGET_NUM")]
    public int? TargetNum { get; set; }

    /// <summary>
    /// 投入数
    /// </summary>
    [Column("INTO_NUM")]
    public int? IntoNum { get; set; }

    /// <summary>
    /// 生産数
    /// </summary>
    [Column("PRODUCT_NUM")]
    public int? ProductNum { get; set; }

    /// <summary>
    /// 排出数
    /// </summary>
    [Column("OUT_NUM")]
    public int? OutNum { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    [Column("UPDATE_TIME")]
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 備考
    /// </summary>
    [Column("NOTE")]
    public string? Note { get; set; }

    /// <summary>
    /// ロット番号
    /// </summary>
    [Column("LOT_NO")]
    public string? LotNo { get; set; }

    /// <summary>
    /// 機種名
    /// </summary>
    [Column("KISHU_NAME")]
    public string? KishuName { get; set; }

    /// <summary>
    /// 生産区分
    /// </summary>
    [Column("PRODUCT_KBN")]
    public string? ProductKbn { get; set; }

    /// <summary>
    /// コイル番号
    /// </summary>
    [Column("COIL_NUM")]
    public long? CoilNum { get; set; }
}
