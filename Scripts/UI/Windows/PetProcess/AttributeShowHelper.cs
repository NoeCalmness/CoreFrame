// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-08-30      18:20
//  * LastModify：2018-09-29      14:12
//  ***************************************************************************************************/

using System.Globalization;
using UnityEngine.UI;

public static class AttributeShowHelper
{
    public static string ValueString(ushort rId, double rValue)
    {
        return ValueForShowString(rId, rValue);
    }

    public static string TypeString(ushort rId)
    {
        return ConfigText.GetDefalutString((int) TextForMatType.AllAttributeText, rId);
    }

    public static string ValueForShowString(ushort rId, double rValue, bool forceRate = false)
    {
        var fieldType = (CreatureFields)rId;
        rValue = ValueForShow(rId, rValue, forceRate);
        if (forceRate)
            return rValue.ToString("P2");
        if (GeneralConfigInfo.IsPercentAttribute(rId))
            return rValue.ToString("P2");
        if (fieldType == CreatureFields.RegenRage)
            return rValue.ToString(CultureInfo.InvariantCulture);
        return rValue.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 用于对属性显示的数值进行修整（去尾或者四舍五入）
    /// </summary>
    /// <param name="rId"></param>
    /// <param name="rValue"></param>
    /// <param name="forceRate"></param>
    /// <returns></returns>
    public static double ValueForShow(ushort rId, double rValue, bool forceRate = false)
    {
        var fieldType = (CreatureFields)rId;
        if (forceRate)
            return Mathd.RoundToInt(rValue * 10000) * 0.0001;
        if (GeneralConfigInfo.IsPercentAttribute(rId))
            return Mathd.RoundToInt(rValue * 10000) * 0.0001;
        if (fieldType == CreatureFields.RegenRage)
            return rValue;
        return Mathd.RoundToInt(rValue);
    }

    #region 扩展

    public static string TypeString(this PItemAttachAttr rAttr)
    {
        if (rAttr == null)
            return string.Empty;

        return TypeString(rAttr.id);
    }

    public static string ValueString(this PItemAttachAttr rAttr)
    {
        if (rAttr == null)
            return string.Empty;

        return ValueForShowString(rAttr.id, rAttr.value, rAttr.type == 2);
    }

    public static string ShowString(this PItemAttachAttr rAttr)
    {
        if (rAttr == null)
            return string.Empty;

        return $"{rAttr.TypeString()}+{rAttr.ValueString()}";
    }


    public static string TypeString(this ItemAttachAttr rAttr)
    {
        if (rAttr == null)
            return string.Empty;

        return TypeString(rAttr.id);
    }

    public static string ValueString(this ItemAttachAttr rAttr)
    {
        if (rAttr == null)
            return string.Empty;

        return ValueForShowString(rAttr.id, rAttr.value, rAttr.type == 2);
    }

    public static string ShowString(this ItemAttachAttr rAttr)
    {
        return $"{rAttr.TypeString()}+{rAttr.ValueString()}";
    }

    #endregion

}


public class AttributeBinder : System.IDisposable
{
    public Text name;
    public Text value;

    public virtual void Bind(PItemAttachAttr rAttr)
    {
        Util.SetText(name, rAttr.TypeString());
        Util.SetText(value, rAttr.ValueString());
    }

    public virtual void Bind(ItemAttachAttr rAttr)
    {
        Util.SetText(name, rAttr.TypeString());
        Util.SetText(value, rAttr.ValueString());
    }

    public virtual void Dispose()
    {
        
    }
}