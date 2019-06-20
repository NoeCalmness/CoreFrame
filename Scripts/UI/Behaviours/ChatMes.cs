using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMes : MonoBehaviour
{
    private GameObject obj;
    private Image headdi1;
    private Image headdi2;
    private GameObject headbgmask;
    private Button head_btn;
    private Text ID;
    private Text chatname;
    private Image img_txt;
    private Image img_img;
    // private Image img_voice;

    private Chathyperlink mes_txt;
    //private Button mes_btn;
    // private Text mes_btn_txt;

    private Text id_key;//图片时
    private Action<ulong> player_show;

    public float This_height = 0;

    public string HeadName;
    public int Gender;
    public int m_proto;
    private GeneralConfigInfo generalInfo;

    void Get()
    {
        generalInfo = GeneralConfigInfo.defaultConfig;
        obj = transform.Find("GameObject").gameObject;
        headbgmask = obj.transform.Find("head_img/mask").gameObject;
        head_btn = obj.transform.Find("head_img").GetComponent<Button>();
        chatname = obj.transform.Find("name").GetComponent<Text>();

        img_txt = obj.transform.Find("mes_img").GetComponent<Image>();
        mes_txt = img_txt.gameObject.transform.Find("mes").GetComponent<Chathyperlink>();

        img_img = obj.transform.Find("img_bg").GetComponent<Image>();

        //img_voice = obj.transform.Find("Sound recording").GetComponent<Image>();
        // mes_btn = obj.transform.Find("Sound recording").GetComponent<Button>();
        // mes_btn_txt = img_voice.transform.Find("Text").GetComponent<Text>();
        ID = obj.transform.Find("ID").GetComponent<Text>();
        id_key = obj.transform.Find("key").GetComponent<Text>();

        img_txt?.SafeSetActive(false);
        img_img?.SafeSetActive(false);
    }
    public void Player_show()
    {
        if (player_show != null && ID.text != null)
        {
            player_show((ulong)Int64.Parse(ID.text));
        }
    }

    public void show_details(bool isshow, string word_name, string word_head, int gender, ulong id, int proto, Action<ulong> show_details = null)
    {
        Get();
        HeadName = word_head;
        m_proto = proto;
        Gender = gender;

        ID.text = id.ToString();
        chatname.text = word_name;
        player_show = show_details;
        if (!isshow)
        {
            head_btn.onClick.AddListener(Player_show);
        }
    }

    public void caht_show(int type, string content, bool Isme, int headboxid, int playerSend = 0)//接收的还是自己的世界的还是好友的，那个类型的，信息
    {
        //0 世界的接受的 //1世界我发的 2好友接收的 3 好友我发的 只有在0的情况下头像才可以点击(isshow)
        //type 0,文本  1,图片  2 语音

        headBoxFriend headBoxa = head_btn.gameObject.GetComponentDefault<headBoxFriend>();
        headBoxa.HeadBox(headboxid);
        if (!Isme)
        {
            Module_Avatar.SetClassAvatar(headbgmask, m_proto, false, Gender);
        }

        if (type == 0)
        {
            img_txt.gameObject.SetActive(true);
            if (playerSend == 0)
            {
                mes_txt.supportRichText = false;
                mes_txt.text = content;
                mes_txt.Set();
            }
            else
            {
                mes_txt.supportRichText = true;
                mes_txt.text = content;
                mes_txt.Set();
                mes_txt.text = mes_txt.gettxt;
            }
            float width = mes_txt.preferredWidth;
            float height = mes_txt.preferredHeight;

            if (width <= generalInfo.ChatMax)
            {
                img_txt.rectTransform.sizeDelta = new Vector2(width + generalInfo.ChatTxtWidth, generalInfo.ChatTxtHeight);//设置图片的宽高
                RectTransform my_height = gameObject.GetComponent<RectTransform>();
                my_height.sizeDelta = new Vector2(my_height.sizeDelta.x, generalInfo.ChatAllHeight);//设置整个背景的高度
                This_height = generalInfo.ChatAllHeight;
            }
            else
            {
                ContentSizeFitter a = mes_txt.gameObject.GetComponentDefault<ContentSizeFitter>();
                a.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                img_txt.rectTransform.sizeDelta = new Vector2(generalInfo.OverChatTxtWidth, height + generalInfo.OverChatTxtHeight);//设置图片的宽高 
                RectTransform my_height = gameObject.GetComponent<RectTransform>();
                my_height.sizeDelta = new Vector2(my_height.sizeDelta.x, height + generalInfo.OverChatBackHeight);//设置整个背景的高度 
                This_height = height + generalInfo.OverChatBackHeight;
            }

        }
        else if (type == 1)
        {
            id_key.text = content;
            mes_txt.text = string.Empty;
            img_img.gameObject.SetActive(true);
            //FaceName icon = ConfigManager.Get<FaceName>(Int32.Parse(content));//用id获取图片名称 
            GameObject a = Level.GetPreloadObject(content);
            if (a != null)
            {
                a.transform.SetParent(img_img.transform);
                a.transform.localScale = new Vector3(1, 1, 1);
                RectTransform sss = a.GetComponent<RectTransform>();
                sss.anchoredPosition = new Vector3(0, 0, 0);
                sss.localPosition = new Vector3(0, 0, 0);
            }
            RectTransform my_heightimg = gameObject.GetComponent<RectTransform>();
            my_heightimg.sizeDelta = new Vector2(my_heightimg.sizeDelta.x, generalInfo.ChatImgAllHeight);//设置整个背景的高度
            This_height = generalInfo.ChatImgAllHeight;
        }
        else
        {
            This_height = 75f;
        }
        //    mes_btn_txt.text = Util.Format("语音", time.ToString());
        //    mes_btn.onClick.AddListener(aa);

    }
}
