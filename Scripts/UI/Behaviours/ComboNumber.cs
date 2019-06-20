using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ComboNumber : MonoBehaviour
{
    #region custom data
    public enum EnumComboTestMode
    {
        None,

        DuringNumberChange,

        DuringFontChange,

        DuringFlicker,
    }

    private enum EnumComboTextType
    {
        Unit,
        Decade,
        Hundurd,
        Count,
    }

    private class ComboTextAnim
    {
        private EnumComboTextType m_type;
        private Text m_text;
        private Tween m_t;
        private int limit = 0;
        private int m_lastValue = 0;
        
        private Transform m_trans { get { return m_text == null ? null : m_text.transform; } }
        private GameObject m_obj { get { return m_text == null ? null : m_text.gameObject; } }

        public Font font { set { if (m_text) m_text.font = value; } }

        public ComboTextAnim(EnumComboTextType t,Text comp)
        {
            m_type = t;
            m_text = comp;
            switch (m_type)
            {
                case EnumComboTextType.Unit:        limit = 0;  break;
                case EnumComboTextType.Decade:      limit = 9;  break;
                case EnumComboTextType.Hundurd:     limit = 99; break;
            }
        }

        public void CheckPlayAnim(int curValue,float dur,TweenCallback callback)
        {
            if (!m_text || (m_t != null && m_t.IsPlaying())) return;

            m_obj.SetActive(curValue > limit);
            switch (m_type)
            {
                case EnumComboTextType.Unit:        m_lastValue = curValue % 10;      break;
                case EnumComboTextType.Decade:      m_lastValue = curValue / 10 % 10; break;
                case EnumComboTextType.Hundurd:     m_lastValue = curValue / 100;     break;
            }
            
            m_text.text = m_lastValue.ToString();
            m_t?.Kill();
            if(m_obj.activeInHierarchy)
            {
                m_t = m_trans.DOScale(m_trans.localScale, dur).OnComplete(() =>
                {
                    if (m_type == EnumComboTextType.Unit) callback?.Invoke();
                });
            }
        }

        public void Reset()
        {
            m_lastValue = 0;
            m_t?.Kill();
            m_obj?.SetActive(false);
        }
    }
    #endregion

    #region test mode

    public EnumComboTestMode testMode;
    public int testValue = 13;
    private int m_testTimes = 1;

    #endregion

    public List<Text> texts;
    public List<Font> changeFonts;

    public float numDur = 0.08f,fontWaitDur = 0.2f;
    public TweenScale startTween;
    public TweenAlpha flickerTween;

    //[SerializeField]
    private int m_tarValue;
    //[SerializeField]
    private int m_curValue;

    private Tween m_changeFontTween;
    private bool m_numberTween;
    private CanvasGroup m_cg;
    private List<ComboTextAnim> m_combos = new List<ComboTextAnim>();

    // Use this for initialization
    void Awake ()
    {
        for (int i = 0; i < texts.Count; i++) m_combos.Add(new ComboTextAnim((EnumComboTextType)i,texts[i]));
        m_cg = GetComponent<CanvasGroup>();
        ResetToStart();
        if (flickerTween) flickerTween.onComplete.AddListener(OnFlickerTweenComplete);
        if (startTween) startTween.onComplete.AddListener(OnStartTweenComplete);
    }

    private void OnDestroy()
    {
        m_combos.Clear();
    }

    private void OnDisable()
    {
        ResetAllParameters();
    }

    public void ResetAllParameters()
    {
        ResetToStart();
        m_numberTween = false;
    }

    private void ResetToStart()
    {
        ResetToStartComplete();
        startTween?.Kill();
    }

    private void ResetToStartComplete()
    {
        if(m_cg) m_cg.alpha = 1;
        foreach (var item in m_combos)
        {
            item.font = changeFonts[0];
            item.Reset();
        }
        m_changeFontTween?.Kill();
        flickerTween?.Kill();
    }

    [ContextMenu("PlayTestTween")]
    public void PlayTestTween()
    {
        if (testMode != EnumComboTestMode.None) m_testTimes = 1;
        StopAllCoroutines();
        StartCoroutine(PlayTest());
    }

    private IEnumerator PlayTest()
    {
        PlayComboTween(testValue);
        if(testMode == EnumComboTestMode.DuringNumberChange)
        {
            float time = testValue * numDur * 0.5f;
            yield return new WaitForSeconds(time);
            PlayComboTween(testValue * 2);
        }
    }

    public void PlayComboTween(int tar = 0)
    {
        if (tar > 0) m_tarValue = tar;
        //Logger.LogWarning("set value is {0}[cur : {1} && istween : {2}]",tar,m_curValue, m_numberTween);
        if (m_numberTween) return;

        //解决连击数字不显示的问题
        if (m_curValue >= tar) m_curValue = 0;

        //still play tween(1.change font tween 2.flicker tween),then only change to add anim
        if(m_curValue > 0)
        {
            //Logger.LogError("still in tween,continue play");
            ResetToStartComplete();
            OnStartTweenComplete();
            return;
        }

        ResetToStart();
        PlayStartTween();
    }

    private void PlayStartTween()
    {
        m_numberTween = true;
        if (m_curValue < m_tarValue)
        {
            m_curValue++;
            foreach (var item in m_combos) item.CheckPlayAnim(m_curValue,0f,null);
        }
        if (startTween) startTween.PlayForward();
        else OnStartTweenComplete();
    }

    private void OnStartTweenComplete(bool reverse = false)
    {
        m_numberTween = true;
        PlayAddNum();
    }

    private void PlayAddNum()
    {
        if (m_curValue < m_tarValue)
        {
            m_curValue++;
            //Logger.LogWarning("{0} set value  {1}", Time.time, m_curValue);
            foreach (var item in m_combos) item.CheckPlayAnim(m_curValue, numDur, PlayAddNum);
        }
        else ChangeFont();
    }
    
    private void ChangeFont()
    {
        //Logger.LogError("out the tween,ChangeFont-------Called");
        m_numberTween = false;
        foreach (var item in m_combos) item.font = changeFonts[1];
        m_changeFontTween = transform.DOScale(transform.localScale,fontWaitDur).OnComplete(OnChangeFontComplete);

        if (m_testTimes > 0 && testMode == EnumComboTestMode.DuringFontChange) TestAddCombo(fontWaitDur * 0.5f);
    }

    private void OnChangeFontComplete()
    {
        //Logger.LogError("out the tween,OnChangeFontComplete-------Called");
        flickerTween?.PlayForward();

        if (m_testTimes > 0 && testMode == EnumComboTestMode.DuringFlicker) TestAddCombo(flickerTween.duration * flickerTween.loopCount * 0.5f); 
    }

    private void TestAddCombo(float dur)
    {
        m_testTimes--;
        transform.DOScale(transform.localScale, dur).OnComplete(() =>
        {
            PlayComboTween(m_tarValue + 15);
        });
    }

    private void OnFlickerTweenComplete(bool reverse)
    {
        ResetToStart();
        m_curValue = 0;
        m_tarValue = 0;
    }
}
