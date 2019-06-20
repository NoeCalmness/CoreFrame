/****************************************************************************************************
 * Copyright (C) 2017-2017 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-
 * 
 ***************************************************************************************************/

using UnityEngine;

[ExecuteInEditMode]
public class LevelEdge : MonoBehaviour
{
    [Tooltip("场景左边界，若左边界大于右边界，则无限制")]
    public double left  = 1;
    [Tooltip("场景右边界，若左边界大于右边界，则无限制")]
    public double right = 0;
    [Tooltip("场景左右边界的修正距离")]
    public double fix = 0.5;

    [Space(10)]
    [Tooltip("场景上边界")]
    public double up   = 0;
    [Tooltip("场景下边界")]
    public double down = 0;

    #region Editor helper

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var el = transform.position; el += (float)left  * transform.right;
        var er = transform.position; er += (float)right * transform.right;

        Gizmos.color = left > right ? Color.red : Color.green;

        var eel = el + (float)up * transform.forward;
        var eer = er + (float)up * transform.forward;

        Gizmos.DrawLine(eel + Vector3.up * 3, eel + Vector3.down);
        Gizmos.DrawLine(eer + Vector3.up * 3, eer + Vector3.down);
        Gizmos.DrawLine(eel, eer);

        eel = el + (float)down * transform.forward;
        eer = er + (float)down * transform.forward;

        Gizmos.DrawLine(eel + Vector3.up * 3, eel + Vector3.down);
        Gizmos.DrawLine(eer + Vector3.up * 3, eer + Vector3.down);
        Gizmos.DrawLine(eel, eer);

        var eu = transform.position; eu += (float)up   * transform.forward;
        var ed = transform.position; ed += (float)down * transform.forward;

        Gizmos.color = down > up ? Color.red : Color.yellow;

        var eeu = eu + (float)left * transform.right;
        var eed = ed + (float)left * transform.right;

        Gizmos.DrawLine(eeu, eed);

        eeu = eu + (float)right * transform.right;
        eed = ed + (float)right * transform.right;

        Gizmos.DrawLine(eeu, eed);

        DrawMonsterBorder();
    }

    private void DrawMonsterBorder()
    {
        var el = transform.position; el += (float)(left + GeneralConfigInfo.AIBorderDis * 0.1f) * transform.right;
        var er = transform.position; er += (float)(right - GeneralConfigInfo.AIBorderDis * 0.1f) * transform.right;

        Gizmos.color = left > right ? Color.red : Color.blue;

        var eel = el + (float)up * transform.forward;
        var eer = er + (float)up * transform.forward;

        Gizmos.DrawLine(eel + Vector3.up * 3, eel + Vector3.down);
        Gizmos.DrawLine(eer + Vector3.up * 3, eer + Vector3.down);
        Gizmos.DrawLine(eel, eer);
    }
#endif

    #endregion
}
