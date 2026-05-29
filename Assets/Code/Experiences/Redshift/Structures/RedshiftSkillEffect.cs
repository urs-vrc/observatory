// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using UnityEngine;

/// <summary>
/// The flat mathematical modifiers injected into the running simulation loop when a skill triggers.
/// </summary> 
[System.Serializable]
public struct RedshiftSkillEffect
{
    [Header("Targeting Logic")]
    public RedshiftSkillTarget skillTarget;

    [Header("Buffs / Debuffs (Negative values for Debuffs)")]
    public float speedLimitModifier;
    public float accelerationModifier;
    
    [Header("Stamina Mechanics")]
    public float staminaRecoveryDelta;
    [Range(0, 240)] 
    public float duration;
}