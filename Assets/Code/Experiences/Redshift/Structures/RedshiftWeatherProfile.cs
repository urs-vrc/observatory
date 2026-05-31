// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using UdonSharp;
using UnityEngine;

public class RedshiftWeatherProfile : UdonSharpBehaviour
{
    [Header("Macro Classifications")]
    public string weatherLabel = "Clear";
    
    [Header("Atmospheric Metrics")]
    public float ambientTemperature = 25.0f;
    [Range(0f, 1f)] public float humidity = 0.4f;
    [Range(0f, 1f)] public float visibility = 1.0f;

    [Header("Wind Configuration")]
    [Range(0f, 360f)] public float windDirectionDegrees = 0f;
    public float windSpeed = 0f;

    [Header("Surface Physics")]
    [Range(0.1f, 1f)] public float surfaceFrictionCoefficient = 1.0f;

    // Getter methods are 100% stable across Udon assembly boundaries
    public float GetSurfaceFriction() => surfaceFrictionCoefficient;
    public float GetAmbientTemperature() => ambientTemperature;
    public float GetHumidity() => humidity;
    public float GetVisibility() => visibility;
    public string GetWeatherLabel() => weatherLabel;

    public Vector3 GetWindVelocityVector()
    {
        float radians = windDirectionDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians)) * windSpeed;
    }
}