# 研究计划：闭门器扭矩检测上位机项目分析

## 目标
深入分析 `winformWebpages` 和 `line-control1` 两个项目，理解其现有技术实现，为闭门器扭矩检测上位机的技术优化提供清晰、可行的方案。

## 研究任务分解

### 第一阶段：`winformWebpages` 项目分析 (预估1小时)
1.  **`HttpServer.cs.cs` 分析**:
    -   理解 HTTP 请求的监听、接收和响应流程。
    -   分析路由处理逻辑，确定请求如何映射到具体处理程序。
2.  **HTML 内容获取分析**:
    -   分析 `indexPage.cs`，确认 HTML 内容是否以硬编码字符串形式存在。
    -   查看 `FullPage.html`，了解其基础结构和内容。
    -   分析 `Form1.cs`，了解 HTTP 服务器如何被启动和集成。
3.  **架构评估**:
    -   总结 `htmlPages` 文件夹的作用和设计模式。
    -   评估当前架构的优点（如简单、无依赖）和缺点（如难以维护、不易扩展）。

### 第二阶段：`line-control1` 项目 PLC 交互分析 (预估1.5小时)
1.  **通信协议分析**:
    -   研究 `Transystem.Lib.PLC/` 目录下的 `OmronPlcFinsTcp.cs`, `MelsecPlcMcTcp.cs` 等文件，确定支持的 PLC 品牌和通信协议 (如 Omron FINS, Mitsubishi MC)。
    -   分析 `IPLCCommuncation.cs` 接口，了解通信库的核心抽象。
2.  **信号处理与数据结构分析**:
    -   分析 `PLCScriptRun.cs`，理解 PLC 脚本的执行逻辑和信号读写操作。
    -   研究 `TbEquipmentSignal.cs`，确定设备信号点的数据模型。
    -   查看 `FrmPLC.cs` 和 `FrmEquipmentSignal.cs`，了解 UI 如何与 PLC 数据交互及监控信号状态。
3.  **通信流程与机制总结**:
    -   绘制 PLC 通信的数据流图。
    -   总结事件处理和数据更新机制。

### 第三阶段：技术优化方案设计与报告撰写 (预估2小时)
1.  **HTML 获取方式优化**:
    -   设计从文件系统直接读取 `.html` 文件的方案。
    -   编写 C# 示例代码 `html_optimization_sample.cs`，演示如何实现该方案。
2.  **PLC 信号监控集成方案**:
    -   设计将 `line-control1` 中的 PLC 监控逻辑集成到新项目的方案。
    -   提供配置和代码示例 `plc_integration_sample.cs`。
3.  **事件处理机制设计**:
    -   提出一个更通用、更解耦的事件处理模型（例如，使用事件聚合器或发布/订阅模式）。
4.  **整体架构改进建议**:
    -   建议采用更现代的架构，如前后端分离，使用Web框架（如ASP.NET Core）替代底层HTTP服务器。
5.  **撰写技术分析报告**:
    -   创建 `/workspace/docs/项目技术分析报告.md`。
    -   综合所有分析结果和优化建议，撰写详细报告，包含架构图、代码示例和部署建议。

## 关键问题
1.  `winformWebpages` 中 HTML 是如何动态生成并响应给客户端的？
2.  `line-control1` 是如何实现与不同品牌 PLC 的兼容性的？
3.  PLC 信号的实时监控是采用轮询还是订阅模式？
4.  如何将两个项目的优点结合，并改进其缺点，形成一个稳定、可扩展的新架构？

## 资源策略
-   **主要分析工具**: `file_read`
-   **编程语言**: C#
-   **输出格式**: Markdown, C#

## 交付成果
-   `docs/research_plan_上位机项目分析.md` - 本研究计划。
-   `docs/项目技术分析报告.md` - 最终的详细技术分析报告。
-   `code/html_optimization_sample.cs` - HTML 获取优化方案的示例代码。
-   `code/plc_integration_sample.cs` - PLC 集成方案的示例代码。
