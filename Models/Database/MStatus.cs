namespace _12_fukotai.Models.Database;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// M_STATUSテーブル（状態マスター）のモデル
/// </summary>
[Table("M_STATUS")]
public class MStatus
{
    /// <summary>
    /// 状態番号（主キー）
    /// </summary>
    [Key]
    [Column("STATUS_NO")]
    public short StatusNo { get; set; }

    /// <summary>
    /// 状態名
    /// </summary>
    [Column("STATUS_NAME")]
    public string StatusName { get; set; } = string.Empty;
}
