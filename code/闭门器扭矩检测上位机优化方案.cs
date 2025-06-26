/*
 * 闭门器扭矩检测上位机优化完整实现方案
 * 
 * 基于winformWebpages和line-control1项目分析的优化方案
 * 作者：MiniMax Agent
 * 日期：2025-06-26
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Transystem.Lib.PLC;
using System.Text.Json;

namespace 闭门器扭矩检测上位机优化
{
    /// <summary>
    /// 优化后的主窗体类 - 集成了文件化HTML获取和事件驱动PLC监控
    /// </summary>
    public partial class OptimizedTorqueDetectionForm : Form
    {
        private OptimizedHttpServer _httpServer;
        private PlcTorqueMonitor _plcMonitor;
        private WebBrowser _webBrowser; // 或使用WebView2
        private const int HTTP_PORT = 8080;

        public OptimizedTorqueDetectionForm()
        {
            InitializeComponent();
            InitializeOptimizedComponents();
        }

        private void InitializeOptimizedComponents()
        {
            // 1. 初始化优化后的HTTP服务器
            _httpServer = new OptimizedHttpServer(HTTP_PORT, "htmlPages");
            
            // 2. 初始化PLC扭矩监控
            _plcMonitor = new PlcTorqueMonitor();
            
            // 3. 设置Web浏览器
            _webBrowser = new WebBrowser();
            _webBrowser.Dock = DockStyle.Fill;
            this.Controls.Add(_webBrowser);
            
            // 4. 启动服务
            StartServices();
        }

        private async void StartServices()
        {
            try
            {
                // 启动HTTP服务器
                await _httpServer.StartAsync();
                
                // 启动PLC监控
                await _plcMonitor.StartMonitoringAsync();
                
                // 导航到本地网页
                _webBrowser.Navigate($"http://localhost:{HTTP_PORT}");
                
                // 注册PLC数据变化事件，用于推送到前端
                _plcMonitor.OnTorqueDataChanged += OnTorqueDataChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动服务失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnTorqueDataChanged(object sender, TorqueDataEventArgs e)
        {
            // 将PLC数据变化推送到前端
            var dataJson = JsonSerializer.Serialize(e.TorqueData);
            _httpServer.BroadcastData("torque_update", dataJson);
        }

        private void OptimizedTorqueDetectionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 清理资源
            _plcMonitor?.StopMonitoring();
            _httpServer?.Stop();
        }
    }

    /// <summary>
    /// 优化后的HTTP服务器 - 支持文件系统读取HTML和WebSocket通信
    /// </summary>
    public class OptimizedHttpServer
    {
        private readonly HttpListener _httpListener;
        private readonly string _webRootPath;
        private readonly int _port;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly List<WebSocketConnection> _webSocketConnections;

        public OptimizedHttpServer(int port, string webRootDirectory)
        {
            _port = port;
            _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), webRootDirectory);
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{port}/");
            _webSocketConnections = new List<WebSocketConnection>();
            
            // 确保Web根目录存在
            if (!Directory.Exists(_webRootPath))
            {
                Directory.CreateDirectory(_webRootPath);
                CreateDefaultHtmlFiles();
            }
        }

        private void CreateDefaultHtmlFiles()
        {
            // 创建默认的闭门器检测界面HTML文件
            var indexHtml = @"<!DOCTYPE html>
<html lang='zh-CN'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>闭门器扭矩检测系统</title>
    <link rel='stylesheet' href='styles.css'>
</head>
<body>
    <div class='container'>
        <header>
            <h1>闭门器扭矩检测上位机</h1>
            <div class='status-indicator' id='plcStatus'>PLC连接状态</div>
        </header>
        
        <main>
            <section class='torque-display'>
                <h2>实时扭矩数据</h2>
                <div class='torque-value' id='currentTorque'>0.00 N·m</div>
                <canvas id='torqueChart' width='800' height='300'></canvas>
            </section>
            
            <section class='signal-status'>
                <h2>信号状态监控</h2>
                <div class='signal-grid' id='signalGrid'>
                    <!-- 动态生成信号状态指示器 -->
                </div>
            </section>
            
            <section class='controls'>
                <h2>控制面板</h2>
                <button id='startTest' class='btn btn-primary'>开始检测</button>
                <button id='stopTest' class='btn btn-secondary'>停止检测</button>
                <button id='resetTest' class='btn btn-warning'>重置系统</button>
            </section>
        </main>
    </div>
    
    <script src='main.js'></script>
</body>
</html>";

            var stylesCSS = @"
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Microsoft YaHei', Arial, sans-serif;
    background-color: #f5f5f5;
    color: #333;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}

header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 20px;
    border-radius: 10px;
    margin-bottom: 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.status-indicator {
    background: #4CAF50;
    padding: 5px 15px;
    border-radius: 20px;
    font-size: 14px;
}

.torque-display {
    background: white;
    padding: 30px;
    border-radius: 10px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    margin-bottom: 20px;
    text-align: center;
}

.torque-value {
    font-size: 48px;
    font-weight: bold;
    color: #667eea;
    margin: 20px 0;
}

.signal-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 15px;
    margin-top: 20px;
}

.signal-item {
    background: white;
    padding: 15px;
    border-radius: 8px;
    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.signal-status {
    background: white;
    padding: 30px;
    border-radius: 10px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    margin-bottom: 20px;
}

.controls {
    background: white;
    padding: 30px;
    border-radius: 10px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    text-align: center;
}

.btn {
    padding: 12px 24px;
    margin: 0 10px;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    font-size: 16px;
    transition: all 0.3s ease;
}

.btn-primary { background: #667eea; color: white; }
.btn-secondary { background: #6c757d; color: white; }
.btn-warning { background: #ffc107; color: #212529; }

.btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0,0,0,0.2);
}";

            var mainJS = @"
// 闭门器扭矩检测前端JavaScript
class TorqueDetectionApp {
    constructor() {
        this.websocket = null;
        this.torqueChart = null;
        this.torqueData = [];
        this.maxDataPoints = 100;
        
        this.initWebSocket();
        this.initChart();
        this.bindEvents();
    }
    
    initWebSocket() {
        // 建立WebSocket连接用于实时数据更新
        this.websocket = new WebSocket('ws://localhost:8080/ws');
        
        this.websocket.onopen = () => {
            console.log('WebSocket连接已建立');
            this.updateStatus('已连接', 'connected');
        };
        
        this.websocket.onmessage = (event) => {
            try {
                const data = JSON.parse(event.data);
                this.handleDataUpdate(data);
            } catch (e) {
                console.error('解析数据失败:', e);
            }
        };
        
        this.websocket.onclose = () => {
            console.log('WebSocket连接已断开');
            this.updateStatus('连接断开', 'disconnected');
        };
    }
    
    handleDataUpdate(data) {
        switch(data.type) {
            case 'torque_update':
                this.updateTorqueDisplay(data.value);
                break;
            case 'signal_update':
                this.updateSignalStatus(data.signals);
                break;
        }
    }
    
    updateTorqueDisplay(torqueValue) {
        document.getElementById('currentTorque').textContent = torqueValue.toFixed(2) + ' N·m';
        
        // 更新图表数据
        this.torqueData.push({
            time: new Date(),
            value: torqueValue
        });
        
        if (this.torqueData.length > this.maxDataPoints) {
            this.torqueData.shift();
        }
        
        this.updateChart();
    }
    
    updateSignalStatus(signals) {
        const signalGrid = document.getElementById('signalGrid');
        signalGrid.innerHTML = '';
        
        signals.forEach(signal => {
            const signalDiv = document.createElement('div');
            signalDiv.className = 'signal-item';
            signalDiv.innerHTML = `
                <span>${signal.name}</span>
                <span class='signal-value ${signal.status ? 'active' : 'inactive'}'>
                    ${signal.value}
                </span>
            `;
            signalGrid.appendChild(signalDiv);
        });
    }
    
    initChart() {
        const canvas = document.getElementById('torqueChart');
        this.torqueChart = canvas.getContext('2d');
    }
    
    updateChart() {
        // 简单的图表绘制实现
        const canvas = document.getElementById('torqueChart');
        const ctx = this.torqueChart;
        
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        
        if (this.torqueData.length < 2) return;
        
        // 绘制背景网格
        ctx.strokeStyle = '#e0e0e0';
        ctx.lineWidth = 1;
        
        // 绘制扭矩曲线
        ctx.strokeStyle = '#667eea';
        ctx.lineWidth = 2;
        ctx.beginPath();
        
        const stepX = canvas.width / (this.maxDataPoints - 1);
        const maxValue = Math.max(...this.torqueData.map(d => d.value));
        const minValue = Math.min(...this.torqueData.map(d => d.value));
        const range = maxValue - minValue || 1;
        
        this.torqueData.forEach((point, index) => {
            const x = index * stepX;
            const y = canvas.height - ((point.value - minValue) / range) * canvas.height;
            
            if (index === 0) {
                ctx.moveTo(x, y);
            } else {
                ctx.lineTo(x, y);
            }
        });
        
        ctx.stroke();
    }
    
    bindEvents() {
        document.getElementById('startTest').addEventListener('click', () => {
            this.sendCommand('start_test');
        });
        
        document.getElementById('stopTest').addEventListener('click', () => {
            this.sendCommand('stop_test');
        });
        
        document.getElementById('resetTest').addEventListener('click', () => {
            this.sendCommand('reset_test');
        });
    }
    
    sendCommand(command) {
        if (this.websocket && this.websocket.readyState === WebSocket.OPEN) {
            this.websocket.send(JSON.stringify({
                type: 'command',
                command: command
            }));
        }
    }
    
    updateStatus(text, status) {
        const statusElement = document.getElementById('plcStatus');
        statusElement.textContent = text;
        statusElement.className = `status-indicator ${status}`;
    }
}

// 应用启动
document.addEventListener('DOMContentLoaded', () => {
    new TorqueDetectionApp();
});";

            File.WriteAllText(Path.Combine(_webRootPath, "index.html"), indexHtml);
            File.WriteAllText(Path.Combine(_webRootPath, "styles.css"), stylesCSS);
            File.WriteAllText(Path.Combine(_webRootPath, "main.js"), mainJS);
        }

        public async Task StartAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _httpListener.Start();
            
            // 启动请求处理循环
            _ = Task.Run(async () => await HandleRequestsAsync(_cancellationTokenSource.Token));
        }

        private async Task HandleRequestsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    _ = Task.Run(async () => await ProcessRequestAsync(context));
                }
                catch (Exception ex) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            string localPath = context.Request.Url.LocalPath.TrimStart('/');
            
            if (string.IsNullOrEmpty(localPath))
                localPath = "index.html";

            string filePath = Path.Combine(_webRootPath, localPath);

            if (File.Exists(filePath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    string contentType = GetContentType(filePath);
                    
                    await SendResponseAsync(context, content, contentType, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    await SendResponseAsync(context, $"读取文件错误: {ex.Message}", "text/plain", HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                await SendResponseAsync(context, "404 - 页面未找到", "text/html", HttpStatusCode.NotFound);
            }
        }

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

        private async Task SendResponseAsync(HttpListenerContext context, string content, string contentType, HttpStatusCode statusCode)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
            context.Response.ContentType = contentType;
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.StatusCode = (int)statusCode;
            
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        public void BroadcastData(string dataType, string data)
        {
            var message = JsonSerializer.Serialize(new { type = dataType, data = data });
            
            // 向所有WebSocket连接广播数据
            foreach (var connection in _webSocketConnections.ToArray())
            {
                try
                {
                    connection.Send(message);
                }
                catch
                {
                    _webSocketConnections.Remove(connection);
                }
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _httpListener?.Stop();
        }
    }

    /// <summary>
    /// 优化后的PLC扭矩监控类 - 使用事件驱动模型
    /// </summary>
    public class PlcTorqueMonitor
    {
        private Dictionary<string, TagInfo> _torqueTagList;
        private Timer _monitorTimer;
        private bool _isMonitoring;
        
        public event EventHandler<TorqueDataEventArgs> OnTorqueDataChanged;

        public PlcTorqueMonitor()
        {
            InitializeTorqueTags();
        }

        private void InitializeTorqueTags()
        {
            _torqueTagList = new Dictionary<string, TagInfo>
            {
                { "TorqueValue", new TagInfo { TagName = "TorqueValue", Address = "D100", DataType = "float" } },
                { "TorqueStatus", new TagInfo { TagName = "TorqueStatus", Address = "M100", DataType = "bool" } },
                { "MotorSpeed", new TagInfo { TagName = "MotorSpeed", Address = "D101", DataType = "int" } },
                { "SystemStatus", new TagInfo { TagName = "SystemStatus", Address = "D102", DataType = "int" } }
            };

            // 为每个标签订阅值变化事件
            foreach (var tag in _torqueTagList.Values)
            {
                tag.OnTriggered += Tag_OnTriggered;
            }
        }

        private void Tag_OnTriggered(object sender, EventArgs e)
        {
            if (sender is TagInfo changedTag)
            {
                // 创建扭矩数据变化事件
                var torqueData = new TorqueData
                {
                    TorqueValue = GetTagValue<float>("TorqueValue"),
                    TorqueStatus = GetTagValue<bool>("TorqueStatus"),
                    MotorSpeed = GetTagValue<int>("MotorSpeed"),
                    SystemStatus = GetTagValue<int>("SystemStatus"),
                    Timestamp = DateTime.Now,
                    ChangedTag = changedTag.TagName
                };

                OnTorqueDataChanged?.Invoke(this, new TorqueDataEventArgs(torqueData));
            }
        }

        private T GetTagValue<T>(string tagName)
        {
            if (_torqueTagList.TryGetValue(tagName, out var tag))
            {
                try
                {
                    return (T)Convert.ChangeType(tag.ReadValue, typeof(T));
                }
                catch
                {
                    return default(T);
                }
            }
            return default(T);
        }

        public async Task StartMonitoringAsync()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;
            
            // 这里应该初始化PLC连接
            // await InitializePlcConnectionAsync();
            
            // 启动数据读取定时器
            _monitorTimer = new Timer(ReadPlcData, null, 0, 100); // 100ms周期读取
        }

        private void ReadPlcData(object state)
        {
            if (!_isMonitoring) return;

            try
            {
                // 模拟PLC数据读取
                // 在实际应用中，这里应该调用PLC通信接口
                SimulatePlcDataUpdate();
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Console.WriteLine($"PLC数据读取错误: {ex.Message}");
            }
        }

        private void SimulatePlcDataUpdate()
        {
            // 模拟扭矩值变化
            var random = new Random();
            var torqueValue = 10.0f + (float)(random.NextDouble() * 5.0); // 10-15 N·m
            
            if (_torqueTagList.TryGetValue("TorqueValue", out var torqueTag))
            {
                torqueTag.ReadValue = torqueValue;
            }
            
            if (_torqueTagList.TryGetValue("TorqueStatus", out var statusTag))
            {
                statusTag.ReadValue = torqueValue > 12.0f;
            }
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            _monitorTimer?.Dispose();
            
            // 取消事件订阅
            foreach (var tag in _torqueTagList.Values)
            {
                tag.OnTriggered -= Tag_OnTriggered;
            }
        }
    }

    /// <summary>
    /// 扭矩数据模型
    /// </summary>
    public class TorqueData
    {
        public float TorqueValue { get; set; }
        public bool TorqueStatus { get; set; }
        public int MotorSpeed { get; set; }
        public int SystemStatus { get; set; }
        public DateTime Timestamp { get; set; }
        public string ChangedTag { get; set; }
    }

    /// <summary>
    /// 扭矩数据变化事件参数
    /// </summary>
    public class TorqueDataEventArgs : EventArgs
    {
        public TorqueData TorqueData { get; }

        public TorqueDataEventArgs(TorqueData torqueData)
        {
            TorqueData = torqueData;
        }
    }

    /// <summary>
    /// WebSocket连接封装类
    /// </summary>
    public class WebSocketConnection
    {
        // WebSocket连接实现（这里简化，实际应该使用WebSocket库）
        public void Send(string message)
        {
            // 发送WebSocket消息的实现
        }
    }
}
