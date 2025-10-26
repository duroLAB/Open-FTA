using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class SaveTreeClass
{
    // Define the current expected application build version.
    // You can update this when your app structure changes.
    public static readonly string CurrentBuildVersion = "2.5.3";

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
        if (!IsVersionCompatible(data.BuildVersion))
        {
            Console.WriteLine($"⚠ Warning: The loaded file was created with build version {data.BuildVersion}, " +
                              $"which may not be fully compatible with current build {CurrentBuildVersion}.");
        }

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

        if (!IsVersionCompatible(data.BuildVersion))
        {
            Console.WriteLine($"⚠ Warning: The loaded file was created with build version {data.BuildVersion}, " +
                              $"which may not be fully compatible with current build {CurrentBuildVersion}.");
        }

        return data;
    }

    /// <summary>
    /// Checks whether the given version is compatible with the current application version.
    /// </summary>
    private static bool IsVersionCompatible(string version)
    {
        // You can make this check stricter if needed.
        // Currently, it only checks the major version part.
        try
        {
            Version current = new Version(CurrentBuildVersion);
            Version other = new Version(version);

            return current.Major == other.Major;
        }
        catch
        {
            // If version format is invalid, treat as incompatible
            return false;
        }
    }
}
