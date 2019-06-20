using UnityEngine;
using System.Text;
using System.Diagnostics;

public class SystemInfoDebug : MonoBehaviour
{
    #region 打印数据
    public static StringBuilder loadInfo = new StringBuilder();
    public static Stopwatch stopwatch = new Stopwatch();

    public static void ClearMessage()
    {
        loadInfo.Remove(0,loadInfo.Length);
        loadInfo.AppendLine(Util.Format("{0} 开始加载....",Time.realtimeSinceStartup));
    }

    public static void DebugMessage(string msg,bool isStart = true)
    {
        if(isStart)
        {
            stopwatch.Reset();
            stopwatch.Start();
            loadInfo.AppendLine(Util.Format("real time : {1} :start msg:{0}   ", msg,Time.realtimeSinceStartup));
        }
        else
        {
            stopwatch.Stop();
            loadInfo.AppendLine(Util.Format("real time : {2} :stop msg:{0}   duraction = {1}", msg, stopwatch.ElapsedMilliseconds,Time.realtimeSinceStartup));
            loadInfo.AppendLine();
        }

    }

    public static void LogMessage(string msg)
    {
        loadInfo.AppendLine(Util.Format("******real time : {1} : --- log msg:{0}   ", msg, Time.realtimeSinceStartup));
    }

    #endregion

    #region 获取设备信息

    //存储临时字符串
    StringBuilder info = new System.Text.StringBuilder();

    private void GetDeviceInfo()
    {
        info.AppendLine("设备与系统信息:");
        GetMessage("设备模型", SystemInfo.deviceModel);
        GetMessage("设备名称", SystemInfo.deviceName);
        GetMessage("设备类型（PC电脑，掌上型）", SystemInfo.deviceType.ToString());
        GetMessage("系统内存大小MB", SystemInfo.systemMemorySize.ToString());
        GetMessage("操作系统", SystemInfo.operatingSystem);
        GetMessage("处理器数量", SystemInfo.processorCount.ToString());
        GetMessage("处理器频率", SystemInfo.processorFrequency.ToString());
        GetMessage("处理器名称", SystemInfo.processorType);
        GetMessage("设备唯一标识符", SystemInfo.deviceUniqueIdentifier);
        GetMessage("显卡名称", SystemInfo.graphicsDeviceName);
        GetMessage("显卡类型", SystemInfo.graphicsDeviceType.ToString());
        GetMessage("显卡版本号", SystemInfo.graphicsDeviceVersion);
        GetMessage("显存大小MB", SystemInfo.graphicsMemorySize.ToString());
        GetMessage("显卡是否支持多线程渲染", SystemInfo.graphicsMultiThreaded.ToString());
        GetMessage("支持的渲染目标数量", SystemInfo.supportedRenderTargetCount.ToString());
    }

    void GetMessage(params string[] str)
    {
        if (str.Length == 2)
        {
            info.AppendLine(str[0] + ":" + str[1]);
        }
    }
    #endregion

    #region Debug

    #if UNITY_EDITOR
    private void OnGUI()
    {
        if (Root.simulateReleaseMode) return;

        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;
        bb.normal.textColor = Color.red;
        bb.fontSize = 35;
        GUILayout.Label(loadInfo.ToString(), bb);
    }
    #endif

    #endregion
}