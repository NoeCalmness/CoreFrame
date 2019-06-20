using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Random = UnityEngine.Random;

public class ExpTest : MonoBehaviour
{
    public int totalExp = 1234;

    StringBuilder sb = new StringBuilder();

    public void Start()
    {
        Calc();
    }

    [ContextMenu("Calc")]
    private void Calc()
    {
        InitProps();
        GetExpDatas();
    }

    private List<PropItemInfo> propConfigs = new List<PropItemInfo>();
    private List<PropItemInfo> currenProps = new List<PropItemInfo>();
    private void InitProps()
    {
        propConfigs = ConfigManager.GetAll<PropItemInfo>().FindAll(o=>o.itemType == PropType.IntentyProp);
        currenProps.Clear();

        int count = Random.Range(3, propConfigs.Count);
        while (currenProps.Count != count && propConfigs.Count > 0)
        {
            PropItemInfo i = propConfigs[Random.Range(1, propConfigs.Count)];
            propConfigs.Remove(i);
            currenProps.Add(i);
            i.ownNum = (uint)Random.Range(1, 10);
            i.useNum = 0;
        }

        sb.Remove(0,sb.Length);
        sb.AppendLine("Select props:");
        foreach (var item in currenProps)
        {
            sb.AppendLine(Util.Format("id : {0} exp : {1} num : {2}", item.ID, item.gainExp, item.ownNum));
        }
        sb.AppendLine("-----------------------------------------------------------------");
    }


    private void GetExpDatas()
    {
        List<IExp> data = ExpUtil.GetValidExps(currenProps, (uint)totalExp);
        int realExp = 0;
        
        sb.AppendLine("Get props:");
        foreach (var item in data)
        {
            PropItemInfo prop = item as PropItemInfo;
            sb.AppendLine(Util.Format("id : {0} exp : {1} use num : {2}", prop.ID, prop.gainExp, item.useNum));
            realExp += (int)prop.gainExp * item.useNum;
        }
        sb.AppendLine("-----------------------------------------------------------------");
        sb.AppendLine(Util.Format("set total exp : {0}  select item total Exp :{1}",totalExp,realExp));
        Logger.LogWarning(sb.ToString());
    }
}
