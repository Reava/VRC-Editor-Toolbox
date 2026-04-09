using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReavaCirclePlacerWindow : EditorWindow
{
    private List<GameObject> objects = new List<GameObject>();
    private float radius = 5f;
    private Vector3 centerPosition = Vector3.zero;
    private bool useSelectedAsCenter = true;
    private bool faceCenter = true;

    private Vector2 scroll;

    [MenuItem("Tools/Reava_/Circle Placer")]
    public static void ShowWindow()
    {
        GetWindow<ReavaCirclePlacerWindow>("Circle Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Circle Placer", EditorStyles.boldLabel);

        GUILayout.Space(5);

        // Object list
        GUILayout.Label("Objects", EditorStyles.label);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(120));

        for (int i = 0; i < objects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            objects[i] = (GameObject)EditorGUILayout.ObjectField(objects[i], typeof(GameObject), true);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                objects.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Slot"))
        {
            objects.Add(null);
        }

        if (GUILayout.Button("Fill From Selection"))
        {
            objects = new List<GameObject>();
            foreach (var t in Selection.transforms)
            {
                objects.Add(t.gameObject);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        radius = EditorGUILayout.FloatField("Radius", radius);

        useSelectedAsCenter = EditorGUILayout.Toggle("Use Selected As Center", useSelectedAsCenter);

        if (!useSelectedAsCenter)
        {
            centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
        }

        faceCenter = EditorGUILayout.Toggle("Face Center", faceCenter);

        GUILayout.Space(10);

        if (GUILayout.Button("Arrange In Circle"))
        {
            Arrange();
        }
    }

    private void Arrange()
    {
        if (objects == null || objects.Count == 0) return;

        Vector3 center = centerPosition;

        if (useSelectedAsCenter && Selection.activeTransform != null)
        {
            center = Selection.activeTransform.position;
        }

        int count = objects.Count;
        float angleStep = 360f / count;

        Undo.RecordObjects(objects.ToArray(), "Arrange In Circle");

        for (int i = 0; i < count; i++)
        {
            if (objects[i] == null) continue;

            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            objects[i].transform.position = center + pos;

            if (faceCenter)
            {
                objects[i].transform.LookAt(center);
            }
        }
    }
}