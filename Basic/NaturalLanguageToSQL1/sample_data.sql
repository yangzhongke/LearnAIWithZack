-- 示例数据库结构和数据
-- 用于测试自然语言转SQL查询系统

-- 创建博客文章表
CREATE TABLE IF NOT EXISTS blog_posts
(
    id
    SERIAL
    PRIMARY
    KEY,
    title
    VARCHAR
(
    200
) NOT NULL,
    content TEXT NOT NULL,
    author VARCHAR
(
    100
) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

-- 创建评论表
CREATE TABLE IF NOT EXISTS comments
(
    id
    SERIAL
    PRIMARY
    KEY,
    post_id
    INTEGER
    NOT
    NULL,
    author
    VARCHAR
(
    100
) NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY
(
    post_id
) REFERENCES blog_posts
(
    id
) ON DELETE CASCADE
    );

-- 清空现有数据（如果有）
TRUNCATE TABLE comments, blog_posts RESTART IDENTITY CASCADE;

-- 插入博客文章样例数据
INSERT INTO blog_posts (title, content, author, created_at)
VALUES ('PostgreSQL入门指南',
        '这是一篇关于PostgreSQL数据库的入门教程，涵盖了基础的CRUD操作、索引优化和查询技巧。适合初学者学习。', '张三',
        NOW() - INTERVAL '7 days'),
       ('C#异步编程最佳实践',
        '本文介绍了C#中async/await的使用方法和最佳实践，包括如何避免死锁、正确处理异常以及性能优化技巧。', '李四',
        NOW() - INTERVAL '5 days'),
       ('微服务架构设计模式', '探讨微服务架构中常见的设计模式和实践经验，包括服务发现、负载均衡、熔断器模式等。', '王五',
        NOW() - INTERVAL '3 days'),
       ('Docker容器化部署指南', '详细介绍如何使用Docker进行应用容器化部署，包括Dockerfile编写、镜像优化和多容器编排。',
        '张三', NOW() - INTERVAL '2 days'),
       ('RESTful API设计原则', '深入讲解RESTful API的设计原则、最佳实践和常见问题，帮助开发者构建更好的API接口。', '李四',
        NOW() - INTERVAL '1 day');

-- 插入评论样例数据
INSERT INTO comments (post_id, author, content, created_at)
VALUES
-- PostgreSQL入门指南的评论
(1, '赵六', '写得很好，对初学者很有帮助！特别是索引优化部分讲得很清楚。', NOW() - INTERVAL '6 days'),
(1, '钱七', '示例代码很清晰，感谢分享。已经收藏了！', NOW() - INTERVAL '6 days'),
(1, '孙八', '能否再详细讲讲查询性能优化的部分？', NOW() - INTERVAL '5 days'),

-- C#异步编程最佳实践的评论
(2, '孙八', '异步编程确实是个重要话题，这篇文章解释得很透彻。', NOW() - INTERVAL '4 days'),
(2, '周九', '希望能看到更多关于性能优化的内容。', NOW() - INTERVAL '4 days'),
(2, '赵六', '关于死锁的部分非常实用，解决了我项目中的问题。', NOW() - INTERVAL '3 days'),

-- 微服务架构设计模式的评论
(3, '吴十', '微服务架构在实际项目中确实很实用。', NOW() - INTERVAL '2 days'),
(3, '钱七', '服务发现部分讲得不错，但是可以再深入一些。', NOW() - INTERVAL '2 days'),

-- Docker容器化部署指南的评论
(4, '周九', '正好最近在学Docker，这篇文章来得很及时！', NOW() - INTERVAL '1 day'),
(4, '吴十', '镜像优化的技巧很实用，减少了我们项目的镜像大小。', NOW() - INTERVAL '1 day'),

-- RESTful API设计原则的评论
(5, '赵六', '对于API设计有了更深的理解，感谢分享！', NOW() - INTERVAL '12 hours'),
(5, '孙八', '最佳实践部分非常有参考价值。', NOW() - INTERVAL '6 hours');

-- 查询验证
SELECT 'blog_posts表记录数:' AS info, COUNT(*) AS count
FROM blog_posts
UNION ALL
SELECT 'comments表记录数:' AS info, COUNT(*) AS count
FROM comments;

-- 显示所有文章及其评论数
SELECT bp.id,
       bp.title,
       bp.author,
       COUNT(c.id) as comment_count,
       bp.created_at
FROM blog_posts bp
         LEFT JOIN comments c ON bp.id = c.post_id
GROUP BY bp.id, bp.title, bp.author, bp.created_at
ORDER BY bp.created_at DESC;

