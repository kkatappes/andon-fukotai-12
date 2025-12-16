namespace _12_fukotai.Services.Database;

using Microsoft.EntityFrameworkCore;
using _12_fukotai.Models.Database;

/// <summary>
/// アンドンシステムのデータベースコンテキスト
/// </summary>
public class AndonDbContext : DbContext
{
    /// <summary>
    /// D_STATUSテーブル（装置状態）
    /// </summary>
    public DbSet<DStatus> DStatus { get; set; } = null!;

    /// <summary>
    /// M_STATUSテーブル（状態マスター）
    /// </summary>
    public DbSet<MStatus> MStatus { get; set; } = null!;

    /// <summary>
    /// M_ERRテーブル（エラーマスター）
    /// </summary>
    public DbSet<MErr> MErr { get; set; } = null!;

    /// <summary>
    /// M_STOPテーブル（停止マスター）
    /// </summary>
    public DbSet<MStop> MStop { get; set; } = null!;

    /// <summary>
    /// M_WAITテーブル（待機マスター）
    /// </summary>
    public DbSet<MWait> MWait { get; set; } = null!;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">DbContextのオプション</param>
    public AndonDbContext(DbContextOptions<AndonDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// モデル構築時の設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // D_STATUS: テーブル名とスキーマ設定
        modelBuilder.Entity<DStatus>()
            .ToTable("D_STATUS", "dbo")
            .HasKey(d => d.MachineNo);

        // M_STATUS: テーブル名とスキーマ設定
        modelBuilder.Entity<MStatus>()
            .ToTable("M_STATUS", "dbo")
            .HasKey(m => m.StatusNo);

        // M_ERR: 複合主キー設定
        modelBuilder.Entity<MErr>()
            .ToTable("M_ERR", "dbo")
            .HasKey(e => new { e.MachineNo, e.ErrNo });

        // M_STOP: 複合主キー設定
        modelBuilder.Entity<MStop>()
            .ToTable("M_STOP", "dbo")
            .HasKey(s => new { s.MachineNo, s.StopNo });

        // M_WAIT: 複合主キー設定
        modelBuilder.Entity<MWait>()
            .ToTable("M_WAIT", "dbo")
            .HasKey(w => new { w.MachineNo, w.WaitNo });
    }
}
