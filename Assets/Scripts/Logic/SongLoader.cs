using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using VContainer;
using VContainer.Unity;


public class SongLoader : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;
    
    [AwakeInject] private readonly MappingConfig _mappingConfig;
    
    [AwakeInject] private readonly AudioConfig _audioConfig;
    
    [AwakeInject] private ConfigLoader _configLoader;
    
    [AwakeInject] private readonly ReadMapInfo _readMapInfo;
    
    
    public AudioSource audioSource;
    public float songSpeed;
    private bool check;
    public bool dontLoadAudio;
    public float audioDelay;
    
    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }
    
    private void FixedUpdate()
    {
        foreach (var config in _configLoader.GetAll())
        {
            var type = config.GetType();
            if (_configLoader.IsChanged(type))
            {
                ApplySettings();
            }
        }
    }
    
    void Start()
    {
        SetAudioClip(audioSource);
            
        audioSource.volume = _audioConfig.SongVolume;
        songSpeed = _mappingConfig.SongSpeed / 100f;
        
        SetAudioClip(audioSource);
        StopSong();
    }

    private void ApplySettings()
    {
        audioSource.volume = _audioConfig.SongVolume;
        songSpeed = _mappingConfig.SongSpeed / 100f;
    }


    public void SetAudioClip(AudioSource source)
    {
        var songPath = _readMapInfo.folderPath + "\\" + _readMapInfo.info._songFilename;
        Debug.Log(songPath);
        StartCoroutine(LoadAudioClip(songPath, source, GetAudioTypeFromExtension(songPath)));
    }

    public AudioType GetAudioTypeFromExtension(string path)
    {
        string extension = System.IO.Path.GetExtension(path).ToLower();
        switch (extension)
        {
            case ".ogg":
                return AudioType.OGGVORBIS;
            case ".egg":
                return AudioType.OGGVORBIS;
            case ".wav":
                return AudioType.WAV;
            default:
                return AudioType.UNKNOWN;
        }
    }

    public IEnumerator LoadAudioClip(string path, AudioSource src, AudioType audioType)
    {
        // Ensure the file path is in the correct format
        string formattedPath = path.Replace("\\", "/"); // Replace backslashes with forward slashes
        string escapedPath = UnityWebRequest.EscapeURL(formattedPath);
        string url = "file:///" + escapedPath;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                src.clip = clip;

                // timeInSeconds = src.clip.length;

                ApplySongSpeed(); // Apply speed settings once the audio is loaded
            }
        }
    }

    public void PlaySong(float offset)
    {
        if (audioSource.clip)
        {
            if (offset >= 0 && offset <= audioSource.clip.length)
            {
                audioSource.time = offset + audioDelay;
                audioSource.Play();
                audioSource.pitch = songSpeed; // Set the playback speed
            }
        }
        else
        {
            Debug.LogWarning("No audio clip loaded.");
        }
    }

    public void StopSong()
    {
        audioSource.Stop();
        StopCoroutine("AnalyzeSamples");
    }

    private void OnSongSpeedChanged()
    {
        if (audioSource.clip != null)
        {
            // Reload the audio clip with the updated speed settings
            ApplySongSpeed();
        }
    }

    private void ApplySongSpeed()
    {
        audioSource.pitch = songSpeed; // Update the pitch to match the new speed
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }
}
