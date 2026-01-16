using Npgsql;
using System.Text;

namespace NaturalLanguageToSQL1;

public class SqlExecutorService
{
    private readonly string _connectionString;

    public SqlExecutorService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string> ExecuteQueryAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        var result = new StringBuilder();
        
        // 获取列名
        var columnCount = reader.FieldCount;
        var columnNames = new List<string>();
        var columnWidths = new List<int>();

        for (int i = 0; i < columnCount; i++)
        {
            var columnName = reader.GetName(i);
            columnNames.Add(columnName);
            columnWidths.Add(columnName.Length);
        }

        // 读取所有数据行以计算列宽
        var rows = new List<object[]>();
        while (await reader.ReadAsync())
        {
            var row = new object[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "";
                row[i] = value;
                columnWidths[i] = Math.Max(columnWidths[i], value.Length);
            }
            rows.Add(row);
        }

        // 如果没有数据
        if (rows.Count == 0)
        {
            return "查询成功，但没有返回数据。";
        }

        // 构建表格
        var separator = new StringBuilder("+");
        for (int i = 0; i < columnCount; i++)
        {
            separator.Append(new string('-', columnWidths[i] + 2));
            separator.Append('+');
        }
        var separatorLine = separator.ToString();

        result.AppendLine(separatorLine);

        // 输出表头
        var header = new StringBuilder("|");
        for (int i = 0; i < columnCount; i++)
        {
            header.Append($" {columnNames[i].PadRight(columnWidths[i])} |");
        }
        result.AppendLine(header.ToString());
        result.AppendLine(separatorLine);

        // 输出数据行
        foreach (var row in rows)
        {
            var line = new StringBuilder("|");
            for (int i = 0; i < columnCount; i++)
            {
                line.Append($" {row[i].ToString()!.PadRight(columnWidths[i])} |");
            }
            result.AppendLine(line.ToString());
        }
        result.AppendLine(separatorLine);
        result.AppendLine($"\n共 {rows.Count} 行数据");

        return result.ToString();
    }
}

