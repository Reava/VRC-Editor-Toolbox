#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

public class BakeryEditorInfo : EditorWindow
{
    [MenuItem("Tools/Reava_/Bakery Editor Info")]
    public static void ShowWindow()
    {
        GetWindow<BakeryEditorInfo>("Bakery Editor Info");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Bakery Editor Info", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool allows you to batch edit Bakery lights.\n\n" +
            "Right click an object on the hierarchy to start!\n\n" +
            "When selecting a parent, all children will be affected",
            MessageType.Info
        );

        EditorGUILayout.Space(8);
    }
}

#endif