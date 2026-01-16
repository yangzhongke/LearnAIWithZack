using Npgsql;
using System.Text;

namespace NaturalLanguageToSQL1;

public class DatabaseSchemaService
{
    private readonly string _connectionString;

    public DatabaseSchemaService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string> GetDatabaseSchemaAsync()
    {
        var schema = new StringBuilder();
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // 获取所有表
        var tables = await GetTablesAsync(connection);
        
        schema.AppendLine("数据库结构信息：");
        schema.AppendLine("===================");
        
        foreach (var table in tables)
        {
            schema.AppendLine($"\n表名: {table}");
            schema.AppendLine("列信息：");
            
            // 获取列信息
            var columns = await GetColumnsAsync(connection, table);
            foreach (var column in columns)
            {
                schema.AppendLine($"  - {column}");
            }
            
            // 获取外键关系
            var foreignKeys = await GetForeignKeysAsync(connection, table);
            if (foreignKeys.Any())
            {
                schema.AppendLine("外键关系：");
                foreach (var fk in foreignKeys)
                {
                    schema.AppendLine($"  - {fk}");
                }
            }
        }
        
        return schema.ToString();
    }

    private async Task<List<string>> GetTablesAsync(NpgsqlConnection connection)
    {
        var tables = new List<string>();
        
        var query = @"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_type = 'BASE TABLE'
            ORDER BY table_name;";
        
        await using var cmd = new NpgsqlCommand(query, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        
        return tables;
    }

    private async Task<List<string>> GetColumnsAsync(NpgsqlConnection connection, string tableName)
    {
        var columns = new List<string>();
        
        var query = @"
            SELECT 
                column_name,
                data_type,
                character_maximum_length,
                is_nullable,
                column_default
            FROM information_schema.columns
            WHERE table_schema = 'public' 
            AND table_name = @tableName
            ORDER BY ordinal_position;";
        
        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("tableName", tableName);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(0);
            var dataType = reader.GetString(1);
            var maxLength = reader.IsDBNull(2) ? "" : $"({reader.GetInt32(2)})";
            var nullable = reader.GetString(3) == "YES" ? "NULL" : "NOT NULL";
            var defaultValue = reader.IsDBNull(4) ? "" : $" DEFAULT {reader.GetString(4)}";
            
            columns.Add($"{columnName} {dataType}{maxLength} {nullable}{defaultValue}");
        }
        
        return columns;
    }

    private async Task<List<string>> GetForeignKeysAsync(NpgsqlConnection connection, string tableName)
    {
        var foreignKeys = new List<string>();
        
        var query = @"
            SELECT
                kcu.column_name,
                ccu.table_name AS foreign_table_name,
                ccu.column_name AS foreign_column_name
            FROM information_schema.table_constraints AS tc
            JOIN information_schema.key_column_usage AS kcu
                ON tc.constraint_name = kcu.constraint_name
                AND tc.table_schema = kcu.table_schema
            JOIN information_schema.constraint_column_usage AS ccu
                ON ccu.constraint_name = tc.constraint_name
                AND ccu.table_schema = tc.table_schema
            WHERE tc.constraint_type = 'FOREIGN KEY'
            AND tc.table_schema = 'public'
            AND tc.table_name = @tableName;";
        
        await using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("tableName", tableName);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(0);
            var foreignTable = reader.GetString(1);
            var foreignColumn = reader.GetString(2);
            
            foreignKeys.Add($"{columnName} -> {foreignTable}.{foreignColumn}");
        }
        
        return foreignKeys;
    }

    public async Task<List<string>> GetTableNamesAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return await GetTablesAsync(connection);
    }
}

