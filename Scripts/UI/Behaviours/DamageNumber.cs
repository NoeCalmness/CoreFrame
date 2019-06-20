using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    public enum EnumDamageDisplayType
    {
        None    = 0,
        Attack  = 1,
        Element = 2,
        Buff    = 4,
    }

    public enum EnumAttDamageType
    {
        Normal,
        Critical,
        Count,
    }

    public GameObject attackParent;
    public Text[] attackTexts = new Text[(int)EnumAttDamageType.Count];

    public GameObject elementParent;
    public Text[] elementTexts = new Text[(int)CreatureElementTypes.Count];

    public GameObject buffParent;
    public Text[] buffTexts = new Text[(int)BuffInfo.EffectFlags.Count];
    public RectTransform rectTransform { get; private set; }

    private Camera m_battleCamera;
    private Camera m_uiCamera;
    private TweenBase m_tween;

    private void Awake()
    {
        InitComponent();
    }

    // Use this for initialization
    private void Start ()
    {
		
	}

    #region private functions

    private void InitComponent()
    {
        SetDamageParentVisible();
        if(!rectTransform) rectTransform = transform as RectTransform;
        if (!m_tween) m_tween = GetComponent<TweenBase>();
    }

    private void SetDamageParentVisible(EnumDamageDisplayType type = EnumDamageDisplayType.None)
    {
        attackParent.SetActive((type & EnumDamageDisplayType.Attack) > 0);
        elementParent.SetActive((type & EnumDamageDisplayType.Element) > 0);
        buffParent.SetActive((type & EnumDamageDisplayType.Buff) > 0);
    }

    private void InitTextData()
    {
        foreach (var item in attackTexts)   { item.text = string.Empty; item.gameObject.SetActive(false); }
        foreach (var item in elementTexts)  { item.text = string.Empty; item.gameObject.SetActive(false); }
        foreach (var item in buffTexts)     { item.text = string.Empty; item.gameObject.SetActive(false); }
    }

    private Text GetTargetText(EnumDamageDisplayType type, int childIndex)
    {
        Text[] texts = null;
        switch (type)
        {
            case EnumDamageDisplayType.Attack: texts = attackTexts; break;
            case EnumDamageDisplayType.Element: texts = elementTexts; break;
            case EnumDamageDisplayType.Buff: texts = buffTexts; break;
        }

        if (texts != null && childIndex < texts.Length) return texts[childIndex];
        else
        {
            Logger.LogWarning("try to get damage number text [type:{0} index:{1}] failed",type,childIndex);
            return null;
        }
    }

    private void SetNormalTextData(int weaponDamage,bool isCri)
    {
        Text t = GetTargetText(EnumDamageDisplayType.Attack, isCri ? 1 : 0);
        if (t)
        {
            t.gameObject.SetActive(weaponDamage > 0);
            t.text = weaponDamage.ToString();
        }
    }

    private void SetElementDamageTextData(CreatureElementTypes elementType,int elementDamage)
    {
        Text t = GetTargetText(EnumDamageDisplayType.Element, (int)elementType);
        if (t)
        {
            t.gameObject.SetActive(elementDamage > 0);
            t.text = elementDamage.ToString();
        }
    }

    private void SetBuffDamageTextData(BuffInfo.EffectFlags flag, int buffDamage)
    {
        Text t = GetTargetText(EnumDamageDisplayType.Buff, (int)flag);
        if (t)
        {
            t.gameObject.SetActive(buffDamage != 0);
            t.text = flag == BuffInfo.EffectFlags.Heal ? Util.Format("+{0}", buffDamage) : buffDamage.ToString();
        }
    }

    #endregion

    #region public functions

    public void InitCamera(Camera battleCamera,Camera uiCamera)
    {
        m_battleCamera = battleCamera;
        m_uiCamera = uiCamera;
    }

    public void ShowDamageNumber(DamageInfo info)
    {
        //todo 1.buff type 2.cure count 
        if (info.fromBuff && info.uiDamage) ShowBuffDamageNumber(info.finalDamage, info.buffEffectFlag);
        else ShowAttDamageNumber(info.finalWeaponDamage,info.crit,info.finalElementDamage,info.elementType);
    }
    
    public void ShowAttDamageNumber(int weaponDamage,bool isCritical = false,int elementDamage = 0,CreatureElementTypes type = CreatureElementTypes.Fire)
    {
        //Logger.LogWarning("weapon damage = {0} element damage = {1}  element type = {2}", weaponDamage,elementDamage,type);
        InitComponent();
        gameObject.SetActive(true);
        InitTextData();
        EnumDamageDisplayType t = (weaponDamage > 0 ? EnumDamageDisplayType.Attack : EnumDamageDisplayType.None) | (elementDamage > 0 ? EnumDamageDisplayType.Element : EnumDamageDisplayType.None); 
        SetDamageParentVisible(t);
        SetNormalTextData(weaponDamage,isCritical);
        SetElementDamageTextData(type, elementDamage);
        m_tween?.PlayForward();
    }

    public void ShowBuffDamageNumber(int buffDamage, BuffInfo.EffectFlags flag)
    {
        //Logger.LogWarning("buff damage =  {0}",buffDamage);
        InitComponent();
        gameObject.SetActive(true);
        InitTextData();
        SetDamageParentVisible(buffDamage > 0 ? EnumDamageDisplayType.Buff : EnumDamageDisplayType.None);
        SetBuffDamageTextData(flag, buffDamage);
        m_tween?.PlayForward();
    }

    public void ResetTransPosition(Creature c,bool isBuff = false)
    {
        Vector3 pos = c.behaviour.GetDamagePos(isBuff);
        if (!m_battleCamera) m_battleCamera = Level.currentMainCamera;
        if (m_battleCamera != null)
        {
            var screenPos = RectTransformUtility.WorldToScreenPoint(m_battleCamera, pos);
            if (m_uiCamera != null)
                RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.rectTransform(), screenPos, m_uiCamera, out pos);
        }

        transform.position = pos;
    }

    #endregion

}
