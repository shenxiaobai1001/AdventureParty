using UnityEngine;
using UnityEngine.UI;

public class UISystem : MonoBehaviour
{
    public GameObject center;
    public Image bgImage;

    public Slider musicSlider;
    public Text musicLabelText;
    public Text musicValueText;

    public Slider soundSlider;
    public Text soundLabelText;
    public Text soundValueText;

    public Toggle toggleShowPlayer;
    public Toggle toggleShowKid;
    public Toggle toggleShowRoad;

    public InputField inputField;
    public InputField inputFieldPlayerName;
    public InputField inputFieldKid;
    public InputField inputFieldRoadLine;
    public Dropdown dropdownKidSkin;
    public Dropdown dropdown_Difficulty;
    public Button btn_close;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        ModData.TryInitFromLocal();
        BindUI();
        EventManager.Instance.AddListener(Events.OnShowSystem, OnShowPanl);
    }

    void OnDestroy()
    {
        EventManager.Instance.RemoveListener(Events.OnShowSystem, OnShowPanl);
    }

    void OnShowPanl(object msg)
    {
        bool show = (bool)msg;
        center.SetActive(show);
    }

    void OnEnable()
    {
        ModData.TryInitFromLocal();
        RefreshFromModData();
    }

    void BindUI()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (soundSlider != null)
            soundSlider.onValueChanged.AddListener(OnSoundSliderChanged);

        if (toggleShowPlayer != null)
            toggleShowPlayer.onValueChanged.AddListener(OnToggleShowPlayer);
        if (toggleShowKid != null)
            toggleShowKid.onValueChanged.AddListener(OnToggleShowKid);
        if (toggleShowRoad != null)
            toggleShowRoad.onValueChanged.AddListener(OnToggleShowRoad);

        if (inputField != null)
            inputField.onEndEdit.AddListener(OnInputEndEdit);

        if (inputFieldPlayerName != null)
            inputFieldPlayerName.onEndEdit.AddListener(OnPlayerNameEndEdit);

        if (inputFieldKid != null)
            inputFieldKid.onEndEdit.AddListener(OnKid);

        if (inputFieldRoadLine != null)
            inputFieldRoadLine.onEndEdit.AddListener(OnRoadLineEndEdit);

        if (dropdownKidSkin != null)
            dropdownKidSkin.onValueChanged.AddListener(OnKidSkinDropdownChanged);

        if (dropdown_Difficulty != null)
            dropdown_Difficulty.onValueChanged.AddListener(OnDifficultyDropdownChanged);

        if (btn_close != null)
            btn_close.onClick.AddListener(OnClosePanel);
    }

    void OnClosePanel()
    {
        center.SetActive(false);
    }

    void RefreshFromModData()
    {
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(Sound.VolumeMusic);
        if (soundSlider != null)
            soundSlider.SetValueWithoutNotify(Sound.VolumeSound);
        UpdateVolumeTexts();

        if (toggleShowPlayer != null)
            toggleShowPlayer.SetIsOnWithoutNotify(ModData.showPlayerName);
        if (toggleShowKid != null)
            toggleShowKid.SetIsOnWithoutNotify(ModData.showKidName);
        if (toggleShowRoad != null)
            toggleShowRoad.SetIsOnWithoutNotify(ModData.showRoadName);

        if (inputField != null)
            inputField.SetTextWithoutNotify(ModData.settingsInputText);

        if (inputFieldPlayerName != null)
            inputFieldPlayerName.SetTextWithoutNotify(ModData.PlayerName);

        if (inputFieldKid != null)
            inputFieldKid.SetTextWithoutNotify(ModData.Kids.ToString());

        if (inputFieldRoadLine != null)
            inputFieldRoadLine.SetTextWithoutNotify(ModData.defaultRoadCount.ToString());

        if (dropdownKidSkin != null)
        {
            dropdownKidSkin.SetValueWithoutNotify(Mathf.Clamp(ModData.KidSkinType, 0, 1));
            dropdownKidSkin.RefreshShownValue();
        }

        if (dropdown_Difficulty != null)
        {
            dropdown_Difficulty.SetValueWithoutNotify(Mathf.Clamp(ModData.difficultyLevel, 0, 2));
            dropdown_Difficulty.RefreshShownValue();
        }

        ApplyDifficultyToCarSpawner();
    }

    void OnMusicSliderChanged(float value)
    {
        Sound.OnSetVolume(value, Sound.VolumeSound);
        UpdateVolumeTexts();
        ModData.SaveSettings();
    }

    void OnSoundSliderChanged(float value)
    {
        Sound.OnSetVolume(Sound.VolumeMusic, value);
        UpdateVolumeTexts();
        ModData.SaveSettings();
    }

    void UpdateVolumeTexts()
    {
        if (musicValueText != null)
            musicValueText.text = $"{Mathf.RoundToInt(Sound.VolumeMusic * 100)}%";
        if (soundValueText != null)
            soundValueText.text = $"{Mathf.RoundToInt(Sound.VolumeSound * 100)}%";
    }

    void OnToggleShowPlayer(bool value)
    {
        ModData.showPlayerName = value;
        ModData.SaveSettings();
        NotifySystemSettingsChanged();
    }

    void OnToggleShowKid(bool value)
    {
        ModData.showKidName = value;
        ModData.SaveSettings();
        NotifySystemSettingsChanged();
    }

    void OnToggleShowRoad(bool value)
    {
        ModData.showRoadName = value;
        ModData.SaveSettings();
        NotifySystemSettingsChanged();
    }

    void OnInputEndEdit(string text)
    {
        ModData.settingsInputText = text ?? "剩余小孩：_  通过：_   已噶：";
        ModData.SaveSettings();
        NotifySystemSettingsChanged();
    }

    void OnPlayerNameEndEdit(string text)
    {
        ModData.PlayerName = text ?? string.Empty;
        ModData.SaveSettings();
        NotifySystemSettingsChanged();
    }

    void OnKid(string text)
    {
        if (!int.TryParse(text, out int newCount))
            return;

        newCount = Mathf.Max(0, newCount);
        int delta = newCount - ModData.Kids;
        ModData.Kids = newCount;
        ModData.SaveSettings();

    }

    void OnRoadLineEndEdit(string text)
    {
        if (!int.TryParse(text, out int count))
            count = ModData.defaultRoadCount;

        ModData.defaultRoadCount = Mathf.Max(1, count);
        if (inputFieldRoadLine != null)
            inputFieldRoadLine.SetTextWithoutNotify(ModData.defaultRoadCount.ToString());
        ModData.SaveSettings();

    }

    void OnKidSkinDropdownChanged(int index)
    {
        ModData.KidSkinType = index;
        ModData.SaveSettings();
    }

    void OnDifficultyDropdownChanged(int index)
    {
        ModData.difficultyLevel = Mathf.Clamp(index, 0, 2);
        ModData.SaveSettings();
        ApplyDifficultyToCarSpawner();
    }

    static void NotifySystemSettingsChanged()
    {
        EventManager.Instance.SendMessage(Events.OnSystemSettingsChanged, null);
    }

    static void ApplyDifficultyToCarSpawner()
    {

    }
}
