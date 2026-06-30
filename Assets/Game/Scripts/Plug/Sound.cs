using System.Collections.Generic;
using System.Timers;
using UnityEngine;

/// <summary>声音</summary>
public class Sound
{
    /// <summary>声音大小-音乐</summary>
    public static float VolumeMusic = 0.5f;

    /// <summary>声音大小-音效</summary>
    public static float VolumeSound = 1;
    public static float VideoVolume = 1;

    static GameObject MusicManager;

    static GameObject[] SoundManager;
    public static Dictionary<string, AudioClip> musicBGM = new Dictionary<string, AudioClip>();
    /// <summary>音效最大数量(同时存在)</summary>
    static int MaxSoundNum = 20;
    /// <summary>当前使用的音效索引</summary>
    static int SoundIndex = 0;

    public static bool hasPlayMusic = false;//是否正在播放音乐
    public static string PlayingMusicName;//正在播放的音乐名称
    static Sound()
    {
        MusicManager = new GameObject("#MusicManager#");
        MusicManager.AddComponent<AudioSource>();
        Object.DontDestroyOnLoad(MusicManager);

        SoundManager = new GameObject[MaxSoundNum];
    }

    public static void OnSetVolume(float musicValue, float soundValue)
    {
        VolumeMusic = musicValue;
        VolumeSound = soundValue;
        VideoVolume = soundValue;
        PlayerPrefs.SetFloat("VolumeMusic", VolumeMusic);
        PlayerPrefs.SetFloat("VolumeSound", VolumeSound);

        if (MusicManager) MusicManager.GetComponent<AudioSource>().volume = VolumeMusic;
    }
    /// <summary>获得音乐大小</summary>
    public static float GetVolumeMusic()
    {
        return VolumeMusic;
    }

    /// <summary>获得音效大小</summary>
    public static float GetVolumeSound()
    {
        return VolumeSound;
    }
    /// <summary>暂停或继续播放音乐</summary>
    public static void PauseOrPlayVolumeMusic(bool pause = true)
    {
        if (pause)
        {
            hasPlayMusic = false;
            MusicManager.GetComponent<AudioSource>().Pause();
        }
        else
        {
            hasPlayMusic = true;
            MusicManager.GetComponent<AudioSource>().Play();
        }
    }

    /// <summary>设置音乐大小</summary>
    public static void SetVolumeMusic(float num)
    {
        VolumeMusic = num;
        MusicManager.GetComponent<AudioSource>().volume = num;
    }

    /// <summary>设置音效大小</summary>
    public static void SetVolumeSound(float num)
    {
        VolumeSound = num;
        foreach (var i in SoundManager)
        {
            if (i == null) continue;
            i.GetComponent<AudioSource>().volume = num;
        }
    }

    /// <summary>播放音乐</summary>
    public static AudioSource PlayMusic(string url, bool loop)
    {
        AudioClip clip = LoadAudioClip(url, true);
        if (clip == null) return null;
        PlayingMusicName = clip.name;
        hasPlayMusic = true;
        return PlayAndReturn(MusicManager, clip, loop);
    }

    /// <summary>播放音乐</summary>
    public static void PlayMusic(string url, bool show_warn_log = true, bool loop = true)
    {
        AudioClip clip = LoadAudioClip(url, show_warn_log);
        if (clip == null) return;
        PlayingMusicName = clip.name;
        Play(MusicManager, clip, loop);
        hasPlayMusic = true;
    }
    private static string sound;
    private static bool delayPlay = true;
    /// <summary>播放音效</summary>
    public static void PlaySound(string url, bool show_warn_log = true)
    {
        if (sound != url || delayPlay)
        {
            sound = url;
            //vv.log("播放音效:", url);

            AudioClip clip = LoadAudioClip(url, show_warn_log);
            if (clip == null) return;

            int id = SoundIndex++ % MaxSoundNum;
            if (SoundManager[id] == null)
            {
                SoundManager[id] = new GameObject("#SoundManager#");
                SoundManager[id].AddComponent<AudioSource>();
                Object.DontDestroyOnLoad(SoundManager[id]);
            }

            Play(SoundManager[id], clip, false);
        }
    }

    /// <summary>加载声音资源</summary>
    public static AudioClip LoadAudioClip(string url, bool show_warn_log)
    {
        AudioClip clip = null;
        try
        {
            if (musicBGM != null && musicBGM.ContainsKey(url))
            {
                clip = musicBGM[url];
            }
            else
            {
                clip = Loaded.Load<AudioClip>($"{url}");
            }
        }
        catch { }
        return clip;
    }

    /// <summary>播放声音</summary>
    static void Play(GameObject obj, AudioClip clip, bool loop)
    {
        AudioSource source = obj.GetComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.volume = loop ? VolumeMusic : VolumeSound;
        source.Play();
    }
    static AudioSource PlayAndReturn(GameObject obj, AudioClip clip, bool loop)
    {
        AudioSource source = obj.GetComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.volume = loop ? VolumeMusic : VolumeSound;
        source.Play();
        return source;
    }

}

