using UnityEngine;
using System.Collections.Generic;

public enum SoundType
{
    Step,
    Jump,
    Land,
    Punch,
    Slide
}

[System.Serializable]
public class SoundData
{
    public SoundType soundType;
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitchMin = 1f;
    [Range(0.1f, 3f)] public float pitchMax = 1f;
    [Range(0f, 1f)] public float spatialBlend = 0f;
    public bool use3D = false;
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundData[] soundDatabase;
    [SerializeField] private GameObject audioSourcePrefab;
    [SerializeField] private int maxConcurrentSounds = 10;

    private static SoundManager instance;
    private AudioSource mainAudioSource;
    private Queue<AudioSource> availableAudioSources = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private Dictionary<SoundType, SoundData> soundLookup = new Dictionary<SoundType, SoundData>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        mainAudioSource = GetComponent<AudioSource>();

        foreach (var soundData in soundDatabase)
        {
            soundLookup[soundData.soundType] = soundData;
        }

        for (int i = 0; i < maxConcurrentSounds; i++)
        {
            CreateAudioSource();
        }
    }

    void CreateAudioSource()
    {
        GameObject audioObj;
        if (audioSourcePrefab != null)
        {
            audioObj = Instantiate(audioSourcePrefab, transform);
        }
        else
        {
            audioObj = new GameObject("AudioSource_" + availableAudioSources.Count);
            audioObj.transform.SetParent(transform);
            audioObj.AddComponent<AudioSource>();
        }

        AudioSource audioSource = audioObj.GetComponent<AudioSource>();
        availableAudioSources.Enqueue(audioSource);
    }

    AudioSource GetAvailableAudioSource()
    {
        if (availableAudioSources.Count > 0)
        {
            return availableAudioSources.Dequeue();
        }

        foreach (var source in activeAudioSources.ToArray())
        {
            if (!source.isPlaying)
            {
                activeAudioSources.Remove(source);
                return source;
            }
        }

        CreateAudioSource();
        return availableAudioSources.Dequeue();
    }

    void ReturnAudioSource(AudioSource source)
    {
        if (activeAudioSources.Contains(source))
        {
            activeAudioSources.Remove(source);
        }
        availableAudioSources.Enqueue(source);
    }

    public static void PlaySound(SoundType soundType, float volumeMultiplier = 1f)
    {
        if (instance == null) return;
        instance.PlaySoundInternal(soundType, null, volumeMultiplier);
    }

    public static void PlaySound3D(SoundType soundType, Vector3 position, float volumeMultiplier = 1f)
    {
        if (instance == null) return;
        instance.PlaySoundInternal(soundType, position, volumeMultiplier);
    }

    void PlaySoundInternal(SoundType soundType, Vector3? position, float volumeMultiplier)
    {
        if (!soundLookup.ContainsKey(soundType)) return;

        SoundData soundData = soundLookup[soundType];
        if (soundData.clips == null || soundData.clips.Length == 0) return;

        AudioClip clipToPlay = soundData.clips[Random.Range(0, soundData.clips.Length)];
        if (clipToPlay == null) return;

        AudioSource audioSource;
        bool is3D = position.HasValue || soundData.use3D;

        if (is3D)
        {
            audioSource = GetAvailableAudioSource();
            activeAudioSources.Add(audioSource);

            if (position.HasValue)
            {
                audioSource.transform.position = position.Value;
            }

            audioSource.spatialBlend = soundData.spatialBlend;
        }
        else
        {
            audioSource = mainAudioSource;
        }

        float pitch = Random.Range(soundData.pitchMin, soundData.pitchMax);
        float volume = soundData.volume * volumeMultiplier;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clipToPlay, volume);

        if (is3D && audioSource != mainAudioSource)
        {
            StartCoroutine(ReturnAudioSourceWhenDone(audioSource, clipToPlay.length / pitch));
        }
    }

    System.Collections.IEnumerator ReturnAudioSourceWhenDone(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnAudioSource(source);
    }

    public static void StopAllSounds()
    {
        if (instance == null) return;

        instance.mainAudioSource.Stop();
        foreach (var source in instance.activeAudioSources)
        {
            source.Stop();
        }
    }

    public static void SetMasterVolume(float volume)
    {
        if (instance == null) return;
        AudioListener.volume = Mathf.Clamp01(volume);
    }
}