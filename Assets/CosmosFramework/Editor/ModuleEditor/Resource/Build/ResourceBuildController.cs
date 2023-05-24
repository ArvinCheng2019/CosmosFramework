﻿using Cosmos.Resource;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Cosmos.Editor.Resource
{
    /// <summary>
    /// AB打包构建器
    /// </summary>
    public class ResourceBuildController
    {
        public static void BuildDataset(ResourceDataset dataset)
        {
            if (dataset == null)
                return;
            var bundleInfos = dataset.ResourceBundleInfoList;
            var extensions = dataset.ResourceAvailableExtenisonList;
            var lowerExtensions = extensions.Select(s => s.ToLower()).ToArray();
            extensions.Clear();
            extensions.AddRange(lowerExtensions);
            var bundleInfoLength = bundleInfos.Count;

            List<ResourceBundleInfo> invalidBundleInfos = new List<ResourceBundleInfo>();

            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundleInfo = bundleInfos[i];
                var bundlePath = bundleInfo.BundlePath;
                if (!AssetDatabase.IsValidFolder(bundlePath))
                {
                    invalidBundleInfos.Add(bundleInfo);
                    continue;
                }
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                importer.assetBundleName = bundleInfo.BundleName;

                var files = Utility.IO.GetAllFiles(bundlePath);
                var fileLength = files.Length;
                bundleInfo.ResourceObjectInfoList.Clear();
                for (int j = 0; j < fileLength; j++)
                {
                    var srcFilePath = files[j].Replace("\\", "/");
                    var srcFileExt = Path.GetExtension(srcFilePath);
                    var lowerFileExt = srcFileExt.ToLower();
                    if (extensions.Contains(lowerFileExt))
                    {
                        //统一使用小写的文件后缀名
                        var lowerExtFilePath = srcFilePath.Replace(srcFileExt, lowerFileExt);

                        var resourceObjectInfo = new ResourceObjectInfo()
                        {
                            BundleName = bundleInfo.BundleName,
                            Extension = lowerFileExt,
                            ObjectName = Path.GetFileNameWithoutExtension(lowerExtFilePath),
                            ObjectPath = lowerExtFilePath,
                            ObjectSize = EditorUtil.GetAssetFileSizeLength(lowerExtFilePath),
                            ObjectFormatBytes = EditorUtil.GetAssetFileSize(lowerExtFilePath),
                        };
                        resourceObjectInfo.ObjectVaild = AssetDatabase.LoadMainAssetAtPath(resourceObjectInfo.ObjectPath) != null;
                        bundleInfo.ResourceObjectInfoList.Add(resourceObjectInfo);
                    }
                }
                long bundleSize = EditorUtil.GetUnityDirectorySize(bundlePath, dataset.ResourceAvailableExtenisonList);
                bundleInfo.BundleSize = bundleSize;
                bundleInfo.BundleKey = bundleInfo.BundleName;
                bundleInfo.BundleFormatBytes = EditorUtility.FormatBytes(bundleSize);
            }
            for (int i = 0; i < invalidBundleInfos.Count; i++)
            {
                bundleInfos.Remove(invalidBundleInfos[i]);
            }
            for (int i = 0; i < bundleInfos.Count; i++)
            {
                var bundle = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundle.BundlePath);
                bundle.DependentBundleKeyList.Clear();
                bundle.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
            }
            for (int i = 0; i < bundleInfos.Count; i++)
            {
                var bundle = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundle.BundlePath);
                importer.assetBundleName = string.Empty;
            }
            EditorUtility.SetDirty(dataset);
#if UNITY_2021_1_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(dataset);
#elif UNITY_2019_1_OR_NEWER
            AssetDatabase.SaveAssets();
