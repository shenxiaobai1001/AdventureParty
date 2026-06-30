using DG.Tweening.Core.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastRunEffect : MonoBehaviour
{
    public float invokeTime = 0.5f;
    public bool toPool = false;
    // Start is called before the first frame update
    void OnEnable()
    {
        Invoke("OnReadyDestory", invokeTime);
    }

    void OnReadyDestory()
    {
        if (toPool)
        {
            SimplePool.Despawn(gameObject);
            gameObject.SetActive(false);
        }
        else 
            Destroy(gameObject);
    }
}
