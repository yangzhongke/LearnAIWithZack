# 快速启动指南

## 第一步：准备 PostgreSQL 数据库

### 方法 1: 使用 Docker（推荐）

```powershell
# 启动 PostgreSQL 容器
docker run --name postgres-test -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:latest

# 等待几秒让数据库启动完成，然后创建测试数据库
docker exec -it postgres-test psql -U postgres -c "CREATE DATABASE testdb;"

# 导入示例数据
docker exec -i postgres-test psql -U postgres -d testdb < sample_data.sql
```

### 方法 2: 使用本地 PostgreSQL

```powershell
# 使用 psql 命令行工具
psql -U your_username -c "CREATE DATABASE testdb;"
psql -U your_username -d testdb -f sample_data.sql
```

## 第二步：配置连接字符串

编辑 `appsettings.json` 文件：

### 使用阿里云通义千问（推荐）：

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres"
  },
  "AI": {
    "ApiKey": "sk-your-dashscope-api-key",
    "BaseUrl": "https://dashscope.aliyuncs.com/compatible-mode/v1/",
    "ModelId": "qwen-max"
  }
}
```

### 使用 OpenAI：

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=testdb;Username=your_username;Password=your_password"
  },
  "AI": {
    "ApiKey": "sk-your-openai-api-key",
    "BaseUrl": "https://api.openai.com/v1/",
    "ModelId": "gpt-4"
  }
}
```

**获取 API Key：**

- 阿里云通义千问：访问 [DashScope控制台](https://dashscope.console.aliyun.com/) 获取 API Key
- OpenAI：访问 [OpenAI Platform](https://platform.openai.com/api-keys) 获取 API Key

**获取 API Key：**

- 阿里云通义千问：访问 [DashScope控制台](https://dashscope.console.aliyun.com/) 获取 API Key
- OpenAI：访问 [OpenAI Platform](https://platform.openai.com/api-keys) 获取 API Key

## 第三步：运行程序

```powershell
# 进入项目目录
cd F:\Git代码库\DotNet\LearnAIWithZack\Basic\NaturalLanguageToSQL1

# 运行程序
dotnet run
```

## 第四步：测试查询

程序启动后，尝试以下查询：

### 基础查询

```
您的查询需求> 查询所有文章
您的查询需求> 查询所有评论
您的查询需求> 查询作者为张三的文章
```

### 关联查询

```
您的查询需求> 查询第一篇文章的所有评论
您的查询需求> 查询每篇文章的评论数量
您的查询需求> 查询有评论的文章及其评论内容
```

### 聚合查询

```
您的查询需求> 统计每个作者发表的文章数量
您的查询需求> 查询评论最多的文章
您的查询需求> 统计总共有多少篇文章和多少条评论
```

### 条件查询

```
您的查询需求> 查询最近3天发布的文章
您的查询需求> 查询标题包含"编程"的文章
您的查询需求> 查询评论数超过2条的文章
```

## 常见问题

### 1. 数据库连接失败

```
❌ 错误: database "testdb" does not exist
```

**解决方法：** 确保已创建数据库 `testdb`

### 2. API 连接失败

```
❌ 错误: Unauthorized
```

**解决方法：**

- 检查 API Key 是否正确
- 确保 API Key 有足够的配额
- 验证 BaseUrl 是否正确

### 3. 网络连接问题

```
❌ 错误: Connection refused
```

**解决方法：**

- 检查网络连接
- 如果使用代理，请配置正确的代理设置

## 推荐的 AI 模型

### 阿里云通义千问（中文友好）

| 模型名称       | 特点        | 推荐场景 |
|------------|-----------|------|
| qwen-max   | 最强性能，准确度高 | 复杂查询 |
| qwen-plus  | 性能均衡，速度快  | 默认推荐 |
| qwen-turbo | 速度最快，成本低  | 简单查询 |

### OpenAI

| 模型名称          | 特点      | 推荐场景 |
|---------------|---------|------|
| gpt-4         | 最强性能    | 复杂查询 |
| gpt-3.5-turbo | 速度快，成本低 | 通用查询 |

## 性能优化建议

1. **首次查询较慢**：网络请求需要时间，首次查询可能需要 2-5 秒
2. **选择合适的模型**：根据需求选择模型，复杂查询使用 qwen-max/gpt-4，简单查询使用 qwen-turbo/gpt-3.5-turbo
3. **网络优化**：如果在国内使用 OpenAI，建议使用代理或选择国内服务（如阿里云通义千问）
4. **成本控制**：使用 turbo 系列模型可以降低成本

## 退出程序

输入以下任一命令退出：

- `exit`
- `quit`

---

**祝您使用愉快！** 🎉

