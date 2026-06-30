using UnityEngine;

public class ModData
{
    public const float BaseScale = 1f;
    public const float ScaleStep = 0.01f;
    public const float ScaleTimeDelta = 5f;

    /// <summary>待生成数量，生成时减一。</summary>
    public static int allReadyCreateChild;
    /// <summary>UI 显示数量，与待生成同步增加，仅小孩上路后减少。</summary>
    public static int AllShowCreateChild;
    public static int allPassChild;
    public static int allFailChild;

    public static float playerScale;
    public static float cPlayerScaleTime;

    public static float armPushSpeed = 10f;
    public static float tcjPushDirX = 1f;
    public static float qlbPushDirX = -1f;

    public static bool showPlayerName = true;
    public static string PlayerName = string.Empty;
    public static bool showKidName = true;
    public static bool showRoadName = true;
    public static int KidSkinType;
    public static int Kids;
    public static int defaultRoadCount = 1;

    /// <summary>难度档位 0=难度一 1=难度二 2=难度三，对应车辆生成间隔秒数 5/4/3。</summary>
    public static int difficultyLevel;

    public static int shineCount = 0;

    /// <summary>加路障5秒：每次触发增加的计时时长（秒）。</summary>
    public static float blockSealAddDuration = 5f;
    /// <summary>减路障5秒：每次触发减少的计时时长（秒）。</summary>
    public static float blockSealRemoveDuration = 5f;
    /// <summary>限时封路剩余时间（秒），由 ModFuncManager 协程递减；UI 读取显示。</summary>
    public static float blockSealRemainingTime;

    /// <summary>路障全局序号（增加路障 / 加路障5秒共用，每生成一个自增）。</summary>
    static int blockSerialCounter;

    public static int AllocateBlockSerialNumber() => ++blockSerialCounter;

    public static string settingsInputText = "剩余小孩：_  通过：_   已噶：";

    const string KeyShowPlayerName = "ShowPlayerName";
    const string KeyShowNornalKid = "KeyShowPlayerName";
    const string KeyShowKidName = "ShowKidName";
    const string KeyShowRoadName = "ShowRoadName";
    const string KeyKidSkinType = "KidSkinType";
    const string KeyVolumeMusic = "VolumeMusic";
    const string KeyVolumeSound = "VolumeSound";
    const string KeyInputText = "UISystem_InputText";
    const string KeyPlayerName = "PlayerName";
    const string KeyDefaultRoadCount = "DefaultRoadCount";
    const string KeyDifficultyLevel = "DifficultyLevel";

    static readonly float[] CarCreateTimeByDifficulty = { 5f, 4f, 3f };

    static bool settingsInitialized;

    public static float GetCarCreateTime()
    {
        int level = Mathf.Clamp(difficultyLevel, 0, CarCreateTimeByDifficulty.Length - 1);
        return CarCreateTimeByDifficulty[level];
    }

    /// <summary>从本地 PlayerPrefs 读取系统设置并写入 ModData（Awake 时调用）</summary>
    public static void TryInitFromLocal()
    {
        Kids = PlayerPrefs.GetInt(KeyShowNornalKid);
        showPlayerName = PlayerPrefs.GetInt(KeyShowPlayerName, 1) == 1;
        showKidName = PlayerPrefs.GetInt(KeyShowKidName, 1) == 1;
        showRoadName = PlayerPrefs.GetInt(KeyShowRoadName, 1) == 1;
        KidSkinType = PlayerPrefs.GetInt(KeyKidSkinType, 0);
        settingsInputText = PlayerPrefs.GetString(KeyInputText, "剩余小孩：_  通过：_   已噶：");
        PlayerName = PlayerPrefs.GetString(KeyPlayerName, string.Empty);
        defaultRoadCount = PlayerPrefs.GetInt(KeyDefaultRoadCount, 1);
        difficultyLevel = PlayerPrefs.GetInt(KeyDifficultyLevel, 0);

        float music = PlayerPrefs.GetFloat(KeyVolumeMusic, 0.5f);
        float sound = PlayerPrefs.GetFloat(KeyVolumeSound, 1f);
        Sound.OnSetVolume(music, sound);

        settingsInitialized = true;
    }

    public static void LoadSettings()
    {
        TryInitFromLocal();
    }

    /// <summary>将当前 ModData 系统设置写入本地</summary>
    public static void SaveSettings()
    {
        PlayerPrefs.SetInt(KeyShowPlayerName, showPlayerName ? 1 : 0);
        PlayerPrefs.SetInt(KeyShowNornalKid, Kids);
        PlayerPrefs.SetInt(KeyShowKidName, showKidName ? 1 : 0);
        PlayerPrefs.SetInt(KeyShowRoadName, showRoadName ? 1 : 0);
        PlayerPrefs.SetInt(KeyKidSkinType, KidSkinType);
        PlayerPrefs.SetString(KeyInputText, settingsInputText ?? "剩余小孩：_  通过：_   已噶：");
        PlayerPrefs.SetString(KeyPlayerName, PlayerName ?? string.Empty);
        PlayerPrefs.SetInt(KeyDefaultRoadCount, defaultRoadCount);
        PlayerPrefs.SetInt(KeyDifficultyLevel, Mathf.Clamp(difficultyLevel, 0, CarCreateTimeByDifficulty.Length - 1));
        PlayerPrefs.SetFloat(KeyVolumeMusic, Sound.VolumeMusic);
        PlayerPrefs.SetFloat(KeyVolumeSound, Sound.VolumeSound);
        PlayerPrefs.Save();
        settingsInitialized = true;
    }

    public static bool IsSettingsInitialized => settingsInitialized;
}
