using System.Text.Json;

namespace OpenFTA.Tests;

/// <summary>Application version stored in .fta files (B10).</summary>
public class SaveTreeClassTests
{
    [Fact]
    public void Current_version_is_the_application_version()
    {
        Version v = typeof(SaveTreeClass).Assembly.GetName().Version!;
        Assert.Equal($"{v.Major}.{v.Minor}.{v.Build}", SaveTreeClass.CurrentBuildVersion);
    }

    [Fact]
    public void Saved_file_contains_the_current_version()
    {
        string path = Path.Combine(Path.GetTempPath(), $"openfta-save-{Guid.NewGuid():N}.fta");
        try
        {
            new SaveTreeClass().SaveToFile(path);

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(SaveTreeClass.CurrentBuildVersion, doc.RootElement.GetProperty("BuildVersion").GetString());
            Assert.Equal(SaveTreeClass.CurrentBuildVersion, SaveTreeClass.LoadFromFile(path).BuildVersion);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("99.0.0", true)]      // newer major version
    [InlineData("1.0.1.0", false)]    // written by 1.0.x (old hard-coded value)
    [InlineData("0.9", false)]
    [InlineData("not a version", false)]
    [InlineData(null, false)]         // very old files without a version
    public void File_from_a_newer_major_version_is_detected(string? fileVersion, bool expected)
    {
        var data = new SaveTreeClass { BuildVersion = fileVersion! };
        Assert.Equal(expected, data.IsFromNewerVersion());
    }

    [Fact]
    public void File_from_the_current_version_is_not_newer()
    {
        Assert.False(new SaveTreeClass().IsFromNewerVersion());
    }
}
