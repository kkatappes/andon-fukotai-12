namespace _12_fukotai.Services.Interfaces;

using _12_fukotai.Models;

/// <summary>
/// アンドンデータ取得サービスのインターフェース
/// </summary>
public interface IAndonDataService
{
    /// <summary>
    /// 全装置の状態リストを取得
    /// </summary>
    /// <returns>装置状態のリスト</returns>
    Task<List<MachineStatusDto>> GetMachineStatusListAsync();

    /// <summary>
    /// 指定した装置番号の状態を取得
    /// </summary>
    /// <param name="machineNo">装置番号</param>
    /// <returns>装置状態（見つからない場合はnull）</returns>
    Task<MachineStatusDto?> GetMachineStatusAsync(int machineNo);
}
