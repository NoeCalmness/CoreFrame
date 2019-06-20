using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class headBoxFriend : MonoBehaviour
{

    private Image headbox_top;
    private Image headbox_bottom;

    void get()
    {
        headbox_bottom = transform.GetComponent<Image>();
        headbox_top = transform.Find("avatar_topbg_img")?.GetComponent<Image>();
    }

    public void HeadBox(int boxid)//设置头像框
    {
        get();
        if (boxid == 0 || boxid == 1)
        {
            Logger.LogError("this player head id is {0}", boxid);
            boxid = 901;//如果为零则自动成为默认头像
        }
        PropItemInfo BoxInfo = ConfigManager.Get<PropItemInfo>(boxid);
        if (BoxInfo == null)
        {
            Logger.LogError("headbox datda error null id is {0}", boxid);
            return;
        }
        string[] mesh = BoxInfo.mesh;
        if (mesh != null && mesh.Length > 0)
        {
            AtlasHelper.SetShared(headbox_bottom, mesh[0]);
            if (mesh.Length > 1 && headbox_top != null)
                AtlasHelper.SetShared(headbox_top, mesh[1]);
        }
        else
        {
            Logger.LogError("headbox datda error" + boxid);
        }
    }
}
