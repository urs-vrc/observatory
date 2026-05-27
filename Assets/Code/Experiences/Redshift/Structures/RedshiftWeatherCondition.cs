using UnityEngine;
using System;

[Serializable]
public struct RedshiftWeatherCondition
{
    [Header("Macro Classifications")]
    [Tooltip("The overarching seasonal parameter.")]
    public RedshiftTrackSeason season;
    
    [Tooltip("The atomic atmospheric condition of the track.")]
    public RedshiftTrackWeather weather;
    
    [Header("Atmospheric Metrics")]
    [Tooltip("Ambient air temperature in Celsius.")]
    public float ambientTemperature;
    
    [Range(0f, 1f)]
    [Tooltip("Humidity percentage represented from 0.0 (Dry) to 1.0 (Saturated).")]
    public float humidity;
    
    [Range(0f, 1f)]
    [Tooltip("Global visibility modifier (0.0 = Pure Blindness, 1.0 = Crisp/Clear).")]
    public float visibility;

    [Header("Wind Configuration")]
    [Range(0f, 360f)]
    [Tooltip("The direction the wind is blowing in degrees.")]
    public float windDirectionDegrees;
    
    [Tooltip("The speed magnitude of the wind.")]
    public float windSpeed;

    [Header("Surface Physics")]
    [Range(0.1f, 1f)]
    [Tooltip("The physical grip coefficient of the track surface (1.0 = Maximum Dry Grip, 0.2 = Sheet Ice).")]
    public float surfaceFrictionCoefficient;

    /// <summary>
    /// Computes the standard directional Vector3 for the wind based on the inspector-friendly degrees and speed.
    /// </summary>
    public Vector3 GetWindVelocityVector()
    {
        var radians = windDirectionDegrees * Mathf.Deg2Rad;
        // Project the angle onto the flat XZ plane (standard for Unity world space movement)
        var direction = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        return direction * windSpeed;
    }
}