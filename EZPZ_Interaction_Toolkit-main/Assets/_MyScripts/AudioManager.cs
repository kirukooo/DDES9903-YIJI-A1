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
            return instance;
        }
    }

    private Dictionary<int, string> audioPathDict;      // 存放音频文件路径

    private AudioSource musicAudioSource;

    private List<AudioSource> unusedSoundAudioSourceList;   // 存放可以使用的音频组件

    private List<AudioSource> usedSoundAudioSourceList;     // 存放正在使用的音频组件

    private Dictionary<int, AudioClip> audioClipDict;       // 缓存音频文件
    [HideInInspector]
    public float musicVolume = 1;
    [HideInInspector]
    public float soundVolume = 1;

    private string musicVolumePrefs = "MusicVolume";

    private string soundVolumePrefs = "SoundVolume";

    private bool mymute = false;
    private int poolCount = 3;         // 对象池数量

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        instance = this;

        audioPathDict = new Dictionary<int, string>()       // 这里设置音频文件路径。需要修改。 TODO
        {
              { 1, "AudioClip/LampOn" },
              { 2, "AudioClip/Bg" },
              { 3, "AudioClip/BookshelfMoving" },
              { 4, "AudioClip/Generic_Pickup_01" },
              { 5, "AudioClip/Unlock01" },
              { 6, "AudioClip/BookshelfMoving01" },
              { 7, "AudioClip/Generic_Pickup_04" },
              { 8, "AudioClip/New Objective 1" },
              { 9, "AudioClip/jumpscare_01" },
              { 10, "AudioClip/UnlockPadlock" },
              { 11, "AudioClip/Big Explosion 04" },        

        };

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        unusedSoundAudioSourceList = new List<AudioSource>();
        usedSoundAudioSourceList = new List<AudioSource>();
        audioClipDict = new Dictionary<int, AudioClip>();

    }

    void Start()
    {
        // 从本地缓存读取声音音量
        if (PlayerPrefs.HasKey(musicVolumePrefs))
        {
            musicVolume = PlayerPrefs.GetFloat(musicVolumePrefs);
        }
        if (PlayerPrefs.HasKey(soundVolumePrefs))
        {
            soundVolume = PlayerPrefs.GetFloat(soundVolumePrefs);
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="id"></param>
    /// <param name="loop"></param>
    public void PlayMusic(int id, bool loop = true)
    {
        // 通过Tween将声音淡入淡出
        DOTween.To(() => musicAudioSource.volume, value => musicAudioSource.volume = value, 0, 0.5f).OnComplete(() =>
        {
            musicAudioSource.clip = GetAudioClip(id);
            musicAudioSource.clip.LoadAudioData();
            musicAudioSource.loop = loop;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.Play();
            DOTween.To(() => musicAudioSource.volume, value => musicAudioSource.volume = value, musicVolume, 0.5f);
        });
    }

    public void PlayQueenSound(Action action = null, params int[] sounds)
    {
        StartCoroutine(PlaySound1(action, sounds));
    }

    IEnumerator PlaySound1(Action action, int[] sounds)
    {
        AudioSource audioSource = null;
        if (unusedSoundAudioSourceList.Count != 0)
        {
            audioSource = UnusedToUsed();
        }
        else
        {
            AddAudioSource();

            audioSource = UnusedToUsed();
        }
        if (sounds.Length > 0)
        {
            audioSource.volume = soundVolume;
            audioSource.mute = mymute;
            audioSource.clip = GetAudioClip(sounds[0]);
            audioSource.clip.LoadAudioData();
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
            //a.Skip(2).Take(5).ToArray();
            if (sounds.Length > 1)
            {
                sounds = GetNewArray(sounds);
                StartCoroutine(PlaySound1(action, sounds));
            }
            else
                StartCoroutine(WaitPlayEnd(audioSource, action));
        }
    }

    int[] GetNewArray(int[] array)
    {
        int[] arr = new int[array.Length - 1];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = array[i + 1];
        }
        return arr;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="id"></param>
    public void PlaySound(int id, Action action = null)
    {
        if (unusedSoundAudioSourceList.Count != 0)
        {
            AudioSource audioSource = UnusedToUsed();
            audioSource.clip = GetAudioClip(id);
            audioSource.clip.LoadAudioData();
            audioSource.Play();

            StartCoroutine(WaitPlayEnd(audioSource, action));
        }
        else
        {
            AddAudioSource();

            AudioSource audioSource = UnusedToUsed();
            audioSource.clip = GetAudioClip(id);
            audioSource.clip.LoadAudioData();
            audioSource.volume = soundVolume;
            audioSource.loop = false;
            audioSource.Play();

            StartCoroutine(WaitPlayEnd(audioSource, action));
        }
    }

    /// <summary>
    /// 播放3d音效
    /// </summary>
    /// <param name="id"></param>
    /// <param name="position"></param>
    public void Play3dSound(int id, Vector3 position)
    {
        AudioClip ac = GetAudioClip(id);
        AudioSource.PlayClipAtPoint(ac, position);
    }

    /// <summary>
    /// 当播放音效结束后，将其移至未使用集合
    /// </summary>
    /// <param name="audioSource"></param>
    /// <returns></returns>
    IEnumerator WaitPlayEnd(AudioSource audioSource, Action action)
    {
        yield return new WaitUntil(() => { return !audioSource.isPlaying; });
        UsedToUnused(audioSource);
        if (action != null)
        {
            action();
        }
    }

    /// <summary>
    /// 获取音频文件，获取后会缓存一份
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private AudioClip GetAudioClip(int id)
    {
        if (!audioClipDict.ContainsKey(id))
        {
            if (!audioPathDict.ContainsKey(id))
                return null;
            AudioClip ac = Resources.Load(audioPathDict[id]) as AudioClip;
            audioClipDict.Add(id, ac);
        }
        return audioClipDict[id];
    }

    /// <summary>
    /// 添加音频组件
    /// </summary> 
    /// <returns></returns>
    private AudioSource AddAudioSource()
    {
        if (unusedSoundAudioSourceList.Count != 0)
        {
            return UnusedToUsed();
        }
        else
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            unusedSoundAudioSourceList.Add(audioSource);
            return audioSource;
        }
    }

    /// <summary>
    /// 将未使用的音频组件移至已使用集合里
    /// </summary>
    /// <returns></returns>
    private AudioSource UnusedToUsed()
    {
        AudioSource audioSource = unusedSoundAudioSourceList[0];
        unusedSoundAudioSourceList.RemoveAt(0);
        usedSoundAudioSourceList.Add(audioSource);
        return audioSource;
    }

    /// <summary>
    /// 将使用完的音频组件移至未使用集合里
    /// </summary>
    /// <param name="audioSource"></param>
    private void UsedToUnused(AudioSource audioSource)
    {
        if (usedSoundAudioSourceList.Contains(audioSource))
        {
            usedSoundAudioSourceList.Remove(audioSource);
        }
        if (unusedSoundAudioSourceList.Count >= poolCount)
        {
            Destroy(audioSource);
        }
        else if (audioSource != null && !unusedSoundAudioSourceList.Contains(audioSource))
        {
            unusedSoundAudioSourceList.Add(audioSource);
        }
    }

    /// <summary>
    /// 修改背景音乐音量
    /// </summary>
    /// <param name="volume"></param>
    public void ChangeMusicVolume(float volume)
    {
        musicVolume = volume;
        musicAudioSource.volume = volume;
        PlayerPrefs.SetFloat(musicVolumePrefs, volume);
    }

    /// <summary>
    /// 背景音静音
    /// </summary>
    /// <param name="isMute"></param>
    public void MusicMute(bool isMute)
    {
        musicAudioSource.mute = isMute;
        mymute = isMute;
    }

    /// <summary>
    /// 音效静音
    /// </summary>
    /// <param name="isMute"></param>
    public void SoundMute(bool isMute)
    {
        for (int i = 0; i < unusedSoundAudioSourceList.Count; i++)
        {
            unusedSoundAudioSourceList[i].mute = isMute;
        }
    }

    /// <summary>
    /// 修改音效音量
    /// </summary>
    /// <param name="volume"></param>
    public void ChangeSoundVolume(float volume)
    {
        soundVolume = volume;
        for (int i = 0; i < unusedSoundAudioSourceList.Count; i++)
        {
            unusedSoundAudioSourceList[i].volume = volume;
        }
        for (int i = 0; i < usedSoundAudioSourceList.Count; i++)
        {
            usedSoundAudioSourceList[i].volume = volume;
        }
        PlayerPrefs.SetFloat(soundVolumePrefs, volume);
        Debug.LogError(volume);
    }
}
