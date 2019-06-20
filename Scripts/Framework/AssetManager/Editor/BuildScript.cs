/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Editor helper script for asset bundle building
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-11-02
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AssetBundles
{
    public class BuildScript
    {
        public const string AssetBundlesOutputPath = "AssetBundles";

        static public string CreateAssetBundleDirectory(bool deleteOld = false)
        {
            var outputPath = Path.Combine(AssetBundlesOutputPath, Util.GetPlatformName()).Replace("\\", "/");
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
            else if (deleteOld)
            {
                FileUtil.DeleteFileOrDirectory(outputPath);
                Directory.CreateDirectory(outputPath);
            }

            return outputPath;
        }

        /// <summary>
        /// Build all asset bundles
        /// </summary>
        /// <param name="incremental">Incremental build ?</param>
        public static void BuildAssetBundles(bool incremental = false)
        {
            Logger.LogInfo("AssetBundles::BuildAssetBundles: Start building asset bundles... Incremental: {0}", incremental);

            var outputPath = CreateAssetBundleDirectory(!incremental);
            var option = incremental ? BuildAssetBundleOptions.None : BuildAssetBundleOptions.ForceRebuildAssetBundle;

            option |= BuildAssetBundleOptions.DisableWriteTypeTree;

            BuildPipeline.BuildAssetBundles(outputPath, option, EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// Build all asset bundles
        /// </summary>
        /// <param name="incremental">Incremental build ?</param>
        public static void BuildAssetBundles(AssetBundleBuild[] builds)
        {
            if (builds == null || builds.Length < 1) return;

            Logger.LogInfo("AssetBundles::BuildAssetBundles: Start building asset bundles from array... Count: {0}", builds.Length);

            var outputPath = CreateAssetBundleDirectory();

            var oldPath = GetAssetBundleManifestFilePath();
            var oldMPath = oldPath.Replace(".manifest", "");
            var back = File.Exists(oldPath);
            var backM = back && File.Exists(oldMPath);

            if (backM)
            {
                File.Copy(oldPath,  oldPath  + ".bak", true);
                File.Copy(oldMPath, oldMPath + ".bak", true);

                Logger.LogDetail("AssetBundles::BuildAssetBundles: Move old manifest file to {0}", oldPath + ".bak");
            }

            BuildPipeline.BuildAssetBundles(outputPath, builds, BuildAssetBundleOptions.DisableWriteTypeTree, EditorUserBuildSettings.activeBuildTarget);

            if (backM)
            {
                FileUtil.DeleteFileOrDirectory(oldPath);
                FileUtil.DeleteFileOrDirectory(oldMPath);

                FileUtil.MoveFileOrDirectory(oldPath  + ".bak", oldPath);
                FileUtil.MoveFileOrDirectory(oldMPath + ".bak", oldMPath);

                Logger.LogDetail("AssetBundles::BuildAssetBundles: Restore old manifest file to {0}", oldPath);
            }
        }

        public static void BuildPlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if (outputPath.Length == 0) return;

            var levels = GetLevelsFromBuildSettings();
            if (levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            var targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if (targetName == null) return;

            // Build and copy AssetBundles.
            BuildAssetBundles();

            var options = new BuildPlayerOptions();
            options.scenes = levels;
            options.locationPathName = outputPath + targetName;
            options.assetBundleManifestPath = GetAssetBundleManifestFilePath();
            options.target  = EditorUserBuildSettings.activeBuildTarget;
            options.options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(options);
        }

        public static void BuildStandalonePlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if (outputPath.Length == 0) return;

            var levels = GetLevelsFromBuildSettings();
            if (levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            var targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if (targetName == null) return;

            // Build and copy AssetBundles.
            BuildAssetBundles();

            CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, AssetBundlesOutputPath));

            AssetDatabase.Refresh();

            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = levels;
            options.locationPathName = outputPath + targetName;
            options.assetBundleManifestPath = GetAssetBundleManifestFilePath();
            options.target = EditorUserBuildSettings.activeBuildTarget;
            options.options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(options);
        }

        public static string GetBuildTargetName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "/HYLR.apk";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "/HYLR.exe";
                case BuildTarget.StandaloneOSX:
                    return "/HYLR.app";
                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                    return "";
                default:
                    Logger.LogError("Target not implemented.");
                    return null;
            }
        }

        static void CopyAssetBundlesTo(string outputPath)
        {
            // Clear streaming assets folder.
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            Directory.CreateDirectory(outputPath);

            var outputFolder = Util.GetPlatformName();

            // Setup the source folder for assetbundles.
            var source = Path.Combine(Path.Combine(System.Environment.CurrentDirectory, AssetBundlesOutputPath), outputFolder);
            if (!Directory.Exists(source))
                Debug.Log("No assetBundle output folder, try to build the assetBundles first.");

            // Setup the destination folder for assetbundles.
            var destination = Path.Combine(outputPath, outputFolder);
            if (Directory.Exists(destination))
                FileUtil.DeleteFileOrDirectory(destination);

            FileUtil.CopyFileOrDirectory(source, destination);
        }

        static string[] GetLevelsFromBuildSettings()
        {
            var levels = new List<string>();
            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
                if (scene.enabled)
                    levels.Add(scene.path);

            return levels.ToArray();
        }

        static string GetAssetBundleManifestFilePath()
        {
            return Path.Combine(Path.Combine(AssetBundlesOutputPath, Util.GetPlatformName()),  Util.GetPlatformName()) + ".manifest";
        }
    }
}
