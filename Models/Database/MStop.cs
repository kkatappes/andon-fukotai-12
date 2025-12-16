namespace _12_fukotai.Models.Database;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// M_STOPテーブル（停止マスター）のモデル
/// </summary>
[Table("M_STOP")]
public class MStop
{
    /// <summary>
    /// 有効フラグ
    /// </summary>
    [Column("ENABLE_FLG")]
    public bool? EnableFlg { get; set; }

    /// <summary>
    /// 装置番号（複合主キー）
    /// </summary>
    [Key]
    [Column("MACHINE_NO", Order = 0)]
    public byte MachineNo { get; set; }

    /// <summary>
    /// 停止番号（複合主キー）
    /// </summary>
    [Key]
    [Column("STOP_NO", Order = 1)]
    public short StopNo { get; set; }

    /// <summary>
    /// 停止名
    /// </summary>
    [Column("STOP_NAME")]
    public string StopName { get; set; } = string.Empty;

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
}