#endif
            dataset.IsChanged = false;
        }
        public static void PrepareBuildAssetBundle(ResourceBuildParams buildParams, List<ResourceBundleInfo> bundleInfos, ref ResourceManifest resourceManifest)
        {
            if (Directory.Exists(buildParams.AssetBundleBuildPath))
                Utility.IO.DeleteFolder(buildParams.AssetBundleBuildPath);
            Directory.CreateDirectory(buildParams.AssetBundleBuildPath);

            var assetBundleNameType = buildParams.AssetBundleNameType;

            var bundleInfoLength = bundleInfos.Count;

            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundleInfo = bundleInfos[i];
                //过滤空包。若文件夹被标记为bundle，且不包含内容，则unity会过滤。因此遵循unity的规范；
                if (bundleInfo.ResourceObjectInfoList.Count <= 0)
                    continue;
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                //这里获取绝对ab绝对路径下，所有资源的bytes，生成唯一MD5 hash
                var path = Path.Combine(EditorUtil.ApplicationPath(), bundleInfo.BundlePath);
                var hash = ResourceUtility.CreateDirectoryMd5(path);
                var bundleKey = string.Empty;
                switch (assetBundleNameType)
                {
                    case AssetBundleNameType.DefaultName:
                        {
                            importer.assetBundleName = bundleInfo.BundleName;
                            bundleKey = bundleInfo.BundleName;
                        }
                        break;
                    case AssetBundleNameType.HashInstead:
                        {
                            importer.assetBundleName = hash;
                            bundleKey = hash;
                            bundleInfo.BundleKey = hash;
                        }
                        break;
                }
                var bundle = new ResourceBundle()
                {
                    BundleKey = bundleKey,
                    BundleName = bundleInfo.BundleName,
                    BundlePath = bundleInfo.BundlePath,
                };
                var objectInfoList = bundleInfo.ResourceObjectInfoList;
                var objectInfoLength = objectInfoList.Count;
                for (int j = 0; j < objectInfoLength; j++)
                {
                    var objectInfo = objectInfoList[j];
                    var resourceObject = new ResourceObject()
                    {
                        ObjectName = objectInfo.ObjectName,
                        ObjectPath = objectInfo.ObjectPath,
                        BundleName = objectInfo.BundleName,
                        Extension = objectInfo.Extension
                    };
                    bundle.ResourceObjectList.Add(resourceObject);
                }
                var bundleBuildInfo = new ResourceManifest.ResourceBundleBuildInfo()
                {
                    BundleHash = hash,
                    ResourceBundle = bundle,
                    BundleSize = 0
                };
                //这里存储hash与bundle，打包出来的包体长度在下一个流程处理
                resourceManifest.ResourceBundleBuildInfoDict.Add(bundleInfo.BundleName, bundleBuildInfo);
            }
            //refresh assetbundle
            AssetDatabase.Refresh();
            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundleInfo = bundleInfos[i];
                bundleInfo.DependentBundleKeyList.Clear();
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                bundleInfo.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
                if (resourceManifest.ResourceBundleBuildInfoDict.TryGetValue(bundleInfo.BundleName, out var bundleBuildInfo))
                {
                    bundleBuildInfo.ResourceBundle.DependentBundleKeytList.Clear();
                    bundleBuildInfo.ResourceBundle.DependentBundleKeytList.AddRange(bundleInfo.DependentBundleKeyList);
                }
            }
        }
        public static void ProcessAssetBundle(ResourceBuildParams buildParams, List<ResourceBundleInfo> bundleInfos, AssetBundleManifest unityManifest, ref ResourceManifest resourceManifest)
        {
            Dictionary<string, ResourceBundleInfo> bundleKeyDict = null;
            if (buildParams.AssetBundleNameType == AssetBundleNameType.HashInstead)
                bundleKeyDict = bundleInfos.ToDictionary(bundle => bundle.BundleKey);
            var bundleKeys = unityManifest.GetAllAssetBundles();
            var bundleKeyLength = bundleKeys.Length;
            for (int i = 0; i < bundleKeyLength; i++)
            {
                var bundleKey = bundleKeys[i];
                var bundlePath = Path.Combine(buildParams.AssetBundleBuildPath, bundleKey);
                long bundleSize = 0;
                if (buildParams.AssetBundleEncryption)
                {
                    var bundleBytes = File.ReadAllBytes(bundlePath);
                    var offset = buildParams.AssetBundleOffsetValue;
                    bundleSize = Utility.IO.AppendAndWriteAllBytes(bundlePath, new byte[offset], bundleBytes);
                }
                else
                {
                    var bundleBytes = File.ReadAllBytes(bundlePath);
                    bundleSize = bundleBytes.LongLength;
                }
                var bundleName = string.Empty;
                switch (buildParams.AssetBundleNameType)
                {
                    case AssetBundleNameType.DefaultName:
                        {
                            bundleName = bundleKey;
                        }
                        break;
                    case AssetBundleNameType.HashInstead:
                        {
                            if (bundleKeyDict.TryGetValue(bundleKey, out var bundleInfo))
                                bundleName = bundleInfo.BundleName;
                        }
                        break;
                }
                if (resourceManifest.ResourceBundleBuildInfoDict.TryGetValue(bundleName, out var resourceBundleBuildInfo))
                {
                    //这里存储打包出来的AB长度
                    resourceBundleBuildInfo.BundleSize = bundleSize;
                }
                var bundleManifestPath = Utility.Text.Append(bundlePath, ".manifest");
                Utility.IO.DeleteFile(bundleManifestPath);
            }

            var bundleInfoLength = bundleInfos.Count;

            #region 还原dataset在editor环境下的依赖
            //这段还原dataset在editor模式的依赖，并还原bundleKey；
            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundleInfo = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                importer.assetBundleName = bundleInfo.BundleName;
                bundleInfo.BundleKey = bundleInfo.BundleName;
            }
            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundleInfo = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                bundleInfo.DependentBundleKeyList.Clear();
                bundleInfo.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
            }
            #endregion

            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundle = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundle.BundlePath);
                importer.assetBundleName = string.Empty;
            }
            //refresh assetbundle
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            System.GC.Collect();
        }
        public static void PorcessManifest(ResourceBuildParams buildParams, ref ResourceManifest resourceManifest)
        {
            //这段生成resourceManifest.json文件
            var encryptionKey = buildParams.ManifestEncryptionKey;
            var encrypt = buildParams.EncryptManifest;
            resourceManifest.BuildVersion = buildParams.BuildVersion;
            var manifestJson = EditorUtil.Json.ToJson(resourceManifest);
            var manifestContext = manifestJson;
            if (encrypt)
            {
                var key = ResourceUtility.GenerateBytesAESKey(encryptionKey);
                manifestContext = Utility.Encryption.AESEncryptStringToString(manifestJson, key);
            }
            Utility.IO.WriteTextFile(buildParams.AssetBundleBuildPath, ResourceConstants.RESOURCE_MANIFEST, manifestContext);

            //删除生成文对应的主manifest文件
            var buildVersionPath = Path.Combine(buildParams.AssetBundleBuildPath, buildParams.BuildVersion);
            var buildVersionManifestPath = Utility.Text.Append(buildVersionPath, ".manifest");
            Utility.IO.DeleteFile(buildVersionPath);
            Utility.IO.DeleteFile(buildVersionManifestPath);
        }
        public static void BuildDoneOption(ResourceBuildParams buildParams)
        {
            if (buildParams.CopyToStreamingAssets)
            {
                string streamingAssetPath = string.Empty;
                if (buildParams.UseStreamingAssetsRelativePath)
                    streamingAssetPath = Path.Combine(Application.streamingAssetsPath, buildParams.StreamingAssetsRelativePath);
                else
                    streamingAssetPath = Application.streamingAssetsPath;
                var buildPath = buildParams.AssetBundleBuildPath;
                if (Directory.Exists(buildPath))
                {
                    Utility.IO.CopyDirectory(buildPath, streamingAssetPath);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        public static void BuildAssetBundle(ResourceDataset dataset, ResourceBuildParams buildParams)
        {
            if (dataset == null)
                return;
            BuildDataset(dataset);
            ResourceManifest resourceManifest = new ResourceManifest();
            var bundleInfos = dataset.GetResourceBundleInfos();
            PrepareBuildAssetBundle(buildParams, bundleInfos, ref resourceManifest);
            var unityManifest = BuildPipeline.BuildAssetBundles(buildParams.AssetBundleBuildPath, buildParams.BuildAssetBundleOptions, buildParams.BuildTarget);
            ProcessAssetBundle(buildParams, bundleInfos, unityManifest, ref resourceManifest);
            PorcessManifest(buildParams, ref resourceManifest);
            BuildDoneOption(buildParams);
        }
    }
}