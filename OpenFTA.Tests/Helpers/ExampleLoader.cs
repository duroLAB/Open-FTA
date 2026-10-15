using System.Runtime.CompilerServices;

namespace OpenFTA.Tests.Helpers;

/// <summary>
/// Loads .fta files from the application's Examples folder.
/// Mirrors the logic of MainForm.LoadFile (without the UI parts).
/// </summary>
public static class ExampleLoader
{
    public static string ExamplesDirectory { get; } = FindExamplesDirectory();

    public static FTAlogic Load(string fileName)
    {
        string path = Path.Combine(ExamplesDirectory, fileName);
        SaveTreeClass data = SaveTreeClass.LoadFromFile(path);

        var logic = new FTAlogic();
        logic.FTAStructure = data.FtaStructure;

        FTAitem? root = logic.FTAStructure.Values.FirstOrDefault(i => i.Parent == Guid.Empty);
        if (root == null)
        {
            root = logic.FTAStructure.Values.First();
            root.Parent = Guid.Empty;
        }

        logic.TopEventGuid = root.GuidCode;
        logic.FindAllChilren();
        logic.AssignLevelsToAllEvents();
        return logic;
    }

    private static string FindExamplesDirectory([CallerFilePath] string thisFile = "")
    {
        // <repo>/OpenFTA.Tests/Helpers/ExampleLoader.cs -> <repo>/Open FTA/Examples
        string repoRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", ".."));
        return Path.Combine(repoRoot, "Open FTA", "Examples");
    }
}
