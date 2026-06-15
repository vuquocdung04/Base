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

    private List<AudioDataBase> audioDataBases;
    private Dictionary<string, AudioConfig> audioLookup;
    private Dictionary<string, float> lastPlayTimes = new();
    private float currentSfxVolume = 1f;

    public void Init()
    {
        Instance = this;
        audioDataBases = DataRepo.Instance.audioDataList;
        BuildAudioLookup();
        ApplyMusicVolume();
        ApplySoundVolume();
    }

    private void BuildAudioLookup()
    {
        audioLookup = new Dictionary<string, AudioConfig>();

        foreach (var dataBase in audioDataBases)
        {
            if (dataBase == null)
            {
                Debug.LogWarning("AudioDataBase null trong DataRepo!");
                continue;
            }

            foreach (var config in dataBase.audioConfigs)
            {
                if (string.IsNullOrEmpty(config.key)) continue;

                string lowerKey = config.key.ToLower();
                if (!audioLookup.TryAdd(lowerKey, config))
                    Debug.LogWarning($"AudioKey bị trùng giữa các database: {config.key} (trong {dataBase.name})");
            }
        }
    }
    public void PlaySfx(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        string lowerKey = key.ToLower();

        if (!audioLookup.TryGetValue(lowerKey, out var config))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Không tìm thấy AudioKey SFX: {key}");
#endif
            return;
        }

        float volume = lowerKey == "coins" ? 1f : currentSfxVolume;
        PlayClipInternal(lowerKey, config.GetRandomClip(), config.GetRandomPitch(), volume);
    }

    public void PlaySfx(AudioClip clip, float pitch = 1f)
    {
        if (clip == null) return;
        PlayClipInternal(clip.name.ToLower(), clip, pitch, currentSfxVolume);
    }

    private void PlayClipInternal(string throttleKey, AudioClip clip, float pitch, float volume)
    {
        if (!UseProfile.OnSound || clip == null) return;

        if (lastPlayTimes.TryGetValue(throttleKey, out float lastTime) &&
            Time.time - lastTime < sfxSpamCooldown) return;
        lastPlayTimes[throttleKey] = Time.time;

        var sfxObj = SimplePool2.Spawn(sfxPrefab, Vector3.zero, Quaternion.identity);
        if (sfxObj == null) return;

        var source = sfxObj.GetComponent<AudioSource>();
        source.clip = clip;
        source.pitch = pitch;
        source.volume = volume;
        source.Play();

        DespawnAfterPlayAsync(sfxObj, clip.length).Forget();
    }

    private async UniTaskVoid DespawnAfterPlayAsync(GameObject obj, float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
        if (obj != null && obj.activeInHierarchy) SimplePool2.Despawn(obj);
    }

    public void PlayMusic(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (!audioLookup.TryGetValue(key.ToLower(), out var config))
        {
            Debug.LogWarning($"Không tìm thấy AudioKey nhạc: {key}");
            return;
        }

        var clip = config.GetRandomClip();
        if (clip == null) return;

        asBg.clip = clip;
        asBg.loop = true;
        asBg.pitch = 1f;
        asBg.Play();
    }
    public void ToggleSound()
    {
        UseProfile.OnSound.Value = !UseProfile.OnSound;
        ApplySoundVolume();
    }

    public void ToggleMusic()
    {
        UseProfile.OnMusic.Value = !UseProfile.OnMusic;
        ApplyMusicVolume();
    }

    private void ApplyMusicVolume()
    {
        asBg.volume = UseProfile.OnMusic ? 0.65f : 0f;
    }

    private void ApplySoundVolume()
    {
        currentSfxVolume = UseProfile.OnSound ? 1f : 0f;
    }
}