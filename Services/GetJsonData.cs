namespace _12_fukotai.Services;

using System.Text.Json;
using _12_fukotai.Models;
using _12_fukotai.Services.Interfaces;

/// <summary>
/// JSONファイルからPLCデータを読み込むサービス
/// </summary>
public class GetJsonData : IGetJsonData
{
    private readonly List<string> _jsonFilePatterns;
    private readonly string _contentRootPath;
    private readonly ILogger<GetJsonData> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string? LastError { get; private set; }
    public DateTime? LastSuccessTime { get; private set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="configuration">設定情報</param>
    /// <param name="env">ホスティング環境情報</param>
    /// <param name="logger">ロガー</param>
    public GetJsonData(
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<GetJsonData> logger)
    {
        _logger = logger;
        _contentRootPath = env.ContentRootPath;

        // appsettings.jsonから複数のファイルパターンを取得
        _jsonFilePatterns = configuration.GetSection("PlcData:JsonFilePath")
            .Get<List<string>>() ?? new List<string> { "src/test_trans.json" };

        // JSON設定
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        _logger.LogInformation(
            "GetJsonData initialized. File patterns: {Patterns}",
            string.Join(", ", _jsonFilePatterns));
    }

    /// <summary>
    /// JSONファイルを非同期で読み込む
    /// </summary>
    public async Task<PlcData?> LoadAsync()
    {
        try
        {
            var mergedData = new PlcData();
            var loadedFilesCount = 0;
            var errors = new List<string>();

            // セットごとの読み込み成功数を追跡
            // セット1: 末尾2,9 / セット2: 末尾98,100
            var set1LoadedCount = 0; // 末尾2,9のセット
            var set2LoadedCount = 0; // 末尾98,100のセット

            // 各パターンに対して処理
            foreach (var pattern in _jsonFilePatterns)
            {
                // 絶対パスに解決
                var fullPattern = Path.Combine(_contentRootPath, pattern);
                var directory = Path.GetDirectoryName(fullPattern) ?? _contentRootPath;
                var filePattern = Path.GetFileName(fullPattern);

                // ディレクトリ存在チェック
                if (!Directory.Exists(directory))
                {
                    var error = $"Directory not found: {directory}";
                    errors.Add(error);
                    _logger.LogDebug(error);
                    continue;  // 次のパターンへ
                }

                // パターンに一致するファイルを検索
                var matchingFiles = Directory.GetFiles(directory, filePattern);

                if (matchingFiles.Length == 0)
                {
                    var error = $"No files found for pattern: {pattern}";
                    errors.Add(error);
                    _logger.LogDebug(error);
                    continue;  // 次のパターンへ
                }

                // 各パターンで最初のファイルのみを使用（通常は1ファイル）
                var filePath = matchingFiles[0];

                try
                {
                    // ファイル読み込み
                    await using var fileStream = File.OpenRead(filePath);

                    // JSONデシリアライズ
                    var plcData = await JsonSerializer.DeserializeAsync<PlcData>(
                        fileStream,
                        _jsonOptions);

                    if (plcData == null)
                    {
                        var error = $"Deserialization returned null for file: {filePath}";
                        errors.Add(error);
                        _logger.LogDebug(error);
                        continue;  // 次のパターンへ
                    }

                    // データをマージ
                    mergedData.Items.AddRange(plcData.Items);
                    loadedFilesCount++;

                    // どのセットに属するかを判定
                    if (pattern.Contains("-2_") || pattern.Contains("-9_"))
                    {
                        set1LoadedCount++;
                    }
                    else if (pattern.Contains("-98_") || pattern.Contains("-100_"))
                    {
                        set2LoadedCount++;
                    }

                    _logger.LogDebug(
                        "Loaded {Count} items from {File}",
                        plcData.Items.Count,
                        Path.GetFileName(filePath));
                }
                catch (JsonException ex)
                {
                    var error = $"JSON parsing error in {Path.GetFileName(filePath)}: {ex.Message}";
                    errors.Add(error);
                    _logger.LogError(ex, "Failed to parse JSON file: {File}", filePath);
                    continue;  // 次のパターンへ
                }
                catch (IOException ex)
                {
                    var error = $"File I/O error in {Path.GetFileName(filePath)}: {ex.Message}";
                    errors.Add(error);
                    _logger.LogError(ex, "Failed to read JSON file: {File}", filePath);
                    continue;  // 次のパターンへ
                }
            }

            // 少なくとも1つのセットが読み込めたか確認
            if (loadedFilesCount == 0)
            {
                LastError = $"No files were loaded successfully. Errors: {string.Join(", ", errors)}";
                _logger.LogWarning(LastError);
                return null;
            }

            // どちらのセットも1つもファイルが読み込めなかった場合のみ警告
            // （片方のセットが読み込まれていれば成功とみなす）
            if (set1LoadedCount == 0 && set2LoadedCount == 0)
            {
                // どちらのセットにも属さないファイルのみが読み込まれた場合
                // （通常は発生しないが念のため）
                _logger.LogDebug(
                    "Files were loaded but none belong to the expected sets (2,9 or 98,100)");
            }

            // 成功
            LastSuccessTime = DateTime.Now;
            LastError = null;

            _logger.LogInformation(
                "Successfully loaded {TotalItems} PLC data items from {FileCount} files (Set1: {Set1Count}, Set2: {Set2Count})",
                mergedData.Items.Count,
                loadedFilesCount,
                set1LoadedCount,
                set2LoadedCount);

            return mergedData;
        }
        catch (Exception ex)
        {
            LastError = $"Unexpected error: {ex.Message}";
            _logger.LogError(ex, "Unexpected error while loading JSON files");
            return null;
        }
    }
}
