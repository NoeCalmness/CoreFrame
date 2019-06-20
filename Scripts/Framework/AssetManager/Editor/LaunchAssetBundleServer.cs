/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Local asset bundle server for testing in editor mode
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-02
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System;

namespace AssetBundles
{
    internal class AssetBundleServer : ScriptableSingleton<AssetBundleServer>
    {
        [SerializeField]
        int m_ServerPID = 0;

        public static bool IsRunning()
        {
            if (instance.m_ServerPID == 0) return false;

            try
            {
                var process = Process.GetProcessById(instance.m_ServerPID);
                if (process == null) return false;

                return !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        public static void KillRunningAssetBundleServer()
        {
            try
            {
                if (instance.m_ServerPID == 0)  return;

                var lastProcess = Process.GetProcessById(instance.m_ServerPID);
                lastProcess.Kill();
                instance.m_ServerPID = 0;
            }
            catch { }
        }

        public static void Run()
        {
            var serverPath = Path.GetFullPath("Assets/Scripts/Framework/AssetManager/Editor/AssetBundleServer.exe");
            var bundlePath = Path.Combine(Environment.CurrentDirectory, "AssetBundles");

            KillRunningAssetBundleServer();

            BuildScript.CreateAssetBundleDirectory();

            var args = bundlePath;
            args = string.Format("\"{0}\" {1}", args, Process.GetCurrentProcess().Id);

            var startInfo = new ProcessStartInfo(serverPath, args);

            startInfo.WorkingDirectory = bundlePath;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            var launchProcess = Process.Start(startInfo);
            if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
            {
                //Unable to start process
                Logger.LogError("AssetBundleServer::Run: Unable Start AssetBundleServer process");
            }
            else
            {
                //We seem to have launched, let's save the PID
                instance.m_ServerPID = launchProcess.Id;
            }
        }
    }
}
