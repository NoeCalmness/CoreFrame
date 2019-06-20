/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Config manager class.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-14
 * 
 ***************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;

public static class ConfigManager
{
    private static Dictionary<int, Config>      m_configs  = new Dictionary<int, Config>();
    private static Dictionary<long, ConfigItem> m_items    = new Dictionary<long, ConfigItem>();

    private static bool m_initialized = false;

    public static IEnumerator Initialize()
    {
        if (m_initialized) yield break;
        m_initialized = true;

        m_configs.Clear();
        m_items.Clear();

        Logger.LogDetail("ConfigManager initialized.");
    }

    public static IEnumerator LoadConfigs(Action<Config, int, int> onProgress = null)
    {
        m_configs.Clear();
        m_items.Clear();

        var assets = AssetManager.GetLoadedAssets("data");
        if (assets == null)
        {
            var op = AssetManager.LoadAssetAsync("data", typeof(Config));
            yield return op;

            assets = AssetManager.GetLoadedAssets("data");
        }

        if (assets == null)
        {
            Logger.LogError("Failed to load config data.");
            yield break;
        }

        var configs = new List<Config>();
        foreach (var asset in assets)
        {
            var config = asset as Config;
            if (!config)
            {
                Logger.LogError("ConfigManager::LoadConfigs: Asset bundle [data] has invalid config asset [{0}], ignore.", asset ? asset.name : "null");
                continue;
            }
            configs.Add(config);
            m_configs.Add(config.hash, config);
        }
        configs.Sort((a, b) => a.orderID < b.orderID ? -1 : 1);

        if (m_configs.Count < 1) Logger.LogWarning("Could not find any config assets!");

        var i = 0;
        foreach (var config in configs)
        {
            ++i;

            var chash  = config.hash;
            var hash   = (chash & 0xFFFFFFFF) << 32;

            config.ForEach(item =>
            {
                item.hash = chash;

                try { item.OnLoad(); }
                #if DEVELOPMENT_BUILD || UNITY_EDITOR
                catch (Exception e)
                {
                    Logger.LogException("ConfigItem::OnLoad: [{0}:{1}]", config.name, item.ID);
                    Logger.LogException(e);
                }
                #else
                catch { }
                #endif

                var index = hash | item.ID & 0xFFFFFFFF;
                #if DEVELOPMENT_BUILD || UNITY_EDITOR
                var old = m_items.Get(index);
                if (old) Logger.LogError("ConfigManager: Duplicated item index in <b><color={4}>[{0}:{1}]</color>, oldSource:<color={4}>[{2}]</color>, newSource:<color={4}>[{3}]</color></b>, use newer one.", config.name, item.ID, GetSource(old.___source), GetSource(item.___source), "#00DDFF");
                #endif
                m_items.Set(index, item);
            });

            onProgress?.Invoke(config, m_configs.Count, i);
        }

        ConfigItem lastItem = null;
        foreach (var item in m_items.Values) // Call InitializeOnce before all item initialized
        {
            if (lastItem == null || lastItem.hash != item.hash)
            {
                try { item.InitializeOnce(); }
                #if DEVELOPMENT_BUILD || UNITY_EDITOR
                catch (Exception e)
                {
                    Logger.LogException("ConfigItem::OnLoad: [{0}:{1}]", item.GetType(), item.ID);
                    Logger.LogException(e);
                }
                #else
                catch { }
                #endif
            }
            lastItem = item;
        }

        // Initialize all config items
        foreach (var item in m_items.Values)
        {
            try { item.Initialize(); }
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            catch (Exception e)
            {
                Logger.LogException("ConfigItem::OnLoad: [{0}:{1}]", item.GetType(), item.ID);
                Logger.LogException(e);
            }
            #else
            catch { }
            #endif
        }

        Logger.LogDetail("Config load complete! {0} config items loaded.", m_items.Count);

        Root.instance.DispatchEvent(Events.CONFIG_LOADED);
    }

    public static T Get<T>(int id) where T : ConfigItem, new()
    {
        return m_items.Get((typeof(T).GetHashCode() & 0xFFFFFFFF) << 32 | id & 0xFFFFFFFF) as T;
    }

