/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-06-20
 * 
 ***************************************************************************************************/

public class Module_Attribute : Module<Module_Attribute>
{
    public const string EventAddSuccess = "EventAddSuccess";
    public const string EventAddFailed = "EventAddFailed";
    public const string EventLevelUp = "EventLevelUp";
    public const string EventResetAttributeSuceess = "EventResetAttributeSuceess";
    public const string EventResetAttributeFailed = "EventResetAttributeFailed";
    public const string EventAddCompute = "EventAddCompute";
    public const string EventChangeAttribute = "EventChangeAttribute";

    public PRoleAttr attrInfo { get { return m_attrInfo; } }
    private PRoleAttr m_attrInfo;

    public PRoleAttrItem[] awakeChangeAttr { get { return m_awakeChangeAttr; } }
    private PRoleAttrItem[] m_awakeChangeAttr;

    private ushort addPoint;//加的潜力点

    public double oldPower { get; private set; }

    public double oldTechnique { get; private set; }

    public double oldEnergy { get; private set; }

    public bool NeedNotice { get { return modulePlayer.roleInfo?.attrPoint > 0 && !isRead; } }
    public bool isRead { get; set; }

    void _Packet(ScRoleInfo p)
    {
        if (p.attr != null)
            p.attr.CopyTo(ref m_attrInfo);
        if (p.awakeChangeAttr != null)
            p.awakeChangeAttr.CopyTo(ref m_awakeChangeAttr);
    }

    public void GMCreatePlayer(ScRoleInfo p)
    {
#if UNITY_EDITOR
        _Packet(p);
#endif
    }

    public void SendAddAttr(ushort powerPoint, ushort techniquePoint, ushort bodyPowerPoint)
    {
        var p = PacketObject.Create<CsAddCustomAttr>();
        p.power = powerPoint;
        p.tenacious = techniquePoint;
        p.energy = bodyPowerPoint;

        addPoint = (ushort)(powerPoint + techniquePoint + bodyPowerPoint);
        session.Send(p);
    }

    public void SendLevelInfo(int level, string[] ags)
    {
        var p = PacketObject.Create<CsRoleGm>();
        p.gmType = (short)level;
        p.args = ags;
        session.Send(p);
    }

    public void ResetAttr()
    {
        var p = PacketObject.Create<CsResetCustomAttrs>();
        session.Send(p);
    }

