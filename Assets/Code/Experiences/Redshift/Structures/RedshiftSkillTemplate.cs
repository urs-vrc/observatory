// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using UnityEngine;

[System.Serializable]
public struct RedshiftSkillTemplate
{
    [Header("Identity")]
    public string skillName;
    public int skillId;

    [Header("1. Activation Gate (Event Bus Link)")]
    [Tooltip("The exact event pulse this skill wakes up to evaluate.")]
    public RedshiftEvents triggerEvent;

    [Header("2. Race Phase Constraints (-1 = Any, 0 = Early, 1 = Mid, 2 = Late, 3 = Final)")]
    [Range(-1, 3)] 
    public int requiredPhase; 

    [Header("3. Field Positioning Constraints (Set 0 to Ignore)")]
    public bool requiresLeadingPack;
    public bool requiresTrailingPack;

    [Header("4. Runtime Structural States")]
    public bool requiresDueling;

    [Header("5. Mathematical Payload & Targeting")]
    [Tooltip("Who does this effect apply to? (Self, OpponentsInFront, OpponentsBehind, ClosestDuelTarget)")]
    public RedshiftSkillTarget skillTarget;
    
    [Tooltip("Positive for buffs, negative for debuffs.")]
    public float speedLimitModifier;
    [Tooltip("Positive for buffs, negative for debuffs.")]
    public float accelerationModifier;
    [Tooltip("Positive heals stamina, negative drains/debuffs stamina.")]
    public float staminaRecoveryDelta;
    public float duration;
    [Tooltip("Cooldown period for the skill, set to zero to make skill single-use")]
    public float cooldown;

    /// <summary>
    /// Extracts the clean, dedicated condition criteria for the evaluation engine.
    /// </summary>
    public RedshiftSkillCondition GetCondition()
    {
        return new RedshiftSkillCondition
        {
            triggerEvent = this.triggerEvent,
            requiredPhase = this.requiredPhase,
            requiresLeadingPack = this.requiresLeadingPack,
            requiresTrailingPack = this.requiresTrailingPack,
            requiresDueling = this.requiresDueling
        };
    }

    /// <summary>
    /// Extracts the mathematical payload to drop into the active buff/debuff slots.
    /// </summary>
    public RedshiftSkillEffect GetEffect()
    {
        return new RedshiftSkillEffect
        {
            skillTarget = this.skillTarget,
            speedLimitModifier = this.speedLimitModifier,
            accelerationModifier = this.accelerationModifier,
            staminaRecoveryDelta = this.staminaRecoveryDelta,
            duration = this.duration
        };
    }
}