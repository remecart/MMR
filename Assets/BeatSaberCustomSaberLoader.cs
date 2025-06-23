using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BeatSaberCustomSaberLoader : MonoBehaviour
{
    private readonly static int CustomColor = Shader.PropertyToID("_CustomColors");

    [Header("Saber Directory")]
    [SerializeField]
    private string saberDirectoryPath = "C:/CustomSabers/";

    [Inject]
    private readonly LifetimeScope _scope;

    [AwakeInject]
    private readonly NoteColorConfig _noteColorConfig;

    [Header("Current Saber")]
    [SerializeField]
    public int currentSaberIndex = 0;

    [SerializeField]
    private string currentSaberName = "Default";

    [Header("Saber Settings")]
    [SerializeField]
    private Transform leftHandParent;

    [SerializeField]
    private Transform rightHandParent;

    [SerializeField]
    private Vector3 spawnPosition = Vector3.zero;

    [SerializeField]
    private bool autoLoadOnStart = true;

    [Header("Debug Info")]
    [SerializeField]
    private bool showDebugInfo = true;

    private List<string> saberBundlePaths = new List<string>();
    private GameObject currentLeftSaber;
    private GameObject currentRightSaber;
    private AssetBundle currentBundle;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    void Start()
    {
        if (autoLoadOnStart)
        {
            ScanForSaberBundles();
            if (saberBundlePaths.Count > 0)
            {
                LoadSaberAtIndex(currentSaberIndex);
            }
        }
    }

    void OnValidate()
    {
        // Index validieren
        if (saberBundlePaths.Count > 0)
        {
            currentSaberIndex = Mathf.Clamp(currentSaberIndex, 0, saberBundlePaths.Count - 1);
        }
    }

    [ContextMenu("Scan Directory")]
    public void ScanForSaberBundles()
    {
        saberBundlePaths.Clear();

        if (!Directory.Exists(saberDirectoryPath))
        {
            Debug.LogError($"❌ Verzeichnis nicht gefunden: {saberDirectoryPath}");

            return;
        }

        // Suche nach AssetBundle-Dateien (meist ohne Endung oder .bundle)
        string[] allFiles = Directory.GetFiles(saberDirectoryPath, "*", SearchOption.AllDirectories);

        foreach (string filePath in allFiles)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            // AssetBundles haben meist keine Endung oder .bundle/.saber
            if (string.IsNullOrEmpty(extension) || extension == ".bundle" || extension == ".saber")
            {
                saberBundlePaths.Add(filePath);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"🔍 {saberBundlePaths.Count} Saber-Bundles gefunden:");
        }

        for (int i = 0; i < saberBundlePaths.Count; i++)
        {
            string fileName = Path.GetFileNameWithoutExtension(saberBundlePaths[i]);
            if (showDebugInfo)
                Debug.Log($"  {i}: {fileName}");
        }

        // Index zurücksetzen wenn nötig
        if (currentSaberIndex >= saberBundlePaths.Count)
        {
            currentSaberIndex = 0;
        }

        UpdateCurrentSaberName();
    }

    public void LoadSaberAtIndex(int index)
    {
        if (saberBundlePaths.Count == 0)
        {
            Debug.LogWarning("⚠️ Keine Saber gefunden! Erst 'Scan Directory' ausführen.");

            return;
        }

        if (index < 0 || index >= saberBundlePaths.Count)
        {
            Debug.LogError($"❌ Ungültiger Index: {index} (0-{saberBundlePaths.Count - 1})");

            return;
        }

        currentSaberIndex = index;
        StartCoroutine(LoadSaberCoroutine(saberBundlePaths[index]));
    }

    public void NextSaber()
    {
        if (saberBundlePaths.Count == 0) return;

        int nextIndex = (currentSaberIndex + 1) % saberBundlePaths.Count;
        LoadSaberAtIndex(nextIndex);

        if (showDebugInfo)
            Debug.Log($"➡️ Nächster Saber: {nextIndex}/{saberBundlePaths.Count - 1}");
    }

    public void PreviousSaber()
    {
        if (saberBundlePaths.Count == 0) return;

        int prevIndex = (currentSaberIndex - 1 + saberBundlePaths.Count) % saberBundlePaths.Count;
        LoadSaberAtIndex(prevIndex);

        if (showDebugInfo)
            Debug.Log($"⬅️ Vorheriger Saber: {prevIndex}/{saberBundlePaths.Count - 1}");
    }

    IEnumerator LoadSaberCoroutine(string bundlePath)
    {
        if (showDebugInfo)
            Debug.Log($"🔄 Lade Saber: {Path.GetFileNameWithoutExtension(bundlePath)}");

        // Alten Saber entfernen
        UnloadCurrentSaber();

        // Bundle laden
        string bundleURL = "file://" + bundlePath.Replace("\\", "/");
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            currentBundle = DownloadHandlerAssetBundle.GetContent(request);

            // Erstes GameObject im Bundle finden und laden
            string[] assetNames = currentBundle.GetAllAssetNames();

            foreach (string assetName in assetNames)
            {
                Object asset = currentBundle.LoadAsset(assetName);

                if (asset is GameObject)
                {
                    GameObject prefab = asset as GameObject;

                    // Suche LeftSaber und RightSaber im Prefab
                    Transform leftSaberPrefab = FindChildRecursive(prefab.transform, "LeftSaber");
                    Transform rightSaberPrefab = FindChildRecursive(prefab.transform, "RightSaber");

                    if (leftSaberPrefab != null && rightSaberPrefab != null)
                    {
                        // LeftSaber instanziieren und an LeftHand anhängen
                        if (leftHandParent != null)
                        {
                            currentLeftSaber = Instantiate(leftSaberPrefab.gameObject);
                            if (currentLeftSaber != null && leftHandParent != null)
                            {
                                currentLeftSaber.transform.SetParent(leftHandParent);
                                currentLeftSaber.transform.localPosition = spawnPosition;
                                currentLeftSaber.transform.localRotation = Quaternion.identity;
                                currentLeftSaber.transform.localScale = new Vector3(12, 12, 12);
                                currentLeftSaber.transform.localPosition = new Vector3(0, 0, 2f);
                                currentLeftSaber.name = $"LeftSaber_{prefab.name}";
                                Renderer[] renderers = currentLeftSaber.GetComponentsInChildren<Renderer>();
                                foreach (Renderer renderer in renderers)
                                {
                                    foreach (var mat in renderer.materials)
                                    {
                                        if (mat.HasProperty(CustomColor) && mat.GetFloat(CustomColor) == 1)
                                        {
                                            // Prüfe, ob es sich um das linke oder rechte Schwert handelt
                                            bool isLeftSaber = currentLeftSaber != null &&
                                                               renderer.gameObject.transform.IsChildOf(currentLeftSaber.transform);
                                            mat.color = isLeftSaber ? _noteColorConfig.LeftColor : _noteColorConfig.RightColor;
                                        }
                                    }
                                }
                            }
                        }

                        // RightSaber instanziieren und an RightHand anhängen
                        if (rightHandParent != null)
                        {
                            currentRightSaber = Instantiate(rightSaberPrefab.gameObject);
                            if (currentRightSaber != null && rightHandParent != null)
                            {
                                currentRightSaber.transform.SetParent(rightHandParent);
                                currentRightSaber.transform.localPosition = spawnPosition;
                                currentRightSaber.transform.localRotation = Quaternion.identity;
                                currentRightSaber.transform.localScale = new Vector3(12, 12, 12);
                                currentRightSaber.transform.localPosition = new Vector3(0, 0, 2f);
                                currentRightSaber.name = $"RightSaber_{prefab.name}";

                                // Iterate over all the materials in the RightSaber + Children
                                Renderer[] renderers = currentRightSaber.GetComponentsInChildren<Renderer>();
                                foreach (Renderer renderer in renderers)
                                {
                                    foreach (var mat in renderer.materials)
                                    {
                                        if (mat.HasProperty(CustomColor) && mat.GetFloat(CustomColor) == 1)
                                        {
                                            // Prüfe, ob es sich um das linke oder rechte Schwert handelt
                                            bool isLeftSaber = currentLeftSaber != null &&
                                                               renderer.gameObject.transform.IsChildOf(currentLeftSaber.transform);
                                            mat.color = isLeftSaber ? _noteColorConfig.LeftColor : _noteColorConfig.RightColor;
                                        }
                                    }
                                }
                            }
                        }

                        UpdateCurrentSaberName();

                        if (showDebugInfo)
                        {
                            Debug.Log($"✅ Saber geladen: {currentSaberName}");
                            if (currentLeftSaber != null && leftHandParent != null)
                                Debug.Log($"👈 LeftSaber angehängt an: {leftHandParent.name}");
                            if (currentRightSaber != null && rightHandParent != null)
                                Debug.Log($"👉 RightSaber angehängt an: {rightHandParent.name}");
                        }

                        #if UNITY_EDITOR
                        if (currentLeftSaber != null)
                            UnityEditor.Selection.activeGameObject = currentLeftSaber;
                        #endif
                    }
                    else
                    {
                        Debug.LogError($"❌ LeftSaber oder RightSaber nicht im Prefab gefunden!");
                        if (showDebugInfo)
                        {
                            Debug.Log("Verfügbare Kinder:");
                            LogAllChildren(prefab.transform, 0);
                        }
                    }

                    break; // Nur das erste GameObject analysieren
                }
            }
        }
        else
        {
            Debug.LogError($"❌ Fehler beim Laden: {request.error}");
        }
    }

    void UnloadCurrentSaber()
    {
        // LeftSaber zerstören
        if (currentLeftSaber != null)
        {
            if (showDebugInfo)
                Debug.Log($"🗑️ Entferne LeftSaber: {currentLeftSaber.name}");

            try
            {
                DestroyImmediate(currentLeftSaber);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Fehler beim Zerstören von LeftSaber: {e.Message}");
            }
            finally
            {
                currentLeftSaber = null;
            }
        }

        // RightSaber zerstören
        if (currentRightSaber != null)
        {
            if (showDebugInfo)
                Debug.Log($"🗑️ Entferne RightSaber: {currentRightSaber.name}");

            try
            {
                DestroyImmediate(currentRightSaber);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Fehler beim Zerstören von RightSaber: {e.Message}");
            }
            finally
            {
                currentRightSaber = null;
            }
        }

        // Bundle freigeben
        if (currentBundle != null)
        {
            try
            {
                currentBundle.Unload(true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Fehler beim Entladen des Bundles: {e.Message}");
            }
            finally
            {
                currentBundle = null;
            }
        }
    }

    void OrganizeSaberChildren(GameObject saberRoot)
    {
        // Diese Funktion wird nicht mehr benötigt - jetzt direkt extrahiert
    }

    void LogAllChildren(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}- {parent.name}");

        for (int i = 0; i < parent.childCount; i++)
        {
            LogAllChildren(parent.GetChild(i), depth + 1);
        }
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        // Direkte Kinder durchsuchen
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name.Contains(childName))
                return child;
        }

        // Rekursiv in Kindern suchen
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), childName);

            if (found != null)
                return found;
        }

        return null;
    }

    void UpdateCurrentSaberName()
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

    // Keyboard Controls (optional)
    void Update()
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

    void OnDestroy()
    {
        UnloadCurrentSaber();
    }

    // Public API für andere Scripts
    public int GetSaberCount() => saberBundlePaths.Count;
    public string GetCurrentSaberName() => currentSaberName;
    public GameObject GetCurrentLeftSaber() => currentLeftSaber;
    public GameObject GetCurrentRightSaber() => currentRightSaber;
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