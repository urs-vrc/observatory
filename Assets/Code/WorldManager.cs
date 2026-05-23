
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WorldManager : UdonSharpBehaviour
{
    [Header("Time of Day")] 
    public Transform SunEntity;
    [UdonSynced] private float hostTime;

    [Header("Weather")] public float WeatherSpeed = 0.01f;
    
    private void Start()
    {
        if (Networking.IsOwner(gameObject))
        {
            SyncInitialTime();
        }
    }

    private void SyncInitialTime()
    {
        // let's pull the time from the owner to get initial state
        var localTime = DateTime.Now;
        hostTime = (localTime.Hour * 3600f) + (localTime.Minute * 60f) + localTime.Second / 86400f;
    }
}
