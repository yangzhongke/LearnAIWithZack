using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using OpenAI;

namespace NaturalLanguageToSQL1;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 自然语言转SQL查询系统 ===\n");

        // 加载配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("PostgreSQL")
                               ?? throw new Exception("未找到数据库连接字符串");

        var apiKey = configuration["AI:ApiKey"]
                     ?? throw new Exception("未找到AI ApiKey配置");
        var baseUrl = configuration["AI:BaseUrl"]
                      ?? throw new Exception("未找到AI BaseUrl配置");
        var modelId = configuration["AI:ModelId"]
                      ?? throw new Exception("未找到AI模型ID");

        try
        {
            // 初始化服务
            Console.WriteLine("正在连接数据库并读取结构信息...");
            var schemaService = new DatabaseSchemaService(connectionString);
            var databaseSchema = await schemaService.GetDatabaseSchemaAsync();

            // 显示数据库结构
            Console.WriteLine(databaseSchema);

            // 初始化AI客户端
            IChatClient chatClient = new OpenAI.Chat.ChatClient(
                modelId,
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions() { Endpoint = new Uri(baseUrl) }
            ).AsIChatClient();

            var sqlGenerator = new SqlGeneratorService(chatClient, databaseSchema);
            var sqlExecutor = new SqlExecutorService(connectionString);

            Console.WriteLine("\n系统已就绪！请输入您的查询需求（输入 'exit' 或 'quit' 退出）\n");

            // 主循环
            while (true)
            {
                Console.Write("您的查询需求> ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                if (userInput.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\n感谢使用，再见！");
                    break;
                }

                try
                {
                    // 生成SQL
                    Console.WriteLine("\n正在生成SQL查询...");
                    var sql = await sqlGenerator.GenerateSqlAsync(userInput);

                    Console.WriteLine($"\n生成的SQL语句：");
                    Console.WriteLine($"  {sql}");

                    /*
                    // 验证SQL安全性
                    if (!sqlGenerator.IsSafeQuery(sql))
                    {
                        Console.WriteLine("\n⚠️  生成的SQL语句不安全，仅支持SELECT查询。");
                        continue;
                    }*/

                    // 执行SQL
                    Console.WriteLine("\n执行查询中...");
                    var result = await sqlExecutor.ExecuteQueryAsync(sql);

                    Console.WriteLine("\n查询结果：");
                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ 错误: {ex.Message}");
                }

                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 系统初始化失败: {ex.Message}");
            Console.WriteLine("\n请检查：");
            Console.WriteLine("1. appsettings.json 中的数据库连接字符串是否正确");
            Console.WriteLine("2. PostgreSQL 数据库是否正在运行");
            Console.WriteLine("3. OpenAI API Key 和 BaseUrl 配置是否正确");
            Console.WriteLine($"\n详细错误信息：\n{ex}");
        }
    }
}