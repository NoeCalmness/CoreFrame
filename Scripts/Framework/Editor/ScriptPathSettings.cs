/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Manage script path.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-05
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ScriptPathSettings : EditorWindow
{
    public static string GetPath(string language)
    {
        var l = language.ToLower();
        return EditorPrefs.GetString("cache_protocol_path_" + l);
    }

    private string CSharp   = "";
    private string CPP      = "";
    private string Erlang   = "";
    private string Go       = "";
    private string Java     = "";

    private void Awake()
    {
        CSharp = EditorPrefs.GetString("cache_protocol_path_c#");
        CPP    = EditorPrefs.GetString("cache_protocol_path_c++");
        Erlang = EditorPrefs.GetString("cache_protocol_path_erlang");
        Go     = EditorPrefs.GetString("cache_protocol_path_go");
        Java   = EditorPrefs.GetString("cache_protocol_path_java");

        if (string.IsNullOrEmpty(CSharp)) CSharp = Application.dataPath + "/Protocols/";
        if (string.IsNullOrEmpty(CPP))    CPP    = ProtocolGenerator.LanguageImplemented("C++")    ? EditorUtil.GetProjectName(true) + "Protocols/CPP/"    : "Language not supported" ;
        if (string.IsNullOrEmpty(Erlang)) Erlang = ProtocolGenerator.LanguageImplemented("Erlang") ? EditorUtil.GetProjectName(true) + "Protocols/Erlang/" : "Language not supported";
        if (string.IsNullOrEmpty(Go))     Go     = ProtocolGenerator.LanguageImplemented("Go")     ? EditorUtil.GetProjectName(true) + "Protocols/Go/"     : "Language not supported";
        if (string.IsNullOrEmpty(Java))   Java   = ProtocolGenerator.LanguageImplemented("Java")   ? EditorUtil.GetProjectName(true) + "Protocols/Java/"   : "Language not supported";
    }

    void OnGUI()
    {
        GUILayout.Label("Protocol script paths", EditorStyles.boldLabel);

        CSharp = EditorGUILayout.TextField("C#:     ", CSharp);
        CPP    = EditorGUILayout.TextField("C++:    ", CPP);
        Erlang = EditorGUILayout.TextField("Erlang: ", Erlang);
        Go     = EditorGUILayout.TextField("Go:     ", Go);
        Java   = EditorGUILayout.TextField("Java:   ", Java);

        GUILayout.Space(15);

        if (GUILayout.Button("Save")) Save();
    }

    private void Save()
    {
        EditorPrefs.SetString("cache_protocol_path_c#",     CSharp);
        EditorPrefs.SetString("cache_protocol_path_c++",    ProtocolGenerator.LanguageImplemented("C++")    ? CPP     : "");
        EditorPrefs.SetString("cache_protocol_path_erlang", ProtocolGenerator.LanguageImplemented("Erlang") ? Erlang  : "");
        EditorPrefs.SetString("cache_protocol_path_go",     ProtocolGenerator.LanguageImplemented("Go")     ? Go      : "");
        EditorPrefs.SetString("cache_protocol_path_java",   ProtocolGenerator.LanguageImplemented("Java")   ? Java    : "");
    }
}
