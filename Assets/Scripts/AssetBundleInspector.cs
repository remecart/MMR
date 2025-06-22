using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class AssetBundleInspector : MonoBehaviour
{
    [Header("AssetBundle Settings")]
    [SerializeField] private string bundlePath = "file://C:/pfad/zu/deinem/bundle";
    
    [Header("Asset Loading")]
    [SerializeField] private string assetNameToLoad = "";
    [SerializeField] private bool organizeAsChildren = false;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    
    [Header("Debug Info")]
    [SerializeField] private bool autoListOnStart = true;
    
    [Header("Controls")]
    [SerializeField] private bool loadBundleButton;
    [SerializeField] private bool listAssetsButton;
    [SerializeField] private bool loadAssetButton;
    
    private AssetBundle currentBundle;
    private List<string> assetNames = new List<string>();
    
    void Start()
    {
        if (autoListOnStart)
        {
            StartCoroutine(LoadAndListAssets());
        }
    }
    
    [ContextMenu("Load AssetBundle")]
    public void LoadAssetBundle()
    {
        StartCoroutine(LoadAndListAssets());
    }
    
    [ContextMenu("List Assets")]
    public void ListAssets()
    {
        if (currentBundle != null)
        {
            ListBundleContents();
        }
        else
        {
            Debug.LogWarning("Kein AssetBundle geladen!");
        }
    }
    
    [ContextMenu("Load Selected Asset")]
    public void LoadSelectedAsset()
    {
        if (string.IsNullOrEmpty(assetNameToLoad))
        {
            Debug.LogWarning("Bitte Asset-Name eingeben!");
            return;
        }
        
        StartCoroutine(LoadSpecificAsset(assetNameToLoad));
    }
    
    IEnumerator LoadAndListAssets()
    {
        Debug.Log($"Lade AssetBundle von: {bundlePath}");
        
        // Altes Bundle freigeben falls vorhanden
        if (currentBundle != null)
        {
            currentBundle.Unload(true);
            currentBundle = null;
        }
        
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            currentBundle = DownloadHandlerAssetBundle.GetContent(request);
            Debug.Log("✅ AssetBundle erfolgreich geladen!");
            
            ListBundleContents();
        }
        else
        {
            Debug.LogError($"❌ Fehler beim Laden: {request.error}");
        }
    }
    
    void ListBundleContents()
    {
        if (currentBundle == null) return;
        
        // Alle Asset-Namen abrufen
        string[] allAssetNames = currentBundle.GetAllAssetNames();
        assetNames.Clear();
        assetNames.AddRange(allAssetNames);
        
        Debug.Log($"📦 AssetBundle Inhalt ({allAssetNames.Length} Assets):");
        Debug.Log("=====================================");
        
        for (int i = 0; i < allAssetNames.Length; i++)
        {
            string assetName = allAssetNames[i];
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetName);
            
            // Asset-Typ ermitteln
            Object asset = currentBundle.LoadAsset(assetName);
            string assetType = asset != null ? asset.GetType().Name : "Unknown";
            
            Debug.Log($"{i + 1:00}. [{assetType}] '{fileName}' (Vollständiger Pfad: {assetName})");
            
            // Wenn nur ein Asset vorhanden ist, automatisch den Namen setzen
            if (allAssetNames.Length == 1)
            {
                assetNameToLoad = fileName;
                Debug.Log($"💡 Asset-Name automatisch gesetzt: '{fileName}'");
            }
        }
        
        Debug.Log("=====================================");
        Debug.Log("💡 Tipp: Kopiere einen Asset-Namen in das 'Asset Name To Load' Feld und verwende 'Load Selected Asset'");
    }
    
    IEnumerator LoadSpecificAsset(string assetName)
    {
        if (currentBundle == null)
        {
            Debug.LogWarning("Erst AssetBundle laden!");
            yield break;
        }
        
        Debug.Log($"🔄 Lade Asset: {assetName}");
        
        // Versuche verschiedene Varianten des Namens
        Object asset = null;
        
        // 1. Exakter Name
        asset = currentBundle.LoadAsset(assetName);
        
        // 2. Mit Dateierweiterung falls nicht gefunden
        if (asset == null)
        {
            foreach (string fullName in assetNames)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(fullName).Equals(assetName, System.StringComparison.OrdinalIgnoreCase))
                {
                    asset = currentBundle.LoadAsset(fullName);
                    assetName = fullName;
                    break;
                }
            }
        }
        
        if (asset != null)
        {
            Debug.Log($"✅ Asset gefunden: {asset.name} (Typ: {asset.GetType().Name})");
            
            // Spezielle Behandlung je nach Asset-Typ
            if (asset is GameObject)
            {
                GameObject prefab = asset as GameObject;
                GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
                instance.name = $"{prefab.name}_FromBundle";
                
                // Optional: Als Child dieses GameObjects setzen für bessere Organisation
                if (organizeAsChildren)
                {
                    instance.transform.SetParent(this.transform);
                }
                
                Debug.Log($"🎮 GameObject '{instance.name}' in Hierarchy erstellt an Position: {instance.transform.position}!");
                
                // Objekt in der Hierarchy auswählen (nur im Editor)
                #if UNITY_EDITOR
                UnityEditor.Selection.activeGameObject = instance;
                UnityEditor.EditorGUIUtility.PingObject(instance);
                #endif
            }
            else if (asset is Texture2D)
            {
                Debug.Log($"🖼️ Textur geladen: {((Texture2D)asset).width}x{((Texture2D)asset).height}");
            }
            else if (asset is AudioClip)
            {
                AudioClip clip = asset as AudioClip;
                Debug.Log($"🔊 Audio geladen: {clip.length:F2}s, {clip.frequency}Hz");
            }
            else
            {
                Debug.Log($"📄 Asset vom Typ {asset.GetType().Name} geladen");
            }
        }
        else
        {
            Debug.LogError($"❌ Asset '{assetName}' nicht gefunden!");
            Debug.Log("Verfügbare Assets:");
            foreach (string availableName in assetNames)
            {
                Debug.Log($"  - {System.IO.Path.GetFileNameWithoutExtension(availableName)}");
            }
        }
    }
    
    void OnDestroy()
    {
        // Bundle beim Zerstören des GameObjects freigeben
        if (currentBundle != null)
        {
            currentBundle.Unload(true);
        }
    }
    
    void OnValidate()
    {
        // Button-Logik
        if (loadBundleButton)
        {
            loadBundleButton = false;
            LoadAssetBundle();
        }
        
        if (listAssetsButton)
        {
            listAssetsButton = false;
            ListAssets();
        }
        
        if (loadAssetButton)
        {
            loadAssetButton = false;
            LoadSelectedAsset();
        }
        
        // Pfad validieren
        if (!string.IsNullOrEmpty(bundlePath) && !bundlePath.StartsWith("file://") && !bundlePath.StartsWith("http"))
        {
            bundlePath = "file://" + bundlePath;
        }
    }
}