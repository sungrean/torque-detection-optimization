// HTML获取优化方案示例

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 优化后的HTTP请求处理类，支持从文件系统读取HTML内容。
/// 这个类可以集成到HttpServer中，替换原有的硬编码HTML方法。
/// </summary>
public class OptimizedRequestHandler
{
    // HTML文件的根目录，可以放在应用程序的可执行文件目录下
    private readonly string _webRootPath;

    public OptimizedRequestHandler(string webRootPath = "html")
    {
        _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), webRootPath);
        if (!Directory.Exists(_webRootPath))
        {
            Directory.CreateDirectory(_webRootPath);
        }
    }

    /// <summary>
    /// 处理动态文件请求，例如 /index.html, /about.html
    /// </summary>
    public async Task HandleFileRequest(HttpListenerContext context, CancellationToken cancellationToken)
    {
        string localPath = context.Request.Url.LocalPath;
        string fileName = localPath.TrimStart('/');

        // 默认文档
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "index.html";
        }

        string filePath = Path.Combine(_webRootPath, fileName);

        if (File.Exists(filePath))
        {
            try
            {
                string content = await File.ReadAllTextAsync(filePath, cancellationToken);
                string contentType = GetContentType(filePath);
                await SendResponse(context, content, contentType, HttpStatusCode.OK, cancellationToken);
            }
            catch (IOException ex)
            {
                await SendResponse(context, $"Error reading file: {ex.Message}", "text/plain", HttpStatusCode.InternalServerError, cancellationToken);
            }
        }
        else
        {
            await SendResponse(context, "404 - Not Found", "text/html", HttpStatusCode.NotFound, cancellationToken);
        }
    }

    /// <summary>
    /// 根据文件扩展名获取MIME类型
    /// </summary>
    private string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream",
        };
    }

    /// <summary>
    /// 发送响应的通用方法
    /// </summary>
    private async Task SendResponse(HttpListenerContext context, string content, string contentType, HttpStatusCode statusCode, CancellationToken cancellationToken)
    {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
        context.Response.ContentType = contentType;
        context.Response.ContentEncoding = System.Text.Encoding.UTF8;
        context.Response.ContentLength64 = buffer.Length;
        context.Response.StatusCode = (int)statusCode;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        context.Response.Close();
    }
}

/*
// 在 HttpServer.cs 中的集成方式：
public class HttpServer
{
    // ... 其他代码 ...
    private readonly OptimizedRequestHandler _requestHandler;

    public HttpServer()
    {
        // ...
        _requestHandler = new OptimizedRequestHandler("html"); // 指定存放网页的目录
        InitializeDefaultRoutes();
    }

    private void InitializeDefaultRoutes()
    {
        // 注册一个通用的文件处理器来替代原来的硬编码方法
        // 注意：这里需要一个更灵活的路由匹配逻辑，例如使用正则表达式
        // 或者在请求处理时动态判断路径是否对应文件。
        // 为简化示例，我们假设所有.html请求都由此处理。
        
        // 示例：将根路径和.html文件指向新的处理器
        AddRoute("/", _requestHandler.HandleFileRequest);
        AddRoute("/index.html", _requestHandler.HandleFileRequest);
        // 你可以添加一个更通用的路由来处理所有文件
        // AddRoute("*", _requestHandler.HandleFileRequest);
    }
    // ... 其他代码 ...
}
*/
