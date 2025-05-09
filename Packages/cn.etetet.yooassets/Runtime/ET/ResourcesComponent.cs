﻿using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET
{
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

    public class ResourcesComponent : Singleton<ResourcesComponent>, ISingletonAwake
    {
        public void Awake()
        {
            YooAssets.Initialize();
        }

        protected override void Destroy()
        {
            YooAssets.OnApplicationQuit();
        }

        public async ETTask CreatePackageAsync(string packageName, bool isDefault = false)
        {
            YooConfig yooConfig = Resources.Load<YooConfig>("YooConfig");
            ResourcePackage package = YooAssets.CreatePackage(packageName);
            if (isDefault)
            {
                YooAssets.SetDefaultPackage(package);
            }

            // 编辑器下的模拟模式
            switch (yooConfig.EPlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                {
                    PackageInvokeBuildResult buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);    
                    string packageRoot = buildResult.PackageRootDirectory;
                    FileSystemParameters editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                    EditorSimulateModeParameters initParameters = new();
                    initParameters.EditorFileSystemParameters = editorFileSystemParams;
                    await package.InitializeAsync(initParameters).Task;
                    break;
                }
                case EPlayMode.OfflinePlayMode:
                {
                    FileSystemParameters buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                    OfflinePlayModeParameters initParameters = new();
                    initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                    await package.InitializeAsync(initParameters).Task;
                    break;
                }
                case EPlayMode.HostPlayMode:
                {
                    string defaultHostServer = GetHostServerURL(yooConfig.Url, package.PackageName);
                    string fallbackHostServer = GetHostServerURL(yooConfig.Url, package.PackageName);
                    IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    FileSystemParameters cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                    FileSystemParameters buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                    HostPlayModeParameters initParameters = new();
                    initParameters.BuildinFileSystemParameters = buildinFileSystemParams; 
                    initParameters.CacheFileSystemParameters = cacheFileSystemParams;
                    await package.InitializeAsync(initParameters).Task;
                    break;
                }
                case EPlayMode.WebPlayMode:
                {
                    string defaultHostServer = GetHostServerURL(yooConfig.Url, package.PackageName);
                    string fallbackHostServer = GetHostServerURL(yooConfig.Url, package.PackageName);
                    IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    FileSystemParameters webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                    FileSystemParameters webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices); //支持跨域下载
    
                    WebPlayModeParameters initParameters = new();
                    initParameters.WebServerFileSystemParameters = webServerFileSystemParams;
                    initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;

                    await package.InitializeAsync(initParameters).Task;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            RequestPackageVersionOperation op = package.RequestPackageVersionAsync();
            await op.Task;
            await package.UpdatePackageManifestAsync(op.PackageVersion).Task;
        }

        string GetHostServerURL(string url, string pacakgeName)
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = url;
            string appVersion = "v1.0";
                
                
#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.Android:
                    return $"{hostServerIP}/CDN/Android/{appVersion}";
                case UnityEditor.BuildTarget.iOS:
                    return $"{hostServerIP}/CDN/IPhone/{appVersion}";
                case UnityEditor.BuildTarget.WebGL:
                {
                    return $"{hostServerIP}/StreamingAssets/Bundles/{pacakgeName}";
                }
                default:
                    return $"{hostServerIP}/CDN/PC/{appVersion}";
            }
#else
		        switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        return $"{hostServerIP}/CDN/Android/{appVersion}";
                    case RuntimePlatform.IPhonePlayer:
                        return $"{hostServerIP}/CDN/IPhone/{appVersion}";
                    case RuntimePlatform.WebGLPlayer:
                    {
                        return $"{hostServerIP}/StreamingAssets/Bundles/{pacakgeName}";
                    }
                    default:
                        return $"{hostServerIP}/CDN/PC/{appVersion}";
                }
#endif
        }

        public void DestroyPackage(string packageName)
        {
            ResourcePackage package = YooAssets.GetPackage(packageName);
            package.DestroyPackage();
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            AssetHandle handle = YooAssets.LoadAssetAsync<T>(location);
            await handle.Task;
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
        {
            AllAssetsHandle allAssetsOperationHandle = YooAssets.LoadAllAssetsAsync<T>(location);
            await allAssetsOperationHandle.Task;
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in allAssetsOperationHandle.AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            allAssetsOperationHandle.Release();
            return dictionary;
        }
    }
}