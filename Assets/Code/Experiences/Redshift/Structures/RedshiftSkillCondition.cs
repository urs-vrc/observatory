// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details

/// <summary>
/// Dictates the situational or positional requirements before a skill can execute.
/// </summary>
public struct RedshiftSkillCondition
{
    /// <summary>
    /// The specific event bus pulse this skill wakes up for
    /// </summary>
    public RedshiftEvents triggerEvent;
    /// <summary>
    /// Phase constraint (0 = Early, 1 = Mid, 2 = Late, 3 = Final Spurt)
    /// </summary>
    public int requiredPhase;
    /// <summary>
    /// Does skill require leading pack?
    /// </summary>
    public bool requiresLeadingPack;
    /// <summary>
    /// Does skill require trailing pack?
    /// </summary>
    public bool requiresTrailingPack;
    /// <summary>
    /// Does skill require the participant to be dueling?
    /// </summary>
    public bool requiresDueling;
}