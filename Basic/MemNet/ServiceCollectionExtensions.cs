using MemNet.Config;
using MemNet.Core;
using MemNet.Embedders;
using MemNet.GraphStores;
using MemNet.LLMs;
using MemNet.VectorStores;
using Microsoft.Extensions.DependencyInjection;

namespace MemNet;

/// <summary>
///     MemNet 服务注册扩展（复刻 Mem0 的配置模式）
/// </summary>
public static class ServiceCollectionExtensions
{

    /// <summary>
    ///     添加 MemNet 服务（使用配置对象）
    /// </summary>
    public static IServiceCollection AddMemNet(
        this IServiceCollection services,
        Action<MemoryConfig> configureOptions)
    {
        // 注册配置
        services.Configure(configureOptions);

        // 注册核心服务
        services.AddScoped<MemoryService>();

        // 注册默认实现
        services.AddHttpClient<OpenAIProvider>();
        services.AddHttpClient<OpenAIEmbedder>();
        services.AddSingleton<InMemoryVectorStore>();
        // 注册知识图谱存储
        services.AddSingleton<InMemoryGraphStore>();

        return services;
    }
}