using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fullscreen : MonoBehaviour {
    private void Start() {
        var canvas = FindObjectOfType<CanvasScaler>();
        Vector2 realSize = canvas.gameObject.GetComponent<RectTransform>().sizeDelta;
        Vector2 theoreSize = canvas.referenceResolution;
        float scale = Mathf.Max(realSize.x / theoreSize.x, realSize.y / theoreSize.y);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}