using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VocationItem : MonoBehaviour
{
    Text _name;
    Text _identity;
    Text _dub;
    Text _desc;
    Text _identityLittle;
    Button _confirmBtn;
    Button tip_btn;
    public float tweenTime = 0.25f;
    public Ease curve = Ease.Linear;

    private void Initialized()
    {
        Initialized_Text();
        _name = transform.Find("top/info/txt_03")?.GetComponent<Text>();
        _identity = transform.Find("top/info/txt_01")?.GetComponent<Text>();
        _dub = transform.Find("top/info/txt_01/txt_06")?.GetComponent<Text>();
        _desc = transform.Find("top/info/txt_07")?.GetComponent<Text>();
        _confirmBtn = transform.Find("top/sure_btn")?.GetComponent<Button>();
        tip_btn = transform.Find("tip_btn").GetComponent<Button>();
        _identityLittle = transform.Find("info/name_txt")?.GetComponent<Text>();
    }

    private void Initialized_Text()
    {
        var str = ConfigManager.Get<ConfigText>((int)TextForMatType.ProfessionText);
        if (!str) return;

        Util.SetText(transform.Find("top/info/txt_04")?.GetComponent<Text>(), str[0]);
        Util.SetText(transform.Find("top/info/txt_05")?.GetComponent<Text>(), str[1]);
        Util.SetText(transform.Find("top/info/txt_08")?.GetComponent<Text>(), str[2]);
        Util.SetText(transform.Find("top/info/txt_09")?.GetComponent<Text>(), str[3]);
        Util.SetText(transform.Find("top/info/txt_10")?.GetComponent<Text>(), str[4]);
        Util.SetText(transform.Find("top/info/txt_11")?.GetComponent<Text>(), str[5]);
        Util.SetText(transform.Find("top/info/txt_12")?.GetComponent<Text>(), str[6]);
        Util.SetText(transform.Find("top/sure_btn/sure_txt")?.GetComponent<Text>(), str[7]);
    }

    public void RefreshVocationItem(int vocationID, bool isSpecial, float _special, float _normal, Action OnClickConfirmBtn)
    {
        Initialized();
        var targetSize = new Vector2(isSpecial ? _special : _normal, transform.rectTransform().sizeDelta.y);
        DOTween.To(() => transform.rectTransform().sizeDelta, x => transform.rectTransform().sizeDelta = x, targetSize, tweenTime).SetEase(curve);
        var professionInfo = ConfigManager.Get<ProfessionInfo>(vocationID);

        if (!professionInfo)
        {
            tip_btn.enabled = true;
            tip_btn.onClick.RemoveAllListeners();
            tip_btn.onClick.AddListener(() => Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(TextForMatType.ProfessionText, 8)));
            return;
        }
        tip_btn.enabled = false;

        Util.SetText(_name, professionInfo.professionNameID);
        Util.SetText(_identity, professionInfo.identity);
        Util.SetText(_dub, professionInfo.dub);
        Util.SetText(_desc, professionInfo.desc);
        Util.SetText(_identityLittle, professionInfo.identity);

        _confirmBtn.onClick.RemoveAllListeners();
        _confirmBtn.onClick.AddListener(() =>
        {
            var t = ConfigManager.Get<ConfigText>((int)TextForMatType.AlertUIText);
            if (!t) return;
            Module_Login.instance.createGender = professionInfo.gender;
            Module_Login.instance.roleProto = professionInfo.ID;
            Window_Alert.ShowAlert(t[1], true, true, true, OnClickConfirmBtn, null, "", t[7]);
        });
    }
}