/// <summary>
/// Defines the valid events that can be sent through <see cref="RedshiftEventBus"/>.
/// These are usually lightweight events that are intended to make it easy for clients
/// to sync states with each other.
/// </summary>
public enum RedshiftEvents
{
    // Participant States
    
    /// <summary>
    /// When a participant joins the track
    /// </summary>
    ParticipantJoinedEvent = 1,
    /// <summary>
    /// When a participant leaves the event, either through unforeseen consequences
    /// or respawning.
    ///
    /// Note: a nuance exists here that this should always fire if a seat occupant
    /// respawns out of it or disconnects, but according to a few people, that is
    /// unreliable due to how vrc works.
    /// </summary>
    ParticipantLeftEvent = 2,
    /// <summary>
    /// When a participant is ready to start. This is used by TrackManager to gauge
    /// how many participants are ready to go, majority vote wins (50%)
    /// </summary>
    ParticipantReadyToStartEvent = 3,
    
    // Participant start states
    
    /// <summary>
    /// When the participant performs a perfect start in the gate
    /// </summary>
    ParticipantPerfectStartEvent = 4,
    /// <summary>
    /// When the participant meets the minimum conditions to start normally in the gate
    /// </summary>
    ParticipantNormalStartEvent = 5,
    /// <summary>
    /// When the participant does not meet the sufficient conditions to start normally
    /// in the gate
    /// </summary>
    ParticipantLateStartEvent = 6,
    /// <summary>
    /// When the participant does not perform the required actions to start (inactivity or ran out of time)
    /// They start slower than late starts.
    ///
    /// We call this one the "12 billion yen event"
    /// </summary>
    ParticipantDeferredLateStartEvent = 7,
    
    // skill activation states
    
    /// <summary>
    /// When a participant activates their speed modifier 
    /// </summary>
    SpeedModActivated = 8,
    /// <summary>
    /// When a participant activates their acceleration modifier
    /// </summary>
    AccelModActivated = 9,
    /// <summary>
    /// When a participant activates their stamina modifier
    /// </summary>
    StaminaModActivated = 10,
    
    // Participant-vs-Participant state
    // useful for some PvP condition skills like ultimate skills
    
    /// <summary>
    /// When a participant is side-by-side with another participant
    /// </summary>
    ParticipantDuelingEvent = 11,
    // we won't use this, but it's a nice to have at some point
    
    /// <summary>
    /// When a participant is either surrounded or blocked in the front of another participant.
    /// This is currently unused.
    /// </summary>
    ParticipantBlockedEvent = 12,
    /// <summary>
    /// When a participant is over-pacing vs the median pace for a track condition.
    /// This is currently unused.
    ///
    /// NB: might be useful for training mode?
    /// </summary>
    ParticipantRushedEvent = 13,
    
    // Weather states fired by the host, used by RedshiftGameManager
    // to tell TrackManager what track conditions to set
    
    /// <summary>
    /// When the weather changes in the track (fired by the host only!)
    /// </summary>
    WeatherChangedEvent = 14,
    /// <summary>
    /// When the season changes in the track (fired by the host only!)
    /// </summary>
    SeasonChangedEvent =15
}
