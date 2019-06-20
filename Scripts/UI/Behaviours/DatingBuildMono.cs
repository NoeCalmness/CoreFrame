using System;
using System.Collections.Generic;
using UnityEngine;

public class DatingBuildMono : MonoBehaviour
{
    Transform m_tfRedDot;//红点

    Level_Home m_levelHome;
    DatingMapBuildConfig m_builData = null;
    DatingSceneConfig m_sceneData = null;
    EnumNPCDatingSceneType m_eBuildSceneType;
    private bool m_bInsidePolygons = false;

    public void Start()
    {
        m_levelHome = Level.current as Level_Home;
        m_tfRedDot = transform.Find("redDot");
    }

    private void OnEnable()
    {
        Module_NPCDating.instance.AddEventListener(Module_NPCDating.EventNotifyDatingMapObject, OnReceiveDatingEvent);
    }

    private void OnDisable()
    {
        Module_NPCDating.instance.RemoveEventListener(Module_NPCDating.EventNotifyDatingMapObject, OnReceiveDatingEvent);
    }

    public void InitData(DatingMapBuildConfig data)
    {
        m_builData = data;
        m_eBuildSceneType = Util.ParseEnum<EnumNPCDatingSceneType>(m_builData.objectName);
        m_sceneData = Module_NPCDating.instance.GetDatingSceneData(m_eBuildSceneType);
    }

    private void OnReceiveDatingEvent(Event_ e)
    {
        EnumDatingNotifyType notifyType = (EnumDatingNotifyType)e.param1;
        switch (notifyType)
        {
            case EnumDatingNotifyType.RefreshRedDot:
                MarkRedDot((List<int>)e.param2);
                break;
            case EnumDatingNotifyType.ClickBuild:
                break;
            case EnumDatingNotifyType.ClickBuildDown:
                OnClickDown((Vector3)e.param2);
                break;
            case EnumDatingNotifyType.ClickBuildUp:
                OnClickUp((Vector3)e.param2);
                break;
            case EnumDatingNotifyType.OnEndDrag:
                OnEndDrag((Vector2)e.param2);
                break;
            default:
                break;
        }
    }

    #region 点击建筑物
    private void OnClickDown(Vector3 clickPos)
    {
        m_bInsidePolygons = InPolygonArea(clickPos);
        if (m_bInsidePolygons) SetPressedIcon();
    }

    private void OnClickUp(Vector3 clickPos)
    {
        if (m_bInsidePolygons)
        {
            m_bInsidePolygons = false;
            SetNormalIcon();
            Module_NPCDating.instance.EnterDatingScene(m_eBuildSceneType);
        }
    }

    private void OnEndDrag(Vector2 endPos)
    {
        SetNormalIcon();
        //Vector3 v3 = new Vector3(endPos.x, endPos.y, 0);
        //m_bInsidePolygons = InPolygonArea(v3);
        //if (m_bInsidePolygons)
        //{
        //    m_bInsidePolygons = false;
        //    Module_NPCDating.instance.EnterDatingScene(m_eBuildSceneType);
        //}
    }

    private bool InPolygonArea(Vector3 clickPos)
    {
        if (m_builData == null) return false;
        List<Vector3> listV3Polygons = new List<Vector3>();
        for (int i = 0; i < m_builData.polygonEdges.Length; i++)
        {
            var viewPos = m_levelHome.datingCamera.WorldToScreenPoint(m_builData.polygonEdges[i].position);
            listV3Polygons.Add(viewPos);
        }

         return IsInPolygon2(clickPos, listV3Polygons);
    }

