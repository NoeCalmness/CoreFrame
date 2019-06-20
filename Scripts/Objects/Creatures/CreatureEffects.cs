/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-
 * 
 ***************************************************************************************************/

using UnityEngine;

public class CreatureEffects : MonoBehaviour
{
    public Creature creature { get; set; }
    public CreatureBehaviour behaviour { get { return creature ? creature.behaviour : null; } }
    public StateMachine animator { get { return creature ? creature.stateMachine : null; } }

    public Effect PlayEffect(StateMachineInfo.Effect effInfo, Buff sourceBuff = null)
    {
        // always ignore empty effect
        if (effInfo.isEmpty) return null;

        var node = string.IsNullOrEmpty(effInfo.spawnAt) ? transform : Util.FindChild(creature.activeRootNode, effInfo.spawnAt);
        if (!node)
        {
            Logger.LogWarning("CreatureEffects::PlayEffect: Could not play effect [{0}], could not find bind node [{1}]", effInfo.effect, effInfo.spawnAt);
            return null;
        }

        for (var i = node.childCount - 1; i >= 0; i--)
        {
            var c = node.GetChild(i);
            if (c.gameObject.activeSelf && c.name.Equals(effInfo.effect))
                c.SafeSetActive(false);
        }

        return Effect.Create(effInfo, node, creature, sourceBuff);
    }

    public FlyingEffect PlayEffect(StateMachineInfo.FlyingEffect effInfo, StateMachineInfo.Effect hitEffect)
    {
        var node = string.IsNullOrEmpty(effInfo.spawnAt) ? transform : Util.FindChild(creature.activeRootNode, effInfo.spawnAt);
        if (!node)
        {
            Logger.LogWarning("CreatureEffects::PlayEffect: Could not play effect [{0}], could not find bind node [{1}]", effInfo.effect, effInfo.spawnAt);
            return null;
        }

        return FlyingEffect.Create(effInfo, creature, 0, !creature.isForward, hitEffect, creature.position_, Vector3_.zero, creature.eulerAngles);
    }

    public FollowTargetEffect PlayEffect(StateMachineInfo.FollowTargetEffect effInfo, Buff sourceBuff, Creature target, Creature initNode)
    {
        // always ignore empty effect
        if (effInfo.isEmpty) return null;

        var node = string.IsNullOrEmpty(effInfo.spawnAt) ? initNode.transform : Util.FindChild(initNode.activeRootNode, effInfo.spawnAt);
        if (!node)
        {
            Logger.LogWarning("CreatureEffects::PlayEffect: Could not play effect [{0}], could not find bind node [{1}]", effInfo.effect, effInfo.spawnAt);
            return null;
        }

        return FollowTargetEffect.Create(effInfo, node, target, initNode.position_, creature, sourceBuff);
    }
}
