using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class LoadModelsByPrefix : EditorWindow
{
    string folderName = "MyFolder"; // just folder name, not full path
    string prefix = "ezgif-frame-";

    [MenuItem("Tools/Load Models By Prefix")]
    public static void ShowWindow()
    {
        GetWindow<LoadModelsByPrefix>("Load Models By Prefix");
    }

    void OnGUI()
    {
        GUILayout.Label("Load models with prefix from folders named:", EditorStyles.boldLabel);

        folderName = EditorGUILayout.TextField("Folder Name", folderName);
        prefix = EditorGUILayout.TextField("Filename Prefix", prefix);

        if (GUILayout.Button("Load Models"))
        {
            LoadModels();
        }
    }

    void LoadModels()
    {
        if (string.IsNullOrEmpty(folderName))
        {
            Debug.LogError("Folder name cannot be empty.");
            return;
        }

        List<string> matchingFolders = new List<string>();
        FindFoldersRecursive("Assets", folderName, matchingFolders);

        if (matchingFolders.Count == 0)
        {
            Debug.LogWarning($"No folders named '{folderName}' found under Assets/");
            return;
        }

        int totalLoaded = 0;
        foreach (string folder in matchingFolders)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefix} t:GameObject", new[] { folder });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string filename = Path.GetFileNameWithoutExtension(assetPath);

                // Skip if the filename contains "detail" (case-insensitive)
                if (filename.ToLower().Contains("detail"))
                {
                    continue;
                }

                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (modelPrefab != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                    instance.transform.position = Vector3.zero;
                    Undo.RegisterCreatedObjectUndo(instance, "Instantiate Model");
                    Debug.Log("Instantiated: " + assetPath);
                    totalLoaded++;
                }
            }
        }

        if (totalLoaded == 0)
        {
            Debug.LogWarning($"No models with prefix '{prefix}' (excluding 'detail') found in folders named '{folderName}'.");
        }
        else
        {
            Debug.Log($"Loaded {totalLoaded} models from folders named '{folderName}' excluding 'detail'.");
        }
    }


    void FindFoldersRecursive(string startPath, string targetFolderName, List<string> results)
    {
        string[] subFolders = AssetDatabase.GetSubFolders(startPath);

        foreach (var folder in subFolders)
        {
            if (Path.GetFileName(folder).Equals(targetFolderName))
            {
                results.Add(folder);
            }
            // recurse into subfolder
            FindFoldersRecursive(folder, targetFolderName, results);
        }
    }
}
