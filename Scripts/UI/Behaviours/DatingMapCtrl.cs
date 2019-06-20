using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DatingMapCtrl : MonoBehaviour
{
    Dictionary<string, DatingBuildMono> m_dicDatingBuildMono = new Dictionary<string, DatingBuildMono>();
    Dictionary<string, DatingMapBuildConfig> m_dicMapData = new Dictionary<string, DatingMapBuildConfig>();
    Transform[] m_childs;
    Level_Home m_levelHome;
    private void Start()
    {
        m_levelHome = Level.current as Level_Home;
        m_childs = GetComponentsInChildren<Transform>();
        Init();
    }

    private void Init()
    {
        var listData = ConfigManager.GetAll<DatingMapBuildConfig>();

        for (int i = 0; i < listData.Count; i++)
        {
            if (!m_dicMapData.ContainsKey(listData[i].objectName)) m_dicMapData.Add(listData[i].objectName, listData[i]);
        }

        for (int i = 0; i < m_childs.Length; i++)
        {
            if(m_dicMapData.ContainsKey(m_childs[i].name)) RefreshData(m_childs[i], m_dicMapData[m_childs[i].name]);
        }

    }

    public static List<string> GetPreRes()
    {
        var listData = ConfigManager.GetAll<DatingMapBuildConfig>();
        var preResList = new List<string>();
        for (int i = 0; i < listData.Count; i++)
        {
            preResList.Add(listData[i].bigImage);
            for (int j = 0; j < listData[i].effects.Length; j++)
            {
                preResList.Add(listData[i].effects[j].effectNames);
            }
        }
        preResList.Distinct();
        return preResList;
    }

    private void RefreshData(Transform node,DatingMapBuildConfig data)
    {
        InitBuild(node, data);
        SetIcon(node, data);
        CreateEffect(node, data);
    }

    private void CreateEffect(Transform node, DatingMapBuildConfig data)
    {
        if (data.effects == null || data.effects.Length <= 0) return;
        for (int i = 0; i < data.effects.Length; i++)
        {
            Transform tfNode = m_levelHome.datingScence.Find(data.effects[i].effectPath.Replace(@"\", "/"));
            GameObject eff = Level.GetPreloadObject<GameObject>(data.effects[i].effectNames);
            if (!eff)
            {
                Level.PrepareAsset<GameObject>(data.effects[i].effectNames, (e) =>
                {
                    if (e == null)
                    {
                        Logger.LogError("Dating::  表DatingMapBuildConfig id={0}的特效资源(第{1}个特效资源,资源名:{2})没有正确加载，请检查资源是否存在或者是否mark", data.ID, i + 1, data.effects[i].effectNames);
                        return;
                    }
                    var path = data.effects[0].effectPath.Replace(@"\", "/");
                    Transform eNode = m_levelHome.datingScence.Find(path);
                    var o = Object.Instantiate(e) as GameObject;
                    o.transform.SetParent(eNode);
                    o.transform.localPosition = data.effects[0].position;
                    o.transform.localScale = data.effects[0].scale;
                    o.transform.localEulerAngles = data.effects[0].rotation;
                });
            }
            else
            {
                eff.transform.SetParent(tfNode);
                eff.transform.localPosition = data.effects[i].position;
                eff.transform.localScale = data.effects[i].scale;
                eff.transform.localEulerAngles = data.effects[i].rotation;
            } 
        }
    }

    private void SetIcon(Transform node, DatingMapBuildConfig data)
    {
        if (string.IsNullOrEmpty(data.bigImage))
        {
            Logger.LogError("Dating::  表DatingMapBuildConfig id={0} 的 bigImage 字段为空，请检查配置", data.ID);
            return;
        }

        var mr = node.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            var spriteRen = node.GetComponentDefault<SpriteRenderer>();
            Texture2D t2d = Level.GetPreloadObject<Texture2D>(data.bigImage, false);
            if (!t2d)
            {
                UIDynamicImage.LoadImage(data.bigImage, (t) =>
                {
                    spriteRen.sprite = t.ToSprite();
                }, node);
            }
            else spriteRen.sprite = t2d.ToSprite();
            //Logger.LogError("Dating::  levelHome约会节点名 = {0} 没有材质球，请检查", node.name);
            //return;
        }
        else
        {
            var mrMat = node.GetComponent<MeshRenderer>().material;
            Texture2D t2d = Level.GetPreloadObject<Texture2D>(data.bigImage, false);
            if (!t2d)
            {
                UIDynamicImage.LoadImage(data.bigImage, (t) =>
                {
                    mrMat.mainTexture = t;
                }, node);
            }
            else mrMat.mainTexture = t2d;
        }

    }

    private void InitBuild(Transform node, DatingMapBuildConfig data)
    {
        if (data.objectType == EnumDatingMapObjectType.Build)
        {
            var dbm = node.GetComponentDefault<DatingBuildMono>();
            dbm.InitData(data);

            if (!m_dicDatingBuildMono.ContainsKey(node.name)) m_dicDatingBuildMono.Add(node.name, dbm);
        }
    }
}
