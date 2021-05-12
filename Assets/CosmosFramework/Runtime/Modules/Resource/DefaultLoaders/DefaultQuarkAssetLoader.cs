﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cosmos.QuarkAsset;
using Cosmos.Resource;

namespace Cosmos
{
    public class DefaultQuarkAssetLoader : IResourceLoadHelper
    {
        public T[] LoadAllAsset<T>(AssetInfo info) where T : UnityEngine.Object
        {
            return null;
        }
        public T LoadAsset<T>(AssetInfo info) where T : UnityEngine.Object
        {
            return QuarkUtility.LoadAssetByName<T>(info.AssetName);
        }
        public Coroutine LoadAssetAsync<T>(AssetInfo info, Action<T> loadDoneCallback, Action<float> loadingCallback = null) where T : UnityEngine.Object
        {
            return Utility.Unity.StartCoroutine(() => QuarkUtility.LoadAssetByName<T>(info.AssetName));
        }
        public Coroutine LoadSceneAsync(SceneAssetInfo info, Action loadDoneCallback, Action<float> loadingCallback = null)
        {
            return null;
        }
        public void UnLoadAllAsset(bool unloadAllLoadedObjects = false)
        {
            QuarkUtility.UnLoadAsset();
        }
        public void UnLoadAsset(object customData, bool unloadAllLoadedObjects = false)
        {
            QuarkUtility.UnLoadAsset();
        }
    }
}
