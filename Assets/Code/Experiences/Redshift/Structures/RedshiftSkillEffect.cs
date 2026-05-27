/// <summary>
/// The flat mathematical modifiers injected into the running simulation loop when a skill triggers.
/// </summary> 
public struct RedshiftSkillEffect
 {
     /// <summary>
     /// The amount of speed added by the skill
     /// </summary>
     public float speedLimitModifier;
     /// <summary>
     /// The amount of acceleration added by the skill
     /// </summary>
     public float accelerationModifier;
     /// <summary>
     /// The amount of stamina to be recovered
     /// </summary>
     public float staminaRecoveryDelta;
     /// <summary>
     /// Duration of the skill
     /// </summary>
     public float duration;
 }