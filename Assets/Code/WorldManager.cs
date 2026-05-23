// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

public class WorldManager : UdonSharpBehaviour
{
    [Header("Time of Day")] 
    public Transform sunEntity;
    [UdonSynced] private float _hostTime;

    private float _systemTimeOffset = 0f;
    private bool _hasCalculatedOffset = false;

    [Header("Weather")] public float WeatherSpeed = 0.01f;
    
    private void Start()
    {
        if (Networking.IsOwner(gameObject))
        {
            SyncInitialTime();
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (player.isLocal && Networking.IsOwner(gameObject))
        {
            // we need to keep the time we already initially synced already
            _hasCalculatedOffset = false;
            UpdateHostTime();
            
        }
        
        base.OnOwnershipTransferred(player);
    }

    private void UpdateHostTime()
    {
        var localSystemTime = GetNormalizedSystemTime();

        if (!_hasCalculatedOffset)
        {
            _systemTimeOffset = localSystemTime - _hostTime;
            _hasCalculatedOffset = true;
        }

        var calculatedTime = localSystemTime - _systemTimeOffset;
        
        if (calculatedTime >= 1f) calculatedTime -= 1f;
        if (calculatedTime < 0f) calculatedTime += 1f;
        
        _hostTime = calculatedTime;
    }

    private void SyncInitialTime()
    {
        // let's pull the time from the owner to get initial state
        _hostTime = GetNormalizedSystemTime();
        _systemTimeOffset = 0f;
        _hasCalculatedOffset = true;
    }

    private static float GetNormalizedSystemTime()
    {
        var localTime = DateTime.Now;
        var totalSeconds = (localTime.Hour * 3600f) + (localTime.Minute * 60f) + localTime.Second;
        
        return totalSeconds / 86400f;
    }
}
