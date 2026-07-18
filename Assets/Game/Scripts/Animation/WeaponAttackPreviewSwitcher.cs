using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cycles weapon preview groups. Only one group is active at a time.
/// </summary>
public class WeaponAttackPreviewSwitcher : MonoBehaviour
{
    [SerializeField] WeaponAttackPreviewGroup[] groups;
    [SerializeField] TextMeshProUGUI titleLabel;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;

    int _index;

    public void Configure(WeaponAttackPreviewGroup[] previewGroups, TextMeshProUGUI label, Button prev, Button next)
    {
        groups = previewGroups;
        titleLabel = label;
        prevButton = prev;
        nextButton = next;
        WireButtons();

        // Keep all groups off while building/saving the scene; Start() shows the first on Play.
        if (groups != null)
        {
            for (var i = 0; i < groups.Length; i++)
            {
                if (groups[i])
                    groups[i].gameObject.SetActive(false);
            }
        }

        if (titleLabel && groups != null && groups.Length > 0 && groups[0])
        {
            titleLabel.text =
                $"1/{groups.Length}  {groups[0].GroupName}  ({groups[0].ClipCount} clips)\n" +
                "Press Play, then use Prev/Next (or A/D)";
        }
    }

    void Awake()
    {
        WireButtons();
    }

    void Start()
    {
        if (groups != null && groups.Length > 0)
        {
            Show(_index);
            FrameCamera();
        }
    }

    void Update()
    {
        if (groups == null || groups.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.E))
            Next();
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Q))
            Prev();
    }

    void WireButtons()
    {
        if (prevButton)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(Prev);
        }

        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(Next);
        }
    }

    public void Next()
    {
        if (groups == null || groups.Length == 0)
            return;
        Show((_index + 1) % groups.Length);
        FrameCamera();
    }

    public void Prev()
    {
        if (groups == null || groups.Length == 0)
            return;
        Show((_index - 1 + groups.Length) % groups.Length);
        FrameCamera();
    }

    void Show(int index)
    {
        if (groups == null || groups.Length == 0)
            return;

        _index = Mathf.Clamp(index, 0, groups.Length - 1);

        for (var i = 0; i < groups.Length; i++)
        {
            if (!groups[i])
                continue;
            groups[i].gameObject.SetActive(i == _index);
        }

        var group = groups[_index];
        if (titleLabel && group)
        {
            titleLabel.text =
                $"{_index + 1}/{groups.Length}  {group.GroupName}  ({group.ClipCount} clips)\n" +
                "← / A / Q  Prev     Next  → / D / E";
        }
    }

    void FrameCamera()
    {
        var cam = Camera.main;
        if (!cam || groups == null || groups.Length == 0)
            return;

        var group = groups[_index];
        if (!group)
            return;

        var count = Mathf.Max(1, group.ClipCount);
        var cols = Mathf.Min(WeaponAttackPreviewGroup.Columns, count);
        var rows = Mathf.CeilToInt(count / (float)WeaponAttackPreviewGroup.Columns);
        var width = cols * WeaponAttackPreviewGroup.ActorSpacing;
        var depth = Mathf.Max(1, rows) * WeaponAttackPreviewGroup.RowSpacing;
        var look = new Vector3(width * 0.4f, 1.2f, depth * 0.35f);

        cam.transform.position = look + new Vector3(0f, Mathf.Max(14f, depth * 0.4f), -Mathf.Max(16f, depth * 0.45f + width * 0.15f));
        cam.transform.LookAt(look);
        cam.fieldOfView = 55f;
    }
}
