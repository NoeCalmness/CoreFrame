using UnityEngine;
using UnityEngine.UI;

public class RankingItem : MonoBehaviour
{
    private Image m_back;
    private Transform firstPerson;
    private Transform secondPerson;
    private Transform thirdPerson;
    private Transform otherPerson;
    private Transform[] rankings;
    private Transform levelIcion;
    //private Image headBox;
    //private Image avatarIcion;
    private Text name_text;
    private Text integral_text;
    private Text union_text;

    private bool isInited;

    private void IniteCompent()
    {
        m_back = transform.Find("Image (1)").GetComponent<Image>();
        firstPerson = transform.Find("rankingNumber/first");
        secondPerson = transform.Find("rankingNumber/second");
        thirdPerson = transform.Find("rankingNumber/third");
        otherPerson = transform.Find("rankingNumber/other");
        rankings = new Transform[] { firstPerson, secondPerson, thirdPerson, otherPerson };
        levelIcion = transform.Find("level_icon");
        //headBox = transform.Find("avatr_mask").GetComponent<Image>();
        //avatarIcion = transform.Find("avatr_mask/mask/Image").GetComponent<Image>();
        name_text = transform.Find("name_text").GetComponent<Text>();
        integral_text = transform.Find("integral_text").GetComponent<Text>();
        union_text = transform.Find("union_text").GetComponent<Text>();
        isInited = true;
    }

    public void RefreshItem(PRank p)
    {
        if (!isInited) IniteCompent();
        //排名
        for (int i = 0; i < rankings.Length; i++)
        {
            rankings[i].gameObject.SetActive((i + 1) == p.rank);
            if (i == rankings.Length - 1 && p.rank >= 4)
            {
                rankings[i].gameObject.SetActive(true);
                Text number = rankings[i].Find("number").GetComponent<Text>();
                if (number != null) number.text = p.rank.ToString();
            }
        }
        //段位图标
        for (int i = 0; i < levelIcion.childCount; i++)
            levelIcion.GetChild(i).gameObject.SetActive((i + 1) == p.danLv);
        //头像
        //Module_Avatar.SetAvatar(avatarIcion.gameObject, p.head, false, 0);
        //名字
        name_text.text = p.name;
        integral_text.text = p.score.ToString();
        union_text.text = p.guild;

        m_back.color = Color.white;
        if (p.roleId == Module_Player.instance.id_) m_back.color = GeneralConfigInfo.defaultConfig.rankSelfColor;
    }
}
