using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages loading and switching of custom Beat Saber sabers from AssetBundles.
/// Supports automatic scanning, color customization, and runtime switching.
/// </summary>
public class BeatSaberCustomSaberLoader : MonoBehaviour
{
    #region Constants
    private static readonly int CustomColorPropertyId = Shader.PropertyToID("_CustomColors");
    private const string LeftSaberName = "LeftSaber";
    private const string RightSaberName = "RightSaber";
    private const float DefaultSaberScale = 16f;
    private const float DefaultSaberZOffset = 2f;
    #endregion

    #region Serialized Fields
    [Header("Saber Directory")]
    [SerializeField, Tooltip("Path to the directory containing custom saber bundles")]
    private string saberDirectoryPath = "C:/CustomSabers/";

    [Header("Current Saber")]
    [SerializeField, Tooltip("Index of the currently selected saber")]
    public int currentSaberIndex = 0;

    [SerializeField]
    public string currentSaberName = "Default";

    [Header("Saber Settings")]
    [SerializeField, Tooltip("Parent transform for the left hand saber")]
    private Transform leftHandParent;

    [SerializeField, Tooltip("Parent transform for the right hand saber")]
    private Transform rightHandParent;

    [SerializeField, Tooltip("Local spawn position for sabers")]
    private Vector3 spawnPosition = Vector3.zero;

    [SerializeField, Tooltip("Automatically load first saber on Start")]
    private bool autoLoadOnStart = true;

    [Header("Debug Settings")]
    [SerializeField, Tooltip("Show debug information in console")]
    private bool showDebugInfo = true;
    #endregion

    #region Dependencies
    [Inject]
    private readonly LifetimeScope scope;

    [AwakeInject]
    private readonly NoteColorConfig _noteColorConfig;
    #endregion

