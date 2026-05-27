using UdonSharp;
using VRC.SDK3.Data;


/// <summary>
/// The event bus is responsible for syncing game states between all necessary components
/// and synchronizing game state between clients, slightly inspired of Autodesk Stingray's
/// networking system.
/// </summary>
public class RedshiftEventBus : UdonSharpBehaviour
{
    private const int TotalEvents = 16; 
    private DataList[] _eventRegistry;
    private bool _isInitialized;

    private void Start()
    {
        InitializeBus();
    }

    /// <summary>
    /// Explicitly initializes the underlying data arrays. 
    /// Ensures safety if external managers attempt to subscribe during their own Awake/Start phases.
    /// </summary>
    private void InitializeBus()
    {
        if (_isInitialized) return;

        _eventRegistry = new DataList[TotalEvents];
        for (var i = 0; i < TotalEvents; i++)
        {
            _eventRegistry[i] = new DataList();
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Registers a subsystem component to listen for a specific Redshift system event.
    /// </summary>
    public void Subscribe(RedshiftEvents eventId, UdonSharpBehaviour listener)
    {
        if (!_isInitialized) InitializeBus();

        var index = (int)eventId;
        if (index < 1 || index >= TotalEvents) return;
        if (listener == null) return;

        var listeners = _eventRegistry[index];
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    /// <summary>
    /// Unregisters a subsystem component from an active event queue. 
    /// Crucial for clearing memory handles during map cleanups or room resets.
    /// </summary>
    public void Unsubscribe(RedshiftEvents eventId, UdonSharpBehaviour listener)
    {
        if (!_isInitialized) return;

        var index = (int)eventId;
        if (index < 1 || index >= TotalEvents) return;
        if (listener == null) return;

        _eventRegistry[index].Remove(listener);
    }

    /// <summary>
    /// Dispatches an execution pulse across all registered listeners for the target event.
    /// Invokes standard, predictable string callbacks matching the specific event signature.
    /// </summary>
    public void Publish(RedshiftEvents eventId)
    {
        if (!_isInitialized) return;

        var index = (int)eventId;
        if (index < 1 || index >= TotalEvents) return;

        var listeners = _eventRegistry[index];
        var targetCallback = GetCallbackMethodName(eventId);

        if (string.IsNullOrEmpty(targetCallback)) return;

        for (var i = 0; i < listeners.Count; i++)
        {
            var target = (UdonSharpBehaviour)listeners[i].Reference;
            
            if (target)
            {
                target.SendCustomEvent(targetCallback);
            }
        }
    }

    /// <summary>
    /// Maps internal event enums to clean, standard string execution entry points.
    /// </summary>
    private string GetCallbackMethodName(RedshiftEvents eventId)
    {
        switch (eventId)
        {
            case RedshiftEvents.ParticipantJoinedEvent:           return "OnParticipantJoined";
            case RedshiftEvents.ParticipantLeftEvent:             return "OnParticipantLeft";
            case RedshiftEvents.ParticipantReadyToStartEvent:     return "OnParticipantReadyToStart";
            case RedshiftEvents.ParticipantPerfectStartEvent:      return "OnParticipantPerfectStart";
            case RedshiftEvents.ParticipantNormalStartEvent:       return "OnParticipantNormalStart";
            case RedshiftEvents.ParticipantLateStartEvent:         return "OnParticipantLateStart";
            case RedshiftEvents.ParticipantDeferredLateStartEvent: return "OnParticipantDeferredLateStart";
            case RedshiftEvents.SpeedModActivated:                return "OnSpeedModActivated";
            case RedshiftEvents.AccelModActivated:                return "OnAccelModActivated";
            case RedshiftEvents.StaminaModActivated:              return "OnStaminaModActivated";
            case RedshiftEvents.ParticipantDuelingEvent:          return "OnParticipantDueling";
            case RedshiftEvents.ParticipantBlockedEvent:          return "OnParticipantBlocked";
            case RedshiftEvents.ParticipantRushedEvent:           return "OnParticipantRushed";
            case RedshiftEvents.WeatherChangedEvent:              return "OnWeatherChanged";
            case RedshiftEvents.SeasonChangedEvent:               return "OnSeasonChanged";
            default: return string.Empty;
        }
    }
}