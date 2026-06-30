using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    public static GameObjectPool Instance { get; private set; }

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bulletPrefab2;
    [SerializeField] private GameObject wolfBullet;
    [SerializeField] private GameObject redWolf;
    [SerializeField] private GameObject ballBoom;
    [SerializeField] private GameObject pinkWolf;
    [SerializeField] private GameObject gradeHint;
    [SerializeField] private GameObject balloon;
    [SerializeField] private GameObject candy;
    [SerializeField] private GameObject electricity;
    [SerializeField] private GameObject fireBall;
    [SerializeField] private GameObject firePill;
    [SerializeField] private int initialPoolSize = 10;

    List<GameObject> allGameObjects = new List<GameObject>();
    bool isClearPool = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    private void CleanQueue<T>(Queue<T> queue) where T : class
    {
        if (queue == null || queue.Count == 0) return;

        var validItems = new List<T>();
        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            if (IsValidObject(item))
            {
                validItems.Add(item);
            }
        }

        foreach (var validItem in validItems)
        {
            queue.Enqueue(validItem);
        }
    }

    private bool IsValidObject<T>(T obj) where T : class
    {
        if (obj == null) return false;
        if (obj is Component component)
        {
            return component != null; // UnityµÄnull¼ì²é
        }
        return true;
    }

}
