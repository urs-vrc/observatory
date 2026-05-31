#if UNITY_EDITOR
// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RedshiftSkillTemplate))]
public class RedshiftSkillTemplateEditor : PropertyDrawer
{
    private readonly Color ColorBuff     = new Color(0.20f, 0.70f, 0.30f, 0.25f); 
    private readonly Color ColorRecovery = new Color(0.15f, 0.50f, 0.85f, 0.25f);
    private readonly Color ColorDebuff   = new Color(0.85f, 0.20f, 0.20f, 0.25f);
    private readonly Color ColorDefault  = new Color(0.25f, 0.25f, 0.25f, 0.15f);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty nameProp   = property.FindPropertyRelative("skillName");
        SerializedProperty idProp     = property.FindPropertyRelative("skillId");
        SerializedProperty eventProp  = property.FindPropertyRelative("triggerEvent");
        SerializedProperty phaseProp  = property.FindPropertyRelative("requiredPhase");
        
        SerializedProperty leadProp   = property.FindPropertyRelative("requiresLeadingPack");
        SerializedProperty trailProp  = property.FindPropertyRelative("requiresTrailingPack");
        SerializedProperty duelProp   = property.FindPropertyRelative("requiresDueling");
        
        SerializedProperty targetProp = property.FindPropertyRelative("skillTarget");
        SerializedProperty speedProp  = property.FindPropertyRelative("speedLimitModifier");
        SerializedProperty accelProp  = property.FindPropertyRelative("accelerationModifier");
        SerializedProperty stamProp   = property.FindPropertyRelative("staminaRecoveryDelta");
        SerializedProperty durProp    = property.FindPropertyRelative("duration");

        label = EditorGUI.BeginProperty(position, label, property);
        
