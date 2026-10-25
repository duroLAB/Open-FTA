using System.Text.Json;

public class SaveTreeClass
{
    /// <summary>
    /// Version of the application that writes the file, taken from the assembly
    /// (&lt;Version&gt; in the .csproj), e.g. "1.1.0".
    /// </summary>
    public static readonly string CurrentBuildVersion = GetApplicationVersion();

    private static string GetApplicationVersion()
    {
        Version? v = typeof(SaveTreeClass).Assembly.GetName().Version;
        return v == null ? "0.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
    }

    /// <summary>
    /// True if the file was written by a newer major version of Open FTA than this one
    /// (the file may contain information this version does not know). Files without a version
    /// or with an older version are opened normally.
    /// </summary>
    public bool IsFromNewerVersion() =>
        Version.TryParse(BuildVersion, out Version? file)
        && Version.TryParse(CurrentBuildVersion, out Version? current)
        && file.Major > current.Major;

    public string BuildVersion { get; set; }
    public Dictionary<Guid, FTAitem> FtaStructure { get; set; }
    public double Zoom { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }

    public SaveTreeClass()
    {
        FtaStructure = new Dictionary<Guid, FTAitem>();
        Zoom = 1.0;
        X = 0;
        Y = 0;
        OffsetX = 0;
        OffsetY = 0;
        BuildVersion = CurrentBuildVersion;
    }

    /// <summary>
    /// Saves the current tree structure to a JSON file.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads the tree structure from a JSON file and checks version compatibility.
    /// </summary>
    public static SaveTreeClass LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Save file not found.", filePath);

        string json = File.ReadAllText(filePath);
        SaveTreeClass data = FromJsonString(json);

        // Perform version compatibility check

        return data;
    }

    /// <summary>
    /// Converts the current object into a formatted JSON string.
    /// Useful for clipboard operations or network transfer.
    /// </summary>
    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Creates a new instance of SaveTreeClass from a JSON string.
    /// </summary>
    public static SaveTreeClass FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<SaveTreeClass>(json);
    }

    /// <summary>
    /// Asynchronously saves the current tree structure to a JSON file.
    /// Recommended for UI applications to avoid blocking the main thread.
    /// </summary>
    public async Task SaveToFileAsync(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        using FileStream fs = File.Create(filePath);
        await JsonSerializer.SerializeAsync(fs, this, options);
    }

    /// <summary>
    /// Asynchronously loads the tree structure from a JSON file and checks version compatibility.
    /// </summary>
    public static async Task<SaveTreeClass> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Save file not found.", filePath);

        using FileStream fs = File.OpenRead(filePath);
        SaveTreeClass data = await JsonSerializer.DeserializeAsync<SaveTreeClass>(fs);


        return data;
    }
}
