using Game.Scripts.Main;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using YooAsset;

public static class Loaded
{
    public static string resourcePath = "Assets/Game/Resources_moved/";
    public static string videoFilePath = "Assets/Game/Resources_video/";
    public static string scencePath = "Assets/Game/Scenes/";
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam 类型="T"></typeparam>
    /// <param 路径="url"></param>
    public static T Load<T>(string url) where T : UnityEngine.Object
    {
        string filename = "";
        if (typeof(T) == typeof(GameObject))      //预制体
        {
            filename = $"{resourcePath}Prefabs/{url}"; 
        }
        else if (typeof(T) == typeof(TextAsset))//表格
        {
            filename = $"{resourcePath}Config/{url}";
        }
        else if (typeof(T) == typeof(Sprite))//图片
        {
            filename = $"{resourcePath}Textures/{url}";
        }
        else if (typeof(T) == typeof(AudioClip))//音频
        {
            filename = $"{resourcePath}Media/{url}";
        }
        else if (typeof(T) == typeof(VideoClip))//视频
        {
            filename = $"{resourcePath}Media/video/{url}";
        }
        else if (typeof(T) == typeof(ScriptableObject)) //配置
        {
            filename = $"{resourcePath}ScriptableObject/{url}";
        }

        var handle = YooUpdateManager.MainPackage.LoadAssetSync<T>(filename/*.ToLower()*/);

        if (handle?.AssetObject != null)
        {
            return handle.AssetObject as T;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 加载所有资源
    /// </summary>
    public static YooAsset.AllAssetsHandle LoadAll<T>(string url) where T : UnityEngine.Object
    {
        var filename = resourcePath + url;
        var handle = YooUpdateManager.MainPackage.LoadAllAssetsAsync<T>(filename/*.ToLower()*/);

        if (handle.AllAssetObjects != null)
        {
            return handle;
        }
        else
        {
            return null;
        }
    }

    /// <summary>加载场景 </summary>
    public static SceneHandle OnLoadScence(string name)
    {
        if (YooUpdateManager.MainPackage != null)
        {
            var sceneMode = LoadSceneMode.Single;
            var physicsMode = LocalPhysicsMode.None;
            var suspendLoad = false;
            string location = $"{scencePath}{name}";
            Debug.Log("加载场景："+location);

            return YooUpdateManager.MainPackage.LoadSceneAsync(location, sceneMode, physicsMode, suspendLoad);
        }
         
        return null;
    }    

    /// <summary>加载视频 </summary>
    public static async Task<string> OnLoadVideoAsync(string location)
    {
        string videoPath = videoFilePath + location;
        if (YooUpdateManager.VideoPackage == null) return string.Empty;
        var handle = YooUpdateManager.VideoPackage.LoadRawFileAsync(videoPath);
        await handle.Task;
        string loadedVideoPath = handle.GetRawFilePath();
        handle.Release();    // 释放加载的资源
        return loadedVideoPath;
    }
}