    public static T Find<T>(Predicate<T> match) where T : ConfigItem
    {
        var config = m_configs.Get(typeof(T).GetHashCode()) as Config<T>;
        if (!config) return null;
        var items = config.items;

        foreach (var item in items)
            if (match(item)) return item;

        return null;
    }

    public static List<T> FindAll<T>(Predicate<T> match) where T : ConfigItem
    {
        var iis = new List<T>();

        var config = m_configs.Get(typeof(T).GetHashCode()) as Config<T>;
        if (!config) return iis;
        var items = config.items;

        foreach (var item in items)
            if (match(item)) iis.Add(item);

        return iis;
    }

    public static List<T> GetAll<T>() where T : ConfigItem
    {
        var items = new List<T>();
        var config = m_configs.Get(typeof(T).GetHashCode()) as Config<T>;
        if (config) items.AddRange(config.items);
        return items;
    }

    public static void Foreach<T>(Predicate<T> call) where T : ConfigItem
    {
        var config = m_configs.Get(typeof(T).GetHashCode()) as Config<T>;
        if (!config) return;

        var items = config.items;
        foreach (var item in items)
        {
            if (!item) continue;
            if (!call(item)) return;
        }
    }

    public static string GetSource(int source)
    {
        #if UNITY_EDITOR
        return _GetSource(source);
        #else
        return source.ToString();
        #endif
    }

    #region Editor helper

#if UNITY_EDITOR
    private static Dictionary<int, string> m_sourceMap = new Dictionary<int, string>();
    private static string _GetSource(int source)
    {
        var s = m_sourceMap?.Get(source, string.Empty);
        if (s != string.Empty) return s;

        m_sourceMap = new Dictionary<int, string>();

        var p  = System.Text.RegularExpressions.Regex.Replace(UnityEngine.Application.dataPath.Replace("/Assets", "/XMLConfigs/"), @"Client\d+", "Client");
        var fs = System.IO.Directory.Exists(p) ? System.IO.Directory.GetFiles(p, "*", System.IO.SearchOption.AllDirectories) : new string[] { };
        foreach (var f in fs)
        {
            var ff = f.Substring(f.IndexOf("XMLConfigs") + 11).Replace("\\", "/");
            var ss = ff.GetHashCode();
            m_sourceMap.Set(ss, ff);

            if (ss == source) s = ff;
        }

        return s;
    }

    public static bool ConfigLoaded(string configName)
    {
        var keys = m_configs.Values;
        foreach (var key in keys) if (key && key.name == configName) return true;
        return false;
    }

    public static void EnsureLoad(string configName)
    {
        if (!ConfigLoaded(configName)) ReloadConfig(configName);
    }

    public static void ReloadConfig(string configName)
    {
        var config = UnityEditor.AssetDatabase.LoadAssetAtPath<Config>("Assets/Data/" + configName + ".asset");
        if (!config) return;

        m_configs.Set(config.hash, config);

        var keys = new List<long>(m_items.Keys);
        foreach (var key in keys)
        {
            if (key >> 32 == config.hash)
                m_items.Remove(key);
        }

        var items = config.GetItemsBase();
        foreach (var item in items)
        {
            item.hash = config.hash;
            item.OnLoad();

            var index = (item.hash & 0xFFFFFFFF) << 32 | item.ID & 0xFFFFFFFF;
            var old = m_items.Get(index);
            if (old) Logger.LogError("ConfigManager: Duplicated item index in <b><color={4}>[{0}:{1}]</color>, oldSource:<color={4}>[{2}]</color>, newSource:<color={4}>[{3}]</color></b>, ignored.", config.name, item.ID, GetSource(old.___source), GetSource(item.___source), "#00DDFF");
            else m_items.Add(index, item);
        }

        if (items.Count > 0) items[0].InitializeOnce();
        foreach (var item in items) item.Initialize();

        Logger.LogInfo("Config [{0}] reloaded!", configName);

        if (Root.instance) Root.instance.DispatchEvent("EditorReloadConfig", Event_.Pop(configName));
    }
#endif

    #endregion
}
