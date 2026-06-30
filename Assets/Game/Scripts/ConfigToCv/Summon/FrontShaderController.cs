using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FrontShaderController : MonoBehaviour {
    public float Size;
    [Range(0f, 1f)]
    public float MagicCut;
    private MeshRenderer MR;

    private void Start() {
        MR = GetComponent<MeshRenderer>();
    }

    private void Update() {
        MR.material.SetFloat("_Size", Size);
        MR.material.SetFloat("_MagicCut", MagicCut);
    }
}