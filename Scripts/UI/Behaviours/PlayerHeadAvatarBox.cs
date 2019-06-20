using UnityEngine;
using UnityEngine.UI;

public class PlayerHeadAvatarBox : MonoBehaviour
{
    private Image headbox_top;
    private Image headbox_bottom;

	void Start ()
    {
        headbox_bottom = transform.GetComponent<Image>();
        headbox_top = transform.Find("avatar_topbg_img").GetComponent<Image>();

        UpdateHeadBox();

        EventManager.AddEventListener(Module_Set.EventHeadChange, UpdateHeadBox);
	}

    private void UpdateHeadBox()
    {
        PropItemInfo info = ConfigManager.Get<PropItemInfo>(Module_Player.instance.roleInfo.headBox == 0 ? 901 : Module_Player.instance.roleInfo.headBox);
        if (info != null && info.mesh != null && info.mesh.Length > 0)
        {
            AtlasHelper.SetShared(headbox_bottom, info.mesh[0]);
            if (info.mesh.Length > 1)
                AtlasHelper.SetShared(headbox_top, info.mesh[1]);
        }
    }

    void OnDestroy ()
    {
        EventManager.RemoveEventListener(Module_Set.EventHeadChange, UpdateHeadBox);
    }
}
