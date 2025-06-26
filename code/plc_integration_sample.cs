// PLC信号监控集成方案示例

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Transystem.Lib.PLC; // 引用包含TagInfo的库

/// <summary>
/// 演示如何将PLC信号监控与UI解耦，并使用事件驱动模型进行优化的示例。
/// </summary>
public class PlcMonitoringIntegration
{
    // 假设这是从line-control1的CurrInfo等地方获取的Tag列表
    private Dictionary<string, TagInfo> _plcTagList;
    private DataGridView _dataGridView; // UI上的表格控件

    /// <summary>
    /// 初始化监控集成
    /// </summary>
    /// <param name="tagList">从PLC层获取的信号点字典</param>
    /// <param name="dgv">需要更新数据的UI表格</param>
    public PlcMonitoringIntegration(Dictionary<string, TagInfo> tagList, DataGridView dgv)
    {
        _plcTagList = tagList;
        _dataGridView = dgv;

        // 关键步骤：为每个需要监控的Tag订阅事件
        SubscribeToTagEvents();
    }

    /// <summary>
    /// 遍历所有Tag并订阅其OnTriggered事件
    /// </summary>
    private void SubscribeToTagEvents()
    {
        foreach (var pair in _plcTagList)
        {
            var tag = pair.Value;
            // 为每个Tag的OnTriggered事件关联一个处理方法
            tag.OnTriggered += Tag_OnTriggered;
        }
    }

    /// <summary>
    /// 事件处理核心方法：当任何一个Tag的值发生变化时，此方法会被调用
    /// </summary>
    /// <param name="sender">触发事件的TagInfo对象</param>
    /// <param name="e">事件参数</param>
    private void Tag_OnTriggered(object sender, EventArgs e)
    {
        if (sender is TagInfo changedTag)
        {
            // 使用Invoke来确保UI更新在主线程上执行，避免跨线程异常
            if (_dataGridView.InvokeRequired)
            {
                _dataGridView.Invoke(new Action(() => UpdateGridRow(changedTag)));
            }
            else
            {
                UpdateGridRow(changedTag);
            }
        }
    }

    /// <summary>
    /// 更新DataGridView中与changedTag对应的特定行
    /// </summary>
    /// <param name="changedTag">值已改变的Tag</param>
    private void UpdateGridRow(TagInfo changedTag)
    {
        // 遍历表格的行，找到与变化了的Tag对应的那一行
        foreach (DataGridViewRow row in _dataGridView.Rows)
        { 
            // 假设TagName是唯一的，并且存储在第一列
            if (row.Cells[0].Value != null && row.Cells[0].Value.ToString() == changedTag.TagName)
            {
                // 只更新需要变化的单元格，而不是刷新整行或整个表格
                row.Cells["ReadValue"].Value = changedTag.ReadValue?.ToString();
                row.Cells["UpdateTime"].Value = changedTag.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                row.Cells["IsChanged"].Value = true; // 可以在这里高亮显示变化
                
                // 添加一个简单的视觉反馈，比如让变化的行背景色闪一下
                HighlightRow(row);

                // 找到并更新后即可退出循环
                break; 
            }
        }
    }

    private async void HighlightRow(DataGridViewRow row)
    {
        var originalColor = row.DefaultCellStyle.BackColor;
        row.DefaultCellStyle.BackColor = Color.Yellow; // 高亮颜色
        await Task.Delay(500); // 持续高亮0.5秒
        row.DefaultCellStyle.BackColor = originalColor; // 恢复原色
    }

    /// <summary>
    /// 在窗体关闭或功能停用时，必须取消订阅以防止内存泄漏
    /// </summary>
    public void UnsubscribeEvents()
    {
        foreach (var pair in _plcTagList)
        {
            pair.Value.OnTriggered -= Tag_OnTriggered;
        }
    }
}

/*
// 在 FrmPLC.cs 中的使用方式：
public partial class FrmPLC : Form
{
    private PlcMonitoringIntegration _monitorIntegration;

    public FrmPLC(string equipmentName)
    {
        InitializeComponent();
        // ...
    }

    private void FrmPLC_Load(object sender, EventArgs e)
    {
        // 1. 加载PLC的Tag列表到dgvPLC (仅首次加载时完全填充)
        InitializeGridWithPlcData(); 

        // 2. 初始化监控集成，传入PLC数据源和UI控件
        _monitorIntegration = new PlcMonitoringIntegration(CurrInfo.LoaderTagList, dgvPLC);

        // 3. 移除或禁用原来的Timer和BackgroundWorker，不再需要轮询！
        // tmrPLC.Enabled = false;
    }

    private void FrmPLC_FormClosing(object sender, FormClosingEventArgs e)
    {
        // 4. 窗体关闭时，取消事件订阅
        _monitorIntegration?.UnsubscribeEvents();
    }

    // ... 其他代码 ...
}
*/
