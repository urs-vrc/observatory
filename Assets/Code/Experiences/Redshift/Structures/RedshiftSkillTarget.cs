// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details

/// <summary>
/// Determines the target of a specific skill
/// </summary>
public enum RedshiftSkillTarget
{
    /// <summary>
    /// the skill targets yourself
    /// </summary>
    Self = 0,
    /// <summary>
    /// the skill targets the participants in front of you
    /// </summary>
    OpponentsInFront = 1,
    /// <summary>
    /// the skill targets the participants behind you
    /// </summary>
    OpponentsBehind = 2,
    /// <summary>
    /// The skill targets the participants you are neck-and-neck with
    /// </summary>
    ClosestDuelTarget = 3,
    /// <summary>
    /// The skill targets the top 5 opponents in the match.
    /// </summary>
    Top5Opponents = 4
}