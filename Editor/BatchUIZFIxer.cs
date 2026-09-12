#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Reava_
{
    public class BatchUIZFixer : EditorWindow
    {
        private GameObject _parent;

        [MenuItem("Tools/Reava_/Batch UI Z Fixer")]
        private static void OpenWindow()
        {
            GetWindow<BatchUIZFixer>("Batch UI Z Fixer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Batch UI Z Fixer",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Sets the local Z position of every RectTransform " +
                "under the selected parent to 0.\n\n" +
                "Only the Z axis is changed.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            _parent = (GameObject)EditorGUILayout.ObjectField(
                "Parent",
                _parent,
                typeof(GameObject),
                true
            );

            EditorGUILayout.Space();

            GUI.enabled = _parent != null;

            if (GUILayout.Button("Fix RectTransforms", GUILayout.Height(30)))
            {
                FixRectTransforms();
            }

            GUI.enabled = true;

            if (_parent == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a parent GameObject to begin.",
                    MessageType.Warning
                );
            }
        }

        private void FixRectTransforms()
        {
            RectTransform[] rectTransforms =
                _parent.GetComponentsInChildren<RectTransform>(true);

            int fixedCount = 0;

            Undo.SetCurrentGroupName("Batch UI Z Fixer");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (RectTransform rectTransform in rectTransforms)
            {
                Vector3 position = rectTransform.localPosition;

                if (Mathf.Approximately(position.z, 0f))
                    continue;

                Undo.RecordObject(
                    rectTransform,
                    "Batch UI Z Fixer"
                );

                position.z = 0f;
                rectTransform.localPosition = position;

                EditorUtility.SetDirty(rectTransform);

                fixedCount++;
            }

            Undo.CollapseUndoOperations(undoGroup);

            if (fixedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(
                    _parent.scene
                );
            }

            Debug.Log(
                $"[Batch UI Z Fixer] Fixed {fixedCount} RectTransform{(fixedCount == 1 ? "" : "s")}.",
                _parent
            );

            EditorUtility.DisplayDialog(
                "Batch UI Z Fixer",
                $"Fixed {fixedCount} RectTransform{(fixedCount == 1 ? "" : "s")}.",
                "OK"
            );
        }
    }
}

#endif