using Not3A.Stage2.Integration;
using UnityEditor;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot.Editor
{
    [CustomEditor(typeof(Stage2CameraZoomSettings))]
    public sealed class Stage3CameraZoomSettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var multiplier = serializedObject.FindProperty("stepMultiplier");
            EditorGUILayout.LabelField("Сила зума колёсиком", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Двигайте ползунок во время игры и сразу пробуйте колесо. " +
                "Настройка сохранится после выхода из Play Mode.", MessageType.Info);
            EditorGUILayout.Slider(multiplier, 1.05f, 3f, new GUIContent("Сила за шаг"));
            if (serializedObject.ApplyModifiedProperties()) AssetDatabase.SaveAssets();
            EditorGUILayout.LabelField("Текущее значение", $"×{multiplier.floatValue:0.00}");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("1.05 — мягко    1.40 — средне    3.00 — резко");
        }
    }
}
