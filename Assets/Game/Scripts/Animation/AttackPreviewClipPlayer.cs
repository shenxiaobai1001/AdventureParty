using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Loops a single AnimationClip via Playables and shows an overhead label.
/// Used by the weapon / cast attack preview scene.
/// </summary>
[RequireComponent(typeof(Animator))]
[DefaultExecutionOrder(100)]
public class AttackPreviewClipPlayer : MonoBehaviour
{
    [SerializeField] AnimationClip clip;
    [SerializeField] string displayName;
    [SerializeField] float playbackSpeed = 1f;
    [SerializeField] float labelHeight = 2.15f;
    [SerializeField] float labelCharacterSize = 0.11f;
    [SerializeField] int labelFontSize = 32;

    PlayableGraph _graph;
    AnimationClipPlayable _playable;
    TextMesh _label;
    Transform _billboard;
    Camera _camera;

    public void Configure(AnimationClip animationClip, string label, float speed = 1f)
    {
        clip = animationClip;
        displayName = label;
        playbackSpeed = Mathf.Max(0.01f, speed);
        EnsureLabel();
        if (isActiveAndEnabled)
            StartLoop();
    }

    void OnEnable()
    {
        EnsureLabel();
        StartLoop();
    }

    void OnDisable()
    {
        DestroyGraph();
    }

    void Update()
    {
        if (!_playable.IsValid() || !clip)
            return;

        var length = clip.length;
        if (length <= 0.0001f)
            return;

        var time = _playable.GetTime();
        if (time >= length)
            _playable.SetTime(time % length);
    }

    void LateUpdate()
    {
        if (!_billboard)
            return;

        if (!_camera)
            _camera = Camera.main;

        if (!_camera)
            return;

        var toCam = _billboard.position - _camera.transform.position;
        if (toCam.sqrMagnitude <= 0.001f)
            return;

        _billboard.rotation = Quaternion.LookRotation(toCam) * Quaternion.Euler(0f, 180f, 0f);
    }

    void StartLoop()
    {
        DestroyGraph();

        if (!clip)
            return;

        var animator = GetComponent<Animator>();
        if (!animator)
            animator = gameObject.AddComponent<Animator>();

        animator.enabled = true;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.runtimeAnimatorController = null;
        animator.Rebind();
        animator.Update(0f);

        _graph = PlayableGraph.Create($"AttackPreview_{displayName}");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var output = AnimationPlayableOutput.Create(_graph, "Animation", animator);
        _playable = AnimationClipPlayable.Create(_graph, clip);
        _playable.SetApplyFootIK(false);
        _playable.SetTime(0);
        _playable.SetDuration(double.MaxValue);
        _playable.SetSpeed(playbackSpeed);
        _playable.Play();
        output.SetSourcePlayable(_playable);
        _graph.Play();
    }

    void EnsureLabel()
    {
        if (_label)
        {
            _label.text = displayName;
            return;
        }

        var go = new GameObject("PreviewLabel");
        _billboard = go.transform;
        _billboard.SetParent(transform, false);
        _billboard.localPosition = new Vector3(0f, labelHeight, 0f);

        _label = go.AddComponent<TextMesh>();
        _label.text = string.IsNullOrEmpty(displayName) ? "(clip)" : displayName;
        _label.characterSize = labelCharacterSize;
        _label.fontSize = labelFontSize;
        _label.anchor = TextAnchor.MiddleCenter;
        _label.alignment = TextAlignment.Center;
        _label.color = Color.white;
        // Prefer default built-in font; never touch TMP outline APIs (they NRE without a font asset).
        _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                      ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    void DestroyGraph()
    {
        if (_graph.IsValid())
            _graph.Destroy();

        _playable = default;
    }
}
