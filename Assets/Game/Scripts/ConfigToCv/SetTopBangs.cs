using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTopBangs : MonoBehaviour {
    void Start() {
        if((float)Screen.height / Screen.width <= 2)
            return;

        var num = -60;

        var rect = gameObject.GetComponent<RectTransform>();

        if (rect.anchorMin.y == 1 && rect.anchorMax.y == 1)//贴顶部
        {
            gameObject.GetComponent<RectTransform>().localPosition=new Vector3(0, num);
        } else if(rect.anchorMin.y == 0 && rect.anchorMax.y == 1)//同时贴顶部和底部
          {
            Vector2 offsetMax = rect.offsetMax;
            offsetMax.y += num;
            rect.offsetMax = offsetMax;
        }
    }
}