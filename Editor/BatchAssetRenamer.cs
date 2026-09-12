#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class BatchAssetRenamer : EditorWindow
{
    private DefaultAsset selectedFolder;

    private string matchString = "";
    private string replaceString = "";
    private string prefix = "";
    private string suffix = "";

    private bool includeSubfolders = false;

    private Vector2 scrollPosition;

    private List<RenameEntry> renameEntries = new List<RenameEntry>();

    private class RenameEntry
    {
        public string assetPath;
        public string oldName;
        public string newName;

        public RenameEntry(string assetPath, string oldName, string newName)
        {
            this.assetPath = assetPath;
            this.oldName = oldName;
            this.newName = newName;
        }
    }

    [MenuItem("Tools/Reava_/Batch Asset Renamer")]
    public static void ShowWindow()
    {
        GetWindow<BatchAssetRenamer>("Batch Asset Renamer");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Batch Asset Renamer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Rename assets in a folder.\n\n" +
            "Icon_3 → Potions_Blue_3.png",
            MessageType.Info
        );

        EditorGUILayout.Space(8);

        // Folder
        EditorGUILayout.LabelField("Folder", EditorStyles.boldLabel);

        selectedFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            selectedFolder,
            typeof(DefaultAsset),
            false
        );

        includeSubfolders = EditorGUILayout.Toggle(
            "Include Subfolders",
            includeSubfolders
        );

        EditorGUILayout.Space(8);

        // Match / Replace
        EditorGUILayout.LabelField("Rename", EditorStyles.boldLabel);

        matchString = EditorGUILayout.TextField(
            "Match",
            matchString
        );

        replaceString = EditorGUILayout.TextField(
            "Replace With",
            replaceString
        );

        prefix = EditorGUILayout.TextField("Prefix", prefix);
        suffix = EditorGUILayout.TextField("Suffix", suffix);

        EditorGUILayout.Space(8);

        using (new EditorGUI.DisabledScope(
            selectedFolder == null ||
            string.IsNullOrEmpty(matchString)
        ))
        {
            if (GUILayout.Button("Preview Rename", GUILayout.Height(30)))
            {
                GeneratePreview();
            }
        }

        EditorGUILayout.Space(10);

        // Preview
        if (renameEntries.Count > 0)
        {
            EditorGUILayout.LabelField(
                "Preview (" + renameEntries.Count + " files)",
                EditorStyles.boldLabel
            );

            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.MinHeight(200)
            );

            foreach (RenameEntry entry in renameEntries)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(
                    entry.oldName,
                    GUILayout.MinWidth(200)
                );

                EditorGUILayout.LabelField(
                    "→",
                    GUILayout.Width(20)
                );

                EditorGUILayout.LabelField(
                    entry.newName,
                    GUILayout.MinWidth(200)
                );

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);

            using (new EditorGUI.DisabledScope(HasConflicts()))
            {
                if (GUILayout.Button(
                    "Rename " + renameEntries.Count + " Files",
                    GUILayout.Height(35)
                ))
                {
                    RenameFiles();
                }
            }

            if (HasConflicts())
            {
                EditorGUILayout.HelpBox(
                    "Some files have conflicting destination names. " +
                    "Rename cannot proceed until the conflicts are resolved.",
                    MessageType.Error
                );
            }
        }
        else if (!string.IsNullOrEmpty(matchString))
        {
            EditorGUILayout.HelpBox(
                "No matching files found.",
                MessageType.Info
            );
        }
    }

    private void GeneratePreview()
    {
        renameEntries.Clear();

        string folderPath = AssetDatabase.GetAssetPath(selectedFolder);

        if (string.IsNullOrEmpty(folderPath) ||
            !AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog(
                "Invalid Folder",
                "Please select a valid Unity project folder.",
                "OK"
            );

            return;
        }

        string absolutePath = Path.GetFullPath(folderPath);

        SearchOption searchOption = includeSubfolders
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        string[] files = Directory.GetFiles(
            absolutePath,
            "*",
            searchOption
        );

        foreach (string absoluteFilePath in files)
        {
            // Ignore Unity metadata files.
            if (absoluteFilePath.EndsWith(".meta"))
                continue;

            string assetPath = absoluteFilePath
                .Replace("\\", "/");

            int assetsIndex = assetPath.IndexOf("Assets/");

            if (assetsIndex < 0)
                continue;

            assetPath = assetPath.Substring(assetsIndex);

            string fileName = Path.GetFileName(assetPath);

            if (!fileName.Contains(matchString))
                continue;

            string extension = Path.GetExtension(fileName);
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string newFileName = prefix + nameWithoutExtension.Replace(matchString,replaceString) + suffix + extension;

            // Nothing actually changed.
            if (fileName == newFileName)
                continue;

            string directory = Path.GetDirectoryName(assetPath)
                .Replace("\\", "/");

            string newAssetPath = directory + "/" + newFileName;

            renameEntries.Add(
                new RenameEntry(
                    assetPath,
                    fileName,
                    newFileName
                )
            );
        }
    }

    private bool HasConflicts()
    {
        HashSet<string> destinationPaths =
            new HashSet<string>();

        foreach (RenameEntry entry in renameEntries)
        {
            string directory = Path.GetDirectoryName(entry.assetPath)
                .Replace("\\", "/");

            string destinationPath =
                directory + "/" + entry.newName;

            // Two files are being renamed to the same name.
            if (!destinationPaths.Add(destinationPath))
                return true;

            // A file with the destination name already exists
            // and isn't itself one of the files we're renaming.
            if (File.Exists(
                Path.GetFullPath(destinationPath)
            ))
            {
                bool destinationIsBeingRenamed = false;

                foreach (RenameEntry other in renameEntries)
                {
                    if (other.assetPath == destinationPath)
                    {
                        destinationIsBeingRenamed = true;
                        break;
                    }
                }

                if (!destinationIsBeingRenamed)
                    return true;
            }
        }

        return false;
    }

    private void RenameFiles()
    {
        if (renameEntries.Count == 0)
            return;

        bool confirmed = EditorUtility.DisplayDialog(
            "Confirm Rename",
            "Rename " + renameEntries.Count +
            " assets?\n\n" +
            matchString +
            " → " +
            replaceString,
            "Rename",
            "Cancel"
        );

        if (!confirmed)
            return;

        AssetDatabase.StartAssetEditing();

        int successCount = 0;
        List<string> errors = new List<string>();

        try
        {
            foreach (RenameEntry entry in renameEntries)
            {
                string directory = Path.GetDirectoryName(
                    entry.assetPath
                ).Replace("\\", "/");

                string newAssetPath =
                    directory + "/" + entry.newName;

                string error = AssetDatabase.MoveAsset(
                    entry.assetPath,
                    newAssetPath
                );

                if (string.IsNullOrEmpty(error))
                {
                    successCount++;
                }
                else
                {
                    errors.Add(
                        entry.oldName + ": " + error
                    );
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        renameEntries.Clear();

        if (errors.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Rename Complete",
                "Successfully renamed " +
                successCount +
                " assets.",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Rename Finished With Errors",
                "Successfully renamed: " +
                successCount +
                "\nErrors: " +
                errors.Count +
                "\n\n" +
                string.Join("\n", errors),
                "OK"
            );
        }
    }
}

#endif