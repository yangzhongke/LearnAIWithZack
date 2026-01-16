using Microsoft.Extensions.AI;

namespace NaturalLanguageToSQL1;

public class SqlGeneratorService
{
    private readonly IChatClient _chatClient;
    private readonly string _databaseSchema;

    public SqlGeneratorService(IChatClient chatClient, string databaseSchema)
    {
        _chatClient = chatClient;
        _databaseSchema = databaseSchema;
    }

    public async Task<string> GenerateSqlAsync(string naturalLanguageQuery)
    {
        var systemPrompt = $@"你是一个PostgreSQL SQL专家。你的任务是根据用户的自然语言描述生成准确的SQL查询语句。

数据库结构如下：
{_databaseSchema}

重要规则：
1. 只生成SELECT查询语句，不要生成INSERT、UPDATE、DELETE等修改数据的语句
2. 只返回SQL语句本身，不要包含任何解释、注释或markdown格式
3. 不要在SQL前后添加```sql或```标记
4. 确保SQL语法正确，符合PostgreSQL标准
5. 如果需要关联多个表，使用适当的JOIN语句
6. 根据数据库结构中的外键关系进行表关联

示例：
用户输入：查询所有文章
你的输出：SELECT * FROM blog_posts;

用户输入：查询id为1的文章的所有评论
你的输出：SELECT * FROM comments WHERE post_id = 1;";

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, naturalLanguageQuery)
        };

        var response = await _chatClient.GetResponseAsync(messages);
        var sql = response.Text?.Trim() ?? string.Empty;

        // 清理可能的markdown标记
        sql = sql.Replace("```sql", "").Replace("```", "").Trim();

        return sql;
    }
}