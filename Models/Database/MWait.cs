namespace _12_fukotai.Models.Database;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// M_WAITテーブル（待機マスター）のモデル
/// </summary>
[Table("M_WAIT")]
public class MWait
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
    /// 待機番号（複合主キー）
    /// </summary>
    [Key]
    [Column("WAIT_NO", Order = 1)]
    public short WaitNo { get; set; }

    /// <summary>
    /// 待機名
    /// </summary>
    [Column("WAIT_NAME")]
    public string? WaitName { get; set; }

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
