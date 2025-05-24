using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FrameSliderEditor : EditorWindow
{
    private string prefix = "frame_";
    private int minFrame = 0;
    private int maxFrame = 99;
    private int currentFrame = 0;
    private List<GameObject> frames = new List<GameObject>();
    private bool isPlaying = false;
    private float frameRate = 24f;
    private double lastUpdateTime = 0;

    private const string PrefKeyPrefix = "FrameSliderEditor_Prefix";
    private const string PrefKeyMin = "FrameSliderEditor_MinFrame";
    private const string PrefKeyMax = "FrameSliderEditor_MaxFrame";

    [MenuItem("Tools/Frame Slider")]
    public static void ShowWindow()
    {
        GetWindow<FrameSliderEditor>("Frame Slider");
    }

    private void OnEnable()
    {
        LoadPrefs();
        LoadFrames();
        ShowOnlyCurrentFrame();
        lastUpdateTime = EditorApplication.timeSinceStartup;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Frame Slider", EditorStyles.boldLabel);

        prefix = EditorGUILayout.TextField("Frame Prefix", prefix);

        minFrame = EditorGUILayout.IntField("Min Frame", minFrame);
        maxFrame = EditorGUILayout.IntField("Max Frame", maxFrame);

        // Clamp to valid range and prevent negative
        minFrame = Mathf.Max(0, minFrame);
        maxFrame = Mathf.Max(minFrame, maxFrame);

        if (GUILayout.Button("Save Settings"))
        {
            SavePrefs();
            LoadFrames();
            currentFrame = Mathf.Clamp(currentFrame, minFrame, maxFrame);
            ShowOnlyCurrentFrame();
        }

        if (!isPlaying && frames.Count > 0)
        {
            int newFrame = EditorGUILayout.IntSlider("Frame", currentFrame, minFrame, maxFrame);
            if (newFrame != currentFrame)
            {
                currentFrame = newFrame;
                ShowOnlyCurrentFrame();
            }
        }
        else if (frames.Count == 0)
        {
            EditorGUILayout.HelpBox("No frames found. Click Save Settings.", MessageType.Warning);
        }

        if (GUILayout.Button("Reload Frames"))
        {
            LoadFrames();
            ShowOnlyCurrentFrame();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(isPlaying ? "Pause" : "Play"))
        {
            isPlaying = !isPlaying;
            lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        frameRate = EditorGUILayout.FloatField("Frame Rate (fps):", frameRate);

        EditorGUILayout.EndHorizontal();
    }

    private void LoadFrames()
    {
        frames.Clear();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = minFrame; i <= maxFrame; i++)
        {
            string targetName = $"{prefix}{i:000}";
            GameObject frame = null;

            foreach (GameObject obj in allObjects)
            {
                if (!obj.scene.IsValid()) continue;

                if (obj.name == targetName)
                {
                    frame = obj;
                    break;
                }
            }

            if (frame != null)
            {
                frames.Add(frame);
            }
            else
            {
                Debug.LogWarning($"Frame object not found: {targetName}");
            }
        }
    }

    private void ShowOnlyCurrentFrame()
    {
        for (int i = 0; i < frames.Count; i++)
        {
            if (frames[i] != null)
                frames[i].SetActive(i == currentFrame - minFrame);
        }
    }

    private void SetupMaterialForOpaque(Material mat)
    {
        if (mat.shader.name == "Standard")
        {
            mat.SetFloat("_Mode", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
            Color c = mat.color;
            c.a = 1f;
            mat.color = c;
        }
    }

    private void OnInspectorUpdate()
    {
        UpdatePlayback();
    }

    private void UpdatePlayback()
    {
        if (!isPlaying) return;

        double currentTime = EditorApplication.timeSinceStartup;
        double elapsed = currentTime - lastUpdateTime;

        if (elapsed >= 1.0 / frameRate)
        {
            lastUpdateTime = currentTime;
            currentFrame++;
            if (currentFrame > maxFrame) currentFrame = minFrame;
            ShowOnlyCurrentFrame();
            Repaint();
        }
    }

    private void SavePrefs()
    {
        EditorPrefs.SetString(PrefKeyPrefix, prefix);
        EditorPrefs.SetInt(PrefKeyMin, minFrame);
        EditorPrefs.SetInt(PrefKeyMax, maxFrame);
    }

    private void LoadPrefs()
    {
        prefix = EditorPrefs.GetString(PrefKeyPrefix, "frame_");
        minFrame = EditorPrefs.GetInt(PrefKeyMin, 0);
        maxFrame = EditorPrefs.GetInt(PrefKeyMax, 99);
    }
}
