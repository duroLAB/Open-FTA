using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;


public class DBEngine
{
    public Dictionary<int, string> MetricUnitsList = new()
{
    { 0, "y⁻¹" },  // roky
    { 1, "d⁻¹" },  // dni
    { 2, "h⁻¹" },  // hodiny
    { 3, "s⁻¹" }   // sekundy
};

    private static DBEngine _instance;

    public static DBEngine Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DBEngine();
                _instance.InitializeDatabase(); // načítanie pri prvom použití
            }
            return _instance;
        }
    }

    private readonly string _connectionString;

    private DBEngine(string dbName = "mydatabase.db")
    {
        string exeFolder = AppContext.BaseDirectory;
        string dbPath = Path.Combine(exeFolder, dbName);
        _connectionString = $"Data Source={dbPath}";

        InitializeDatabase();
    }

    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Povolenie foreign key constraints
        using (var pragmaCmd = connection.CreateCommand())
        {
            pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
            pragmaCmd.ExecuteNonQuery();
        }

        // ReferenceTable
        using (var createRefTableCmd = connection.CreateCommand())
        {
            createRefTableCmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS ReferenceTable (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Publisher TEXT,
                Authors TEXT,
                Year INTEGER
            );
        ";
            createRefTableCmd.ExecuteNonQuery();
        }

        // ReliabilityTable s cudzím kľúčom (bez CASCADE)
        using (var createRelTableCmd = connection.CreateCommand())
        {
            createRelTableCmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS ReliabilityTable (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Val REAL,
                MetricUnit INTEGER NOT NULL,
                ReferenceId TEXT NOT NULL,
                FOREIGN KEY (ReferenceId) REFERENCES ReferenceTable(Id)
            );
        ";
            createRelTableCmd.ExecuteNonQuery();
        }
    }


    ///////////////////////
    ///////////////////////
    ///////////RREFERENCE TABLE METHODS
    ///
    // CREATE
    public bool InsertReference(string id, string title, string? publisher = null, string? authors = null, int? year = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO ReferenceTable (Id, Title, Publisher, Authors, Year)
        VALUES (@id, @title, @publisher, @authors, @year);
    ";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@publisher", publisher ?? string.Empty);
        cmd.Parameters.AddWithValue("@authors", authors ?? string.Empty);
        cmd.Parameters.AddWithValue("@year", year.HasValue ? year.Value : DBNull.Value);

        return cmd.ExecuteNonQuery() > 0;
    }

    // READ
    public bool GetReferenceById(string id, out string title, out string publisher, out string authors, out int? year)
    {
        title = publisher = authors = string.Empty;
        year = null;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Title, Publisher, Authors, Year FROM ReferenceTable WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            title = reader.GetString(0);
            publisher = reader.GetString(1);
            authors = reader.GetString(2);
            year = reader.IsDBNull(3) ? null : reader.GetInt32(3);
            return true;
        }

        return false;
    }

    public bool GetReferenceById(string id, out string title)
    {
        bool res = GetReferenceById(id, out string title2, out string publisher, out string authors, out int? year);
        title = title2 + " " + publisher+" "+authors+" "+year;
        return res;
    }

    // UPDATE
    public bool UpdateReference(string id, string? title = null, string? publisher = null, string? authors = null, int? year = null)
    {
        if (!GetReferenceById(id, out string existingTitle, out string existingPublisher, out string existingAuthors, out int? existingYear))
            return false;

        title ??= existingTitle;
        publisher ??= existingPublisher;
        authors ??= existingAuthors;
        year ??= existingYear;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        UPDATE ReferenceTable
        SET Title = @title,
            Publisher = @publisher,
            Authors = @authors,
            Year = @year
        WHERE Id = @id;
    ";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@publisher", publisher);
        cmd.Parameters.AddWithValue("@authors", authors);
        cmd.Parameters.AddWithValue("@year", year.HasValue ? year.Value : DBNull.Value);

        return cmd.ExecuteNonQuery() > 0;
    }

    // DELETE s kontrolou, či sa referencie nepoužívajú
    public bool CanDeleteReference(string referenceId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM ReliabilityTable WHERE ReferenceId = @refId;";
        cmd.Parameters.AddWithValue("@refId", referenceId);

        long count = (long)cmd.ExecuteScalar();
        return count == 0;
    }

    public bool DeleteReference(string referenceId)
    {
        if (!CanDeleteReference(referenceId))
            return false;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM ReferenceTable WHERE Id = @refId;";
        cmd.Parameters.AddWithValue("@refId", referenceId);

        return cmd.ExecuteNonQuery() > 0;
    }

    // DataTable
    public DataTable GetReferenceDataTable(bool full)
    {
        var dt = new DataTable();
        if (full)
        {
            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("Publisher", typeof(string));
            dt.Columns.Add("Authors", typeof(string));
            dt.Columns.Add("Year", typeof(int));
        }
        else
        {
            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Title", typeof(string));
        }


        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        if(full)
            cmd.CommandText = "SELECT Id, Title, Publisher, Authors, Year FROM ReferenceTable;";
        else
            cmd.CommandText = "SELECT Id, Title FROM ReferenceTable;";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if(full)
            dt.Rows.Add(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? (object)DBNull.Value : reader.GetInt32(4)
            );
            else
            { 
                dt.Rows.Add(
                reader.GetString(0),
                reader.GetString(1)
            );
            }   
        }

        return dt;
    }



    ////////////////////////    
    ///////////////////////
    ///Reliability TABLE METHODS
    ///

    // CREATE
    public bool InsertReliability(string id, string title, double val, int metricUnit, string referenceId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO ReliabilityTable (Id, Title, Val, MetricUnit, ReferenceId)
        VALUES (@id, @title, @val, @metric, @refId);
    ";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@val", val);
        cmd.Parameters.AddWithValue("@metric", metricUnit);
        cmd.Parameters.AddWithValue("@refId", referenceId);

        return cmd.ExecuteNonQuery() > 0;
    }

    // READ
    public bool GetReliabilityById(string id, out string title, out double val, out int metricUnit, out string referenceId)
    {
        title = string.Empty;
        val = 0;
        metricUnit = 0;
        referenceId = string.Empty;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Title, Val, MetricUnit, ReferenceId FROM ReliabilityTable WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            title = reader.GetString(0);
            val = reader.GetDouble(1);
            metricUnit = reader.GetInt32(2);
            referenceId = reader.GetString(3);
            return true;
        }

        return false;
    }

    // UPDATE
    public bool UpdateReliability(string id, string? title = null, double? val = null, int? metricUnit = null, string? referenceId = null)
    {
        if (!GetReliabilityById(id, out string existingTitle, out double existingVal, out int existingMetric, out string existingRef))
            return false;

        title ??= existingTitle;
        val ??= existingVal;
        metricUnit ??= existingMetric;
        referenceId ??= existingRef;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        UPDATE ReliabilityTable
        SET Title = @title,
            Val = @val,
            MetricUnit = @metric,
            ReferenceId = @refId
        WHERE Id = @id;
    ";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@val", val.Value);
        cmd.Parameters.AddWithValue("@metric", metricUnit.Value);
        cmd.Parameters.AddWithValue("@refId", referenceId);

        return cmd.ExecuteNonQuery() > 0;
    }

    // DELETE
    public bool DeleteReliability(string id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM ReliabilityTable WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // DataTable
    public DataTable GetReliabilityDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(string));
        dt.Columns.Add("Title", typeof(string));
        dt.Columns.Add("Val", typeof(double));
        dt.Columns.Add("MetricUnit", typeof(string)); // symbol jednotky
        dt.Columns.Add("ReferenceId", typeof(string));

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, Val, MetricUnit, ReferenceId FROM ReliabilityTable;";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            string id = reader.GetString(0);
            string title = reader.GetString(1);
            double val = reader.GetDouble(2);
            int metricCode = reader.GetInt32(3);
            string referenceId = reader.GetString(4);

            string metricSymbol = MetricUnitsList.ContainsKey(metricCode) ? MetricUnitsList[metricCode] : "?";

            dt.Rows.Add(id, title, val, metricSymbol, referenceId);
        }

        return dt;
    }

    public DataTable GetReliabilityDataTablev2()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(string));
        dt.Columns.Add("Title", typeof(string));
        dt.Columns.Add("Val", typeof(double));
        dt.Columns.Add("MetricUnit", typeof(string));
        dt.Columns.Add("ReferenceTitle", typeof(string)); // názov referencie

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        SELECT r.Id, r.Title, r.Val, r.MetricUnit, ref.Title
        FROM ReliabilityTable r
        LEFT JOIN ReferenceTable ref ON r.ReferenceId = ref.Id
        ORDER BY r.Title;
    ";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var row = dt.NewRow();
            row["Id"] = reader.GetString(0);
            row["Title"] = reader.GetString(1);
            row["Val"] = reader.IsDBNull(2) ? 0.0 : reader.GetDouble(2);

            int metricUnitIndex = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
            row["MetricUnit"] = MetricUnitsList.ContainsKey(metricUnitIndex) ? MetricUnitsList[metricUnitIndex] : "?";

            row["ReferenceTitle"] = reader.IsDBNull(4) ? "" : reader.GetString(4);

            dt.Rows.Add(row);
        }

        return dt;
    }

    public void SeedData()
    {
        // Prv než vložíme dáta, inicializujeme databázu
        InitializeDatabase();

        // Vytvorenie niekoľkých Reference záznamov
        var referenceIds = new List<string>();

        referenceIds.Add(Guid.NewGuid().ToString());
        InsertReference(referenceIds[0], "Kniha A", "Vydavateľ A", "Autor 1", 2020);

        referenceIds.Add(Guid.NewGuid().ToString());
        InsertReference(referenceIds[1], "Kniha B", "Vydavateľ B", "Autor 2, Autor 3", 2021);

        referenceIds.Add(Guid.NewGuid().ToString());
        InsertReference(referenceIds[2], "Kniha C", "Vydavateľ C", "Autor 4", 2022);

        // Vytvorenie Reliability záznamov pre každú referenciu
        for (int i = 0; i < referenceIds.Count; i++)
        {
            string relId = Guid.NewGuid().ToString();
            string title = $"Reliability Test {i + 1}";
            double val = Math.Round(0.8 + 0.05 * i, 2); // náhodné hodnoty
            int metricUnit = i % MetricUnitsList.Count; // použitie rôznych jednotiek

            InsertReliability(relId, title, val, metricUnit, referenceIds[i]);
        }

        // Pridanie niekoľkých Reliability pre rovnakú referenciu (overenie foreign key)
        string relIdExtra = Guid.NewGuid().ToString();
        InsertReliability(relIdExtra, "Reliability Extra", 0.99, 3, referenceIds[0]);

        Console.WriteLine("Seed data inserted successfully!");
    }

}
