// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using UdonSharp;
using UnityEngine;

public class RedshiftParticipantRuntime : UdonSharpBehaviour
{
    [Header("Linear Track State")]
    public float raceProgressDistance = 0f;
    public int assignedLane = 0;
    public float laneWidth = 1.2f;

    [Header("Live Physics Attributes")]
    public float currentVelocity = 0f;
    public float currentStaminaPool = 100f;
    public float maxStaminaPool = 100f;
    
    [Header("State Machine Flags")]
    public int currentPhase = 0;
    public bool isDueling = false;
    public bool isLeadingPack = false;
    public bool isTrailingPack = false;
}