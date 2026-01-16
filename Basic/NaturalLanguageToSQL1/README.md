# 自然语言转SQL查询系统

这是一个基于 .NET 9 的控制台应用程序，可以将自然语言查询转换为 PostgreSQL SQL 语句并执行查询。

## 功能特性

- ✅ 自动读取 PostgreSQL 数据库的表结构、列信息和外键关系
- ✅ 使用 AI (Ollama) 将自然语言转换为 SQL 查询语句
- ✅ 执行生成的 SQL 查询并以表格形式输出结果
- ✅ 安全检查：仅支持 SELECT 查询，防止数据被修改
- ✅ 友好的交互式命令行界面

## 前置要求

1. **.NET 9.0 SDK** - [下载地址](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **PostgreSQL 数据库** - 确保数据库正在运行
3. **OpenAI 兼容的 API** - 支持以下服务：
    - OpenAI API
    - 阿里云通义千问 (DashScope)
    - 其他兼容 OpenAI API 的服务

## 配置

编辑 `appsettings.json` 文件，配置数据库连接和 AI 服务：

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=your_database;Username=your_username;Password=your_password"
  },
  "AI": {
    "ApiKey": "your_api_key_here",
    "BaseUrl": "https://dashscope.aliyuncs.com/compatible-mode/v1/",
    "ModelId": "qwen-max"
  }
}
```

### 配置说明

- `PostgreSQL`: PostgreSQL 数据库连接字符串
    - `Host`: 数据库服务器地址
    - `Port`: 端口号（默认 5432）
    - `Database`: 数据库名称
    - `Username`: 用户名
    - `Password`: 密码

- `AI:ApiKey`: OpenAI API Key 或兼容服务的 API Key
- `AI:BaseUrl`: API 服务地址
    - 阿里云通义千问：`https://dashscope.aliyuncs.com/compatible-mode/v1/`
    - OpenAI：`https://api.openai.com/v1/`
    - 其他兼容服务的地址
- `AI:ModelId`: 使用的 AI 模型
    - 阿里云：`qwen-max`、`qwen-plus`、`qwen-turbo`
    - OpenAI：`gpt-4`、`gpt-3.5-turbo`

## 快速开始

### 1. 准备测试数据库（可选）

如果您没有现成的数据库，可以使用以下 SQL 创建示例数据：

```sql
-- 创建博客文章表
CREATE TABLE blog_posts (
    id SERIAL PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    content TEXT NOT NULL,
    author VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 创建评论表
CREATE TABLE comments (
    id SERIAL PRIMARY KEY,
    post_id INTEGER NOT NULL,
    author VARCHAR(100) NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (post_id) REFERENCES blog_posts(id) ON DELETE CASCADE
);

-- 插入样例数据
INSERT INTO blog_posts (title, content, author) VALUES
('PostgreSQL入门指南', '这是一篇关于PostgreSQL数据库的入门教程。', '张三'),
('C#异步编程最佳实践', '本文介绍了C#中async/await的使用方法。', '李四'),
('微服务架构设计模式', '探讨微服务架构中常见的设计模式。', '王五');

INSERT INTO comments (post_id, author, content) VALUES
(1, '赵六', '写得很好，对初学者很有帮助！'),
(1, '钱七', '示例代码很清晰，感谢分享。'),
(2, '孙八', '异步编程确实是个重要话题。'),
(3, '吴十', '微服务架构在实际项目中确实很实用。');
```

### 2. 运行程序

```bash
cd F:\Git代码库\DotNet\LearnAIWithZack\Basic\NaturalLanguageToSQL1
dotnet run
```

### 3. 使用示例

程序启动后，会显示数据库结构信息，然后您可以输入自然语言查询：

```
您的查询需求> 查询所有文章

生成的SQL语句：
  SELECT * FROM blog_posts;

查询结果：
+----+---------------------------+---------------------------------------------+--------+---------------------+---------------------+
| id | title                     | content                                     | author | created_at          | updated_at          |
+----+---------------------------+---------------------------------------------+--------+---------------------+---------------------+
| 1  | PostgreSQL入门指南         | 这是一篇关于PostgreSQL数据库的入门教程。      | 张三   | 2026-01-17 10:00:00 | 2026-01-17 10:00:00 |
| 2  | C#异步编程最佳实践         | 本文介绍了C#中async/await的使用方法。        | 李四   | 2026-01-17 10:00:00 | 2026-01-17 10:00:00 |
| 3  | 微服务架构设计模式         | 探讨微服务架构中常见的设计模式。              | 王五   | 2026-01-17 10:00:00 | 2026-01-17 10:00:00 |
+----+---------------------------+---------------------------------------------+--------+---------------------+---------------------+

共 3 行数据
```

更多查询示例：

- "查询作者为张三的所有文章"
- "查询第一篇文章的所有评论"
- "统计每篇文章的评论数量"
- "查询最近发布的5篇文章"
- "查询包含'编程'关键词的文章标题"

输入 `exit` 或 `quit` 退出程序。

## 项目结构

```
NaturalLanguageToSQL1/
├── Program.cs                    # 主程序入口
├── DatabaseSchemaService.cs      # 数据库结构读取服务
├── SqlGeneratorService.cs        # SQL生成服务（AI）
├── SqlExecutorService.cs         # SQL执行服务
├── appsettings.json              # 配置文件
├── NaturalLanguageToSQL1.csproj  # 项目文件
└── README.md                     # 说明文档
```

## 技术栈

- **.NET 9.0** - 应用框架
- **Npgsql** - PostgreSQL 数据库连接
- **Microsoft.Extensions.AI** - AI 抽象层
- **Microsoft.Extensions.AI.OpenAI** - OpenAI API 客户端
- **Microsoft.Extensions.Configuration** - 配置管理

## 安全特性

- ✅ 只允许执行 SELECT 查询
- ✅ 自动检测并阻止危险的 SQL 关键词（INSERT、UPDATE、DELETE 等）
- ✅ 防止 SQL 注入
- ✅ 参数化查询

## 故障排除

### 1. 数据库连接失败

- 检查 PostgreSQL 服务是否正在运行
- 验证 appsettings.json 中的连接字符串是否正确
- 确认用户名和密码正确

### 2. AI 服务连接失败

- 确认 Ollama 服务正在运行：`ollama serve`
- 检查模型是否已下载：`ollama list`
- 如果没有模型，请下载：`ollama pull qwen2.5:latest`

### 3. SQL 生成不准确

- 尝试使用更大或更新的 AI 模型
- 使用更清晰、更具体的自然语言描述
- 检查数据库结构是否正确读取

## 扩展建议

- [ ] 支持更多数据库类型（MySQL、SQL Server 等）
- [ ] 添加查询历史记录
- [ ] 支持多轮对话优化查询
- [ ] 添加查询结果导出功能（CSV、JSON）
- [ ] Web API 接口封装
- [ ] 图形化界面

## 许可证

MIT License

