/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Config item extend methods.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2019-02-13
 * 
 ***************************************************************************************************/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ConfigExtendedMethods
{
    public static string ToXml(this Vector2 vec) { return Util.Format("{0},{1}", vec.x, vec.y); }
    public static string ToXml(this Vector3 vec, string node = "", int depth = 1, bool list = false) { var xml = Util.Format(list ? "<{3} x=\"{0}\" y=\"{1}\" z=\"{2}\" />" : "{0},{1},{2}{3}", vec.x, vec.y, vec.z, list ? node : ""); return list ? Util.PadStringLeft(xml, depth * 2) : xml; }
    public static string ToXml(this Vector4 vec) { return Util.Format("{0},{1},{2},{3}", vec.x, vec.y, vec.z, vec.w); }
    public static string ToXml(this Vector2_ vec) { return Util.Format("{0},{1}", vec.x, vec.y); }
    public static string ToXml(this Vector3_ vec, string node = "", int depth = 1, bool list = false) { var xml = Util.Format(list ? "<{3} x=\"{0}\" y=\"{1}\" z=\"{2}\" />" : "{0},{1},{2}{3}", vec.x, vec.y, vec.z, list ? node : ""); return list ? Util.PadStringLeft(xml, depth * 2) : xml; }
    public static string ToXml(this Vector4_ vec) { return Util.Format("{0},{1},{2},{3}", vec.x, vec.y, vec.z, vec.w); }
    public static string ToXml(this Quaternion qua) { return Util.Format("{0},{1},{2},{3}", qua.x, qua.y, qua.z, qua.w); }
    public static string ToXml(this Color col) { return Util.Format("#{0:X00000000}", Util.ColorToInt(col)); }

    public static string ToXml(this Keyframe key, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} time=\"{1}\" value=\"{2}\" inTangent=\"{3}\" outTangent=\"{4}\" tangentMode=\"{5}\" />", node, key.time, key.value, key.inTangent, key.outTangent, key.tangentMode);
        return Util.PadStringLeft(xml, depth * 2);
    }

    public static string ToXml(this AnimationCurve curve, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0}>{1}</{0}>", node, curve == null || curve.length < 1 ? "" : "\r\n" + curve.keys.ToXml("keys", depth + 1) + Util.PadStringRight("\r\n", depth * 2));
        return Util.PadStringLeft(xml, depth * 2);
    }

    public static string ToXml(this TransitionInfo.Condition cond, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} type=\"{1}\" value=\"{2}\" {3}/>", node, cond.param, cond.threshold, cond.GetModeString(true));
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this TransitionInfo.Transition trans, string node = "", int depth = 1)
    {
        var fields = trans.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ID=\"{1}\"", node, trans.ID), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] =='<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(trans)) continue;
            var ixml = field.GetValue(trans).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(trans).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this StateMachineInfo.AttackBox box, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} type=\"{1}\" start=\"{2}\" size=\"{3}\" attackInfo=\"{4}\" bulletAttackInfo=\"{5}\" />", node, (int)box.type, box.start.ToXml(), box.size.ToXml(), box.attackInfo, box.bulletAttackInfo);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StateMachineInfo.Section section, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} startFrame=\"{1}\"", node, section.startFrame.ToXml());
        xml = Util.PadStringLeft(xml, depth * 2);
        xml += section.attackBox.type == StateMachineInfo.AttackBox.AttackBoxType.None ? " />" : ">\r\n" + section.attackBox.ToXml("attackBox", depth + 1) + "\r\n" + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this StateMachineInfo.FrameData data, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} startFrame=\"{1}\" {2}{3}{4}/>", node, data.startFrame, (!data.disable ? "" : "disable=\"1\" "),
            (data.intValue0 == 0 ? "" : "intValue0=\"" + data.intValue0 + "\" "), (data.intValue1 == 0 ? "" : "intValue1=\"" + data.intValue1 + "\" "),
            (data.doubleValue0 == 0 ? "" : "doubleValue0=\"" + data.doubleValue0 + "\" "));
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StateMachineInfo.Effect effect, string node = "", int depth = 1)
    {
        var fields = effect.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} startFrame=\"{1}\"", node, effect.startFrame), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "startFrame" || field.FieldType == typeof(bool) && !(bool)field.GetValue(effect)) continue;
            var ixml = field.GetValue(effect).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(effect).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this StateMachineInfo.FlyingEffect effect, string node = "", int depth = 1)
    {
        if (effect.isEmpty) return null;

        var fields = effect.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} startFrame=\"{1}\"", node, effect.startFrame), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "startFrame" || field.FieldType == typeof(bool) && !(bool)field.GetValue(effect)) continue;
            var ixml = field.GetValue(effect).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(effect).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this StateMachineInfo.SingleSound sound, string node = "", int depth = 1)
    {
        if (string.IsNullOrEmpty(sound.sound)) return "";

        var xml = Util.Format("<{0} isVoice=\"{1}\" sound=\"{2}\" weight=\"{3}\"  proto=\"{4}\" />", node, sound.isVoice ? 1 : 0, sound.sound, sound.weight, sound.proto);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StateMachineInfo.SoundEffect sound, string node = "", int depth = 1)
    {
        if ((sound.sounds == null || sound.sounds.Length < 1) && (sound.femaleSounds == null || sound.femaleSounds.Length < 1)) return null;

        var fields = sound.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} startFrame=\"{1}\"", node, sound.startFrame), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "startFrame" || field.FieldType == typeof(bool) && !(bool)field.GetValue(sound)) continue;
            var ixml = field.GetValue(sound).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(sound).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this StateMachineInfo.StateDetail detail, string node = "", int depth = 1)
    {
        var overwrite = node == "override";
        var ofs = overwrite ? detail.__overrideParams ?? new string[] { } : null;
        var igs = StateOverrideInfo.StateOverride.ignoreFields;
        var fields = detail.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} {1}=\"{2}\"", node, overwrite ? "level" : "state", overwrite ? detail.level.ToString() : detail.state), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            var n = field.Name;
            if (n[0] == '<' || n.StartsWith("__") || n == "ID" || n == "state" || n == "level" || !overwrite && n == "damageMul" || !overwrite && field.FieldType == typeof(bool) && !(bool)field.GetValue(detail) || overwrite && (igs.Contains(n, true) || !ofs.Contains(n, true))) continue;
            var ixml = field.GetValue(detail).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(detail).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this AttackInfo.PassiveTransition info, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} fromGroup=\"{1}\" toState=\"{2}\" toToughState=\"{3}\" />", node, info.fromGroup, info.toState, info.toToughState);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this CreatureStateInfo.StateInfo info, string node = "", int depth = 1)
    {
        if (info.isEmpty) return null;

        var xml = Util.Format("<{0} ID=\"{1}\" name=\"{2}\" />", node, info.ID, info.name);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this FlyingEffectInfo.Section section, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} startTime=\"{1}\" endTime=\"{2}\"", node, section.startTime.ToXml(), section.endTime.ToXml());
        xml = Util.PadStringLeft(xml, depth * 2);
        xml += section.attackBox.type == StateMachineInfo.AttackBox.AttackBoxType.None ? " />" : ">\r\n" + section.attackBox.ToXml("attackBox", depth + 1) + "\r\n" + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this WeaponInfo.Weapon weapon, string node = "", int depth = 1)
    {
        var fields = weapon.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(weapon).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(weapon).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this WeaponInfo.SingleWeapon weapon, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} index=\"{1}\" model=\"{2}\" bindID=\"{3}\" effects=\"{4}\"/>", node, weapon.index, weapon.model, weapon.bindID, weapon.effects);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this BuffInfo.BuffEffect effect, string node = "", int depth = 1)
    {
        effect.Initialize();

        var remap = Buff.nameRemap[(int)effect.type];
        var xml = "";
        for (var i = 0; i < remap.Length; ++i)
            xml += " " + remap[i] + "=\"" + effect.paramss[i] + "\"";

        xml = Util.Format("<{0} type=\"{1}\" {6}{5}interval=\"{2}\" applyCount=\"{3}\"{4} />", node, effect.type, effect.interval, effect.applyCount, xml, effect.keyEffect ? "keyEffect=\"1\" " : "", effect.flag != BuffInfo.EffectFlags.Unknow ? "flag=\"" + effect.flag.ToString() + "\" " : "");
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this BuffInfo.BuffTrigger trigger, string node = "", int depth = 1)
    {
        trigger.Initialize();

        var nameRemap = new string[][]
        {
            new string[] { },                                                        // Normal
            new string[] { },                                                        // StartFight
            new string[] { },                                                        // Dead
            new string[] { },                                                        // TakeDamage
            new string[] { },                                                        // Attacked
            new string[] { },                                                        // Shooted
            new string[] { },                                                        // CritHurted
            new string[] { "value", "isValue" },                                     // Health
            new string[] { "value", "isValue" },                                     // TargetHealth
            new string[] { "value", "isValue" },                                     // Rage
            new string[] { "value", "isValue" },                                     // TargetRage
            new string[] { "attributeID", "value" },                                 // Field
            new string[] { "attributeID", "value" },                                 // TargetField
            new string[] { "step1", "step2", "step3", "step4", "step5", "isValue" }, // TotalDamage
            new string[] { "value", "isPercent", "notRest", "damping" },             // ElementDamage
            new string[] { },                                                        // Count, unused
        };

        var remap = nameRemap[(int)trigger.type];
        var xml = "";
        for (var i = 0; i < remap.Length; ++i)
            xml += " " + remap[i] + "=\"" + trigger.paramss[i] + "\"";

        xml = Util.Format("<{0} type=\"{1}\" chance=\"{2}\"{3} />", node, trigger.type, trigger.chance, xml);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this ComboInputInfo.SingleSpell spell, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} spellName=\"{1}\" rage=\"{5}\" inputs=\"{2}\" textColor=\"{3}\" backColor=\"{4}\"/>", node, spell.spellName, spell.inputs.ToXml(), spell.textColor.ToXml(), spell.backColor.ToXml(), spell.rage);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this NpcClickBox.NodeClickBox box, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} npcPosType=\"{1}\" position=\"{2}\" euler=\"{3}\" size=\"{4}\"/>", node, box.npcPosType, box.position.ToXml(), box.euler.ToXml(), box.size.ToXml());
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this ComboInputInfo.SpellGroup group, string node = "", int depth = 1)
    {
        if (group.spells == null || group.spells.Length < 1) return null;

        var fields = group.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} group=\"{1}\"", node, group.group), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "group" || field.FieldType == typeof(bool) && !(bool)field.GetValue(group)) continue;
            var ixml = field.GetValue(group).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(group).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this CameraShotInfo.ShotState state, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} time=\"{1}\" offset=\"{2}\" euler=\"{3}\" fieldOfView=\"{4}\" overrideSmooth=\"{5}\" forceCut=\"{6}\" hideScene=\"{7}\" removeCameraEdge=\"{8}\" hideCombatUI=\"{9}\" maskAsset=\"{10}\" maskDuration=\"{11}\">\r\n", node, state.time, state.offset.ToXml(), state.euler.ToXml(), state.fieldOfView, state.overrideSmooth, state.forceCut.ToXml(), state.hideScene.ToXml(), state.removeCameraEdge.ToXml(), state.hideCombatUI.ToXml(), state.maskAsset ?? string.Empty, state.maskDuration);
        xml += state.blend.ToXml("blend", depth + 1) + "\r\n";
        xml += Util.PadStringLeft(Util.Format("</{0}>", node), depth * 2);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this CameraBlend blend, string node = "", int depth = 1)
    {
        var curve = blend.blendType == CameraBlendType.Custom ? blend.curve : null;
        string xml = null;
        if (curve == null || curve.length < 1) xml = Util.Format("<{0} blendType=\"{1}\" />", node, blend.blendType);
        else
        {
            xml = Util.Format("<{0} blendType=\"{1}\">\r\n", node, blend.blendType);
            xml += curve.ToXml("curve", depth + 1) + "\r\n";
            xml += Util.PadStringLeft(Util.Format("</{0}>", node), depth * 2);
        }

        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this SceneEventInfo.SceneEvent scenEvent, string node = "", int depth = 1)
    {
        var fields = scenEvent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.Name == "state" || field.Name == "groupMask" || field.FieldType == typeof(bool) && !(bool)field.GetValue(scenEvent)) continue;
            var ixml = field.GetValue(scenEvent).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(scenEvent).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this SceneEventInfo.SceneBehaviour behaviour, string node = "", int depth = 1)
    {
        StringBuilder description = new StringBuilder();
        string strParamName = string.Empty;
        string vecParamName = string.Empty;

        string[][] names = new string[][]
        {
            new string[] {},                                                                //NONE
            new string[] { "MonsterID", "Group", "Level", "IsBoss","CameraAnim","FrameEventID","ForceDirection" ,"GetReward" },       //CreateMonster
            new string[3] { "MonsterID", "Group", "Amout" },                                //KillMonster
            new string[3] { "TimerID", "TimeAmout", "IsShow" },                             //StartCountDown    
            new string[3] { "TimerID", "TimeAmout", "AbsoluteValue" },                      //AddTimerValue
            new string[1] { "TimerID" },                                                    //DelTimer
            new string[] {},                                                                //StageClear
            new string[] {},                                                                //StageFail
            new string[3] { "ObjectID", "BuffID", "Duraction" },                            //AddBuffer
            new string[2] { "PlotID","PlotType" },                                          //StartStoryDialogue
            new string[2] { "Time", "TexId" },                                              //ShowMessage
            new string[2] { "CounterId", "NumberChange" },                                  //OperatingCounter
            new string[3] { "MonsterID", "Group","LeaveTime" },                             //LeaveMonster
            new string[3] { "MonsterID", "Group" ,"SetType"},                               //SetState
            new string[] {},                                                                //CheckStageFirstTime
            new string[3] { "MonsterID", "Group","Pause" },                                 //AIPauseState
            new string[] {},                                                                //BackToHome
            new string[] {"GuideID"},                                                       //StartGuide
            new string[] {"AudioType","Loop"},                                              //PlayAudio
            new string[] {},                                                                //StopAudio
            new string[] { "BossTimerID", "TimeAmout", "BossId1", "BossId2"},               //BossComing    
            new string[] { "LevelId", "Position", "EventId", "DelayTime"},                  //TransportScene
            new string[] { "TriggerID", "State" , "Flag", "Random"},                        //CreateTrigger 
            new string[] { "TriggerID", "State"},                                           //OperateTrigger
            new string[] { "Direction", "AdditiveValue", "AbsoluteValue"},                  //OperateSceneArea
            new string[] { "SceneActorID", "LogicID", "ReletivePos", "Level", "ForceDirection", "Group"},              //CreateSceneActor
            new string[] { "LogicID"},                                                      //OperateSceneActor
            new string[] { "SceneActorID", "LogicID", "StateType",},                         //DelSceneActorEvent
            new string[] { "MonsterID", "Group", "Level"},                                  //CreateLittle
            new string[] { "RandomID", "MaxValue"},                                         //BuildRandom
            new string[] { "MonsterID", "Group"},                                           //MoveMonsterPos
            new string[] { "LogicID"},                                                      //CreateAssistant
            new string[] { "LogicID"},                                                      //CreateAssistantNpc
            new string[] { "ConditionID" },                                                 //DeleteCondition
            new string[] { "ConditionID", "IncreaseAmount" },                               //IncreaseConditionAmount
            new string[] { "RandomID", "MaxValue"},                                         //RebuildRandom
        };

        switch (behaviour.sceneBehaviorType)
        {
            
            case SceneEventInfo.SceneBehaviouType.PlayAudio:
            case SceneEventInfo.SceneBehaviouType.StopAudio:
                strParamName = "AudioName";
                break;
            case SceneEventInfo.SceneBehaviouType.LeaveMonster:
            case SceneEventInfo.SceneBehaviouType.SetState:
            case SceneEventInfo.SceneBehaviouType.TransportScene:
            case SceneEventInfo.SceneBehaviouType.CreateSceneActor:
            case SceneEventInfo.SceneBehaviouType.OperateSceneActor:
            case SceneEventInfo.SceneBehaviouType.DelSceneActorEvent:
                strParamName = "State";
                break;
            case SceneEventInfo.SceneBehaviouType.CreateTrigger:
                strParamName = "Effect";
                break;
            case SceneEventInfo.SceneBehaviouType.CreateAssistant:
                strParamName = "BornState";
                break;
        }

        switch (behaviour.sceneBehaviorType)
        {
            case SceneEventInfo.SceneBehaviouType.CreateMonster:
            case SceneEventInfo.SceneBehaviouType.MoveMonsterPos:
            case SceneEventInfo.SceneBehaviouType.CreateAssistant:
                vecParamName = "ReletivePos";
                break;
            case SceneEventInfo.SceneBehaviouType.CreateTrigger:
                vecParamName = "Range";
                break;
            case SceneEventInfo.SceneBehaviouType.CreateLittle:
                vecParamName = "Offset";
                break;
        }

        string[] paramNames = names[(int)behaviour.sceneBehaviorType];
        //fill int paramers
        if (paramNames != null && paramNames.Length > 0)
        {
            //开始输出不同的字段
            for (int i = 0; i < paramNames.Length; i++)
            {
                description.AppendFormat("{0}=\"{1}\" ", paramNames[i], behaviour.parameters[i]);
            }
        }

        //fill vec paramers
        if (!string.IsNullOrEmpty(vecParamName)) description.AppendFormat("{0}=\"{1}\" ", vecParamName, behaviour.vecParam.ToXml());

        //fill string paramers
        if (!string.IsNullOrEmpty(strParamName)) description.AppendFormat("{0}=\"{1}\" ", strParamName, behaviour.strParam);

        var xml = Util.Format("<{0} sceneBehaviorType=\"{1}\" {2} />", node, behaviour.sceneBehaviorType, description.ToString());
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this SceneEventInfo.SceneCondition condition, string node = "", int depth = 1)
    {
        string[][] names = new string[][]
            {
                new string[] {},                                                            //NONE
                new string[] {},                                                            //EnterScene
                new string[1] { "TimerID" },                                                //CountDownEnd
                new string[3] { "MonsterID", "Group", "Amout"},                             //MonsterDeath
                new string[] {"notFirst"},                                                  //StageFirstTime
                new string[1] { "PlotID" },                                                 //StoryDialogueEnd    
                new string[2] { "CounterID", "Number" },                                    //CounterNumber
                new string[2] { "MonsterID", "Group" },                                     //MonsterLeaveEnd
                new string[3] { "MonsterID", "Group", "LessThan" },                         //MonsterHPLess
                new string[1] { "GuideID" },                                                //GuideEnd 
                new string[1] { "BossTimerID"},                                             //BossComingEnd
                new string[1] { "Vocation"},                                                //PlayerVocation
                new string[4] { "TriggerID", "TriggerType", "PlayerNum", "MonsterID"},      //EnterTrigger
                new string[2] { "LogicID", "StateType"},                                    //SceneActorState
                new string[] {},                                                            //WindowCombatVisible
                new string[] {"notFirst"},                                                  //EnterForFirstTime
                new string[] { "RandomID", "MinValue", "MaxValue", "Value"},                //RandomInfo
                new string[] { "MonsterID", "Group"},                                       //MonsterAttack      
                new string[1] {"LessThan"},                                                  //PlayerHPLess
                new string[2] {"Times", "LogicID"},                                         //HitTimes
            };

        string strParamName = string.Empty;
        switch (condition.sceneEventType)
        {
            case SceneEventInfo.SceneConditionType.SceneActorState:
                strParamName = "Status";
                break;
        }

        StringBuilder description = new StringBuilder();
        string[] paramNames = names[(int)condition.sceneEventType];
        if (paramNames != null && paramNames.Length > 0)
        {
            //开始输出不同的字段
            for (int i = 0; i < paramNames.Length; i++)
            {
                description.AppendFormat("{0}=\"{1}\" ", paramNames[i], condition.parameters[i]);
            }
        }

        //fill string paramers
        if (!string.IsNullOrEmpty(strParamName)) description.AppendFormat("{0}=\"{1}\" ", strParamName, condition.strParam);
        if(condition.conditionId > 0)
        {
            description.AppendFormat("{0}=\"{1}\" ", "ConditionID", condition.conditionId);
        }

        var xml = Util.Format("<{0} sceneEventType=\"{1}\" {2}/>", node, condition.sceneEventType, description.ToString());
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this SceneFrameEventInfo.SceneFrameEventItem e, string node = "", int depth = 1)
    {
        var fields = e.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(e)) continue;
            var ixml = field.GetValue(e).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(e).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this MonsterAttriubuteInfo.MonsterAttriubute a, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} level=\"{1}\" health=\"{2}\" attack=\"{3}\" defence=\"{4}\" critical=\"{5}\" criticalMul=\"{6}\" resilience=\"{7}\" firm=\"{8}\" attackSpeed=\"{9}\" />",
            node, a.level,a.health, a.attack, a.defence, a.critical, a.criticalMul, a.resilience, a.firm, a.attackSpeed);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this AIConfig.LockEnermyInfo a, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} distance=\"{1}\" lockRate=\"{2}\"/>", node, a.distance, a.lockRate);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this AIConfig.AIStratergy a, string node = "", int depth = 1)
    {
        var fields = a.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(a)) continue;
            var ixml = field.GetValue(a).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(a).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this AIConfig.SingleAIStratergy a, string node = "", int depth = 1)
    {
        var fields = a.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(a) || field.Name.Contains("min") || field.Name.Contains("max"))
                continue;

            //对于单挑策略，如果执行次数为-1的时候就不导出到表中显示
            if (field.Name.Equals("repeatTimes") && field.GetValue(a).Equals(-1))
                continue;
            
            var ixml = field.GetValue(a).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(a).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }
    public static string ToXml(this BossEffectInfo.money a, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} itemId=\"{1}\" count=\"{2}\"/>", node, a.itemId, a.count);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this AIConfig.AICondition a, string node = "", int depth = 1)
    {
        string[][] paraNames = new string[][]
        {
            new string[]{"life"},                   //HpHigh
            new string[]{"life"},                   //HpLow
            new string[]{""},                       //CheckAttackState
            new string[]{"buffId"},                 //HasBuff
            new string[]{"direction"},              //MonDir
            new string[]{"moveState"},              //MoveState
            new string[]{"left"},                 //MonOnThePlayerLeft
        };

        string[] names = paraNames[(int)a.conditionType];

        StringBuilder description = new StringBuilder();
        if (a.paramers.Length < names.Length)
        {
            Logger.LogError("conditionType = {0}的参数长度不正确",a.conditionType);
        }
        else
        {
            //开始输出不同的字段
            for (int i = 0; i < names.Length; i++)
            {
                description.AppendFormat("{0}=\"{1}\" ", names[i], a.paramers[i]);
            }
        }
        var xml = Util.Format("<{0} conditionType=\"{1}\" {2}/>", node, a.conditionType, description.ToString());
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this AIConfig.AIBehaviour a, string node = "", int depth = 1)
    {
        var xml = string.Empty;

        if(a.IsLoop())
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < a.loopDuraction.Length; i++)
            {
                b.AppendFormat("{0},", a.loopDuraction[i]);
            }
            b.Remove(b.Length - 1, 1);

            xml = Util.Format("<{0} behaviourType=\"{1}\" state=\"{2}\" loopDuraction=\"{3}\"/>",
             node, a.behaviourType, a.state, b.ToString());
        }
        else
        {
            xml = Util.Format("<{0} behaviourType=\"{1}\" state=\"{2}\" group=\"{3}\"/>",
                 node, a.behaviourType, a.state,a.group);
        }

        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this NpcActionInfo.NpcPosition pos, string node = "", int depth = 1)
    {
        var fields = pos.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(pos)) continue;
            var ixml = field.GetValue(pos).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(pos).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this NpcActionInfo.AnimAndVoice animAndVoice, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} npcLvType=\"{1}\" state=\"{2}\" stateMonologue=\"{3}\" />", node, animAndVoice.npcLvType, animAndVoice.state, animAndVoice.stateMonologue);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this GeneralConfigInfo.StoryData s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} contextInterval=\"{1}\" npcLeaveTime=\"{2}\" genderTextId=\"{3}\" storySpeed=\"{4}\" storyPreLoadNum=\"{5}\"/>", node, s.contextInterval, s.npcLeaveTime,s.genderTextId,s.storySpeed,s.storyPreLoadNum);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this GeneralConfigInfo.InterludeTime s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} fadeInTime=\"{1}\" remainInTime=\"{2}\" remainOutTime=\"{3}\" fadeOutTime=\"{4}\"/>", node, s.fadeInTime, s.remainInTime,s.remainOutTime, s.fadeOutTime);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this ShowCreatureInfo.CreatureOrNpcData data, string node = "", int depth = 1)
    {
        var fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(data).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(data).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this ShowCreatureInfo.SizeAndPos data, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} size=\"{1}\" fov=\"{2}\" pos=\"{3}\" rotation=\"{4}\" />", node, data.size,data.fov, data.pos.ToXml(), data.rotation.ToXml());
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this WeaponAttribute.WeaponLevel data, string node = "", int depth = 1)
    {
        var fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(data).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(data).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }
    public static string ToXml(this WeaponAttribute.LevelInfo data, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} id=\"{1}\"  type=\"{2}\"  value=\"{3}\" />", node, data.id, data.type, data.value);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this NpcInfo.NpcFetterTask s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} fetterLv=\"{1}\" taskId=\"{2}\" eventId=\"{3}\" hintStoryID=\"{4}\" />", node, s.fetterLv, s.taskId, s.eventId, s.hintStoryID);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this CooperationTask.KillMonster data, string node = "", int depth = 1)
    {
        var fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(data).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(data).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this EquipAnimationInfo.AnimationData data, string node = "", int depth = 1)
    {
        var fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(data).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(data).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this EquipAnimationInfo.GotoData data, string node = "", int depth = 1)
    {
        var fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(data).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(data).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    #region 剧情相关

    public static string ToXml(this StoryInfo weapon, string node = "", int depth = 1)
    {
        var fields = weapon.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(weapon).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(weapon).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.StoryItem s, string node = "", int depth = 1)
    {
        var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(s)) continue;
            var ixml = field.GetValue(s).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(s).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.TalkingRoleData s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} roleId=\"{1}\" rolePos=\"{2}\" highLight=\"{3}\"/>", node, s.roleId, s.rolePos,s.highLight);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.TalkingRoleState s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} roleId=\"{1}\" state=\"{2}\"/>", node, s.roleId, s.state);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.BlackScreenData s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} isBlackScreen=\"{1}\" immediately=\"{2}\"/>", node, s.isBlackScreen, s.imme);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.CameraShakeData s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} delayTime=\"{1}\" shakeId=\"{2}\"/>", node, s.delayTime, s.shakeId);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.StorySoundEffect s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} delayTime=\"{1}\" soundName=\"{2}\"/>", node, s.delayTime, s.soundName);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.GivePropData s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} itemTypeId=\"{1}\" level=\"{2}\" star=\"{3}\" num=\"{4}\"/>", node, s.itemTypeId, s.level,s.star,s.num);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.MusicData s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} musicName=\"{1}\" loop=\"{2}\"/>", node, s.musicName, s.loop);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this StoryInfo.ModelData s, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} model=\"{1}\" positionIndex=\"{2}\"/>", node, s.model, s.positionIndex);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    #endregion

    #region guide info

    private static string[][] guideParames = new string[][]
    {
        new string[] {},
        new string[] {},
        new string[] { "dialogId"},
        new string[] { "titleId", "audio"},
    };

    public static string ToXml(this GuideInfo.GuideItem s, string node = "", int depth = 1)
    {
        var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(s)) continue;

            if (s.type == EnumGuideType.Dialog || s.type == EnumGuideType.Interlude)
            {
                if (field.Name != "type" && !guideParames[(int)s.type].Contains(field.Name)) continue;
            }
            else if(s.type == EnumGuideType.NormalGuide || s.type == EnumGuideType.TipGuide)
            {
                if (field.Name == "dialogId" || guideParames[2].Contains(field.Name) || guideParames[3].Contains(field.Name)) continue;
            }
            
            var ixml = field.GetValue(s).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;


            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(s).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this GuideInfo.GuideSuccessCondition c, string node = "", int depth = 1)
    {
        var xml = string.Empty;
        switch (c.type)
        {
            case EnumGuideCondition.InputKey:
                xml = Util.Format("<{0} type=\"{1}\" {2}=\"{3}\" tipPos=\"{4}\"/>", node, c.type, "InputKey",c.intParams.ToXml(),c.tipPos);
                break;
            case EnumGuideCondition.Position:
                xml = Util.Format("<{0} type=\"{1}\" {2}=\"{3}\" tipPos=\"{4}\"/>", node, c.type, "Position",c.floatParams.ToXml(),c.tipPos);
                break;
        }
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this GuideInfo.GuideIcon c, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} icon=\"{1}\" position=\"{2}\"/>", node, c.icon, c.position.ToXml());
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this GuideInfo.HotAreaData c, string node = "", int depth = 1)
    {
        string[][] hotStr = new string[][]
        {
            new string[] {},                                    //None
            new string[] {"restrainId"},                        //CheckID
            new string[] {"runeId","level","star"},             //Rune
            new string[] {},                                    //CurrentWeapon
            new string[] {"protoIds"},                          //ProtoID
        };

        string[] names = hotStr[(int)c.restrainType];
        StringBuilder s = new StringBuilder();
        if (c.restrainType == EnumGuideRestrain.ProtoID)
        {
            s.AppendFormat("{0}=\"{1}\" ", names[0], c.restrainParames.ToXml());
        }
        else
        {
            for (int i = 0; i < c.restrainParames.Length; i++)
            {
                s.AppendFormat("{0}=\"{1}\" ", names[i], c.restrainParames[i]);
            }
        }
        var xml = Util.Format("<{0} hotWindow=\"{1}\" hotArea=\"{2}\" restrainType=\"{3}\" {4}restrainChild=\"{5}\" effect=\"{6}\" tipHotArea=\"{7}\" protoArea=\"{8}\"/>", 
            node, c.hotWindow, c.hotArea.ToXml(),c.restrainType, s.ToString(), c.restrainChild,c.effect,c.tipHotArea,c.protoArea);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this LabyrinthInfo.LabyrinthReward c, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} propId=\"{1}\" rate=\"{2}\"/>", node, c.propId, c.rate);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this ItemAttachAttr c, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} id=\"{1}\" type=\"{2}\" value=\"{1}\"/>", node, c.id, c.type,c.value);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this EvolveEquipInfo.EvolveMaterial c, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} propId=\"{1}\" num=\"{2}\" />", node, c.propId, c.num);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this TaskInfo.TaskStarDetail s, string node = "", int depth = 1)
    {
        var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(s)) continue;
            var ixml = field.GetValue(s).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(s).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }
    
    public static string ToXml(this TaskInfo.TaskStarReward s, string node = "", int depth = 1)
    {
        var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.FieldType == typeof(bool) && !(bool)field.GetValue(s)) continue;
            var ixml = field.GetValue(s).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(s).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this TaskInfo.TaskStarCondition c, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} type=\"{1}\" value=\"{2}\" />", node, c.type, c.value);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this TaskInfo.TaskStarProp c, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} propId=\"{1}\" level=\"{2}\" star=\"{3}\" num=\"{4}\" />", node, c.propId, c.level, c.star, c.num);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }
 
  
    public static string ToXml(this GuideInfo.GuideConfigCondition c, string node = "", int depth = 1)
    {
        string[][] intNames = new string[][]
            {
                new string[] {},                                                            //NONE
                new string[] {},                                                            //OpenWindow
                new string[1] { "StageId" },                                                //EnterStage
                new string[1] { "GuideId" },                                                //GuideEnd
                new string[1] { "StoryId"},                                                 //StoryEnd
                new string[1] { "Level" },                                                  //PlayerLevel    
                new string[] {},                                                            //RuneMaxLevel
                new string[1] { "PropId" },                                                 //GetProp
                new string[] {},                                                            //OpenLabyrinth
                new string[] {},                                                            //OpenBorderland
                new string[2] { "TaskId","Finish"},                                         //TaskFinish
                new string[2] { "TaskId", "Chanllenge"},                                    //TaskChanllenge
                new string[1] { "Chanllenge" },                                             //PVPChanllenge
                new string[] {},                                                            //SpecialTweenEnd
                new string[] {},                                                            //EnterTrain
                new string[] {},                                                            //DailyFinish
                new string[] {},                                                            //DefalutOperateGuideEnd
                new string[3] {"NpcID","TaskId","Finish"},                                  //NpcDating
            };

        StringBuilder description = new StringBuilder();
        string[] paramNames = intNames[(int)c.type];
        if (paramNames != null && paramNames.Length > 0)
        {
            //开始输出不同的字段
            for (int i = 0; i < paramNames.Length; i++)
            {
                description.AppendFormat("{0}=\"{1}\" ", paramNames[i], c.intParames[i]);
            }
        }
        
        if(c.type == EnumGuideContitionType.OpenWindow) description.AppendFormat("{0}=\"{1}\" ", "WindowName", c.strParames);

        var xml = Util.Format("<{0} type=\"{1}\" {2}/>", node, c.type, description.ToString());
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    #endregion

    public static string ToXml(this StateOverrideInfo.StateOverride item, string node = "", int depth = 1)
    {
        var fields = item.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft("<" + node, depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name.StartsWith("__") || field.FieldType == typeof(bool) && !(bool)field.GetValue(item)) continue;
            var ixml = field.GetValue(item).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + ixml + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);

        return xml;
    }

    public static string ToXml(this IList list, string node = "", int depth = 1)
    {
        var str = "";
        var tree = false;
        if (!node.EndsWith("s")) node += "s";
        var snode = node.Remove(node.Length - 1);
        foreach (var item in list)
        {
            var xml = item.ToXml(snode, depth + 1, true);
            if (!tree && xml.IndexOf("<") > -1)
            {
                tree = true;
                str += Util.PadStringLeft("<" + node + ">\r\n", depth * 2);
            }
            str += xml + (tree ? "\r\n" : ";");
        }
        if (tree) str += Util.PadStringLeft("</" + node + ">", depth * 2);
        return str == "" ? null : str;
    }

    public static string ToXml(this ConfigItem item, string node = "", int depth = 1)
    {
        var fields = item.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft("<item ID=\"" + item.ID + "\"", depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<' || field.Name == "ID" || field.Name.StartsWith("__") || field.FieldType == typeof(bool) && !(bool)field.GetValue(item)) continue;
            var ixml = field.GetValue(item).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + ixml + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</item>", depth * 2);

        return xml;
    }

    public static string ToXml(this Config config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
        sb.AppendLine("<config name=\"" + config.name + "\">");
        var items = config.GetItemsBase();
        foreach (var item in items) sb.AppendLine(item.ToXml());
        sb.AppendLine("</config>");

        return sb.ToString();
    }

    public static string ToXml(this object obj, string node = "", int depth = 1, bool list = false)
    {
        if (obj == null) return "";
        var type = obj.GetType();
        var code = Type.GetTypeCode(type);

        if (code == TypeCode.Boolean) return (bool)obj ? "1" : "0";

        if (code == TypeCode.Byte || code == TypeCode.Char || code == TypeCode.Double || code == TypeCode.Int16 || code == TypeCode.Int32 || code == TypeCode.Int64 || code == TypeCode.SByte || code == TypeCode.Single || code == TypeCode.String || code == TypeCode.UInt16 || code == TypeCode.UInt32 || code == TypeCode.UInt64)
            return obj.ToString();

        if (type == typeof(Vector2))                         return ((Vector2)obj).ToXml();
        if (type == typeof(Vector3))                         return ((Vector3)obj).ToXml(node, depth, list);
        if (type == typeof(Vector4))                         return ((Vector4)obj).ToXml();
        if (type == typeof(Vector2_))                        return ((Vector2_)obj).ToXml();
        if (type == typeof(Vector3_))                        return ((Vector3_)obj).ToXml(node, depth, list);
        if (type == typeof(Vector4_))                        return ((Vector4_)obj).ToXml();
        if (type == typeof(Quaternion))                      return ((Quaternion)obj).ToXml(); 
        if (type == typeof(Color))                           return ((Color)obj).ToXml();
        if (type == typeof(Keyframe))                        return ((Keyframe)obj).ToXml(node, depth);
        if (type == typeof(CameraBlend))                     return ((CameraBlend)obj).ToXml(node, depth);
        if (type == typeof(TransitionInfo.Condition))        return ((TransitionInfo.Condition)obj).ToXml(node, depth);
        if (type == typeof(TransitionInfo.Transition))       return ((TransitionInfo.Transition)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.AttackBox))      return ((StateMachineInfo.AttackBox)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.Section))        return ((StateMachineInfo.Section)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.FrameData))      return ((StateMachineInfo.FrameData)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.Effect))         return ((StateMachineInfo.Effect)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.FlyingEffect))   return ((StateMachineInfo.FlyingEffect)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.SingleSound))    return ((StateMachineInfo.SingleSound)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.SoundEffect))    return ((StateMachineInfo.SoundEffect)obj).ToXml(node, depth);
        if (type == typeof(StateMachineInfo.StateDetail))    return ((StateMachineInfo.StateDetail)obj).ToXml(node, depth);
        if (type == typeof(AttackInfo.PassiveTransition))    return ((AttackInfo.PassiveTransition)obj).ToXml(node, depth);
        if (type == typeof(CreatureStateInfo.StateInfo))     return ((CreatureStateInfo.StateInfo)obj).ToXml(node, depth);
        if (type == typeof(FlyingEffectInfo.Section))        return ((FlyingEffectInfo.Section)obj).ToXml(node, depth);
        if (type == typeof(WeaponInfo.Weapon))               return ((WeaponInfo.Weapon)obj).ToXml(node, depth);
        if (type == typeof(WeaponInfo.SingleWeapon))         return ((WeaponInfo.SingleWeapon)obj).ToXml(node, depth);
        if (type == typeof(BuffInfo.BuffEffect))             return ((BuffInfo.BuffEffect)obj).ToXml(node, depth);
        if (type == typeof(BuffInfo.BuffTrigger))            return ((BuffInfo.BuffTrigger)obj).ToXml(node, depth);
        if (type == typeof(ComboInputInfo.SingleSpell))      return ((ComboInputInfo.SingleSpell)obj).ToXml(node, depth);
        if (type == typeof(ComboInputInfo.SpellGroup))       return ((ComboInputInfo.SpellGroup)obj).ToXml(node, depth);
        if (type == typeof(CameraShotInfo.ShotState))        return ((CameraShotInfo.ShotState)obj).ToXml(node, depth);
        if (type == typeof(SceneEventInfo.SceneEvent))       return ((SceneEventInfo.SceneEvent)obj).ToXml(node, depth);
        if (type == typeof(SceneEventInfo.SceneBehaviour))   return ((SceneEventInfo.SceneBehaviour)obj).ToXml(node, depth);
        if (type == typeof(SceneEventInfo.SceneCondition))   return ((SceneEventInfo.SceneCondition)obj).ToXml(node, depth);
        if (type == typeof(SceneFrameEventInfo.SceneFrameEventItem)) return ((SceneFrameEventInfo.SceneFrameEventItem)obj).ToXml(node, depth);
        if (type == typeof(MonsterAttriubuteInfo.MonsterAttriubute)) return ((MonsterAttriubuteInfo.MonsterAttriubute)obj).ToXml(node, depth);
		if (type == typeof(AIConfig.LockEnermyInfo))         return ((AIConfig.LockEnermyInfo)obj).ToXml(node, depth);
        if (type == typeof(AIConfig.AIStratergy))            return ((AIConfig.AIStratergy)obj).ToXml(node, depth);
        if (type == typeof(AIConfig.SingleAIStratergy))      return ((AIConfig.SingleAIStratergy)obj).ToXml(node, depth);
        if (type == typeof(AIConfig.AICondition))            return ((AIConfig.AICondition)obj).ToXml(node, depth);
        if (type == typeof(AIConfig.AIBehaviour))            return ((AIConfig.AIBehaviour)obj).ToXml(node, depth);
        if (type == typeof(NpcActionInfo.NpcPosition))       return ((NpcActionInfo.NpcPosition)obj).ToXml(node, depth);
        if (type == typeof(NpcActionInfo.AnimAndVoice))      return ((NpcActionInfo.AnimAndVoice)obj).ToXml(node, depth);
        if (type == typeof(GeneralConfigInfo.StoryData))        return ((GeneralConfigInfo.StoryData)obj).ToXml(node, depth);
        if (type == typeof(GeneralConfigInfo.InterludeTime))    return ((GeneralConfigInfo.InterludeTime)obj).ToXml(node, depth);
        if (type == typeof(ShowCreatureInfo.CreatureOrNpcData)) return ((ShowCreatureInfo.CreatureOrNpcData)obj).ToXml(node, depth);
        if (type == typeof(ShowCreatureInfo.SizeAndPos))        return ((ShowCreatureInfo.SizeAndPos)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo))                          return ((StoryInfo)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.StoryItem))                return ((StoryInfo.StoryItem)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.TalkingRoleData))          return ((StoryInfo.TalkingRoleData)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.TalkingRoleState))         return ((StoryInfo.TalkingRoleState)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.BlackScreenData))          return ((StoryInfo.BlackScreenData)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.CameraShakeData))          return ((StoryInfo.CameraShakeData)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.StorySoundEffect))         return ((StoryInfo.StorySoundEffect)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.GivePropData))             return ((StoryInfo.GivePropData)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.MusicData))                return ((StoryInfo.MusicData)obj).ToXml(node, depth);
        if (type == typeof(StoryInfo.ModelData))                return ((StoryInfo.ModelData)obj).ToXml(node, depth);

        if (type == typeof(GuideInfo.GuideConfigCondition))     return ((GuideInfo.GuideConfigCondition)obj).ToXml(node, depth);
        if (type == typeof(GuideInfo.GuideItem))                return ((GuideInfo.GuideItem)obj).ToXml(node, depth);
        if (type == typeof(GuideInfo.GuideSuccessCondition))    return ((GuideInfo.GuideSuccessCondition)obj).ToXml(node, depth);
        if (type == typeof(GuideInfo.GuideIcon))                return ((GuideInfo.GuideIcon)obj).ToXml(node, depth);
        if (type == typeof(GuideInfo.HotAreaData))              return ((GuideInfo.HotAreaData)obj).ToXml(node, depth);
        if (type == typeof(LabyrinthInfo.LabyrinthReward))      return ((LabyrinthInfo.LabyrinthReward)obj).ToXml(node, depth);

        if (type == typeof(ItemAttachAttr))                     return ((ItemAttachAttr)obj).ToXml(node, depth);
        if (type == typeof(EvolveEquipInfo.EvolveMaterial))     return ((EvolveEquipInfo.EvolveMaterial)obj).ToXml(node, depth);
        if (type == typeof(TaskInfo.TaskStarDetail))            return ((TaskInfo.TaskStarDetail)obj).ToXml(node, depth);
        if (type == typeof(TaskInfo.TaskStarReward))            return ((TaskInfo.TaskStarReward)obj).ToXml(node, depth);
        if (type == typeof(TaskInfo.TaskStarCondition))         return ((TaskInfo.TaskStarCondition)obj).ToXml(node, depth);
        if (type == typeof(TaskInfo.TaskStarProp))              return ((TaskInfo.TaskStarProp)obj).ToXml(node, depth);
        
        if (type == typeof(WeaponAttribute.WeaponLevel))        return ((WeaponAttribute.WeaponLevel)obj).ToXml(node, depth);
        if (type == typeof(WeaponAttribute.LevelInfo))          return ((WeaponAttribute.LevelInfo)obj).ToXml(node, depth);
        if (type == typeof(PetAttributeInfo.PetAttribute))      return ((PetAttributeInfo.PetAttribute)obj).ToXml(node, depth);
        if (type == typeof(CooperationTask.KillMonster))        return ((CooperationTask.KillMonster)obj).ToXml(node, depth);

        if (type == typeof(EquipAnimationInfo))                   return ((EquipAnimationInfo)obj).ToXml(node, depth);
        if (type == typeof(EquipAnimationInfo.AnimationData))     return ((EquipAnimationInfo.AnimationData)obj).ToXml(node, depth);
        if (type == typeof(EquipAnimationInfo.GotoData))          return ((EquipAnimationInfo.GotoData)obj).ToXml(node, depth);
        //if (type == typeof(DailyTask .Award))                   return ((DailyTask.Award)obj).ToXml(node, depth);
        if (type == typeof(TaskConfig))                           return ((TaskConfig)obj).ToXml(node,depth);
        if (type == typeof(TaskConfig.TaskFinishCondition))       return ((TaskConfig.TaskFinishCondition)obj).ToXml(node,depth);

        if (type == typeof(StateOverrideInfo.StateOverride))      return ((StateOverrideInfo.StateOverride)obj).ToXml(node, depth);
        if (type == typeof(NpcInfo.NpcFetterTask))                return ((NpcInfo.NpcFetterTask)obj).ToXml(node, depth);
        if (type == typeof(DialogueAnswersConfig.AnswerItem))     return ((DialogueAnswersConfig.AnswerItem)obj).ToXml(node, depth);
        if (type == typeof(DatingMapBuildConfig.ClickPolygonEdge))return ((DatingMapBuildConfig.ClickPolygonEdge)obj).ToXml(node, depth);
        if (type == typeof(DatingMapBuildConfig.Effect))          return ((DatingMapBuildConfig.Effect)obj).ToXml(node, depth);
        if (type == typeof(GeneralConfigInfo.DatingMapCamera))    return ((GeneralConfigInfo.DatingMapCamera)obj).ToXml(node, depth);
        if (type == typeof(DatingEventInfo.DatingEventItem))      return ((DatingEventInfo.DatingEventItem)obj).ToXml(node, depth);
        if (type == typeof(DatingEventInfo.Condition))            return ((DatingEventInfo.Condition)obj).ToXml(node, depth);
        if (type == typeof(DatingEventInfo.Behaviour))            return ((DatingEventInfo.Behaviour)obj).ToXml(node, depth);

        if (type == typeof(BossEffectInfo.money))                 return ((BossEffectInfo.money)obj).ToXml(node, depth);
        if (type == (typeof(NpcClickBox.NodeClickBox)))           return ((NpcClickBox.NodeClickBox)obj).ToXml(node, depth);

        if (typeof(IList).IsAssignableFrom(type))               return ((IList)obj).ToXml(node, depth);
        if (type.IsSubclassOf(typeof(ConfigItem)))              return ((ConfigItem)obj).ToXml(node, depth);
        if (type.IsSubclassOf(typeof(Config)))                  return ((Config)obj).ToXml();

        return "";
    }


    public static string ToXml(this TaskConfig item, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} taskID=\"{1}\" taskNameID=\"{2}\" taskIconID=\"{3}\" descID=\"{4}\"  markSceneID=\"{5}\"  taskConditionType=\"{6}\"  targetID=\"{7}\"   addMood=\"{8}\" />", node,item.ID, item.taskNameID, item.taskIconID,item.taskDescID,item.markSceneID,item.taskFinishType,item.targetID,item.addMood);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this TaskConfig.TaskFinishCondition item, string node = "", int depth = 1)
    {
        var xml = Util.Format("<{0} condition=\"{1}\" value=\"{2}\" />", node, item.condition, item.value);
        xml = Util.PadStringLeft(xml, depth * 2);
        return xml;
    }

    public static string ToXml(this DialogueAnswersConfig.AnswerItem data, string node = "", int depth = 1)
    {
        var fields = data.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(data).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(data).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this DatingMapBuildConfig.ClickPolygonEdge polygon, string node = "", int depth = 1)
    {
        var fields = polygon.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(polygon).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(polygon).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this DatingMapBuildConfig.Effect effect, string node = "", int depth = 1)
    {
        var fields = effect.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(effect).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(effect).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this GeneralConfigInfo.DatingMapCamera datingMap, string node = "", int depth = 1)
    {
        var fields = datingMap.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(datingMap).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(datingMap).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this DatingEventInfo.DatingEventItem datingEvent, string node = "", int depth = 1)
    {
        var fields = datingEvent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(datingEvent).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(datingEvent).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this DatingEventInfo.Condition condition, string node = "", int depth = 1)
    {
        var fields = condition.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(condition).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(condition).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static string ToXml(this DatingEventInfo.Behaviour behaviour, string node = "", int depth = 1)
    {
        var fields = behaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var xml = Util.PadStringLeft(Util.Format("<{0} ", node), depth * 2, ' ');
        var tree = false;
        var tstr = "\r\n";
        foreach (var field in fields)
        {
            if (field.Name[0] == '<') continue;

            var ixml = field.GetValue(behaviour).ToXml(field.Name, depth + 1);
            if (ixml == null) continue;

            if (ixml.IndexOf("<") > -1)
            {
                tree = true;
                tstr += ixml + "\r\n";
            }
            else xml += " " + (field.Name + "=\"" + field.GetValue(behaviour).ToXml() + "\"");
        }
        xml += tree ? ">" : " />";
        if (tree) xml += tstr + Util.PadStringLeft("</" + node + ">", depth * 2);
        return xml;
    }

    public static object FromXml(int source, Type type, string str, string sub = "", string node = "")
    {
        if (type.IsEnum) return Util.ParseEnum(type, str);

        var code = Type.GetTypeCode(type);

        if (code == TypeCode.Boolean || code == TypeCode.Byte || code == TypeCode.Char || code == TypeCode.Double || code == TypeCode.Int16 || code == TypeCode.Int32 || code == TypeCode.Int64 || code == TypeCode.SByte || code == TypeCode.Single || code == TypeCode.String || code == TypeCode.UInt16 || code == TypeCode.UInt32 || code == TypeCode.UInt64)
            return Util.Parse(type, str);

        if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) ||
            type == typeof(Vector2_) || type == typeof(Vector3_) || type == typeof(Vector4_) ||
            type == typeof(Quaternion))
        {
            var vec = new Vector4_();
            if (str.IndexOf("<") > -1)
            {
                var match = Regex.Match(str, @"x\s*=\s*""([^""]*)""");
                if (match.Success) vec.x = Util.Parse<double>(match.Groups[1].Value);
                match = Regex.Match(str, @"y\s*=\s*""([^""]*)""");
                if (match.Success) vec.y = Util.Parse<double>(match.Groups[1].Value);
                match = Regex.Match(str, @"z\s*=\s*""([^""]*)""");
                if (match.Success) vec.z = Util.Parse<double>(match.Groups[1].Value);
                match = Regex.Match(str, @"w\s*=\s*""([^""]*)""");
                if (match.Success) vec.w = Util.Parse<double>(match.Groups[1].Value);
            }
            else
            {
                var vals = Util.ParseString<double>(str, false, ',');
                vec = new Vector4_(vals.Length > 0 ? vals[0] : 0, vals.Length > 1 ? vals[1] : 0, vals.Length > 2 ? vals[2] : 0, vals.Length > 3 ? vals[3] : 0);
            }

            if (type == typeof(Quaternion)) return new Quaternion((float)vec.x, (float)vec.y, (float)vec.z, (float)vec.w);
            if (type == typeof(Vector2))  return (Vector2)(Vector2_)vec;
            if (type == typeof(Vector3))  return (Vector3)(Vector3_)vec;
            if (type == typeof(Vector4))  return (Vector4)vec;
            if (type == typeof(Vector2_)) return (Vector2_)vec;
            if (type == typeof(Vector3_)) return (Vector3_)vec;

            return vec;
        }

        if (type.GetCustomAttribute<AutoAttribute>() != null)
        {
            object attr = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success)
                        field.SetValue(attr, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else
                        field.SetValue(attr, FromXml(source, field.FieldType, ""));
                }
            }
            return attr;
        }

        if (type == typeof(Color)) return Util.BuildColor(str);

        if (type == typeof(Keyframe))
        {
            var key = new Keyframe();

            var match = Regex.Match(str, @"time\s*=\s*""([^""]*)""");
            if (match.Success) key.time = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"value\s*=\s*""([^""]*)""");
            if (match.Success) key.value = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"inTangent\s*=\s*""([^""]*)""");
            if (match.Success) key.inTangent = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"outTangent\s*=\s*""([^""]*)""");
            if (match.Success) key.outTangent = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"tangentMode\s*=\s*""([^""]*)""");
            if (match.Success) key.tangentMode = Util.Parse<int>(match.Groups[1].Value);

            return key;
        }

        if (type == typeof(AnimationCurve))
        {
            var match = Regex.Match(sub, @"(<keys[^\/<]+\/>)|(?:(<keys[^\/<>]*>)((?:(?!<\/keys>).|\s)+)<\/keys>)");
            if (!match.Success) return new AnimationCurve();
            var keys = (Keyframe[])FromXml(source, typeof(Keyframe[]), !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, "keys");
            return keys == null ? new AnimationCurve() : new AnimationCurve(keys);
        }

        if (type == typeof(CameraBlend))
        {
            var blend = CameraBlend.defaultBlend;

            var match = Regex.Match(str, @"blendType\s*=\s*""([^""]*)""");
            if (match.Success) blend.blendType = Util.ParseEnum<CameraBlendType>(match.Groups[1].Value);

            if (blend.blendType == CameraBlendType.Custom)
            {
                match = Regex.Match(str, @"(<curve[^\/<]+\/>)|(?:(<curve[^\/<>]*>)((?:(?!<\/curve>).|\s)+)<\/curve>)");
                if (match.Success) blend.curve = (AnimationCurve)FromXml(source, typeof(AnimationCurve), !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, "curve");

                if (blend.curve == null) blend.blendType = CameraBlendType.Cut;
            }

            return blend;
        }

        if (type == typeof(TransitionInfo.Condition))
        {
            var c = new TransitionInfo.Condition();
            c.mode = ConditionMode.Equals;

            var match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) c.param = match.Groups[1].Value;

            match = Regex.Match(str, @"value\s*=\s*""([^""]*)""");
            if (match.Success) c.threshold = match.Groups[1].Value;

            match = Regex.Match(str, @"mode\s*=\s*""([^""]*)""");
            if (match.Success) c.mode = TransitionInfo.Condition.ParseMode(match.Groups[1].Value);

            return c;
        }

        if (type == typeof(TransitionInfo.Transition))
        {
            var t = new TransitionInfo.Transition();
            t.conditions = string.IsNullOrEmpty(sub) ? new TransitionInfo.Condition[] { } : (TransitionInfo.Condition[])FromXml(source, typeof(TransitionInfo.Condition[]), "<", sub, "conditions");

            var fields = t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(t, FromXml(source, field.FieldType, ""));
                }
            }

            return t;
        }

        if (type == typeof(StateMachineInfo.AttackBox))
        {
            var c = StateMachineInfo.AttackBox.empty;

            var match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) c.ParseType(Util.Parse<int>(match.Groups[1].Value));

            match = Regex.Match(str, @"start\s*=\s*""([^""]*)""");
            if (match.Success) c.start = (Vector3_)FromXml(source, c.start.GetType(), match.Groups[1].Value);

            match = Regex.Match(str, @"size\s*=\s*""([^""]*)""");
            if (match.Success) c.size = (Vector3_)FromXml(source, c.start.GetType(), match.Groups[1].Value);

            match = Regex.Match(str, @"attackInfo\s*=\s*""([^""]*)""");
            if (match.Success) c.attackInfo = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"bulletAttackInfo\s*=\s*""([^""]*)""");
            if (match.Success) c.bulletAttackInfo = Util.Parse<int>(match.Groups[1].Value);

            return c;
        }

        if (type == typeof(StateMachineInfo.Section))
        {
            var s = new StateMachineInfo.Section();
            s.attackBox = string.IsNullOrEmpty(sub) ? StateMachineInfo.AttackBox.empty : (StateMachineInfo.AttackBox)FromXml(source, s.attackBox.GetType(), sub);

            var match = Regex.Match(str, @"startFrame\s*=\s*""([^""]*)""");
            if (match.Success) s.startFrame = Util.Parse<int>(match.Groups[1].Value);

            return s;
        }

        if (type == typeof(StateMachineInfo.FrameData))
        {
            var s = new StateMachineInfo.FrameData();

            var match = Regex.Match(str, @"startFrame\s*=\s*""([^""]*)""");
            if (match.Success) s.startFrame = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"(?:hide|weak|disable|invisible)\s*=\s*""([^""]*)""");
            if (match.Success) s.disable = (bool)FromXml(source, typeof(bool), match.Groups[1].Value);

            match = Regex.Match(str, @"(?:shakeID|index|buff|toughLevel)\s*=\s*""([^""]*)""");
            if (match.Success) s.intValue0 = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"(?:bindID|duration)\s*=\s*""([^""]*)""");
            if (match.Success) s.intValue1 = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"(?:alpha|rate)\s*=\s*""([^""]*)""");
            if (match.Success) s.doubleValue0 = Util.Parse<double>(match.Groups[1].Value);

            return s;
        }

        if (type == typeof(FlyingEffectInfo.Section))
        {
            var s = new FlyingEffectInfo.Section();
            s.attackBox = string.IsNullOrEmpty(sub) ? StateMachineInfo.AttackBox.empty : (StateMachineInfo.AttackBox)FromXml(source, s.attackBox.GetType(), sub);

            var match = Regex.Match(str, @"startTime\s*=\s*""([^""]*)""");
            if (match.Success) s.startTime = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"endTime\s*=\s*""([^""]*)""");
            if (match.Success) s.endTime = Util.Parse<int>(match.Groups[1].Value);

            return s;
        }

        if (type == typeof(WeaponInfo.Weapon))
        {
            var s = new WeaponInfo.Weapon();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }

            return s;
        }

        if (type == typeof(WeaponInfo.SingleWeapon))
        {
            var s = new WeaponInfo.SingleWeapon();

            var match = Regex.Match(str, @"index\s*=\s*""([^""]*)""");
            if (match.Success) s.index = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"model\s*=\s*""([^""]*)""");
            if (match.Success) s.model = match.Groups[1].Value;

            match = Regex.Match(str, @"bindID\s*=\s*""([^""]*)""");
            if (match.Success) s.bindID = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"effects\s*=\s*""([^""]*)""");
            if (match.Success) s.effects = match.Groups[1].Value;

            return s;
        }

        if (type == typeof(BuffInfo.BuffEffect))
        {
            var s = new BuffInfo.BuffEffect()
            {
                paramss = new double[BuffInfo.MAX_EFFECT_PARAM_COUNT],
                sparamss = new string[BuffInfo.MAX_EFFECT_PARAM_COUNT],
            };

    var n = string.Empty;
            var match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) s.type = (BuffInfo.EffectTypes)FromXml(source, typeof(BuffInfo.EffectTypes), match.Groups[1].Value);

            match = Regex.Match(str, @"flag\s*=\s*""([^""]*)""");
            if (match.Success) s.flag = (BuffInfo.EffectFlags)FromXml(source, typeof(BuffInfo.EffectFlags), match.Groups[1].Value);

            match = Regex.Match(str, @"ignoreTrigger\s*=\s*""([^""]*)""");
            if (match.Success) s.ignoreTrigger = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"keyEffect\s*=\s*""([^""]*)""");
            if (match.Success) s.keyEffect = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"waitKeyEffect\s*=\s*""([^""]*)""");
            if (match.Success) s.waitKeyEffect = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"interval\s*=\s*""([^""]*)""");
            if (match.Success) s.interval = new BuffInfo.DefaultGrowInt(Util.Parse<int>(match.Groups[1].Value));
            n = "intervals";
            match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
            if (match.Success)
                s.intervalGrow = (BuffInfo.Grow[])FromXml(source, typeof(BuffInfo.Grow[]), !string.IsNullOrEmpty(match.Groups[1].Value) ?
                                                                                                            match.Groups[1].Value :
                                                                                                            match.Groups[2].Value, match.Groups[3].Value, n);
            else s.intervalGrow = (BuffInfo.Grow[])FromXml(source, typeof(BuffInfo.Grow[]), "");
            match = Regex.Match(str, @"applyCount\s*=\s*""([^""]*)""");
            if (match.Success) s.applyCount = new BuffInfo.DefaultGrowInt(Util.Parse<int>(match.Groups[1].Value));
            n = "applyCounts";
            match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
            if (match.Success)
                s.applyCountGrow = (BuffInfo.Grow[])FromXml(source, typeof(BuffInfo.Grow[]), !string.IsNullOrEmpty(match.Groups[1].Value) ?
                                                                                                            match.Groups[1].Value :
                                                                                                            match.Groups[2].Value, match.Groups[3].Value, n);
            else s.applyCountGrow = (BuffInfo.Grow[])FromXml(source, typeof(BuffInfo.Grow[]), "");

            n = "paramTrigger";
            match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
            if (match.Success)
                s.paramTrigger = (BuffInfo.EffectParamTrigger)FromXml(source, typeof(BuffInfo.EffectParamTrigger), !string.IsNullOrEmpty(match.Groups[1].Value) ?
                                                                                                            match.Groups[1].Value :
                                                                                                            match.Groups[2].Value, match.Groups[3].Value, n);
            else s.paramTrigger = (BuffInfo.EffectParamTrigger)FromXml(source, typeof(BuffInfo.EffectParamTrigger), "");

            n = "expression";
            match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
            if (match.Success) s.expression = (BuffInfo.ExpressionInfo)FromXml(source, typeof(BuffInfo.ExpressionInfo), !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n);
            else s.expression = (BuffInfo.ExpressionInfo)FromXml(source, typeof(BuffInfo.ExpressionInfo), "");

            var remap = Buff.nameRemap[(int)s.type];
            for (var i = 0; i < remap.Length; ++i)
            {
                match = Regex.Match(str, remap[i] + @"\s*=\s*""([^""]*)""");
                if (match.Success)
                {
                    var matched = match.Groups[1].Value;
                    s.sparamss[i] = matched;

                    var param = Util.Parse<double>(matched);
                    if (i == 1 && param == 0 && s.type == BuffInfo.EffectTypes.Dispel)
                    {
                        var et = typeof(BuffInfo.EffectTypes);
                        if (Enum.IsDefined(et, match.Groups[1].Value))
                        {
                            var buffType = (BuffInfo.EffectTypes)Enum.Parse(et, match.Groups[1].Value, true);
                            param = (int)buffType;
                        }
                    }
                    s.paramss[i] = param;
                }
            }

            return s;
        }

        if (type == typeof (BuffInfo.DefaultGrowInt))
        {
            return Activator.CreateInstance(type, new object[] {FromXml(source, typeof(int), str)});
        }
        if (type == typeof(BuffInfo.DefaultGrowDouble))
        {
            return Activator.CreateInstance(type, new object[] { FromXml(source, typeof(double), str) });
        }

        if (type == typeof(BuffInfo.BuffTrigger))
        {
            var s = new BuffInfo.BuffTrigger()
            {
                paramss = new double[BuffInfo.MAX_TRIGGER_PARAM_COUNT],
                sparamss = new string[BuffInfo.MAX_TRIGGER_PARAM_COUNT],
            };

            var nameRemap = new string[][]
            {
                new string[] { },                                                        // Normal
                new string[] { },                                                        // StartFight
                new string[] { },                                                        // Dead
                new string[] { },                                                        // TakeDamage
                new string[] { "lockTrigger", "actionMask"},                            // Attacked
                new string[] { },                                                        // Shooted
                new string[] { },                                                        // CritHurted
                new string[] { "value", "isValue" },                                     // Health
                new string[] { "value", "isValue" },                                     // TargetHealth
                new string[] { "value", "isValue" },                                     // Rage
                new string[] { "value", "isValue" },                                     // TargetRage
                new string[] { "attributeID", "value" },                                 // Field
                new string[] { "attributeID", "value" },                                 // TargetField
                new string[] { "step1", "step2", "step3", "step4", "step5", "isValue" }, // TotalDamage
                new string[] { "value", "isPercent", "notRest", "damping" },             // ElementDamage
                new string[] { },                                                        // FirstHit
                new string[] { },                                                        // FollowTargetEnd
                new string[] { "lockTrigger"},                                           // Kill
                new string[] { "value" },                                                // Combo
                new string[] { "value" },                                                // ComboBreak
                new string[] { "propertyType", "value", "isValue"},                              // PercentDamage
                new string[] { "actionMask" },                                           // UseSkill
                new string[] { },                                                        // DealDamage
                new string[] { "actionMask","mark" },                                      // Attack
                new string[] {  },                                                       // Crit
                new string[] { "propertyType","value","isValue"},                        // DamageOverFlow
                new string[] { "group"},                                                 // EnterState
                new string[] { "group"},                                                 // TargetGroup
                new string[] { },                                                        // Count, unused
            };

            var match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) s.type = (BuffInfo.BuffTriggerTypes)FromXml(source, typeof(BuffInfo.BuffTriggerTypes), match.Groups[1].Value);

            match = Regex.Match(str, @"chance\s*=\s*""([^""]*)""");
            if (match.Success) s.chance = Util.Parse<float>(match.Groups[1].Value);
            else s.chance = 1;

            match = Regex.Match(str, @"triggerCount\s*=\s*""([^""]*)""");
            if (match.Success) s.triggerCount = Util.Parse<int>(match.Groups[1].Value);
            else s.triggerCount = 1;

            match = Regex.Match(str, @"triggerMax\s*=\s*""([^""]*)""");
            if (match.Success) s.triggerMax = Util.Parse<int>(match.Groups[1].Value);
            else s.triggerMax = 0;

            match = Regex.Match(str, @"followTriggerCount\s*=\s*""([^""]*)""");
            if (match.Success) s.followTriggerCount = Util.Parse<int>(match.Groups[1].Value);
            else s.followTriggerCount = 1;

            match = Regex.Match(str, @"listenSource\s*=\s*""([^""]*)""");
            if (match.Success) s.listenSource = Util.Parse<bool>(match.Groups[1].Value);
            else s.listenSource = false;

            var e = "expression";
            match = Regex.Match(sub, @"(<" + e + @"[^\/<]+\/>)|(?:(<" + e + @"[^\/<>]*>)((?:(?!<\/" + e + @">).|\s)+)<\/" + e + @">)");
            if (match.Success)
            {
                s.useExpression = true;
                s.expression = (BuffInfo.ExpressionInfo)
                            FromXml(source, typeof (BuffInfo.ExpressionInfo),
                            !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value,
                            match.Groups[3].Value, e);
            }
            else
            {
                s.useExpression = false;
                s.expression = (BuffInfo.ExpressionInfo)FromXml(source, typeof(BuffInfo.ExpressionInfo), "");
            }

            var remap = nameRemap[(int)s.type];
            for (var i = 0; i < remap.Length; ++i)
            {
                match = Regex.Match(str, remap[i] + @"\s*=\s*""([^""]*)""");
                var matched = match.Groups[1].Value;
                if (match.Success)
                {
                    s.sparamss[i] = matched;
                    s.paramss[i] = Util.Parse<double>(matched);
                }
            }

            return s;
        }

        if (type == typeof (BuffInfo.ParamsGrow))
        {
            var list = FromXml(source, typeof (List<BuffInfo.Grow>), str, sub, node) as List<BuffInfo.Grow>;
            return new BuffInfo.ParamsGrow() { Datas = new List<BuffInfo.Grow>(list)};
        }

        if (type == typeof(ComboInputInfo.SingleSpell))
        {
            var s = new ComboInputInfo.SingleSpell();

            var match = Regex.Match(str, @"spellName\s*=\s*""([^""]*)""");
            if (match.Success) s.spellName = match.Groups[1].Value;

            match = Regex.Match(str, @"rage\s*=\s*""([^""]*)""");
            if (match.Success) s.rage = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"inputs\s*=\s*""([^""]*)""");
            if (match.Success) s.inputs = Util.ParseString<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"textColor\s*=\s*""([^""]*)""");
            if (match.Success) s.textColor = Util.BuildColor(match.Groups[1].Value);

            match = Regex.Match(str, @"backColor\s*=\s*""([^""]*)""");
            if (match.Success) s.backColor = Util.BuildColor(match.Groups[1].Value);

            return s;
        }

        if (type == typeof(ComboInputInfo.SpellGroup))
        {
            var t = new ComboInputInfo.SpellGroup();

            if (string.IsNullOrEmpty(str)) return t;

            var fields = t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(t, FromXml(source, field.FieldType, ""));
                }
            }

            return t;
        }

        if (type == typeof(CameraShotInfo.ShotState))
        {
            var t = new CameraShotInfo.ShotState(0);

            var match = Regex.Match(str, @"time\s*=\s*""([^""]*)""");
            if (match.Success) t.time = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"offset\s*=\s*""([^""]*)""");
            if (match.Success) t.offset = (Vector3)FromXml(source, typeof(Vector3), match.Groups[1].Value);

            match = Regex.Match(str, @"euler\s*=\s*""([^""]*)""");
            if (match.Success) t.euler = (Vector3)FromXml(source, typeof(Vector3), match.Groups[1].Value);

            match = Regex.Match(str, @"fieldOfView\s*=\s*""([^""]*)""");
            if (match.Success) t.fieldOfView = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"overrideSmooth\s*=\s*""([^""]*)""");
            if (match.Success) t.overrideSmooth = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"forceCut\s*=\s*""([^""]*)""");
            if (match.Success) t.forceCut = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"hideScene\s*=\s*""([^""]*)""");
            if (match.Success) t.hideScene = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"removeCameraEdge\s*=\s*""([^""]*)""");
            if (match.Success) t.removeCameraEdge = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"hideCombatUI\s*=\s*""([^""]*)""");
            if (match.Success) t.hideCombatUI = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"maskDuration\s*=\s*""([^""]*)""");
            if (match.Success) t.maskDuration = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"maskAsset\s*=\s*""([^""]*)""");
            if (match.Success) t.maskAsset = match.Groups[1].Value;

            t.blend = (CameraBlend)FromXml(source, typeof(CameraBlend), sub, "", "blend");

            return t;
        }

        if (type == typeof(StateMachineInfo.Effect))
        {
            var t = new StateMachineInfo.Effect() as object;

            var fields = t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(t, FromXml(source, field.FieldType, ""));
                }
            }

            return t;
        }

        if (type == typeof(StateMachineInfo.FlyingEffect))
        {
            var t = StateMachineInfo.FlyingEffect.empty as object;

            if (string.IsNullOrEmpty(str)) return t;

            var fields = t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(t, FromXml(source, field.FieldType, ""));
                }
            }

            return t;
        }

        if (type == typeof(StateMachineInfo.SingleSound))
        {
            var c = StateMachineInfo.SingleSound.empty;

            var match = Regex.Match(str, @"isVoice\s*=\s*""([^""]*)""");
            if (match.Success) c.isVoice = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"sound\s*=\s*""([^""]*)""");
            if (match.Success) c.sound = match.Groups[1].Value;

            match = Regex.Match(str, @"weight\s*=\s*""([^""]*)""");
            if (match.Success) c.weight = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"proto\s*=\s*""([^""]*)""");
            if (match.Success) c.proto = Util.Parse<int>(match.Groups[1].Value);

            return c;
        }

        if (type == typeof(StateMachineInfo.SoundEffect))
        {
            var t = new StateMachineInfo.SoundEffect();

            if (string.IsNullOrEmpty(str)) return t;

            var fields = t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(t, FromXml(source, field.FieldType, ""));
                }
            }

            return t;
        }

        if (type == typeof(WeaponAttribute.WeaponLevel))
        {
            var s = new WeaponAttribute.WeaponLevel();
            //由于weapon是自定义结构体，所以需要手动拆装箱一次，避免setvalue无效
        
            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(WeaponAttribute.LevelInfo))
        {
            var s = new WeaponAttribute.LevelInfo();
            var match = Regex.Match(str, @"id\s*=\s*""([^""]*)""");
            if (match.Success) s.id = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) s.type = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"value\s*=\s*""([^""]*)""");
            if (match.Success) s.value = Util.Parse<float>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(NpcInfo.NpcFetterTask))
        {
            var s = new NpcInfo.NpcFetterTask();
            var match = Regex.Match(str, @"fetterLv\s*=\s*""([^""]*)""");
            if (match.Success) s.fetterLv = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"taskId\s*=\s*""([^""]*)""");
            if (match.Success) s.taskId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"eventId\s*=\s*""([^""]*)""");
            if (match.Success) s.eventId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"hintStoryID\s*=\s*""([^""]*)""");
            if (match.Success) s.hintStoryID = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(CooperationTask.KillMonster))
        {
            var s = new CooperationTask.KillMonster();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(EquipAnimationInfo.AnimationData))
        {
            var t = new EquipAnimationInfo.AnimationData();

            if (string.IsNullOrEmpty(str)) return t;

            var fields = t.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(t, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(t, FromXml(source, field.FieldType, ""));
                }
            }

            return t;
        }

        if (type == typeof(EquipAnimationInfo.GotoData))
        {
            var t = new EquipAnimationInfo.GotoData();

            if (string.IsNullOrEmpty(str)) return t;

            var match = Regex.Match(str, @"gotoIds\s*=\s*""([^""]*)""");
            if (match.Success) t.gotoIds =Util.ParseString<ushort>(match.Groups[1].Value);

            match = Regex.Match(str, @"state\s*=\s*""([^""]*)""");
            if (match.Success) t.state = match.Groups[1].Value;

            match = Regex.Match(str, @"cameraEndPos\s*=\s*""([^""]*)""");
            if (match.Success) t.cameraEndPos = (Vector3)FromXml(source, typeof(Vector3), match.Groups[1].Value);

            match = Regex.Match(str, @"playerEndRotate\s*=\s*""([^""]*)""");
            if (match.Success) t.playerEndRotate = (Vector3)FromXml(source, typeof(Vector3), match.Groups[1].Value);

            match = Regex.Match(str, @"cameraDelay\s*=\s*""([^""]*)""");
            if (match.Success) t.cameraDelay = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"playerRotDelay\s*=\s*""([^""]*)""");
            if (match.Success) t.playerRotDelay = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"cameraUnitTime\s*=\s*""([^""]*)""");
            if (match.Success) t.cameraUnitTime = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"playerRotUnitTime\s*=\s*""([^""]*)""");
            if (match.Success) t.playerRotUnitTime = Util.Parse<float>(match.Groups[1].Value);

            return t;
        }

        if (type == typeof(StateMachineInfo.StateDetail))
        {
            var s = new StateMachineInfo.StateDetail();
            s.sections = string.IsNullOrEmpty(sub) ? new StateMachineInfo.Section[] { } : (StateMachineInfo.Section[])FromXml(source, typeof(StateMachineInfo.Section[]), "<", sub, "sections");

            var ofs = node == "overrides" ? new List<string>() : null;
            var igs = ofs == null ? null : StateOverrideInfo.StateOverride.ignoreFields;
            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.Name.StartsWith("__") || field.Name[0] == '<' || igs != null && igs.Contains(field.Name, true)) continue;

                var matched = false;

                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success)
                {
                    field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                    matched = true;
                }
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success)
                    {
                        field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                        matched = true;
                    }
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }

                if (matched && ofs != null) ofs.Add(field.Name);
            }
            if (ofs != null) s.__overrideParams = ofs.ToArray();
            else
            {
                s.__overrideParams = new string[] { };
                s.level = -1;
                s.damageMul = 1.0;
            }

            return s;
        }

        if (type == typeof(AttackInfo.PassiveTransition))
        {
            var t = new AttackInfo.PassiveTransition();

            var match = Regex.Match(str, @"fromGroup\s*=\s*""([^""]*)""");
            if (match.Success) t.fromGroup = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"toState\s*=\s*""([^""]*)""");
            if (match.Success) t.toState = match.Groups[1].Value;

            match = Regex.Match(str, @"toToughState\s*=\s*""([^""]*)""");
            if (match.Success) t.toToughState = match.Groups[1].Value;

            return t;
        }

        if (type == typeof(CreatureStateInfo.StateInfo))
        {
            var s = new CreatureStateInfo.StateInfo();

            var match = Regex.Match(str, @"ID\s*=\s*""([^""]*)""");
            if (match.Success) s.ID = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"name\s*=\s*""([^""]*)""");
            if (match.Success) s.name = match.Groups[1].Value;

            return s;
        }

        if (type == typeof(SceneEventInfo.SceneEvent))
        {
            var s = new SceneEventInfo.SceneEvent();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(SceneEventInfo.SceneBehaviour))
        {
            var s = new SceneEventInfo.SceneBehaviour();

            Match match = Regex.Match(str, @"sceneBehaviorType\s*=\s*""([^""]*)""");
            if (match.Success) s.sceneBehaviorType = Util.ParseEnum<SceneEventInfo.SceneBehaviouType>(match.Groups[1].Value);

            string strParamName = string.Empty;
            string vecParamName = string.Empty;

            string[][] names = new string[][]
            {
                new string[] {},                                                                //NONE
                new string[] { "MonsterID", "Group", "Level", "IsBoss","CameraAnim" ,"FrameEventID","ForceDirection" ,"GetReward"},       //CreateMonster
                new string[3] { "MonsterID", "Group", "Amout" },                                //KillMonster
                new string[3] { "TimerID", "TimeAmout", "IsShow" },                             //StartCountDown    
                new string[3] { "TimerID", "TimeAmout", "AbsoluteValue" },                      //AddTimerValue
                new string[1] { "TimerID" },                                                    //DelTimer
                new string[] {},                                                                //StageClear
                new string[] {},                                                                //StageFail
                new string[3] { "ObjectID", "BuffID", "Duraction" },                            //AddBuffer
                new string[2] { "PlotID","PlotType" },                                          //StartStoryDialogue
                new string[2] { "Time", "TexId" },                                              //ShowMessage
                new string[2] { "CounterId", "NumberChange" },                                  //OperatingCounter
                new string[3] { "MonsterID", "Group","LeaveTime" },                             //LeaveMonster
                new string[3] { "MonsterID", "Group","SetType" },                               //SetState
                new string[] {},                                                                //CheckStageFirstTime
                new string[3] { "MonsterID", "Group","Pause" },                                 //AIPauseState
                new string[] {},                                                                //BackToHome
                new string[] {"GuideID"},                                                       //StartGuide
                new string[] {"AudioType","Loop"},                                              //PlayAudio
                new string[] {},                                                                //StopAudio
                new string[] { "BossTimerID", "TimeAmout", "BossId1", "BossId2"},               //BossComing    
                new string[] { "LevelId", "Position", "EventId", "DelayTime"},                  //TransportScene
                new string[] { "TriggerID", "State" , "Flag", "Random"},                        //CreateTrigger
                new string[] { "TriggerID", "State"},                                           //OperateTrigger
                new string[] { "Direction", "AdditiveValue", "AbsoluteValue"},                  //OperateSceneArea
                new string[] { "SceneActorID", "LogicID", "ReletivePos", "Level", "ForceDirection","Group"},              //CreateSceneActor
                new string[] { "LogicID"},                                                      //OperateSceneActor
                new string[] { "SceneActorID", "LogicID", "StateType"},                         //DelSceneActorEvent
                new string[] { "MonsterID", "Group", "Level"},                                  //CreateLittle
                new string[] { "RandomID", "MaxValue"},                                         //BuildRandom
                new string[] { "MonsterID", "Group"},                                           //MoveMonsterPos
                new string[] { "LogicID"},                                                      //CreateAssistant
                new string[] { "LogicID"},                                                      //CreateAssistantNpc
                new string[] { "ConditionID" },                                                 //DeleteCondition
                new string[] { "ConditionID", "IncreaseAmount" },                               //IncreaseConditionAmount
                new string[] { "RandomID", "MaxValue"},                                         //RebuildRandom
                new string[] { "ObstructID", "State", "ObstructMask" },                         //CreateObstruct
                new string[] { "ObstructID", "State" },                                         //OperateObstruct  
            };

            switch (s.sceneBehaviorType)
            {

                case SceneEventInfo.SceneBehaviouType.PlayAudio:
                case SceneEventInfo.SceneBehaviouType.StopAudio:
                    strParamName = "AudioName";
                    break;
                case SceneEventInfo.SceneBehaviouType.LeaveMonster:
                case SceneEventInfo.SceneBehaviouType.SetState:
                case SceneEventInfo.SceneBehaviouType.TransportScene:
                case SceneEventInfo.SceneBehaviouType.CreateSceneActor:
                case SceneEventInfo.SceneBehaviouType.OperateSceneActor:
                case SceneEventInfo.SceneBehaviouType.DelSceneActorEvent:
                    strParamName = "State";
                    break;
                case SceneEventInfo.SceneBehaviouType.CreateTrigger:
                    strParamName = "Effect";
                    break;
                case SceneEventInfo.SceneBehaviouType.CreateAssistant:
                    strParamName = "BornState";
                    break;
            }

            switch (s.sceneBehaviorType)
            {
                case SceneEventInfo.SceneBehaviouType.CreateMonster:
                case SceneEventInfo.SceneBehaviouType.MoveMonsterPos:
                case SceneEventInfo.SceneBehaviouType.CreateAssistant:
                    vecParamName = "ReletivePos";
                    break;
                case SceneEventInfo.SceneBehaviouType.CreateTrigger:
                    vecParamName = "Range";
                    break;
                case SceneEventInfo.SceneBehaviouType.CreateLittle:
                    vecParamName = "Offset";
                    break;
            }

            string[] paramNames = names[(int)s.sceneBehaviorType];
            if (paramNames != null && paramNames.Length > 0)
            {
                s.parameters = new int[paramNames.Length];
                for (int i = 0; i < paramNames.Length; i++)
                {
                    match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", paramNames[i]));
                    if (match.Success)
                        s.parameters[i] = Util.Parse<int>(match.Groups[1].Value);
                }
            }

            if (!string.IsNullOrEmpty(strParamName))
            {
                match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", strParamName));
                if (match.Success)
                    s.strParam = match.Groups[1].Value;
            }

            if (!string.IsNullOrEmpty(vecParamName))
            {
                match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", vecParamName));
                if (match.Success)
                    s.vecParam = (Vector4)FromXml(source, typeof(Vector4), match.Groups[1].Value);
            }
            return s;
        }

        if (type == typeof(SceneEventInfo.SceneCondition))
        {
            var s = new SceneEventInfo.SceneCondition();

            Match match = Regex.Match(str, @"sceneEventType\s*=\s*""([^""]*)""");
            if (match.Success) s.sceneEventType = Util.ParseEnum<SceneEventInfo.SceneConditionType>(match.Groups[1].Value);

            string[][] names = new string[][]
            {
                new string[] {},                                                            //NONE
                new string[] {},                                                            //EnterScene
                new string[1] { "TimerID" },                                                //CountDownEnd
                new string[3] { "MonsterID", "Group", "Amout" },                            //MonsterDeath
                new string[] {"notFirst"},                                                  //StageFirstTime
                new string[1] { "PlotID" },                                                 //StoryDialogueEnd    
                new string[2] { "CounterID", "Number" },                                    //CounterNumber
                new string[2] { "MonsterID", "Group" },                                     //MonsterLeaveEnd
                new string[3] { "MonsterID", "Group", "LessThan" },                         //MonsterHPLess
                new string[1] { "GuideID" },                                                //GuideEnd 
                new string[1] { "BossTimerID"},                                             //BossComingEnd
                new string[1] { "Vocation"},                                                //PlayerVocation
                new string[4] { "TriggerID", "TriggerType", "PlayerNum", "MonsterID"},      //EnterTrigger
                new string[2] { "LogicID", "StateType"},                                    //SceneActorState
                new string[] {},                                                            //WindowCombatVisible
                new string[] {"notFirst"},                                                  //EnterForFirstTime
                new string[] { "RandomID", "MinValue", "MaxValue", "Value"},                //RandomInfo
                new string[] { "MonsterID", "Group"},                                       //MonsterAttack
                new string[1] {"LessThan"},                                                 //PlayerHPLess
                new string[2] {"Times", "LogicID"},                                         //HitTimes
            };

            string strParamName = string.Empty;
            switch (s.sceneEventType)
            {
                case SceneEventInfo.SceneConditionType.SceneActorState:
                    strParamName = "State";
                    break;
            }

            string[] paramNames = names[(int)s.sceneEventType];
            if (paramNames != null && paramNames.Length > 0)
            {
                s.parameters = new int[paramNames.Length];
                //开始输出不同的字段
                for (int i = 0; i < paramNames.Length; i++)
                {
                    match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", paramNames[i]));
                    if (match.Success)
                    {
                        s.parameters[i] = Util.Parse<int>(match.Groups[1].Value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(strParamName))
            {
                match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", strParamName));
                if (match.Success)
                    s.strParam = match.Groups[1].Value;
            }

            //set condition id
            match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", "ConditionID"));
            if (match.Success)
                s.conditionId = Util.Parse<int>(match.Groups[1].Value);
            else
                s.conditionId = 0;
            return s;
        }

        if (type == typeof(SceneFrameEventInfo.SceneFrameEventItem))
        {
            var s = new SceneFrameEventInfo.SceneFrameEventItem();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(MonsterAttriubuteInfo.MonsterAttriubute))
        {
            object attr = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(attr, FromXml(source, field.FieldType, ""));
                }
            }
            return attr;
        }

        if (type == typeof(AIConfig.LockEnermyInfo))
        {
            var s = new AIConfig.LockEnermyInfo();

            var match = Regex.Match(str, @"distance\s*=\s*""([^""]*)""");
            if (match.Success) s.distance = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"lockRate\s*=\s*""([^""]*)""");
            if (match.Success) s.lockRate = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }
        
        if (type == typeof(AIConfig.AIStratergy))
        {
            object attr = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(attr, FromXml(source, field.FieldType, ""));
                }
            }
            return attr;
        }

        if (type == typeof(AIConfig.SingleAIStratergy))
        {
            object attr = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    if (field.Name.Equals("repeatTimes"))
                    {
                        field.SetValue(attr, -1);
                        continue;
                    }
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(attr, FromXml(source, field.FieldType, ""));
                }
            }
            return attr;
        }

        if (type == typeof(AIConfig.AICondition))
        {
            var s = new AIConfig.AICondition();

            string[][] paraNames = new string[][]
            {
                new string[]{"life"},                   //HpHigh
                new string[]{"life"},                   //HpLow
                new string[]{""},                       //CheckAttackState
                new string[]{"buffId"},                 //HasBuff
                new string[]{"direction"},              //MonDir
                new string[]{"moveState"},              //MoveState
            	new string[]{"left"},                 //MonOnThePlayerLeft
            };

            Match match = Regex.Match(str, @"conditionType\s*=\s*""([^""]*)""");
            if (match.Success) s.conditionType = Util.ParseEnum<AIConfig.AICondition.AIConditionType>(match.Groups[1].Value);
            
            string[] names = paraNames[(int)s.conditionType];
            if (names != null)
            {
                s.paramers = new int[names.Length];
                //开始输出不同的字段
                for (int i = 0; i < names.Length; i++)
                {
                    match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", names[i]));
                    if (match.Success)
                    {
                        s.paramers[i] = Util.Parse<int>(match.Groups[1].Value);
                    }
                }
            }
            return s;
        }

        if (type == typeof(AIConfig.AIBehaviour))
        {
            var s = new AIConfig.AIBehaviour();

            var match = Regex.Match(str, @"behaviourType\s*=\s*""([^""]*)""");
            if (match.Success) s.behaviourType = Util.ParseEnum<AIConfig.AIBehaviour.AIBehaviourType>(match.Groups[1].Value);
            
            match = Regex.Match(str, @"state\s*=\s*""([^""]*)""");
            if (match.Success) s.state = match.Groups[1].Value;
            match = Regex.Match(str, @"loopDuraction\s*=\s*""([^""]*)""");
            if (match.Success)
            {
                string[] array = match.Groups[1].Value.Split(',');
                s.loopDuraction = new int[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    s.loopDuraction[i] = Util.Parse<int>(array[i]);
                }
            }
            else
                s.loopDuraction = new int[2] { 0, 0 };

            match = Regex.Match(str, @"group\s*=\s*""([^""]*)""");
            if (match.Success) s.group = Util.Parse<int>(match.Groups[1].Value);

            return s;
        }

        if (type == typeof(NpcActionInfo.NpcPosition))
        {
            var s = new NpcActionInfo.NpcPosition();
            object ss = s;

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(ss, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(ss, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(ss, FromXml(source, field.FieldType, ""));
                }
            }
            return (NpcActionInfo.NpcPosition)ss;
        }

        if (type == typeof(BossEffectInfo.money))
        {
            var s = new BossEffectInfo.money();

            var match = Regex.Match(str, @"itemId\s*=\s*""([^""]*)""");
            if (match.Success) s.itemId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"count\s*=\s*""([^""]*)""");
            if (match.Success) s.count = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(NpcActionInfo.AnimAndVoice))
        {
            var s = new NpcActionInfo.AnimAndVoice();

            var match = Regex.Match(str, @"npcLvType\s*=\s*""([^""]*)""");
            if (match.Success) s.npcLvType = Util.ParseEnum<NpcLevelType>(match.Groups[1].Value);

            match = Regex.Match(str, @"state\s*=\s*""([^""]*)""");
            if (match.Success) s.state = match.Groups[1].Value;

            match = Regex.Match(str, @"stateMonologue\s*=\s*""([^""]*)""");
            if (match.Success) s.stateMonologue = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(GeneralConfigInfo.StoryData))
        {
            var s = new GeneralConfigInfo.StoryData();

            var match = Regex.Match(str, @"contextInterval\s*=\s*""([^""]*)""");
            if (match.Success) s.contextInterval = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"npcLeaveTime\s*=\s*""([^""]*)""");
            if (match.Success) s.npcLeaveTime = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"genderTextId\s*=\s*""([^""]*)""");
            if (match.Success) s.genderTextId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"storySpeed\s*=\s*""([^""]*)""");
            if (match.Success) s.storySpeed = Util.Parse<float>(match.Groups[1].Value);
            match = Regex.Match(str, @"storyPreLoadNum\s*=\s*""([^""]*)""");
            if (match.Success) s.storyPreLoadNum = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(GeneralConfigInfo.InterludeTime))
        {
            var s = new GeneralConfigInfo.InterludeTime();

            var match = Regex.Match(str, @"fadeInTime\s*=\s*""([^""]*)""");
            if (match.Success) s.fadeInTime = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"remainInTime\s*=\s*""([^""]*)""");
            if (match.Success) s.remainInTime = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"remainOutTime\s*=\s*""([^""]*)""");
            if (match.Success) s.remainOutTime = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"fadeOutTime\s*=\s*""([^""]*)""");
            if (match.Success) s.fadeOutTime = Util.Parse<float>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(ShowCreatureInfo.CreatureOrNpcData))
        {
            var s = new ShowCreatureInfo.CreatureOrNpcData();
            //由于weapon是自定义结构体，所以需要手动拆装箱一次，避免setvalue无效
            object ss = s;

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(ss, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(ss, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(ss, FromXml(source, field.FieldType, ""));
                }
            }
            return (ShowCreatureInfo.CreatureOrNpcData)ss;
        }

        if (type == typeof(ShowCreatureInfo.SizeAndPos))
        {
            var s = new ShowCreatureInfo.SizeAndPos();

            var match = Regex.Match(str, @"size\s*=\s*""([^""]*)""");
            if (match.Success) s.size = Util.Parse<float>(match.Groups[1].Value);

            match = Regex.Match(str, @"fov\s*=\s*""([^""]*)""");
            if (match.Success) s.fov = Util.Parse<float>(match.Groups[1].Value); 

            match = Regex.Match(str, @"pos\s*=\s*""([^""]*)""");
            if (match.Success) s.pos = (Vector3_)FromXml(source, s.pos.GetType(), match.Groups[1].Value);

            match = Regex.Match(str, @"rotation\s*=\s*""([^""]*)""");
            if (match.Success) s.rotation = (Vector3)FromXml(source, s.rotation.GetType(), match.Groups[1].Value);

            return s;
        }

        #region 剧情相关

        if (type == typeof(StoryInfo))
        {
            var s = new StoryInfo();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
           
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            s.___source = source;
            return s;
        }

        if (type == typeof(StoryInfo.StoryItem))
        {
            var s = new StoryInfo.StoryItem();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(StoryInfo.TalkingRoleData))
        {
            var s = new StoryInfo.TalkingRoleData();

            var match = Regex.Match(str, @"roleId\s*=\s*""([^""]*)""");
            if (match.Success) s.roleId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"rolePos\s*=\s*""([^""]*)""");
            if (match.Success) s.rolePos = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"highLight\s*=\s*""([^""]*)""");
            if (match.Success) s.highLight = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(StoryInfo.TalkingRoleState))
        {
            var s = new StoryInfo.TalkingRoleState();

            var match = Regex.Match(str, @"roleId\s*=\s*""([^""]*)""");
            if (match.Success) s.roleId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"state\s*=\s*""([^""]*)""");
            if (match.Success) s.state = match.Groups[1].Value;
            return s;
        }

        if (type == typeof(StoryInfo.BlackScreenData))
        {
            var s = new StoryInfo.BlackScreenData();

            var match = Regex.Match(str, @"isBlackScreen\s*=\s*""([^""]*)""");
            if (match.Success) s.isBlackScreen = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"immediately\s*=\s*""([^""]*)""");
            if (match.Success) s.imme = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(StoryInfo.CameraShakeData))
        {
            var s = new StoryInfo.CameraShakeData();

            var match = Regex.Match(str, @"delayTime\s*=\s*""([^""]*)""");
            if (match.Success) s.delayTime = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"shakeId\s*=\s*""([^""]*)""");
            if (match.Success) s.shakeId = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(StoryInfo.StorySoundEffect))
        {
            var s = new StoryInfo.StorySoundEffect();

            var match = Regex.Match(str, @"delayTime\s*=\s*""([^""]*)""");
            if (match.Success) s.delayTime = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"soundName\s*=\s*""([^""]*)""");
            if (match.Success) s.soundName = match.Groups[1].Value;
            return s;
        }

        if (type == typeof(StoryInfo.GivePropData))
        {
            var s = new StoryInfo.GivePropData();

            var match = Regex.Match(str, @"itemTypeId\s*=\s*""([^""]*)""");
            if (match.Success) s.itemTypeId = Util.Parse<ushort>(match.Groups[1].Value);

            match = Regex.Match(str, @"level\s*=\s*""([^""]*)""");
            if (match.Success) s.level = Util.Parse<byte>(match.Groups[1].Value);

            match = Regex.Match(str, @"star\s*=\s*""([^""]*)""");
            if (match.Success) s.star = Util.Parse<byte>(match.Groups[1].Value);

            match = Regex.Match(str, @"num\s*=\s*""([^""]*)""");
            if (match.Success) s.num = Util.Parse<uint>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(StoryInfo.ModelData))
        {
            var s = new StoryInfo.ModelData();

            var match = Regex.Match(str, @"model\s*=\s*""([^""]*)""");
            if (match.Success) s.model = match.Groups[1].Value;

            match = Regex.Match(str, @"positionIndex\s*=\s*""([^""]*)""");
            if (match.Success) s.positionIndex = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(StoryInfo.MusicData))
        {
            var s = new StoryInfo.MusicData();

            var match = Regex.Match(str, @"musicName\s*=\s*""([^""]*)""");
            if (match.Success) s.musicName = match.Groups[1].Value;

            match = Regex.Match(str, @"loop\s*=\s*""([^""]*)""");
            if (match.Success) s.loop = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }
        #endregion

        #region guide info

        if (type == typeof(GuideInfo.GuideItem))
        {
            var s = new GuideInfo.GuideItem();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(GuideInfo.GuideSuccessCondition))
        {
            var s = new GuideInfo.GuideSuccessCondition();

            var match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) s.type = Util.ParseEnum<EnumGuideCondition>(match.Groups[1].Value);

            match = Regex.Match(str, @"tipPos\s*=\s*""([^""]*)""");
            if (match.Success) s.tipPos = (Vector3)FromXml(source, typeof(Vector3), match.Groups[1].Value);

            switch (s.type)
            {
                case EnumGuideCondition.InputKey:
                    {
                        match = Regex.Match(str, @"InputKey\s*=\s*""([^""]*)""");
                        if (match.Success)
                        {
                            var vals = Util.ParseString<string>(match.Groups[1].Value, false, ';');
                            List<int> l = new List<int>();
                            for (int i = 0; i < vals.Length; i++)
                            {
                                if (string.IsNullOrEmpty(vals[i])) continue;
                                l.Add(Util.Parse<int>(vals[i]));
                            }
                            s.intParams = l.ToArray();
                        }
                        break;
                    }
                case EnumGuideCondition.Position:
                    {
                        match = Regex.Match(str, @"Position\s*=\s*""([^""]*)""");
                        if (match.Success)
                        {
                            var vals = Util.ParseString<string>(match.Groups[1].Value, false, ';');
                            List<float> l = new List<float>();
                            for (int i = 0; i < vals.Length; i++)
                            {
                                if (string.IsNullOrEmpty(vals[i])) continue;
                                l.Add(Util.Parse<float>(vals[i]));
                            }
                            s.floatParams = l.ToArray();
                        }
                        break;
                    }
            }
            return s;
        }

        if (type == typeof(GuideInfo.GuideIcon))
        {
            var s = new GuideInfo.GuideIcon();

            var match = Regex.Match(str, @"icon\s*=\s*""([^""]*)""");
            if (match.Success) s.icon = match.Groups[1].Value;

            match = Regex.Match(str, @"position\s*=\s*""([^""]*)""");
            if (match.Success) s.position = (Vector3)FromXml(source, typeof(Vector3),match.Groups[1].Value);
            return s;
        }

        if (type == typeof(GuideInfo.HotAreaData))
        {
            var s = new GuideInfo.HotAreaData();

            var match = Regex.Match(str, @"hotWindow\s*=\s*""([^""]*)""");
            if (match.Success) s.hotWindow = match.Groups[1].Value;

            match = Regex.Match(str, @"hotArea\s*=\s*""([^""]*)""");
            if (match.Success) s.hotArea = (string[])FromXml(source, typeof(string[]), match.Groups[1].Value);

            match = Regex.Match(str, @"protoArea\s*=\s*""([^""]*)""");
            if (match.Success) s.protoArea = (string[])FromXml(source, typeof(string[]), match.Groups[1].Value);

            match = Regex.Match(str, @"restrainType\s*=\s*""([^""]*)""");
            if (match.Success) s.restrainType = Util.ParseEnum<EnumGuideRestrain>(match.Groups[1].Value);

            string[][] hotStr = new string[][]
            {
                new string[] {},                                    //None
                new string[] {"restrainId"},                        //CheckID
                new string[] {"runeId","level","star"},             //Rune
                new string[] {},                                    //CurrentWeapon
                new string[] {"protoIds"},                          //ProtoID
            };

            string[] names = hotStr[(int)s.restrainType];
            if (s.restrainType == EnumGuideRestrain.ProtoID)
            {
                match = Regex.Match(str, @"protoIds\s*=\s*""([^""]*)""");
                if (match.Success) s.restrainParames = (int[])FromXml(source, typeof(int[]), match.Groups[1].Value);
            }
            else
            {
                int len = names.Length;
                s.restrainParames = new int[len];
                for (int i = 0; i < len; i++)
                {
                    match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", names[i]));
                    if (match.Success) s.restrainParames[i] = Util.Parse<int>(match.Groups[1].Value);
                }
            }
            
            match = Regex.Match(str, @"restrainChild\s*=\s*""([^""]*)""");
            if (match.Success) s.restrainChild = match.Groups[1].Value;

            match = Regex.Match(str, @"effect\s*=\s*""([^""]*)""");
            if (match.Success) s.effect = match.Groups[1].Value;

            match = Regex.Match(str, @"tipHotArea\s*=\s*""([^""]*)""");
            if (match.Success) s.tipHotArea = Util.Parse<int>(match.Groups[1].Value);

            return s;
        }

        if (type == typeof (BuffInfo.EffectParamTrigger))
        {
            var t = new BuffInfo.EffectParamTrigger();
            Match match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) t.type = Util.ParseEnum<BuffInfo.ParamTriggerTypes>(match.Groups[1].Value);

            match = Regex.Match(str, @"listenSource\s*=\s*""([^""]*)""");
            if (match.Success) t.listenSource = Util.Parse<bool>(match.Groups[1].Value);

            match = Regex.Match(str, @"lockTrigger\s*=\s*""([^""]*)""");
            if (match.Success) t.param0 = Util.Parse<double>(match.Groups[1].Value);

            match = Regex.Match(str, @"value\s*=\s*""([^""]*)""");
            if (match.Success) t.param1 = Util.Parse<double>(match.Groups[1].Value);
            return t;
        }

        if (type == typeof(GuideInfo.GuideConfigCondition))
        {
            var s = new GuideInfo.GuideConfigCondition();

            Match match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) s.type = Util.ParseEnum<EnumGuideContitionType>(match.Groups[1].Value);

            string[][] intNames = new string[][]
            {
                new string[] {},                                                            //NONE
                new string[] {},                                                            //OpenWindow
                new string[1] { "StageId" },                                                //EnterStage
                new string[1] { "GuideId" },                                                //GuideEnd
                new string[1] { "StoryId"},                                                 //StoryEnd
                new string[1] { "Level" },                                                  //PlayerLevel    
                new string[] {},                                                            //RuneMaxLevel
                new string[1] { "PropId" },                                                 //GetProp
                new string[] {},                                                            //OpenLabyrinth
                new string[] {},                                                            //OpenBorderland
                new string[2] { "TaskId","Finish"},                                         //TaskFinish
                new string[2] { "TaskId", "Chanllenge"},                                    //TaskChanllenge
                new string[1] { "Chanllenge" },                                             //PVPChanllenge
                new string[] {},                                                            //SpecialTweenEnd
                new string[] {},                                                            //EnterTrain
                new string[] {},                                                            //DailyFinish
                new string[] {},                                                            //DefalutOperateGuideEnd
                new string[2] {"TaskId","Finish"},                                  //NpcDating
            };

            string[] paramNames = intNames[(int)s.type];
            if (paramNames != null && paramNames.Length > 0)
            {
                s.intParames = new int[paramNames.Length];
                //开始输出不同的字段
                for (int i = 0; i < paramNames.Length; i++)
                {
                    match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", paramNames[i]));
                    if (match.Success) s.intParames[i] = Util.Parse<int>(match.Groups[1].Value);
                }
            }

            if(s.type == EnumGuideContitionType.OpenWindow)
            {
                match = Regex.Match(str, Util.Format(@"{0}\s*=\s*""([^""]*)""", "WindowName"));
                if (match.Success) s.strParames = match.Groups[1].Value;
            }
            return s;
        }

        if (type == typeof(PetAttributeInfo.PetAttribute) ||
            type == typeof(ItemPair) ||
            type == typeof(PetUpGradeInfo.UpGradeCost) || 
            type == typeof(PetUpGradeInfo.UpGradeInfo) || 
            type == typeof(BaseAttribute) ||
            type == typeof(ColorGroup) ||
            type == typeof(ColorGroup.Entry) || 
            type == typeof(ColorGroup.ValueColorEntry) || 
            type == typeof(CombatConfig.ShowText) || 
            type == typeof(PetSkill.Skill) || 
            type == typeof(PetSkill.skillBuffParam) || 
            type == typeof(BuffInfo.Grow) ||
            type == typeof(StateMachineInfo.FollowTargetEffect) ||
            type == typeof(SlantingThrowData) ||
            type == typeof(StateLevel) ||
            type == typeof(SuitInfo) ||
            type == typeof(SuitInfo.SkillEffect) ||
            type == typeof(SoulInfo)||
            type == typeof(GeneralConfigInfo.LineParam) ||
            type == typeof(SoulCost)||
            type == typeof(TaskConfig)||
            type == typeof(TaskConfig.TaskFinishCondition))
        {
            object attr = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(attr, FromXml(source, field.FieldType, ""));
                }
            }
            return attr;
        }

        if (type == typeof (AwakeInfo))
        {
            object attr = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.Name == "states")
                {
                    var reg = new Regex("statesString" + "\\s*=\\s*\"([^\"]*)\"");
                    var matchResult = reg.Match(str);
                    if (matchResult.Success)
                    {
                        var stateStr = matchResult.Groups[1].Value;
                        var subStates = stateStr.Split(';');
                        if (subStates == null || subStates.Length == 0)
                            continue;

                        List<StateLevel> states = new List<StateLevel>();
                        for (int i = 0; i < subStates.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(subStates[i]))
                                continue;
                            subStates[i] = subStates[i].Trim();
                            var datas = subStates[i].Split(',');
                            if (datas != null && datas.Length == 2)
                            {
                                states.Add(new StateLevel() {state = datas[0].Trim(), level = Util.Parse<int>(datas[1].Trim())});
                            }
                        }

                        field.SetValue(attr, states.ToArray());
                    }
                    continue;
                }
                var r = new Regex(" " + field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(attr, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(attr, FromXml(source, field.FieldType, ""));
                }
            }
            return attr;
        }
        #endregion

        if (type == typeof(LabyrinthInfo.LabyrinthReward))
        {
            var s = new LabyrinthInfo.LabyrinthReward();

            var match = Regex.Match(str, @"propId\s*=\s*""([^""]*)""");
            if (match.Success) s.propId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"rate\s*=\s*""([^""]*)""");
            if (match.Success) s.rate = Util.Parse<float>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(ItemAttachAttr))
        {
            var s = new ItemAttachAttr();

            var match = Regex.Match(str, @"id\s*=\s*""([^""]*)""");
            if (match.Success) s.id = Util.Parse<ushort>(match.Groups[1].Value);

            match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) s.type = Util.Parse<byte>(match.Groups[1].Value);

            match = Regex.Match(str, @"value\s*=\s*""([^""]*)""");
            if (match.Success) s.value = Util.Parse<double>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(EvolveEquipInfo.EvolveMaterial))
        {
            var s = new EvolveEquipInfo.EvolveMaterial();

            var match = Regex.Match(str, @"propId\s*=\s*""([^""]*)""");
            if (match.Success) s.propId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"num\s*=\s*""([^""]*)""");
            if (match.Success) s.num = Util.Parse<uint>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(TaskInfo.TaskStarDetail))
        {
            var s = new TaskInfo.TaskStarDetail();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(TaskInfo.TaskStarReward))
        {
            var s = new TaskInfo.TaskStarReward();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(TaskInfo.TaskStarCondition))
        {
            var s = new TaskInfo.TaskStarCondition();

            var match = Regex.Match(str, @"type\s*=\s*""([^""]*)""");
            if (match.Success) s.type = Util.Parse<EnumPVEDataType>(match.Groups[1].Value);

            match = Regex.Match(str, @"value\s*=\s*""([^""]*)""");
            if (match.Success) s.value = Util.Parse<int>(match.Groups[1].Value);
            
            return s;
        }

        if (type == typeof(TaskInfo.TaskStarProp))
        {
            var s = new TaskInfo.TaskStarProp();

            var match = Regex.Match(str, @"propId\s*=\s*""([^""]*)""");
            if (match.Success) s.propId = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"level\s*=\s*""([^""]*)""");
            if (match.Success) s.level = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"star\s*=\s*""([^""]*)""");
            if (match.Success) s.star = Util.Parse<int>(match.Groups[1].Value);

            match = Regex.Match(str, @"num\s*=\s*""([^""]*)""");
            if (match.Success) s.num = Util.Parse<int>(match.Groups[1].Value);
            return s;
        }

        if (type == typeof(StateOverrideInfo.StateOverride))
        {
            var o = new StateOverrideInfo.StateOverride();
            var fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.Name.StartsWith("__") || field.Name[0] == '<') continue;
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(o, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(o, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(o, FromXml(source, field.FieldType, ""));
                }
            }
            o.originOverrides = new StateMachineInfo.StateDetail[o.overrides.Length];
            for (int i = 0; i < o.originOverrides.Length; i++)
            {
                o.originOverrides[i] = o.overrides[i].Clone() as StateMachineInfo.StateDetail;
            }

            return o;
        }
        #region NPC约会
        if (type == typeof(DialogueAnswersConfig.AnswerItem))
        {
            var s = new DialogueAnswersConfig.AnswerItem();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(DatingMapBuildConfig.ClickPolygonEdge))
        {
            var build = new DatingMapBuildConfig.ClickPolygonEdge();
            var fields = build.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(build, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(build, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(build, FromXml(source, field.FieldType, ""));
                }
            }
            return build;
        }

        if (type == typeof(DatingMapBuildConfig.Effect))
        {
            var effect = new DatingMapBuildConfig.Effect();
            var fields = effect.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(effect, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(effect, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(effect, FromXml(source, field.FieldType, ""));
                }
            }
            return effect;
        }

        if (type == typeof(GeneralConfigInfo.DatingMapCamera))
        {
            var s = new GeneralConfigInfo.DatingMapCamera();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(DatingEventInfo.DatingEventItem))
        {
            var s = new DatingEventInfo.DatingEventItem();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(DatingEventInfo.Condition))
        {
            var s = new DatingEventInfo.Condition();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        if (type == typeof(DatingEventInfo.Behaviour))
        {
            var s = new DatingEventInfo.Behaviour();

            var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(s, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(s, FromXml(source, field.FieldType, ""));
                }
            }
            return s;
        }

        #endregion

        if (typeof(IList).IsAssignableFrom(type))
        {
            var tree = str.IndexOf("<") > -1;
            var ts = type.GetGenericArguments();
            var t  = ts.Length > 0 ? ts[0] : type.GetElementType();

            var list = new List<object>();

            if (!tree)
            {
                var vals = Util.ParseString<string>(str, true, ';');
                for (var i = 0; i < vals.Length; ++i)
                {
                    var item = FromXml(source, t, vals[i]);
                    list.Add(item);
                }
            }
            else
            {
                var snode = node.EndsWith("s") ? node.Remove(node.Length - 1) : node;
                var reg0  = Util.Format(@"<\s*{0}\s*>((?:(?!<\/{0}>).|\s)*)<\s*\/\s*{0}\s*>", node);
                var reg1  = Util.Format(@"(<\s*{0}[^\/<]+\/>)|(?:(<\s*{0}[^\/<>]*>)((?:(?!<\s*\/\s*{0}\s*>).|\s)+)<\s*\/\s*{0}\s*>)", snode);
                var match = Regex.Match(sub, reg0);

                match = t == typeof(string) ? Regex.Match(!match.Success ? sub : match.Groups[1].Value, snode + @"\s*=\s*""([^""]*)""") : Regex.Match(!match.Success ? sub : match.Groups[1].Value, reg1);

                while (match.Success)
                {
                    var item = t == typeof(string) ? FromXml(source, t, match.Groups[1].Value) : FromXml(source, t, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, node);
                    list.Add(item);
                    match = match.NextMatch();
                }
            }

            var ilist = (type.IsArray ? Array.CreateInstance(t, list.Count) : Activator.CreateInstance(type)) as IList;
            for (var i = 0; i < list.Count; ++i)
            {
                if (type.IsArray) ilist[i] = list[i];
                else ilist.Add(list[i]);
            }

            return ilist;
        }

        if (type.IsSubclassOf(typeof(ConfigItem)))
        {
            var item = Activator.CreateInstance(type) as ConfigItem;
            var fields = item.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            item.___source = source;
            foreach (var field in fields)
            {
                if (field.Name.StartsWith("__") || field.Name[0] == '<') continue;
                var r = new Regex(field.Name + "\\s*=\\s*\"([^\"]*)\"");
                var match = r.Match(str);
                if (match.Success) field.SetValue(item, FromXml(source, field.FieldType, match.Groups[1].Value));
                else
                {
                    var n = field.Name;
                    if (typeof(IList).IsAssignableFrom(field.FieldType) && !n.EndsWith("s")) n += "s";
                    match = Regex.Match(sub, @"(<" + n + @"[^\/<]+\/>)|(?:(<" + n + @"[^\/<>]*>)((?:(?!<\/" + n + @">).|\s)+)<\/" + n + @">)");
                    if (match.Success) field.SetValue(item, FromXml(source, field.FieldType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, n));
                    else field.SetValue(item, FromXml(source, field.FieldType, ""));
                }
            }

            return item;
        }

        if (type.IsSubclassOf(typeof(Config)))
        {
            var c = ScriptableObject.CreateInstance(type) as Config;

            var nr = new Regex(@"<config\s+name=""(\w+)"">");
            var match = nr.Match(str);
            if (!match.Success || match.Groups[1].Value.ToLower() != "config_" + type.Name.ToLower()) return c;

            var name = match.Groups[1].Value;
            var r    = new Regex(@"(<item[^\/<]+\/>)|(?:(<item[^\/<>]+>)((?:(?!<\/item>).|\s)+)<\/item>)");
            var items = new List<ConfigItem>();

            match = r.Match(str);
            while (match.Success)
            {
                items.Add(FromXml(source, c.itemType, !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value, match.Groups[3].Value, node) as ConfigItem);
                match = match.NextMatch();
            }

            c.name = name;
            c.SetItems(items);
            return c;
        }

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}