    #region Private Fields
    private List<string> saberBundlePaths = new List<string>();
    private GameObject currentLeftSaber;
    private GameObject currentRightSaber;
    private AssetBundle currentBundle;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        AwakeInjector.InjectInto(this, scope);
    }

    private void Start()
    {
        if (autoLoadOnStart)
        {
            InitializeSabers();
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    private void OnValidate()
    {
        ValidateCurrentIndex();
    }

    private void OnDestroy()
    {
        UnloadCurrentSaber();
    }
    #endregion

    #region Public API
    /// <summary>
    /// Gets the total number of available sabers.
    /// </summary>
    public int GetSaberCount() => saberBundlePaths.Count;

    /// <summary>
    /// Gets the name of the currently loaded saber.
    /// </summary>
    public string GetCurrentSaberName() => currentSaberName;

    /// <summary>
    /// Gets the current left saber GameObject.
    /// </summary>
    public GameObject GetCurrentLeftSaber() => currentLeftSaber;

    /// <summary>
    /// Gets the current right saber GameObject.
    /// </summary>
    public GameObject GetCurrentRightSaber() => currentRightSaber;

    /// <summary>
    /// Scans the saber directory for available saber bundles.
    /// </summary>
    [ContextMenu("Scan Directory")]
    public void ScanForSaberBundles()
    {
        LogDebug("🔍 Scanning for saber bundles...");
        
        saberBundlePaths.Clear();

        if (!ValidateDirectory())
        {
            return;
        }

        CollectSaberBundles();
        LogFoundSabers();
        ValidateCurrentIndex();
        UpdateCurrentSaberName();
    }

    /// <summary>
    /// Loads a saber at the specified index.
    /// </summary>
    public void LoadSaberAtIndex(int index)
    {
        if (!ValidateLoadRequest(index))
        {
            return;
        }

        currentSaberIndex = index;
        StartCoroutine(LoadSaberCoroutine(saberBundlePaths[index]));
    }

    /// <summary>
    /// Switches to the next available saber.
    /// </summary>
    public void NextSaber()
    {
        if (saberBundlePaths.Count == 0) return;

        int nextIndex = (currentSaberIndex + 1) % saberBundlePaths.Count;
        LoadSaberAtIndex(nextIndex);
        LogDebug($"➡️ Next saber: {nextIndex}/{saberBundlePaths.Count - 1}");
    }

    /// <summary>
    /// Switches to the previous available saber.
    /// </summary>
    public void PreviousSaber()
    {
        if (saberBundlePaths.Count == 0) return;

        int prevIndex = (currentSaberIndex - 1 + saberBundlePaths.Count) % saberBundlePaths.Count;
        LoadSaberAtIndex(prevIndex);
        LogDebug($"⬅️ Previous saber: {prevIndex}/{saberBundlePaths.Count - 1}");
    }
    #endregion

    #region Private Methods - Initialization
    private void InitializeSabers()
    {
        ScanForSaberBundles();
        if (saberBundlePaths.Count > 0)
        {
            LoadSaberAtIndex(currentSaberIndex);
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousSaber();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextSaber();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            LoadSaberAtIndex(currentSaberIndex); // Reload current
        }
    }
    #endregion

    #region Private Methods - Validation
    private bool ValidateDirectory()
    {
        if (!Directory.Exists(saberDirectoryPath))
        {
            Debug.LogError($"❌ Directory not found: {saberDirectoryPath}");
            return false;
        }
        return true;
    }

    private bool ValidateLoadRequest(int index)
    {
        if (saberBundlePaths.Count == 0)
        {
            Debug.LogWarning("⚠️ No sabers found! Run 'Scan Directory' first.");
            return false;
        }

        if (index < 0 || index >= saberBundlePaths.Count)
        {
            Debug.LogError($"❌ Invalid index: {index} (valid range: 0-{saberBundlePaths.Count - 1})");
            return false;
        }

        return true;
    }

    private void ValidateCurrentIndex()
    {
        if (saberBundlePaths.Count > 0)
        {
            currentSaberIndex = Mathf.Clamp(currentSaberIndex, 0, saberBundlePaths.Count - 1);
        }
    }
    #endregion

    #region Private Methods - File Operations
    private void CollectSaberBundles()
    {
        string[] allFiles = Directory.GetFiles(saberDirectoryPath, "*", SearchOption.AllDirectories);

        foreach (string filePath in allFiles)
        {
            if (IsSaberBundle(filePath))
            {
                saberBundlePaths.Add(filePath);
            }
        }
    }

    private bool IsSaberBundle(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        
        // AssetBundles typically have no extension or use .bundle/.saber
        return string.IsNullOrEmpty(extension) || 
               extension == ".bundle" || 
               extension == ".saber";
    }
    #endregion

    #region Private Methods - Saber Loading
    private IEnumerator LoadSaberCoroutine(string bundlePath)
    {
        string saberName = Path.GetFileNameWithoutExtension(bundlePath);
        LogDebug($"🔄 Loading saber: {saberName}");

        UnloadCurrentSaber();

        yield return StartCoroutine(LoadAssetBundle(bundlePath));

        if (currentBundle != null)
        {
            ProcessLoadedBundle();
        }
    }

    private IEnumerator LoadAssetBundle(string bundlePath)
    {
        string bundleURL = "file://" + bundlePath.Replace("\\", "/");
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            currentBundle = DownloadHandlerAssetBundle.GetContent(request);
        }
        else
        {
            Debug.LogError($"❌ Failed to load bundle: {request.error}");
        }
    }

    private void ProcessLoadedBundle()
    {
        string[] assetNames = currentBundle.GetAllAssetNames();

        foreach (string assetName in assetNames)
        {
            Object asset = currentBundle.LoadAsset(assetName);

            if (asset is GameObject prefab)
            {
                if (ProcessSaberPrefab(prefab))
                {
                    break; // Only process the first valid GameObject
                }
            }
        }
    }

    private bool ProcessSaberPrefab(GameObject prefab)
    {
        Transform leftSaberPrefab = FindChildRecursive(prefab.transform, LeftSaberName);
        Transform rightSaberPrefab = FindChildRecursive(prefab.transform, RightSaberName);

        if (leftSaberPrefab == null || rightSaberPrefab == null)
        {
            LogSaberNotFound(prefab);
            return false;
        }

        InstantiateSabers(leftSaberPrefab, rightSaberPrefab, prefab.name);
        UpdateCurrentSaberName();
        LogSuccessfulLoad();

#if UNITY_EDITOR
        if (currentLeftSaber != null)
        {
            UnityEditor.Selection.activeGameObject = currentLeftSaber;
        }
#endif

        return true;
    }

    private void InstantiateSabers(Transform leftSaberPrefab, Transform rightSaberPrefab, string prefabName)
    {
        InstantiateLeftSaber(leftSaberPrefab, prefabName);
        InstantiateRightSaber(rightSaberPrefab, prefabName);
    }

    private void InstantiateLeftSaber(Transform leftSaberPrefab, string prefabName)
    {
        if (leftHandParent == null) return;

        currentLeftSaber = Instantiate(leftSaberPrefab.gameObject);
        ConfigureSaber(currentLeftSaber, leftHandParent, $"LeftSaber_{prefabName}", true);
    }

    private void InstantiateRightSaber(Transform rightSaberPrefab, string prefabName)
    {
        if (rightHandParent == null) return;

        currentRightSaber = Instantiate(rightSaberPrefab.gameObject);
        ConfigureSaber(currentRightSaber, rightHandParent, $"RightSaber_{prefabName}", false);
    }

    private void ConfigureSaber(GameObject saber, Transform parent, string saberName, bool isLeftSaber)
    {
        if (saber == null || parent == null) return;

        saber.transform.SetParent(parent);
        saber.transform.localPosition = new Vector3(spawnPosition.x, spawnPosition.y, DefaultSaberZOffset);
        saber.transform.localRotation = Quaternion.identity;
        saber.transform.localScale = Vector3.one * DefaultSaberScale;
        saber.name = saberName;

        if (isLeftSaber)
        {
            _leftSaber = saber;
        }
        else
        {
            _rightSaber = saber;
        }

        ApplySaberColors(saber, isLeftSaber);
    }
    
    [AwakeInject]
    private ConfigLoader _configLoader;

    private void FixedUpdate()
    {
        foreach (var config in _configLoader.GetAll())
        {
            var type = config.GetType();
            if (_configLoader.IsChanged(type))
            {
                ApplySaberColors(_leftSaber, true);
                ApplySaberColors(_rightSaber, false);
            }
        }
    }

    public TrailRenderer LeftTrail;
    public TrailRenderer RightTrail;

    private GameObject _leftSaber;
    private GameObject _rightSaber;
    
    private void ApplySaberColors(GameObject saber, bool isLeftSaber)
    {
        Renderer[] renderers = saber.GetComponentsInChildren<Renderer>();
        Color targetColor = isLeftSaber ? _noteColorConfig.LeftColor : _noteColorConfig.RightColor;

        if (isLeftSaber)
        {
            LeftTrail.startColor = _noteColorConfig.LeftColor;
            LeftTrail.endColor = _noteColorConfig.LeftColor;
        }
        else
        {
            RightTrail.startColor = _noteColorConfig.RightColor;
            RightTrail.endColor = _noteColorConfig.RightColor;
        }
        

        foreach (Renderer renderer in renderers)
        {
            ApplyColorToRenderer(renderer, targetColor);
        }
    }

    private void ApplyColorToRenderer(Renderer renderer, Color targetColor)
    {
        foreach (Material material in renderer.materials)
        {
            if (ShouldApplyCustomColor(material))
            {
                material.color = targetColor;
            }
        }
    }

    private bool ShouldApplyCustomColor(Material material)
    {
        return !material.HasProperty(CustomColorPropertyId) || 
               (material.HasProperty(CustomColorPropertyId) && material.GetFloat(CustomColorPropertyId) == 1);
    }
    #endregion

    #region Private Methods - Cleanup
    private void UnloadCurrentSaber()
    {
        DestroySaber(ref currentLeftSaber, "LeftSaber");
        DestroySaber(ref currentRightSaber, "RightSaber");
        UnloadBundle();
    }

    private void DestroySaber(ref GameObject saber, string saberType)
    {
        if (saber == null) return;

        LogDebug($"🗑️ Removing {saberType}: {saber.name}");

        try
        {
            DestroyImmediate(saber);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error destroying {saberType}: {e.Message}");
        }
        finally
        {
            saber = null;
        }
    }

    private void UnloadBundle()
    {
        if (currentBundle == null) return;

        try
        {
            currentBundle.Unload(true);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error unloading bundle: {e.Message}");
        }
        finally
        {
            currentBundle = null;
        }
    }
    #endregion

    #region Private Methods - Utilities
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        // Search direct children first
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.Contains(childName))
            {
                return child;
            }
        }

        // Search recursively in children
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void UpdateCurrentSaberName()
    {
        if (saberBundlePaths.Count > 0 && currentSaberIndex < saberBundlePaths.Count)
        {
            currentSaberName = Path.GetFileNameWithoutExtension(saberBundlePaths[currentSaberIndex]);
        }
        else
        {
            currentSaberName = "None";
        }
    }
    #endregion

    #region Private Methods - Logging
    private void LogDebug(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log(message);
        }
    }

    private void LogFoundSabers()
    {
        LogDebug($"🔍 Found {saberBundlePaths.Count} saber bundles:");
        
        for (int i = 0; i < saberBundlePaths.Count; i++)
        {
            string fileName = Path.GetFileNameWithoutExtension(saberBundlePaths[i]);
            LogDebug($"  {i}: {fileName}");
        }
    }

    private void LogSaberNotFound(GameObject prefab)
    {
        Debug.LogError($"❌ {LeftSaberName} or {RightSaberName} not found in prefab!");
        
        if (showDebugInfo)
        {
            Debug.Log("Available children:");
            LogAllChildren(prefab.transform, 0);
        }
    }

    private void LogSuccessfulLoad()
    {
        if (!showDebugInfo) return;

        Debug.Log($"✅ Saber loaded: {currentSaberName}");
        
        if (currentLeftSaber != null && leftHandParent != null)
        {
            Debug.Log($"👈 LeftSaber attached to: {leftHandParent.name}");
        }
        
        if (currentRightSaber != null && rightHandParent != null)
        {
            Debug.Log($"👉 RightSaber attached to: {rightHandParent.name}");
        }
    }

    private void LogAllChildren(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}- {parent.name}");

        for (int i = 0; i < parent.childCount; i++)
        {
            LogAllChildren(parent.GetChild(i), depth + 1);
        }
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(BeatSaberCustomSaberLoader))]
public class BeatSaberCustomSaberLoaderEditor : Editor
{
    private BeatSaberCustomSaberLoader loader;

