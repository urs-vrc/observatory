// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using UdonSharp;
using UnityEngine;

public class RedshiftWeatherController : UdonSharpBehaviour
{
    [Header("Event Routing Dependency")]
    public RedshiftEventBus eventBus;

    [Header("Weather Profile Registry Database")]
    [Tooltip("Drop your child WeatherProfile GameObjects here.")]
    public RedshiftWeatherProfile[] weatherProfiles;

    private int _currentConditionIndex = 0;
    private int _lastEvaluatedWeatherEnum = -1;
    private float _liveSurfaceFriction = 1.0f;
    private Vector3 _liveWindVector = Vector3.zero;
    private float _liveAmbientTemperature = 20.0f;
    private float _liveHumidity = 0.5f;
    private float _liveVisibility = 1.0f;
    private string _activeWeatherLabel = "Clear";

    // Global Public Getters for the RedshiftGameManager loop
    public float GetLiveSurfaceFriction() => _liveSurfaceFriction;
    public Vector3 GetLiveWindVector() => _liveWindVector;
    public float GetLiveAmbientTemperature() => _liveAmbientTemperature;
    public float GetLiveHumidity() => _liveHumidity;
    public float GetLiveVisibility() => _liveVisibility;
    public string GetActiveWeatherLabel() => _activeWeatherLabel;

    /// <summary>
    /// PUSH GATEWAY: Called by WorldManager via primitive float parameter.
    /// </summary>
    public void UpdateWeatherIntensityData(float rawIntensity)
    {
        if (weatherProfiles == null || weatherProfiles.Length == 0) return;

        int targetIndex = Mathf.FloorToInt(rawIntensity * weatherProfiles.Length);
        _currentConditionIndex = Mathf.Clamp(targetIndex, 0, weatherProfiles.Length - 1);

        RedshiftWeatherProfile activeProfile = weatherProfiles[_currentConditionIndex];
        if (activeProfile == null) return;

        // Extract safely using methods, completely avoiding the field access compiler bug!
        _liveSurfaceFriction     = activeProfile.GetSurfaceFriction();
        _liveAmbientTemperature  = activeProfile.GetAmbientTemperature();
        _liveHumidity            = activeProfile.GetHumidity();
        _liveVisibility          = activeProfile.GetVisibility();
        _liveWindVector          = activeProfile.GetWindVelocityVector();
        _activeWeatherLabel      = activeProfile.GetWeatherLabel();
    }
}