    /// <summary>
    /// 加点改变的属性
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScAddCustomAttr p)
    {
        if (p.result == 0)
        {
            PRoleAttrItem[] attrs = null;
            p.changeAttrs.CopyTo(ref attrs);

            ChangeAttribute(attrs);
            modulePlayer.roleInfo.attrPoint -= addPoint;
            DispatchModuleEvent(EventAddSuccess);//加点成功要返回新的属性值
        }
        else DispatchModuleEvent(EventAddFailed, p);
    }

    /// <summary>
    /// 升级改变的属性
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScRoleLevelUp p)
    {
        PRoleAttrItem[] attrs = null;
        p.changeAttrs.CopyTo(ref attrs);

        oldPower = attrInfo.power[0];
        oldTechnique = attrInfo.tenacious[0];
        oldEnergy = attrInfo.energy[0];

        moduleAttribute.isRead = false;
        ChangeAttribute(attrs);

        DispatchModuleEvent(EventLevelUp, p);//升级角色属性的变化
    }

    /// <summary>
    /// 洗点改变的属性
    /// </summary>
    /// <param name="p"></param>
    void _Packet(ScResetCustomAttrs p)
    {
        if (p.result == 0)
        {
            p.roleAttrs.CopyTo(ref m_attrInfo);

            modulePlayer.roleInfo.attrPoint = p.attrPoint;
            DispatchModuleEvent(EventResetAttributeSuceess);//洗点属性变化
        }
        else
            DispatchModuleEvent(EventResetAttributeFailed,p.result);
    }

    /// <summary>
    /// 换符文,强化符文,升星符文改变的属性
    /// </summary>
    /// <param name="attrChange"></param>
    void _Packet(ScRoleAttrChange attrChange)
    {
        PRoleAttrItem[] attrs = null;
        attrChange.changeAttrs.CopyTo(ref attrs);
        ChangeAttribute(attrs);
        DispatchModuleEvent(EventChangeAttribute);
    }

    void _Packet(ScAwakeAttrChanged attrChange)
    {
        attrChange.changeAttrs?.CopyTo(ref m_awakeChangeAttr);
    }

    private void ChangeAttribute(PRoleAttrItem[] attrList)
    {
        for (int i = 0; i < attrList.Length; i++)
        {
            switch (attrList[i].id)
            {
                case 2:
                    m_attrInfo.power[0] = attrList[i].value[0];//总属性
                    m_attrInfo.power[1] = attrList[i].value[1];//初始属性
                    m_attrInfo.power[2] = attrList[i].value[2];//没乘百分数的属性
                    break;
                case 3:
                    m_attrInfo.tenacious[0] = attrList[i].value[0];
                    m_attrInfo.tenacious[1] = attrList[i].value[1];
                    m_attrInfo.tenacious[2] = attrList[i].value[2];
                    break;
                case 4:
                    m_attrInfo.energy[0] = attrList[i].value[0];
                    m_attrInfo.energy[1] = attrList[i].value[1];
                    m_attrInfo.energy[2] = attrList[i].value[2];
                    break;
                case 5:
                    m_attrInfo.maxHp[0] = attrList[i].value[0];
                    m_attrInfo.maxHp[1] = attrList[i].value[1];
                    m_attrInfo.maxHp[2] = attrList[i].value[2];
                    break;
                case 7:
                    m_attrInfo.attack[0] = attrList[i].value[0];
                    m_attrInfo.attack[1] = attrList[i].value[1];
                    m_attrInfo.attack[2] = attrList[i].value[2];
                    break;
                case 8:
                    m_attrInfo.defense[0] = attrList[i].value[0];
                    m_attrInfo.defense[1] = attrList[i].value[1];
                    m_attrInfo.defense[2] = attrList[i].value[2];
                    break;
                case 9:
                    m_attrInfo.knock[0] = attrList[i].value[0];
                    m_attrInfo.knock[1] = attrList[i].value[1];
                    m_attrInfo.knock[2] = attrList[i].value[2];
                    break;
                case 10:
                    m_attrInfo.knockRate[0] = attrList[i].value[0];
                    m_attrInfo.knockRate[1] = attrList[i].value[1];
                    m_attrInfo.knockRate[2] = attrList[i].value[2];
                    break;
                case 11:
                    m_attrInfo.artifice[0] = attrList[i].value[0];
                    m_attrInfo.artifice[1] = attrList[i].value[1];
                    m_attrInfo.artifice[2] = attrList[i].value[2];
                    break;
                case 12:
                    m_attrInfo.attackSpeed[0] = attrList[i].value[0];
                    m_attrInfo.attackSpeed[1] = attrList[i].value[1];
                    m_attrInfo.attackSpeed[2] = attrList[i].value[2];
                    break;
                case 13:
                    m_attrInfo.moveSpeed[0] = attrList[i].value[0];
                    m_attrInfo.moveSpeed[1] = attrList[i].value[1];
                    m_attrInfo.moveSpeed[2] = attrList[i].value[2];
                    break;
                case 14:
                    m_attrInfo.bone[0] = attrList[i].value[0];
                    m_attrInfo.bone[1] = attrList[i].value[1];
                    m_attrInfo.bone[2] = attrList[i].value[2];
                    break;
                case 15:
                    m_attrInfo.brutal[0] = attrList[i].value[0];
                    m_attrInfo.brutal[1] = attrList[i].value[1];
                    m_attrInfo.brutal[2] = attrList[i].value[2];
                    break;
                case 16:
                    m_attrInfo.angerSec[0] = attrList[i].value[0];
                    m_attrInfo.angerSec[1] = attrList[i].value[1];
                    m_attrInfo.angerSec[2] = attrList[i].value[2];
                    break;
                case 17:
                    m_attrInfo.gunAttack[0] = attrList[i].value[0];
                    m_attrInfo.gunAttack[1] = attrList[i].value[1];
                    m_attrInfo.gunAttack[2] = attrList[i].value[2];
                    break;
                case 18:
                    m_attrInfo.elementAttack[0] = attrList[i].value[0];
                    m_attrInfo.elementAttack[1] = attrList[i].value[1];
                    m_attrInfo.elementAttack[2] = attrList[i].value[2];
                    break;
                case 19:
                    m_attrInfo.elementDefenseWind[0] = attrList[i].value[0];
                    m_attrInfo.elementDefenseWind[1] = attrList[i].value[1];
                    m_attrInfo.elementDefenseWind[2] = attrList[i].value[2];
                    break;
                case 20:
                    m_attrInfo.elementDefenseFire[0] = attrList[i].value[0];
                    m_attrInfo.elementDefenseFire[1] = attrList[i].value[1];
                    m_attrInfo.elementDefenseFire[2] = attrList[i].value[2];
                    break;
                case 21:
                    m_attrInfo.elementDefenseWater[0] = attrList[i].value[0];
                    m_attrInfo.elementDefenseWater[1] = attrList[i].value[1];
                    m_attrInfo.elementDefenseWater[2] = attrList[i].value[2];
                    break;
                case 22:
                    m_attrInfo.elementDefenseThunder[0] = attrList[i].value[0];
                    m_attrInfo.elementDefenseThunder[1] = attrList[i].value[1];
                    m_attrInfo.elementDefenseThunder[2] = attrList[i].value[2];
                    break;
                case 23:
                    m_attrInfo.elementDefenseIce[0] = attrList[i].value[0];
                    m_attrInfo.elementDefenseIce[1] = attrList[i].value[1];
                    m_attrInfo.elementDefenseIce[2] = attrList[i].value[2];
                    break;
                default:
                    break;
            }
        }
    }

    protected override void OnGameDataReset()
    {
        base.OnGameDataReset();
        m_attrInfo = null;
        isRead = false;
    }
}
