namespace _12_fukotai.Models.Database;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// M_ERRテーブル（エラーマスター）のモデル
/// </summary>
[Table("M_ERR")]
public class MErr
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
    /// エラー番号（複合主キー）
    /// </summary>
    [Key]
    [Column("ERR_NO", Order = 1)]
    public short ErrNo { get; set; }

    /// <summary>
    /// エラー名
    /// </summary>
    [Column("ERR_NAME")]
    public string? ErrName { get; set; }

    /// <summary>
    /// エラー種別
    /// </summary>
    [Column("ERR_TYPE")]
    public byte? ErrType { get; set; }

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
