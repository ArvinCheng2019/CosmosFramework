﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cosmos.Quark;
using Cosmos.Resource;
namespace Cosmos
{
    [DefaultExecutionOrder(2000)]
    public class CosmosConfig : MonoBehaviour
    {
        [Header("Cosmos入口配置")]
        public bool LaunchAppDomainModules = true;
        public bool PrintModulePreparatory = true;
        public bool LoadDefaultHelper = true;
        public ResourceLoadMode ResourceLoadMode;
        public QuarkAssetLoadMode QuarkAssetLoadMode;
        public string QuarkAssetBundleUrl = "StreamingAssets";
        protected virtual void Awake()
        {
            if (LoadDefaultHelper)
                CosmosEntry.LaunchAppDomainHelpers();
            CosmosEntry.PrintModulePreparatory = PrintModulePreparatory;
            if (LaunchAppDomainModules)
            {
                CosmosEntry.LaunchAppDomainModules();
                CosmosEntry.ResourceManager.SwitchBuildInLoadMode(ResourceLoadMode);
                if (ResourceLoadMode == ResourceLoadMode.QuarkAsset)
                {
                    QuarkUtility.QuarkAssetLoadMode = QuarkAssetLoadMode;
                    switch (QuarkAssetLoadMode)
                    {
                        case QuarkAssetLoadMode.BuiltAssetBundle:
                            QuarkManager.Instance.LocalAssetBundleUrl = QuarkAssetBundleUrl;
                            break;
                    }
                }
            }
        }
    }
}