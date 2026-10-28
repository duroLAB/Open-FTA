using Microsoft.Data.Sqlite;

namespace OpenFTA.Tests;

/// <summary>The user's database is a copy of the shipped one and is never overwritten (B11).</summary>
public class DBEngineTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"openfta-db-{Guid.NewGuid():N}");
    private string Template => Path.Combine(_dir, "app folder", "MainAppDB.db");
    private string User => Path.Combine(_dir, "AppData", "OpenFTA", "MainAppDB.db");

    public DBEngineTests() => Directory.CreateDirectory(Path.GetDirectoryName(Template)!);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();   // release the file handles before deleting
        try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
    }

    private static string? Title(DBEngine db, string id) =>
        db.GetReferenceById(id, out string title, out _, out _, out _) ? title : null;

    [Fact]
    public void First_start_copies_the_shipped_database()
    {
        new DBEngine(Template).InsertReference("ref1", "Shipped reference");

        string path = DBEngine.PrepareUserDatabase(User, Template);

        Assert.Equal(User, path);
        Assert.Equal("Shipped reference", Title(new DBEngine(path), "ref1"));
    }

    [Fact]
    public void Existing_user_database_is_not_overwritten()
    {
        new DBEngine(Template).InsertReference("ref1", "Shipped reference");
        new DBEngine(DBEngine.PrepareUserDatabase(User, Template)).InsertReference("mine", "My reference");

        // a new version ships a changed database
        new DBEngine(Template).InsertReference("ref2", "New shipped reference");
        var db = new DBEngine(DBEngine.PrepareUserDatabase(User, Template));

        Assert.Equal("My reference", Title(db, "mine"));
        Assert.Null(Title(db, "ref2"));
    }

    [Fact]
    public void Changes_are_written_to_the_user_database_not_to_the_shipped_one()
    {
        new DBEngine(Template);
        new DBEngine(DBEngine.PrepareUserDatabase(User, Template)).InsertReference("mine", "My reference");

        Assert.Null(Title(new DBEngine(Template), "mine"));
    }

    [Fact]
    public void Without_shipped_database_an_empty_one_is_created()
    {
        var db = new DBEngine(DBEngine.PrepareUserDatabase(User, Path.Combine(_dir, "missing.db")));

        Assert.True(File.Exists(User));
        Assert.Equal(0, db.GetReferenceDataTable(false).Rows.Count);
    }
}
