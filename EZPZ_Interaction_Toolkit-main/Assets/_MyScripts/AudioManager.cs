using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    var go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private const string AUDIO_FOLDER = "AudioClips/";

    private AudioSource musicAudioSource;
    private List<AudioSource> unusedPool;
    private List<AudioSource> usedPool;
    private Dictionary<string, AudioClip> clipCache;

    [HideInInspector] public float musicVolume = 1;
    [HideInInspector] public float soundVolume = 1;

    private bool muted = false;
    private int poolCount = 5;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        unusedPool = new List<AudioSource>();
        usedPool = new List<AudioSource>();
        clipCache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
    }

    void Start()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1);
    }

    /// <summary>
    /// Load and cache a clip by name (file name under Resources/AudioClips/).
    /// </summary>
    private AudioClip GetClip(string name)
    {
        if (clipCache.TryGetValue(name, out var clip) && clip != null)
            return clip;

        clip = Resources.Load<AudioClip>(AUDIO_FOLDER + name);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] Clip not found: Resources/{AUDIO_FOLDER}{name}");
            return null;
        }

        clipCache[name] = clip;
        return clip;
    }

    #region Music

    /// <summary>
    /// Play background music by clip name, with fade transition.
    /// </summary>
    public void PlayMusic(string name, bool loop = true)
    {
        var clip = GetClip(name);
        if (clip == null) return;

        DOTween.To(() => musicAudioSource.volume, v => musicAudioSource.volume = v, 0, 0.5f).OnComplete(() =>
        {
            musicAudioSource.clip = clip;
            musicAudioSource.loop = loop;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.Play();
            DOTween.To(() => musicAudioSource.volume, v => musicAudioSource.volume = v, musicVolume, 0.5f);
        });
    }

    public void StopMusic()
    {
        DOTween.To(() => musicAudioSource.volume, v => musicAudioSource.volume = v, 0, 0.5f)
            .OnComplete(() => musicAudioSource.Stop());
    }

    #endregion

    #region Sound

    /// <summary>
    /// Play a sound effect by clip name.
    /// </summary>
    public void PlaySound(string name, Action onComplete = null)
    {
        var clip = GetClip(name);
        if (clip == null) return;

        var src = GetSource();
        src.clip = clip;
        src.volume = soundVolume;
        src.mute = muted;
        src.loop = false;
        src.Play();
        StartCoroutine(WaitEnd(src, onComplete));
    }

    /// <summary>
    /// Play a sound with custom volume (0-1).
    /// </summary>
    public void PlaySound(string name, float volume, Action onComplete = null)
    {
        var clip = GetClip(name);
        if (clip == null) return;

        var src = GetSource();
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume) * soundVolume;
        src.mute = muted;
        src.loop = false;
        src.Play();
        StartCoroutine(WaitEnd(src, onComplete));
    }

    /// <summary>
    /// Play a sound with loop option. Returns the AudioSource for control.
    /// </summary>
    public AudioSource PlaySound(string name, bool loop, Action onComplete = null)
    {
        var clip = GetClip(name);
        if (clip == null) return null;

        var src = GetSource();
        src.clip = clip;
        src.volume = soundVolume;
        src.mute = muted;
        src.loop = loop;
        src.Play();
        if (!loop)
            StartCoroutine(WaitEnd(src, onComplete));
        return src;
    }

    /// <summary>
    /// Play a 3D sound at a world position.
    /// </summary>
    public void Play3DSound(string name, Vector3 position)
    {
        var clip = GetClip(name);
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, soundVolume);
    }

    /// <summary>
    /// Play multiple sounds in sequence.
    /// </summary>
    public void PlayQueue(Action onComplete, params string[] names)
    {
        if (names == null || names.Length == 0) { onComplete?.Invoke(); return; }
        StartCoroutine(QueueRoutine(onComplete, names, 0));
    }

    private IEnumerator QueueRoutine(Action onComplete, string[] names, int index)
    {
        if (index >= names.Length) { onComplete?.Invoke(); yield break; }

        var clip = GetClip(names[index]);
        if (clip == null) { StartCoroutine(QueueRoutine(onComplete, names, index + 1)); yield break; }

        var src = GetSource();
        src.clip = clip;
        src.volume = soundVolume;
        src.mute = muted;
        src.loop = false;
        src.Play();
        yield return new WaitWhile(() => src.isPlaying);
        ReturnSource(src);
        StartCoroutine(QueueRoutine(onComplete, names, index + 1));
    }

    /// <summary>
    /// Play multiple sounds simultaneously. Callback fires when all finish.
    /// </summary>
    public void PlaySimultaneous(Action onComplete, params string[] names)
    {
        if (names == null || names.Length == 0) { onComplete?.Invoke(); return; }
        StartCoroutine(SimultaneousRoutine(onComplete, names));
    }

    private IEnumerator SimultaneousRoutine(Action onComplete, string[] names)
    {
        var sources = new List<AudioSource>();
        foreach (var n in names)
        {
            var clip = GetClip(n);
            if (clip == null) continue;
            var src = GetSource();
            src.clip = clip;
            src.volume = soundVolume;
            src.mute = muted;
            src.loop = false;
            src.Play();
            sources.Add(src);
        }

        bool done = false;
        while (!done)
        {
            done = true;
            foreach (var s in sources)
            {
                if (s.isPlaying) { done = false; break; }
            }
            yield return null;
        }

        foreach (var s in sources)
            ReturnSource(s);

        onComplete?.Invoke();
    }

    #endregion

    #region Control

    public void StopAllSounds()
    {
        foreach (var src in usedPool)
        {
            if (src != null)
            {
                src.Stop();
                src.loop = false;
                unusedPool.Add(src);
            }
        }
        usedPool.Clear();
    }

    public bool IsPlaying()
    {
        foreach (var src in usedPool)
            if (src != null && src.isPlaying) return true;
        return false;
    }

    public bool IsPlaying(string name)
    {
        var clip = GetClip(name);
        if (clip == null) return false;
        foreach (var src in usedPool)
            if (src != null && src.isPlaying && src.clip == clip) return true;
        return false;
    }

    public void ChangeMusicVolume(float volume)
    {
        musicVolume = volume;
        musicAudioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void ChangeSoundVolume(float volume)
    {
        soundVolume = volume;
        foreach (var src in unusedPool) if (src) src.volume = volume;
        foreach (var src in usedPool) if (src) src.volume = volume;
        PlayerPrefs.SetFloat("SoundVolume", volume);
    }

    public void MusicMute(bool isMute)
    {
        musicAudioSource.mute = isMute;
        muted = isMute;
    }

    public void SoundMute(bool isMute)
    {
        muted = isMute;
        foreach (var src in unusedPool) if (src) src.mute = isMute;
        foreach (var src in usedPool) if (src) src.mute = isMute;
    }

    #endregion

    #region Pool

    private AudioSource GetSource()
    {
        if (unusedPool.Count > 0)
            return TakeFromPool();

        if (usedPool.Count < poolCount)
        {
            AddSource();
            return TakeFromPool();
        }

        // Pool exhausted — create temporary
        var src = gameObject.AddComponent<AudioSource>();
        usedPool.Add(src);
        return src;
    }

    private void AddSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        unusedPool.Add(src);
    }

    private AudioSource TakeFromPool()
    {
        var src = unusedPool[0];
        unusedPool.RemoveAt(0);
        usedPool.Add(src);
        return src;
    }

    private void ReturnSource(AudioSource src)
    {
        if (usedPool.Contains(src))
            usedPool.Remove(src);

        if (unusedPool.Count >= poolCount)
        {
            if (src) Destroy(src);
        }
        else if (src && !unusedPool.Contains(src))
        {
            unusedPool.Add(src);
        }
    }

    private IEnumerator WaitEnd(AudioSource src, Action onComplete)
    {
        yield return new WaitWhile(() => src != null && src.isPlaying);
        ReturnSource(src);
        onComplete?.Invoke();
    }

    #endregion
}