        Rect boxRect = new Rect(position.x, position.y, position.width, position.height - 4);
        GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);
        
        Rect headerRect = new Rect(position.x + 6, position.y + 6, position.width - 12, EditorGUIUtility.singleLineHeight);
        string headerName = string.IsNullOrEmpty(nameProp.stringValue) ? "New Skill Variant" : nameProp.stringValue;
        EditorGUI.LabelField(headerRect, $"[ID: {idProp.intValue}] {headerName}", EditorStyles.boldLabel);
        
        float currentY = position.y + EditorGUIUtility.singleLineHeight + 12;

        float fieldWidth = (position.width - 24) * 0.5f;
        Rect nameRect = new Rect(position.x + 8, currentY, fieldWidth, EditorGUIUtility.singleLineHeight);
        Rect idRect = new Rect(position.x + 12 + fieldWidth, currentY, fieldWidth, EditorGUIUtility.singleLineHeight);
        
        EditorGUI.PropertyField(nameRect, nameProp, new GUIContent("Skill Name"));
        EditorGUI.PropertyField(idRect, idProp, new GUIContent("System ID"));
        
        currentY += EditorGUIUtility.singleLineHeight + 8;
        DrawLineSeparator(position.x + 8, currentY, position.width - 16);
        currentY += 6;
        
        Rect gateHeaderRect = new Rect(position.x + 8, currentY, position.width - 16, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(gateHeaderRect, "Activation Constraints & Gates", EditorStyles.miniBoldLabel);
        currentY += EditorGUIUtility.singleLineHeight + 4;

        Rect eventRect = new Rect(position.x + 8, currentY, fieldWidth, EditorGUIUtility.singleLineHeight);
        Rect phaseRect = new Rect(position.x + 12 + fieldWidth, currentY, fieldWidth, EditorGUIUtility.singleLineHeight);
        
        EditorGUI.PropertyField(eventRect, eventProp, new GUIContent("Trigger Event"));
        
        string[] phaseLabels = new string[] { "Any Phase (-1)", "Early Phase (0)", "Mid Phase (1)", "Late Phase  (2)", "Final Spurt (3)" };
        int selectedIndex = phaseProp.intValue + 1;
        selectedIndex = EditorGUI.Popup(phaseRect, "Required Phase", selectedIndex, phaseLabels);
        phaseProp.intValue = selectedIndex - 1;

        currentY += EditorGUIUtility.singleLineHeight + 6;

        float flagWidth = (position.width - 24) / 3f;
        Rect leadRect = new Rect(position.x + 8, currentY, flagWidth, EditorGUIUtility.singleLineHeight);
        Rect trailRect = new Rect(position.x + 8 + flagWidth, currentY, flagWidth, EditorGUIUtility.singleLineHeight);
        Rect duelRect = new Rect(position.x + 8 + (flagWidth * 2), currentY, flagWidth, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(leadRect, leadProp, new GUIContent("Req Lead"));
        EditorGUI.PropertyField(trailRect, trailProp, new GUIContent("Req Trail"));
        EditorGUI.PropertyField(duelRect, duelProp, new GUIContent("Req Duel"));

        currentY += EditorGUIUtility.singleLineHeight + 8;
        DrawLineSeparator(position.x + 8, currentY, position.width - 16);
        currentY += 6;

        
        Rect targetRect = new Rect(position.x + 8, currentY, position.width - 16, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(targetRect, targetProp, new GUIContent("Target Delivery"));
        currentY += EditorGUIUtility.singleLineHeight + 6;

        // Evaluate character identity roles to select the correct hardcoded color band
        Color highlightColor = ColorDefault;
        string typeLabel = "UNCONFIGURED STATUS MODIFIER";

        bool hasNegative = speedProp.floatValue < 0f || accelProp.floatValue < 0f || stamProp.floatValue < 0f;
        bool hasPositiveVelocity = speedProp.floatValue > 0f || accelProp.floatValue > 0f;
        bool hasPositiveStamina = stamProp.floatValue > 0f;

        if (hasNegative)
        {
            highlightColor = ColorDebuff;
            typeLabel = "PAYLOAD MATRIX: TACTICAL DEBUFF";
        }
        else if (hasPositiveVelocity)
        {
            highlightColor = ColorBuff;
            typeLabel = "PAYLOAD MATRIX: PERFORMANCE BUFF";
        }
        else if (hasPositiveStamina)
        {
            highlightColor = ColorRecovery;
            typeLabel = "PAYLOAD MATRIX: STAMINA RECOVERY";
        }
        
        Rect modBoxRect = new Rect(position.x + 4, currentY, position.width - 8, (EditorGUIUtility.singleLineHeight * 2) + 14);
        EditorGUI.DrawRect(modBoxRect, highlightColor);

        Rect modHeaderRect = new Rect(position.x + 8, currentY + 4, position.width - 16, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(modHeaderRect, typeLabel, EditorStyles.miniBoldLabel);
        currentY += EditorGUIUtility.singleLineHeight + 6;

        float modWidth = (position.width - 28) * 0.5f;
        Rect speedRect = new Rect(position.x + 8, currentY, modWidth, EditorGUIUtility.singleLineHeight);
        Rect accelRect = new Rect(position.x + 12 + modWidth, currentY, modWidth, EditorGUIUtility.singleLineHeight);
        currentY += EditorGUIUtility.singleLineHeight + 4;
        
        Rect stamValueRect = new Rect(position.x + 8, currentY, modWidth, EditorGUIUtility.singleLineHeight);
        Rect durValueRect = new Rect(position.x + 12 + modWidth, currentY, modWidth, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(speedRect, speedProp, new GUIContent("Speed Delta"));
        EditorGUI.PropertyField(accelRect, accelProp, new GUIContent("Accel Delta"));
        EditorGUI.PropertyField(stamValueRect, stamProp, new GUIContent("Stamina Delta"));
        EditorGUI.PropertyField(durValueRect, durProp, new GUIContent("Duration (s)"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight * 7) + 58f;
    }

    private void DrawLineSeparator(float x, float y, float width)
    {
        Rect lineRect = new Rect(x, y, width, 1);
        EditorGUI.DrawRect(lineRect, new Color(0.15f, 0.15f, 0.15f, 0.3f));
    }
}
#endif