    void OnEnable()
    {
        loader = (BeatSaberCustomSaberLoader)target;
    }

    public override void OnInspectorGUI()
    {
        // Standard Inspector zeichnen
        DrawDefaultInspector();

        EditorGUILayout.Space(20);

        // Schönes GUI für Controls
        EditorGUILayout.BeginVertical("box");

        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = new Color(0.3f, 0.8f, 1f);
        EditorGUILayout.LabelField("🗡️ Beat Saber Controls", headerStyle);

        EditorGUILayout.Space(10);

        // Directory Scan Button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("🔍 Scan Saber Directory", GUILayout.Height(30)))
        {
            loader.ScanForSaberBundles();
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        // Info Box
        if (loader.GetSaberCount() > 0)
        {
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField($"📦 Gefundene Saber: {loader.GetSaberCount()}");
            EditorGUILayout.LabelField($"🎯 Aktueller Saber: {loader.GetCurrentSaberName()}");

            // Parent Info anzeigen
            var leftHandParent = serializedObject.FindProperty("leftHandParent").objectReferenceValue as Transform;
            var rightHandParent = serializedObject.FindProperty("rightHandParent").objectReferenceValue as Transform;

            if (leftHandParent != null && rightHandParent != null)
            {
                EditorGUILayout.LabelField($"👈 Left Hand: {leftHandParent.name}");
                EditorGUILayout.LabelField($"👉 Right Hand: {rightHandParent.name}");
            }
            else
            {
                EditorGUILayout.LabelField("⚠️ Left/Right Hand Parents nicht gesetzt!", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Keine Saber gefunden! Verzeichnis scannen.", MessageType.Warning);
        }

        EditorGUILayout.Space(10);

        // Navigation Buttons
        EditorGUILayout.BeginHorizontal();

        // Previous Button
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        GUI.enabled = loader.GetSaberCount() > 0;
        if (GUILayout.Button("⬅️ Previous", GUILayout.Height(25)))
        {
            loader.PreviousSaber();
        }

        // Reload Button
        GUI.backgroundColor = new Color(1f, 1f, 0.6f);
        if (GUILayout.Button("🔄 Reload", GUILayout.Height(25)))
        {
            loader.LoadSaberAtIndex(loader.GetComponent<BeatSaberCustomSaberLoader>().currentSaberIndex);
        }

        // Next Button
        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("Next ➡️", GUILayout.Height(25)))
        {
            loader.NextSaber();
        }

        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Quick Load Buttons
        if (loader.GetSaberCount() > 0)
        {
            EditorGUILayout.LabelField("⚡ Quick Load:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            // Zeige erste 5 Saber als Buttons
            int maxButtons = Mathf.Min(5, loader.GetSaberCount());
            for (int i = 0; i < maxButtons; i++)
            {
                string buttonText = $"{i}";

                // Aktueller Saber bekommt andere Farbe
                if (i == loader.GetComponent<BeatSaberCustomSaberLoader>().currentSaberIndex)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(buttonText, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    loader.LoadSaberAtIndex(i);
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (loader.GetSaberCount() > 5)
            {
                EditorGUILayout.LabelField($"... und {loader.GetSaberCount() - 5} weitere", EditorStyles.miniLabel);
            }
        }

        EditorGUILayout.Space(10);

        // Keyboard Hints
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("⌨️ Keyboard Shortcuts:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("← → Pfeiltasten: Saber wechseln");
        EditorGUILayout.LabelField("R: Aktuellen Saber neu laden");
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        // Auto-Update bei Änderungen
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif