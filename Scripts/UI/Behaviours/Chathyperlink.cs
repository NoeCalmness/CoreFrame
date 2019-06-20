using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Chathyperlink : Text
{
    // 解析完最终的文本
    public string gettxt;

    public string m_OutputText;

    // 超链接信息列表
    public List<string> Newvaule = new List<string>();

    private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

    protected static readonly StringBuilder s_TextBuilder = new StringBuilder();
    
    private List<GameObject> Clone = new List<GameObject>();
    
    // 超链接正则
    private static readonly Regex s_HrefRegex = new Regex(@"{a href=([^}\n\s]+)}(.*?)({=a})", RegexOptions.Singleline);

    //color正则 
    private static readonly Regex m_color = new Regex(@"<color=([^>\n\s]+)>(.*?)(</color>)", RegexOptions.Singleline);

    protected static readonly StringBuilder s_TextColor = new StringBuilder();

    private List<ColorInfo> colorinfo = new List<ColorInfo>();

    #region 图片
    ////图片池
    //protected readonly List<Image> m_ImagesPool = new List<Image>();
    ////图片最后一个顶点的索引
    //private readonly List<int> m_ImagesVertexIndex = new List<int>();

    //// 图片正则取出所需要的属性
    //private static readonly Regex s_ImageRegex =
    //    new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

    //// 加载精灵图片方法
    //public static Func<string, Sprite> funLoadSprite;

    #endregion

    public void Set(bool isshow = false)
    {
        for (int i = 0; i < Clone.Count; i++)
        {
            GameObject.Destroy(Clone[i]);
        }
        Clone.Clear();
        Newvaule.Clear();
        m_HrefInfos.Clear();
        UpdateQuadImage();
        SetLine(isshow);
    }

    protected void UpdateQuadImage()
    {
#if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
        {
            return;
        }
#endif
        m_HrefInfos.Clear();
        if (m_OutputText != null)
        {
            text = OnTextChange(text);
            m_OutputText = GetOutputText(text);

            if (m_OutputText != null && m_OutputText != "")
            {
                gettxt = ColorRegex(m_OutputText);
            }

            #region 图片
            //m_ImagesVertexIndex.Clear();
            //foreach (Match match in s_ImageRegex.Matches(m_OutputText))
            //{
            //    var picIndex = match.Index;
            //    var endIndex = picIndex * 4 + 3;
            //    m_ImagesVertexIndex.Add(endIndex);
            //    m_ImagesPool.RemoveAll(image => image == null);
            //    if (m_ImagesPool.Count == 0)
            //    {
            //        GetComponentsInChildren<Image>(m_ImagesPool);
            //    }
            //    if (m_ImagesVertexIndex.Count > m_ImagesPool.Count)
            //    {
            //        var resources = new DefaultControls.Resources();
            //        var go = DefaultControls.CreateImage(resources);
            //        go.layer = gameObject.layer;
            //        var rt = go.transform as RectTransform;
            //        if (rt)
            //        {
            //            rt.SetParent(rectTransform);
            //            rt.anchorMin = new Vector2(0, 1);
            //            rt.anchorMax = new Vector2(0, 1);
            //            rt.pivot = new Vector2(0.5f, 0.5f);
            //            rt.localPosition = Vector3.zero;
            //            rt.localRotation = Quaternion.identity;
            //            rt.localScale = Vector3.one;
            //        }
            //        m_ImagesPool.Add(go.GetComponent<Image>());
            //    }


            //    var spriteName = match.Groups[1].Value;
            //    var size = float.Parse(match.Groups[2].Value);
            //    var img = m_ImagesPool[m_ImagesVertexIndex.Count - 1];
            //    if (img.sprite == null || img.sprite.name != spriteName)
            //    {
            //        AtlasHelper.SetIcons(img.gameObject, spriteName);
            //    }
            //    img.rectTransform.sizeDelta = new Vector2(size, size);
            //    img.enabled = true;

            //}
            //for (int i = m_ImagesVertexIndex.Count; i < m_ImagesPool.Count; i++)
            //{
            //    if (m_ImagesPool[i])
            //    {
            //        m_ImagesPool[i].enabled = false;
            //    }
            //}
            #endregion
        }
    }
    //空格的半角圆角切换 更改之后的超链接使用的是圆角空格
    private string OnTextChange(string str)
    {
        if (str.Contains(" ")) str = str.Replace(" ", "\u00A0");
        return str;
    }

    private string SetColor(string txt)
    {
        txt = txt.Replace("(", "<color=");
        txt = txt.Replace(")", ">");
        txt = txt.Replace("/", "</color>");
        return txt;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate(m_OutputText, settings);


        var orignText = m_Text;
        m_Text = m_OutputText;

        base.OnPopulateMesh(toFill);
        m_Text = orignText;

        #region 图片
        //for (int i = 0; i < m_ImagesVertexIndex.Count; i++)
        //{
        //    var endIndex = m_ImagesVertexIndex[i];
        //    var rt = m_ImagesPool[i].rectTransform;
        //    var size = rt.sizeDelta;
        //    if (endIndex < toFill.currentVertCount)
        //    {
        //        toFill.PopulateUIVertex(ref vert, endIndex);
        //        rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2, vert.position.y + size.y / 2);

        //        // 抹掉左下角的小黑点
        //        toFill.PopulateUIVertex(ref vert, endIndex - 3);
        //        var pos = vert.position;
        //        for (int j = endIndex, m = endIndex - 3; j > m; j--)
        //        {
        //            toFill.PopulateUIVertex(ref vert, endIndex);
        //            vert.position = pos;
        //            toFill.SetUIVertex(vert, j);
        //        }
        //    }

        //}
        //if (m_ImagesVertexIndex.Count != 0)
        //{
        //    m_ImagesVertexIndex.Clear();
        //}

        #endregion

        #region 处理超链接包围框
        //foreach (var hrefInfo in m_HrefInfos)
        //{
        //    hrefInfo.boxes.Clear();
        //    if (hrefInfo.startIndex >= toFill.currentVertCount)
        //    {
        //        continue;
        //    }

        //    // 将超链接里面的文本顶点索引坐标加入到包围框
        //    toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
        //    var pos = vert.position;
        //    var bounds = new Bounds(pos, Vector3.zero);
        //    for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
        //    {
        //        if (i >= toFill.currentVertCount)
        //        {
        //            break;
        //        }

        //        toFill.PopulateUIVertex(ref vert, i);
        //        pos = vert.position;
        //        if (pos.x < bounds.min.x) // 换行重新添加包围框
        //        {
        //            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        //            bounds = new Bounds(pos, Vector3.zero);
        //        }
        //        else
        //        {
        //            bounds.Encapsulate(pos); // 扩展包围框
        //        }
        //    }
        //    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        //}
        #endregion

        #region 添加下划线 会出现前后逐渐消失情况

        //TextGenerator _UnderlineText = new TextGenerator();
        //_UnderlineText.Populate("_", settings);
        //IList<UIVertex> _TUT = _UnderlineText.verts;
        //foreach (var item in m_HrefInfos)
        //{
        //    for (int i = 0; i < item.boxes.Count; i++)
        //    {
        //        //计算下划线的位置
        //        Vector3[] _ulPos = new Vector3[4];
        //        _ulPos[0] = item.boxes[i].position + new Vector2(0.0f, fontSize * 0.2f);
        //        _ulPos[1] = _ulPos[0] + new Vector3(item.boxes[i].width, 0.0f);
        //        _ulPos[2] = item.boxes[i].position + new Vector2(item.boxes[i].width, 0.0f);
        //        _ulPos[3] = item.boxes[i].position;

        //        //绘制下划线
        //        for (int j = 0; j < 4; j++)
        //        {
        //            m_TempVerts[j] = _TUT[j];
        //            m_TempVerts[j].color = Color.blue;
        //            m_TempVerts[j].position = _ulPos[j];
        //            if (j == 3)
        //                toFill.AddUIVertexQuad(m_TempVerts);
        //        }
        //    }
        //}
        #endregion
    }
    
    // 获取超链接解析后的最后输出文本
    protected virtual string GetOutputText(string outputText)
    {
        s_TextBuilder.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;
        foreach (Match match in s_HrefRegex.Matches(outputText))
        {
            s_TextBuilder.Append(outputText.Substring(indexText, match.Index - indexText));

            Newvaule.Add(match.Groups[2].ToString());
            var group = match.Groups[1];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);

            s_TextBuilder.Append(match.Groups[2].Value);
            indexText = match.Index + match.Length;
        }
        s_TextBuilder.Append(outputText.Substring(indexText, outputText.Length - indexText));

        if (m_HrefInfos.Count > 0)
            transform.GetComponentDefault<Chatclick>();
        return s_TextBuilder.ToString();
    }
    
    // 超链接信息类
    private class HrefInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();
    }

    #region  超链接下划线
    private void SetLine(bool isshow)
    {
        if (isshow)
        {
            GameObject txtss = gameObject.transform.Find("mes (1)").gameObject;

            for (int i = 0; i < Newvaule.Count; i++)
            {
                var value = ColorRegex(Newvaule[i]);
                SettextLine(txtss, gettxt, value);
            }
        }
    }

    private void SettextLine(GameObject txtss, string txts, string Newvalues)
    {
        txtss.gameObject.SetActive(false);
        GameObject colone = GameObject.Instantiate(txtss);
        colone.gameObject.SetActive(true);

        Clone.Add(colone);

        colone.transform.SetParent(gameObject.transform);
        RectTransform rt = colone.GetComponent<RectTransform>();
        colone.transform.localScale = new Vector3(1, 1, 1);
        colone.transform.localPosition = new Vector3(0, 0, 0);
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

        Text txt = colone.GetComponent<Text>();

        string[] spl = new string[1] { Newvalues };

        string[] all = txts.Split(spl, StringSplitOptions.RemoveEmptyEntries);

        string lasttxtone = "";
        for (int i = 0; i < all.Length; i++)
        {
            int tempLen = StrLength(ColorRegex(all[i]));
            var count = tempLen / 2;
            for (int j = 0; j < count; j++)
            {
                lasttxtone += "\u3000";
            }
            if (i < all.Length - 1)
            {
                lasttxtone += Newvalues;
            }
        }

        bool start = txts.StartsWith(Newvalues);
        bool end = txts.EndsWith(Newvalues);
        if (start) lasttxtone = Newvalues + lasttxtone;
        if (end && (txts.Length > Newvalues.Length))lasttxtone = lasttxtone + Newvalues;
        
        int Len = StrLength(Newvalues);
        var line = "";
        for (int i = 0; i < Len; i++)
        {
            line += "_";
        }
        txt.text = lasttxtone.Replace(Newvalues, line);
    }
    private int StrLength(string txt)
    {
        ASCIIEncoding ascii= new ASCIIEncoding();
        byte[] str = ascii.GetBytes(txt);
        var len = 0;
        for (int j = 0; j < str.Length; j++)
        {
            if ((int)str[j] == 63) len += 2;
            else len += 1;
        }
        return len;
    }

    private string ColorRegex(string mes)
    {
        var indexText = 0;
        foreach (Match match in m_color.Matches(mes))
        {
            s_TextColor.Append(mes.Substring(indexText, match.Index - indexText));
            var group = match.Groups[1];
            ColorInfo info = new ColorInfo();
            info.mes = match.Groups[0].ToString();
            info.color = match.Groups[1].ToString();
            info.vaule = match.Groups[2].ToString();
            colorinfo.Add(info);
        }
        for (int i = 0; i < colorinfo.Count; i++)
        {
            mes = mes.Replace(colorinfo[i].mes, colorinfo[i].vaule);
        }
        mes = mes.Replace("<i>","");
        mes = mes.Replace("</i>","");
        return mes;
    }

    #endregion

    public string Value()
    {
        if (m_HrefInfos == null || m_HrefInfos.Count <= 0) return string.Empty;
        else return m_HrefInfos[0].name;
    }
}

public class ColorInfo
{
    public string mes;
    public string color;
    public string vaule;
}