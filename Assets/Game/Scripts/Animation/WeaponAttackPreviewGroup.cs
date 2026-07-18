using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One weapon/animation family. Spawns preview actors only while enabled so
/// switching groups stays cheap at runtime.
/// </summary>
public class WeaponAttackPreviewGroup : MonoBehaviour
{
    public const float ActorSpacing = 12f;
    public const float RowSpacing = 4.5f;
    public const int Columns = 8;

    [SerializeField] string groupName;
    [SerializeField] WeaponAttackPreviewKit kit;
    [SerializeField] GameObject characterPrefab;
    [SerializeField] AnimationClip[] clips;
    [SerializeField] string[] labels;

    readonly List<GameObject> _spawned = new List<GameObject>();
    Transform _actorRoot;

    public string GroupName => string.IsNullOrEmpty(groupName) ? name : groupName;
    public int ClipCount => clips != null ? clips.Length : 0;

    public void Configure(
        string displayName,
        WeaponAttackPreviewKit weaponKit,
        GameObject prefab,
        AnimationClip[] animationClips,
        string[] clipLabels)
    {
        groupName = displayName;
        kit = weaponKit;
        characterPrefab = prefab;
        clips = animationClips;
        labels = clipLabels;
        name = displayName;
    }

    void OnEnable()
    {
        Spawn();
    }

    void OnDisable()
    {
        ClearSpawned();
    }

    void Spawn()
    {
        ClearSpawned();

        if (!characterPrefab || clips == null || clips.Length == 0)
            return;

        if (!_actorRoot)
        {
            var rootGo = new GameObject("Actors");
            _actorRoot = rootGo.transform;
            _actorRoot.SetParent(transform, false);
        }

        for (var i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            if (!clip)
                continue;

            var label = labels != null && i < labels.Length && !string.IsNullOrEmpty(labels[i])
                ? labels[i]
                : clip.name;

            var actor = Instantiate(characterPrefab, _actorRoot);
            actor.name = $"{i + 1:00}_{label}";
            actor.SetActive(false);

            var col = i % Columns;
            var row = i / Columns;
            actor.transform.localPosition = new Vector3(col * ActorSpacing, 0f, row * RowSpacing);
            actor.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            WeaponAttackPreviewUtil.StripGameplayScripts(actor);
            WeaponAttackPreviewUtil.ApplyWeaponKit(actor, kit);
            WeaponAttackPreviewUtil.SoftenRendererCost(actor);

            var animator = actor.GetComponentInChildren<Animator>(true);
            if (!animator)
                animator = actor.AddComponent<Animator>();

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = null;

            var player = animator.gameObject.GetComponent<AttackPreviewClipPlayer>();
            if (!player)
                player = animator.gameObject.AddComponent<AttackPreviewClipPlayer>();

            var speed = label.IndexOf("Cast", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? 0.5f
                : 1f;
            player.Configure(clip, label, speed);

            actor.SetActive(true);
            _spawned.Add(actor);
        }
    }

    void ClearSpawned()
    {
        for (var i = 0; i < _spawned.Count; i++)
        {
            if (!_spawned[i])
                continue;

            if (Application.isPlaying)
                Destroy(_spawned[i]);
            else
                DestroyImmediate(_spawned[i]);
        }

        _spawned.Clear();

        if (_actorRoot)
        {
            for (var i = _actorRoot.childCount - 1; i >= 0; i--)
            {
                var child = _actorRoot.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }
    }
}
