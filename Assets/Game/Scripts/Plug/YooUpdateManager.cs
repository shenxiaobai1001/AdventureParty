using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YooAsset;

namespace Game.Scripts.Main
{
    public partial class YooUpdateManager : MonoBehaviour
    {
        //主体资源包
        public static ResourcePackage package = null;

        //主体资源包
        public static ResourcePackage MainPackage = null;
        //主体资源包
        public static ResourcePackage VideoPackage = null;
        //默认包名
        public static string mainPackageName = "DefaultPackage";
        public static string videoPackageName = "VideoPackage";

        public static string packageName = "DefaultPackage";

        /// <summary> YooAsset运行模式</summary>
        public static EPlayMode mPlayMode = EPlayMode.OfflinePlayMode;

        /// <summary> 热更新资源地址</summary>
        static public string UpdateResUrl = "";
        public static string packageVersion = "";
        private void Awake()
        {
            Debug.Log("YooUpdateManager开始运行=" );
            // 必须关闭垂直同步
            QualitySettings.vSyncCount = 0;  // 关键！必须先设置这个
            // 设置目标帧率
            Application.targetFrameRate = 144;
            StartCoroutine(OnInitPackage());
        }

        IEnumerator OnInitPackage()
        {
            packageName = mainPackageName;
            yield return Init(packageName);
            packageName = videoPackageName;
            yield return Init(packageName);
            yield return OnStartLoadInitScene();
        }


        /// <summary>初始化YooAsset》设置运行模式》检查资源版本》下载资源 </summary>
        public static IEnumerator Init(string initpackage)
        {
            if (!YooAssets.Initialized)
            {
                // 初始化资源系统.
                YooAssets.Initialize();
            }

            // 创建默认的资源包
            package = YooAssets.TryGetPackage(initpackage);
            if (package == null)
                package = YooAssets.CreatePackage(initpackage);

            Debug.Log("开始初始化资源包=" + package.PackageName);
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);

            // 设置YooAsset运行模式
            if (Application.isEditor) mPlayMode = EPlayMode.EditorSimulateMode;

            // 根据运行模式启动资源下载
            switch (mPlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                    yield return YooBuildByEditorSimulateMode();
                    yield return RequestPackageVersion(); //获取资源版本
                    yield return UpdatePackageManifest(); //获取资源包清单
                    break;
                case EPlayMode.OfflinePlayMode:
                    yield return YooBuildByOfflinePlayMode();
                    yield return RequestPackageVersion(); //获取资源版本
                    yield return UpdatePackageManifest(); //获取资源包清单
                    break;
            }
        }
        public static IEnumerator YooBuildByEditorSimulateMode()
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
                Debug.Log("资源包初始化成功！");
            else
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }

        public static IEnumerator YooBuildByOfflinePlayMode()
        {
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
                Debug.Log("资源包初始化成功！");
            else
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }
        public static IEnumerator RequestPackageVersion()
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                packageVersion = operation.PackageVersion;
                Debug.Log($"Request package Version : {packageVersion}");
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }

        public static IEnumerator UpdatePackageManifest()
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                switch (packageName)
                {
                    case "DefaultPackage":
                        MainPackage = package;
                        break;
                    case "VideoPackage":
                        VideoPackage = package;
                        break;
                }

            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }
        public static IEnumerator OnStartLoadInitScene()
        {
            Debug.Log("开始加载场景====" + YooUpdateManager.MainPackage);
            var location = "Assets/Game/Scenes/PPTScence";
            var sceneMode = LoadSceneMode.Single;
            var physicsMode = LocalPhysicsMode.None;
            var suspendLoad = false;
            var handle = YooUpdateManager.MainPackage.LoadSceneAsync(location, sceneMode, physicsMode, suspendLoad);
            yield return handle;
        }
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    public class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
}