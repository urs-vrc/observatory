// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Purpose-built track definition baker for VRChat using Cinemachine Splines
/// This is meant to ease development for first party and third party worlds.
/// </summary>
[CustomEditor(typeof(RedshiftTrack))]
public class RedshiftCinemachineBaker : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RedshiftTrack bakedTrack = (RedshiftTrack)target;
        CinemachineSmoothPath spline = bakedTrack.GetComponent<CinemachineSmoothPath>();

        if (!spline)
        {
            EditorGUILayout.HelpBox("Please attach a CinemachineSmoothPath component to this same GameObject to use the baker pipeline!", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Bake Cinemachine Spline to Udon Matrix"))
        {
            Undo.RecordObject(bakedTrack, "Bake Track Geometry");

            var length = spline.PathLength;
            var interval = bakedTrack.SamplingInterval <= 0.1f ? 1.0f : bakedTrack.SamplingInterval;
            var totalSamples = Mathf.CeilToInt(length / interval);

            bakedTrack.TotalTrackLength = length;
            bakedTrack.BakedPositions = new Vector3[totalSamples];
            bakedTrack.BakedRotations = new Vector3[totalSamples] != null ? new Quaternion[totalSamples] : new Quaternion[totalSamples];

            for (var i = 0; i < totalSamples; i++)
            {
                var sampleDistance = i * interval;
                bakedTrack.BakedPositions[i] = spline.EvaluatePositionAtUnit(sampleDistance, CinemachinePathBase.PositionUnits.Distance);
                bakedTrack.BakedRotations[i] = spline.EvaluateOrientationAtUnit(sampleDistance, CinemachinePathBase.PositionUnits.Distance);
            }

            EditorUtility.SetDirty(bakedTrack);
            Debug.Log($"[TrackBaker] Successfully baked {totalSamples} milestone positions onto Udon target container array!");
        }
    }
}

#endif