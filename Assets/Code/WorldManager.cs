// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WorldManager : UdonSharpBehaviour
{
    [Header("Time of Day")] 
    public Transform sunEntity;
    [UdonSynced] 
    public float hostTime;

    private float _systemTimeOffset = 0f;
    private bool _hasCalculatedOffset = false;
    
    private float _smoothTime = 0f;
    [Tooltip("How fast the skybox transitions to match the host time. Higher = faster catchup.")]
    public float timeSmoothingSpeed = 0.5f;

    [Header("Debug Controls")]
    [Tooltip("Tick this box to control the skybox time manually with the slider below.")]
    public bool debugOverride = false;
    [Range(0f, 1f)] [Tooltip("0.0 = Midnight, 0.25 = 6AM, 0.5 = Noon, 0.75 = 6PM")]
    public float debugTime = 0.5f;
    
    // shader IDs
    private int _sunDirID;
    private int _weatherID;
    

    [Header("Weather")] 
    public float weatherSpeed = 0.01f;
    public float currentRawWeatherIntensity = 0f;
    
    private void Start()
    {
        _sunDirID = VRCShader.PropertyToID("_UdonSunDirection");
        _weatherID = VRCShader.PropertyToID("_UdonWeatherIntensity");
        if (Networking.IsOwner(gameObject))
        {
            SyncInitialTime();
            _smoothTime = hostTime;
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player.isLocal && Networking.IsOwner(gameObject))
        {
            SyncInitialTime();
            _smoothTime = hostTime; 
        }
    }
    
    public override void OnDeserialization()
    {
        if (!Networking.IsOwner(gameObject) && !_hasCalculatedOffset)
        {
            var localSystemTime = GetNormalizedSystemTime();
            _systemTimeOffset = localSystemTime - hostTime;
            _hasCalculatedOffset = true;
            _smoothTime = hostTime; 
        }
    }

    private void Update()
    {
        if (Networking.IsOwner(gameObject)) 
            UpdateHostTime();
        
        UpdateSkybox();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (player.isLocal && Networking.IsOwner(gameObject))
        {
            _hasCalculatedOffset = false;
            UpdateHostTime();
        }
        
        base.OnOwnershipTransferred(player);
    }

    private void UpdateHostTime()
    {
        if (debugOverride)
        {
            hostTime = debugTime;
            return;
        }

        var localSystemTime = GetNormalizedSystemTime();

        if (!_hasCalculatedOffset)
        {
            _systemTimeOffset = localSystemTime - hostTime;
            _hasCalculatedOffset = true;
        }

        var calculatedTime = localSystemTime - _systemTimeOffset;
        
        if (calculatedTime >= 1f) calculatedTime -= 1f;
        if (calculatedTime < 0f) calculatedTime += 1f;
        
        hostTime = calculatedTime;
    }

    private void SyncInitialTime()
    {
        if (debugOverride)
        {
            hostTime = debugTime;
        }
        else
        {
            hostTime = GetNormalizedSystemTime();
        }
        _systemTimeOffset = 0f;
        _hasCalculatedOffset = true;
    }

    private static float GetNormalizedSystemTime()
    {
        var localTime = DateTime.Now;
        var totalSeconds = (localTime.Hour * 3600f) + (localTime.Minute * 60f) + localTime.Second;
        
        return totalSeconds / 86400f;
    }

    private void UpdateSkybox()
    {
        _smoothTime = Mathf.LerpAngle(_smoothTime * 360f, hostTime * 360f, Time.deltaTime * timeSmoothingSpeed) / 360f;
    
        // Shift the phase by -90 degrees so Noon is overhead and Midnight is underground
        var angle = (_smoothTime * 360f) - 90f;

        if (!sunEntity) return;
    
        sunEntity.rotation = Quaternion.Euler(angle, -90f, 0f);
        var sunDirection = -sunEntity.forward;

        var weatherWave = Mathf.Sin(Time.timeSinceLevelLoad * weatherSpeed);
        currentRawWeatherIntensity = (weatherWave * 0.5f) + 0.5f;
        var currentWeatherIntensity = currentRawWeatherIntensity;
    
        VRCShader.SetGlobalVector(_sunDirID, new Vector4(sunDirection.x, sunDirection.y, sunDirection.z, 0f));
        VRCShader.SetGlobalFloat(_weatherID, currentWeatherIntensity);
    }
}