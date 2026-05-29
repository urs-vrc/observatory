// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details

/// <summary>
/// Defines the discrete performance tiers for participant statistics.
/// Each tier represents a flat mathematical weight inside the physics engine.
/// </summary>
public enum RedshiftParticipantStatTiers
{
    C = 1,
    B = 2,
    A = 3,
    S = 4,
    Ss = 5
}

/// <summary>
/// Holds the internal, three-axis performance runtime data for a participant.
/// </summary>
public struct RedshiftParticipantStats
{
    /// <summary>
    /// Dictates the baseline maximum velocity ceiling (V_max) on flat straights.
    /// This also dictates how much maximum speed you can select on your speed slider on late phase.
    /// </summary>
    public RedshiftParticipantStatTiers speed;
    /// <summary>
    /// The total energy capacity pool and basic consumption efficiency scaling.
    /// </summary>
    public RedshiftParticipantStatTiers stamina;
    /// <summary>
    /// Controls acceleration curves and provides physical resistance against adverse weather modifiers.
    /// </summary>
    public RedshiftParticipantStatTiers power;

    public static RedshiftParticipantStats CreateDefaultStats()
    {
        return new RedshiftParticipantStats
        {
            speed = RedshiftParticipantStatTiers.A,
            stamina = RedshiftParticipantStatTiers.A,
            power = RedshiftParticipantStatTiers.A
        };
    }
}