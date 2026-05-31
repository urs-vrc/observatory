// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using UdonSharp;
using UnityEngine;

public class RedshiftTrack : UdonSharpBehaviour
{
    [Header("Baked Data Matrix")]
    [Tooltip("Total length of the path loop circuit in meters.")]
    public float TotalTrackLength;
    
    [Tooltip("The precision step resolution interval used during the editor bake pass.")]
    public float SamplingInterval = 1.0f;

    [HideInInspector] public Vector3[] BakedPositions;
    [HideInInspector] public Quaternion[] BakedRotations;

    /// <summary>
    /// Evaluates a 3D position and rotation at an arbitrary linear distance marker along the track.
    /// Runs completely inside the VRChat whitelisted runtime envelope.
    /// </summary>
    public void SampleTrackAtDistance(float distance, out Vector3 position, out Quaternion rotation)
    {
        if (BakedPositions == null || BakedPositions.Length == 0)
        {
            position = transform.position;
            rotation = transform.rotation;
            return;
        }

        // Handle infinite lap wrapping math smoothly
        var wrappedDistance = distance % TotalTrackLength;
        if (wrappedDistance < 0) wrappedDistance += TotalTrackLength;

        // Determine exact index array slots to sample
        var floatingIndex = wrappedDistance / SamplingInterval;
        var indexA = Mathf.FloorToInt(floatingIndex) % BakedPositions.Length;
        var indexB = (indexA + 1) % BakedPositions.Length;
        var t = floatingIndex - Mathf.Floor(floatingIndex);

        // Perform lightweight linear interpolation across raw nodes
        position = Vector3.Lerp(BakedPositions[indexA], BakedPositions[indexB], t);
        rotation = Quaternion.Slerp(BakedRotations[indexA], BakedRotations[indexB], t);
    }    
}