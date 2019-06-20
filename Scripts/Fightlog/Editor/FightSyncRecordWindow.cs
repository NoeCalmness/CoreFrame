using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class FightSyncRecordWindow : EditorWindow
{
    private string logFile;
    private string recoverPath;
    private string logPath;
    private readonly Dictionary<int, ArrayList> cache = new Dictionary<int, ArrayList>();

    [MenuItem("HYLR/RecordWindow &R")]
    public static void Init()
    {
        var w = EditorWindow.CreateInstance<FightSyncRecordWindow>();
        w.Show();
    }

    private enum OperatorType
    {
        Record,
        Recover,
        TransformToJson,
        TransformLog,

        Max
    }

    private OperatorType currentOperator;
    private GUIStyle miniButton;

    void OnGUI()
    {
        UpdateGUIStyles();

        if (!Application.isPlaying)
        {
            TransformLog();
            return;
        }

        FightRecordManager.bRecordLog = EditorGUILayout.BeginToggleGroup("记录战斗日志", FightRecordManager.bRecordLog);
        EditorGUILayout.BeginFadeGroup(FightRecordManager.bRecordLog ? 1 : .4f);
        {
            FightRecordManager.bLogStack = EditorGUILayout.Toggle("是否打印堆栈", FightRecordManager.bLogStack);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndToggleGroup();


        EditorGUILayout.BeginVertical(GUI.skin.box);
        currentOperator = (OperatorType)GUILayout.SelectionGrid((int)currentOperator, new string[] { "记录战斗数据", "播放录像", "把录像格式转为Json格式", "日志格式转换"}, (int)OperatorType.Max);
        if (currentOperator == OperatorType.Record)
        {
            ShowRecord();
        }
        else if (currentOperator == OperatorType.Recover)
        {
            ShowRecover();
        }
        else if (currentOperator == OperatorType.TransformToJson)
            TransformToJson();
        else if (currentOperator == OperatorType.TransformLog)
            TransformLog();

        EditorGUILayout.EndVertical();
    }

    private void TransformLog()
    {
        var assembly = Assembly.GetAssembly(typeof(PhysicsManager));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Log File", logFile, GUI.skin.label);
        if (GUILayout.Button("...", miniButton))
        {
            logFile = EditorUtility.OpenFilePanel("请选择日志文件", logFile, "log");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("开始转换"))
        {
            TransformLog(logFile, assembly);
            EditorUtility.DisplayDialog("日志文件格式转换", "转换成功", "ok");
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Log Path", logPath, GUI.skin.label);
        if (GUILayout.Button("...", miniButton))
        {
            logPath = EditorUtility.OpenFolderPanel("选择日志目录", logPath, "");
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("转换整个目录的日志"))
        {
            var dir = new DirectoryInfo(logPath);
            var files = dir.GetFiles("*.log", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                TransformLog(files[i].FullName, assembly);
            }
        }
    }

    private void TransformLog(string rLogFile, Assembly assembly)
    {
        if (!rLogFile.EndsWith(".log"))
        {
            EditorUtility.DisplayDialog("错误提示", "不是有效的日志文件,只能转换.log后缀的文件", "确定");
            return;
        }

        var file = rLogFile;
        PacketObject.CollectAllRegisteredPackets();
        Stream stream = File.OpenRead(file);

        if (file.EndsWith(".log"))
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream = new MemoryStream(Util.DecompressData(bytes));
        }

        StringBuilder sb = new StringBuilder();
        Command command = Command.Create();
        while (stream.CanRead && stream.Position < stream.Length)
        {
            command.UnSerialize(stream);
            EditorUtility.DisplayProgressBar("格式转换", command.cache._name, (float) stream.Position/stream.Length);

            var stack = command.cache as LogStack;
            if (stack != null)
            {
                for (var i = 0; i < stack.method.Length; i++)
                {
                    var m = stack.method[i];
                    ArrayList arr;
                    if (!cache.TryGetValue(m.typeHash, out arr))
                    {
                        arr = TypeSearch.Search(t => t.Name.GetHashCode() == m.typeHash, assembly);
                        cache.Add(m.typeHash, arr);
                    }
                    if (arr.Count > 0)
                    {
                        var type = (System.Type) arr[0];
                        if (type != null)
                        {
                            var methods = type.GetMethods();
                            if (m.methodIndex < methods.Length)
                            {
                                var method = methods[m.methodIndex];
                                sb.AppendLine($"({type.Name})   {method}");
                            }
                        }
                    }
                }
                continue;
            }

            var s = LitJson.JsonMapper.ToJson(command.cache);
            var index = s.IndexOf("tag");
            if (index >= 0)
            {
                var endIndex = s.IndexOf(',', index);
                var old = s.Substring(index, endIndex > index ? endIndex - index : s.Length - index);
                s = s.Replace(old,
                    $"tag\":\"{(TagType) (byte) command.cache.GetType().GetField("tag").GetValue(command.cache)}\"");
            }
            sb.AppendLine(s);
        }
        command.Destroy();
        EditorUtility.ClearProgressBar();
        var writer = File.CreateText(file.Replace(".log", ".txt"));
        writer.Write(sb.ToString());
        writer.Close();
    }

    private void TransformToJson()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("RecoverDataPath", recoverPath, GUI.skin.label);
        if (GUILayout.Button("...", miniButton))
        {
            recoverPath = EditorUtility.OpenFilePanel("请选择录像数据路径", recoverPath, "gr");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("开始转换"))
        {
            DisplayError(FightRecordManager.TransformToJson(recoverPath));
        }
    }

    private void UpdateGUIStyles()
    {
        if (miniButton == null)
            miniButton = new GUIStyle(GUI.skin.button) { fixedWidth = 30 };
    }

    private void ShowRecover()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("RecoverDataPath", recoverPath, GUI.skin.label);
        if (GUILayout.Button("...", miniButton))
        {
            recoverPath = EditorUtility.OpenFilePanel("请选择录像数据路径", recoverPath, "*.*");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.IntField("当前帧", FightRecordManager.Frame);
        FightRecordManager.DebugFrame = EditorGUILayout.IntField("DebugFrame", FightRecordManager.DebugFrame);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.SelectableLabel("播放速度");
        FightRecordManager.PlaySpeed = EditorGUILayout.Slider(FightRecordManager.PlaySpeed, 0.01f, 5);
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("开始播放录像"))
        {
            var error = FightRecordManager.StartRecover(recoverPath);
            DisplayError(error);
        }
        EditorGUILayout.EndVertical();
    }

    private static void DisplayError(GameRecover.EnumPlayError error)
    {
        if (error != GameRecover.EnumPlayError.None)
        {
            var str = string.Empty;
            switch (error)
            {
                case GameRecover.EnumPlayError.FileTypeError:
                    str = "文件后缀名错误,后缀名必须是(*.gr)";
                    break;
                case GameRecover.EnumPlayError.FileDontExits:
                    str = "文件路径错误";
                    break;
                case GameRecover.EnumPlayError.FileContentError:
                    str = "文件内容错误。不是正常的录像文件,或者里面根本没有内容";
                    break;
            }
            EditorUtility.DisplayDialog("播放录像失败", str, "确定");
        }
    }

    private void ShowRecord()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        FightRecordManager.bAutoSaveRecord = EditorGUILayout.Toggle("是否自动保存游戏录像", FightRecordManager.bAutoSaveRecord);

        if (GUILayout.Button("结束录制"))
        {
            FightRecordManager.EndRecord(true, false);
        }

        EditorGUILayout.EndVertical();
    }
}
