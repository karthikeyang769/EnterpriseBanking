using System;
using Npgsql;

try
{
    var connString = "Host=localhost;Database=EnterpriseBankDB;Username=postgres;Password=postgres";
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("Successfully connected to the database!");

    // List all tables
    Console.WriteLine("\n--- Tables in Database ---");
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog', 'information_schema') ORDER BY table_schema, table_name;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"[{reader.GetString(0)}].{reader.GetString(1)}");
        }
    }

    // Query rows count in users table
    Console.WriteLine("\n--- Rows count in Users tables ---");
    string[] userTables = { "public.\"Users\"", "public.Users", "public.users", "identity.Users", "identity.users" };
    foreach (var table in userTables)
    {
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {table}";
            var count = cmd.ExecuteScalar();
            Console.WriteLine($"{table}: {count} row(s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{table}: Table does not exist or failed to query ({ex.Message})");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error connecting or querying database: " + ex.Message);
}
