using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }


    public AudioSource asBg;
    [Header("SFX Pooling")]
    public GameObject sfxPrefab;
    public float sfxSpamCooldown = 0.08f;
    private AudioDataBase audioDataBase;
    private Dictionary<string, AudioConfig> audioLookup;
    private Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();
    private float currentSfxVolume = 1f;


    public void Init()
    {
        Instance = this;
        audioDataBase = DataRepo.Instance.audioData;
        BuildAudioLookup();
        SetInitVolumes();
    }

    private void BuildAudioLookup()
    {
        audioLookup = new Dictionary<string, AudioConfig>();
        foreach (var config in audioDataBase.audioConfigs)
        {
            if (string.IsNullOrEmpty(config.key))
                continue;

            string lowerKey = config.key.ToLower();

            if (!audioLookup.ContainsKey(lowerKey))
                audioLookup.Add(lowerKey, config);
            else
                Debug.LogWarning($"Tìm thấy AudioKey bị trùng: {config.key}");
        }
    }

    private void SetInitVolumes()
    {
        SetMusicVolume();
        SetSoundVolume(1f);
    }

    /// <summary>
    /// Phát một SFX (âm thanh ngắn) dựa trên Key.
    /// </summary>
    public void PlaySfx(string key)
    {
        if (!UseProfile.OnSound || string.IsNullOrEmpty(key)) return;

        string lowerKey = key.ToLower();

        if (lastPlayTimes.TryGetValue(lowerKey, out float lastTime))
        {
            if (Time.time - lastTime < sfxSpamCooldown)
            {
                return;
            }
        }

        lastPlayTimes[lowerKey] = Time.time;

        if (!audioLookup.TryGetValue(lowerKey, out var config))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Không tìm thấy AudioKey SFX: {key}");
#endif
            return;
        }

        AudioClip clipToPlay = config.GetRandomClip();
        if (clipToPlay == null) return;

        GameObject sfxObj = SimplePool2.Spawn(sfxPrefab, Vector3.zero, Quaternion.identity);
        if (sfxObj == null) return;

        AudioSource source = sfxObj.GetComponent<AudioSource>();

        source.clip = clipToPlay;
        source.pitch = config.GetRandomPitch();

        source.volume = lowerKey == "coin" ? 0.2f : currentSfxVolume;

        source.Play();

        DespawnAfterPlayAsync(sfxObj, clipToPlay.length).Forget();
    }

    public void PlaySfx(AudioClip clip, float pitch = 1)
    {
        if (!UseProfile.OnSound) return;

        string clipName = clip.name.ToLower();
        if (lastPlayTimes.TryGetValue(clipName, out float lastTime))
        {
            if (Time.time - lastTime < sfxSpamCooldown) return;
        }
        lastPlayTimes[clipName] = Time.time;

        GameObject sfxObj = SimplePool2.Spawn(sfxPrefab, Vector3.zero, Quaternion.identity);
        if (sfxObj == null) return;

        AudioSource source = sfxObj.GetComponent<AudioSource>();
        source.clip = clip;
        source.pitch = pitch;
        source.volume = currentSfxVolume;

        source.Play();

        DespawnAfterPlayAsync(sfxObj, clip.length).Forget();
    }


    private async UniTaskVoid DespawnAfterPlayAsync(GameObject obj, float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

        if (obj != null && obj.activeInHierarchy)
        {
            SimplePool2.Despawn(obj);
        }
    }
    public void PlayMusic(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        string lowerKey = key.ToLower();

        if (!audioLookup.TryGetValue(lowerKey, out AudioConfig config))
        {
            Debug.LogWarning($"Không tìm thấy AudioKey nhạc: {key}");
            return;
        }

        AudioClip clipToPlay = config.GetRandomClip();
        if (clipToPlay == null) return;

        asBg.clip = clipToPlay;
        asBg.loop = true;
        asBg.pitch = 1f;

        asBg.Play();
        RefreshMusicVolume();
    }
    public void RefreshMusicVolume()
    {
        SetMusicVolume();
    }
    private void SetMusicVolume()
    {
        asBg.volume = UseProfile.OnMusic ? 0.2f : 0f;
    }

    private void SetSoundVolume(float volume)
    {
        currentSfxVolume = UseProfile.OnSound ? volume : 0f;
    }
}