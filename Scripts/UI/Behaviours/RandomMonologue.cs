using UnityEngine;
using UnityEngine.UI;

public class RandomMonologue : MonoBehaviour
{
    private bool isNeedCreature;
    private Creature m_creature;
    private bool m_isIdle;
    private string[] texts;
    private int index;
    private float timer;
    private Text text;
    private Transform textTran;
    private int testTime = 0;

    public void InitializedData(int id, Creature creature = null)
    {
        isNeedCreature = creature != null;
        m_creature = creature;

        if (m_creature != null)
        {
            m_creature.RemoveEventListener(CreatureEvents.ENTER_STATE, OnStateChaged);
            m_creature.AddEventListener(CreatureEvents.ENTER_STATE, OnStateChaged);
        }

        text = transform.Find("bg/content")?.GetComponent<Text>();
        textTran = transform.Find("bg");

        var config = ConfigManager.Get<ConfigText>(id);
        if (config == null || config.text == null || config.text.Length < 1) return;
        texts = config.text;
    }

    private void OnStateChaged()
    {
        if (m_creature == null) return;
        m_isIdle = m_creature.currentState.info.state.Equals(NpcMono.NPC_IDLE_STATE);
    }

    private void OnDisable()
    {
        timer = 0;
        textTran.SafeSetActive(false);
    }

    private void Update()
    {
        if (textTran.gameObject.activeInHierarchy)
        {
            timer = 0;
            return;
        }
        if (isNeedCreature && !m_isIdle && m_creature == null || textTran == null)
        {
            timer = 0;
            return;
        }

        timer += Time.deltaTime;
        //if (testTime != (int)timer) Logger.LogDetail("current time= {0}", testTime);
        testTime = (int)timer;
        if (timer >= GeneralConfigInfo.defaultConfig.waitLogueTime)
        {
            index = Random.Range(0, texts.Length);
            Util.SetText(text, texts[index]);
            textTran.gameObject.SafeSetActive(true);
            timer = 0;
        }
    }
}
