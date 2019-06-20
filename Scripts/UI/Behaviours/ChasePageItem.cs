using System;
using UnityEngine;
using UnityEngine.UI;

public class ChasePageItem : MonoBehaviour
{
    private Transform[] roots;

    private bool isInited;
    private void IniteCompent()
    {
        if (isInited) return;

        Transform frame_01 = transform.Find("level_01");
        Transform frame_02 = transform.Find("level_02");
        Transform frame_03 = transform.Find("level_03");
        Transform frame_04 = transform.Find("level_04");
        Transform frame_05 = transform.Find("level_05");
        Transform frame_06 = transform.Find("level_06");
        roots = new Transform[] { frame_01, frame_02, frame_03, frame_04, frame_05, frame_06 };

        isInited = true;
    }

    public void RefreshPage(Chase_PageItem item, Action<ChaseTask> OnClick = null)
    {
        if (item == null) return;
        IniteCompent();
        //选模板

        for (int i = 0; i < roots.Length; i++)
        {
            if (i > item.current_Tasks.Count - 1)
            {
                roots[i].SafeSetActive(false);
                return;
            }
            roots[i].SafeSetActive(true);
            var taskinfo = item.current_Tasks[i];

            _RefreshItem(roots[i], taskinfo, i, OnClick);
        }
    }

    private void _RefreshItem(Transform tran, TaskInfo task, int index, Action<ChaseTask> _OnClick = null)
    {
        if (tran == null || task == null) return;

        var taskItem = tran.GetComponentDefault<ChaseTaskItem>();
        var chaseTask = Module_Chase.instance.allChaseTasks.Find((p) => p.taskConfigInfo.ID == task.ID);
        var images = tran.GetComponentsInChildren<Image>(true);

        tran.SafeSetActive(false);
        tran.SafeSetActive(true);
        if (chaseTask != null)
        {
            taskItem.RefreshTaskItem(chaseTask, index, _OnClick);
            for (int i = 0; i < images.Length; i++)
                images[i].saturation = 1;
        }
        else
        {
            taskItem.RefreshTaskItem(task, index);
            for (int i = 0; i < images.Length; i++)
                images[i].saturation = 0;
        }
    }
}
