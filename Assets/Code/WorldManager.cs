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
    [UdonSynced] public float _hostTime;

    private float _systemTimeOffset = 0f;
    private bool _hasCalculatedOffset = false;

    [Header("Debug Controls")]
    [Tooltip("Tick this box to control the skybox time manually with the slider below.")]
    public bool debugOverride = false;
    [Range(0f, 1f)] [Tooltip("0.0 = Midnight, 0.25 = 6AM, 0.5 = Noon, 0.75 = 6PM")]
    public float debugTime = 0.5f;
    
    // shader IDs
    private readonly int _sunDirID = Shader.PropertyToID("_SunDirection");
    private readonly int _weatherID = Shader.PropertyToID("_WeatherIntensity");
    

    [Header("Weather")] public float WeatherSpeed = 0.01f;
    
    private void Start()
    {
        if (Networking.IsOwner(gameObject))
        {
            SyncInitialTime();
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
            // we need to keep the time we already initially synced already
            _hasCalculatedOffset = false;
            UpdateHostTime();
        }
        
        base.OnOwnershipTransferred(player);
    }

    private void UpdateHostTime()
    {
        if (debugOverride)
        {
            _hostTime = debugTime;
            return;
        }

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
        if (debugOverride)
        {
            _hostTime = debugTime;
        }
        else
        {
            _hostTime = GetNormalizedSystemTime();
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
        var angle = _hostTime * 360f;

        if (!sunEntity) return;
        
        sunEntity.rotation = Quaternion.Euler(angle, -90f, 0f);
        var sunDirection = sunEntity.forward;
        RenderSettings.skybox.SetVector(_sunDirID, new Vector4(sunDirection.x, sunDirection.y, sunDirection.z, 0f));

        var weatherWave = Mathf.Sin(Time.timeSinceLevelLoad * WeatherSpeed);
        var currentWeatherIntensity = (weatherWave * 0.5f) + 0.5f;
        
        RenderSettings.skybox.SetFloat(_weatherID, currentWeatherIntensity);
    }
}