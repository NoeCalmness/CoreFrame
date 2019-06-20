/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Custom FileSystemWatcher for auto script generate.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-04-05
 * 
 ***************************************************************************************************/

using System.IO;
using System.Collections.Generic;
using System.Threading;

public delegate void OnFileSystemChanged(WaitForChangedResult r);

public class FileWatcher
{
    private FileSystemWatcher m_watcher = null;
    private OnFileSystemChanged m_handler = null;
    private Thread m_thread = null;

    private Queue<WaitForChangedResult> m_queue = new Queue<WaitForChangedResult>();
    private Queue<WaitForChangedResult> m_tmp = new Queue<WaitForChangedResult>();

    public FileSystemWatcher watcher { get { return m_watcher; } }

    public OnFileSystemChanged handler { get { return m_handler; } set { m_handler = value; } }

    public FileWatcher(string path, OnFileSystemChanged onChanged, string filter = "*.*", bool sub = true)
    {
        m_watcher = new FileSystemWatcher(path, filter);
        m_watcher.IncludeSubdirectories = sub;
        m_watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

        m_handler = onChanged;
        m_thread = new Thread(_StartWatch);

        UnityEditor.EditorApplication.update += Update;
    }

    public void StartWatch()
    {
        if (m_thread.IsAlive) return;
        m_thread.Start();
    }

    public void StopWatch()
    {
        m_thread.Interrupt();
    }

    private void _StartWatch()
    {
        m_watcher.EnableRaisingEvents = true;

        while (m_thread.IsAlive)
            m_queue.Enqueue(m_watcher.WaitForChanged(WatcherChangeTypes.All));
    }

    private void _StopWatch()
    {
        m_watcher.EnableRaisingEvents = false;
    }

    private void Update()
    {
        if (m_handler == null) return;

        lock (m_queue) { while (m_queue.Count > 0) m_tmp.Enqueue(m_queue.Dequeue()); }

        while (m_tmp.Count > 0) m_handler.Invoke(m_tmp.Dequeue());
    }
}