    /// <summary>
    /// 设置按下时的图片
    /// </summary>
    private void SetPressedIcon()
    {
        if (string.IsNullOrEmpty(m_builData.pressImage))
        {
            Logger.LogDetail("Dating::  表DatingMapBuildConfig id={0}的配置中 建筑物的按下时的图片名为空，请检查配置", m_builData.ID);
            return;
        }

        var mr = transform.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            var spriteRen = transform.GetComponentDefault<SpriteRenderer>();
            UIDynamicImage.LoadImage(m_builData.pressImage, (t) =>
            {
                spriteRen.sprite = t.ToSprite();
            }, transform);
        }
        else
        {
            var mrMat = transform.GetComponent<MeshRenderer>().material;
            if (mrMat == null) Logger.LogError("Dating::  levelHome约会节点名 = {0} 没有材质球，请检查", transform.name);
            else UIDynamicImage.LoadImage(m_builData.pressImage, (t) => {mrMat.mainTexture = t;}, transform);
        }

    }

    /// <summary>
    /// 设置松开时的图片
    /// </summary>
    private void SetNormalIcon()
    {
        if (string.IsNullOrEmpty(m_builData.bigImage))
        {
            Logger.LogDetail("Dating::  表DatingMapBuildConfig id={0}的配置中 建筑物正常显示的图片名为空，请检查配置", m_builData.ID);
            return;
        }

        var mr = transform.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            var spriteRen = transform.GetComponentDefault<SpriteRenderer>();
            UIDynamicImage.LoadImage(m_builData.bigImage, (t) =>
            {
                spriteRen.sprite = t.ToSprite();
            }, transform);
        }
        else
        {
            var mrMat = transform.GetComponent<MeshRenderer>().material;
            if (mrMat == null) Logger.LogError("Dating::  levelHome约会节点名 = {0} 没有材质球，请检查", transform.name);
            else UIDynamicImage.LoadImage(m_builData.bigImage, (t) => { mrMat.mainTexture = t; }, transform);
        }
    }

    /// <summary>
    /// 播放点击动画
    /// </summary>
    private void PlayClickTween()
    {

    }
    #endregion

    #region 红点标记
    private void MarkRedDot(EnumNPCDatingSceneType sceneType)
    {
        m_tfRedDot?.SafeSetActive(m_eBuildSceneType == sceneType);
    }

    private void MarkRedDot(List<int> levelIds)
    {
        if (levelIds == null || levelIds.Count == 0) m_tfRedDot?.SafeSetActive(false);
        else
        {
            bool bSelfLevelId = levelIds.IndexOf(m_sceneData == null ? 0 : m_sceneData.levelId) != -1;
            List<int> entersceneList = new List<int>(Module_NPCDating.instance.openWindowData.enterScenes);
            bool bEnterScene = entersceneList != null && entersceneList.Contains(m_sceneData == null ? 0 : m_sceneData.levelId);
            bool bMark = bSelfLevelId && !bEnterScene;
            m_tfRedDot?.SafeSetActive(bMark);
        }

    }
    #endregion

    #region 计算当前点是否在多边形内部的几种做法

    /// <summary>  
    /// 判断点是否在多边形内.  
    /// ----------原理----------  
    /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，  
    /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。  
    /// 所以，我们可以顺序考虑多边形的每条边，求出交点的总个数。还有一些特殊情况要考虑。假如考虑边(P1,P2)，  
    /// 1)如果射线正好穿过P1或者P2,那么这个交点会被算作2次，处理办法是如果P的从坐标与P1,P2中较小的纵坐标相同，则直接忽略这种情况  
    /// 2)如果射线水平，则射线要么与其无交点，要么有无数个，这种情况也直接忽略。  
    /// 3)如果射线竖直，而P0的横坐标小于P1,P2的横坐标，则必然相交。  
    /// 4)再判断相交之前，先判断P是否在边(P1,P2)的上面，如果在，则直接得出结论：P再多边形内部。  
    /// </summary>  
    /// <param name="checkPoint">要判断的点</param>  
    /// <param name="polygonPoints">多边形的顶点</param>  
    /// <returns></returns>  
    public static bool IsInPolygon(Vector3 checkPoint, List<Vector3> polygonPoints)
    {
        int counter = 0;
        int i;
        double xinters;
        Vector3 p1, p2;
        int pointCount = polygonPoints.Count;
        p1 = polygonPoints[0];
        for (i = 1; i <= pointCount; i++)
        {
            p2 = polygonPoints[i % pointCount];
            if (checkPoint.y > Math.Min(p1.y, p2.y)//校验点的Y大于线段端点的最小Y  
                && checkPoint.y <= Math.Max(p1.y, p2.y))//校验点的Y小于线段端点的最大Y  
            {
                if (checkPoint.x <= Math.Max(p1.x, p2.x))//校验点的X小于等线段端点的最大X(使用校验点的左射线判断).  
                {
                    if (p1.y != p2.y)//线段不平行于X轴  
                    {
                        xinters = (checkPoint.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
                        if (p1.x == p2.x || checkPoint.x <= xinters)
                        {
                            counter++;
                        }
                    }
                }

            }
            p1 = p2;
        }

        if (counter % 2 == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>  
    /// 判断点是否在多边形内.  
    /// ----------原理----------  
    /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，  
    /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。  
    /// </summary>  
    /// <param name="clickPoint">要判断的点</param>  
    /// <param name="polygonPoints">多边形的顶点</param>  
    /// <returns></returns>  
    public static bool IsInPolygon2(Vector3 clickPoint, List<Vector3> polygonPoints)
    {
        bool inside = false;
        int pointCount = polygonPoints.Count;
        Vector3 p1, p2;
        for (int i = 0, j = pointCount - 1; i < pointCount; j = i, i++)//第一个点和最后一个点作为第一条线，之后是第一个点和第二个点作为第二条线，之后是第二个点与第三个点，第三个点与第四个点...  
        {
            p1 = polygonPoints[i];
            p2 = polygonPoints[j];
            if (clickPoint.y < p2.y)
            {//p2在射线之上  
                if (p1.y <= clickPoint.y)
                {//p1正好在射线中或者射线下方  
                    if ((clickPoint.y - p1.y) * (p2.x - p1.x) > (clickPoint.x - p1.x) * (p2.y - p1.y))//斜率判断,在P1和P2之间且在P1P2右侧  
                    {
                        //射线与多边形交点为奇数时则在多边形之内，若为偶数个交点时则在多边形之外。  
                        //由于inside初始值为false，即交点数为零。所以当有第一个交点时，则必为奇数，则在内部，此时为inside=(!inside)  
                        //所以当有第二个交点时，则必为偶数，则在外部，此时为inside=(!inside)  
                        inside = !inside;
                    }
                }
            }
            else if (clickPoint.y < p1.y)
            {
                //p2正好在射线中或者在射线下方，p1在射线上  
                if ((clickPoint.y - p1.y) * (p2.x - p1.x) < (clickPoint.x - p1.x) * (p2.y - p1.y))//斜率判断,在P1和P2之间且在P1P2右侧  
                {
                    inside = !inside;
                }
            }
        }
        return inside;
    }
    #endregion

}
