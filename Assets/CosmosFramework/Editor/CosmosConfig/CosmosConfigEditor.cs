﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cosmos.QuarkAsset;
using Cosmos.Resource;
using System;
namespace Cosmos.CosmosEditor
{
    [CustomEditor(typeof(CosmosConfig), true)]
    public class CosmosConfigEditor : Editor
    {
        SerializedObject targetObject;
        CosmosConfig cosmosConfig;
        private void OnEnable()
        {
            cosmosConfig = target as CosmosConfig;
            targetObject = new SerializedObject(cosmosConfig);
        }
        public override void OnInspectorGUI()
        {
            targetObject.Update();
            cosmosConfig.LoadDefaultHelper = EditorGUILayout.Toggle("LoadDefaultHelper", cosmosConfig.LoadDefaultHelper);
            cosmosConfig.LaunchAppDomainModules = EditorGUILayout.Toggle("LaunchAppDomainModules", cosmosConfig.LaunchAppDomainModules);
            if (cosmosConfig.LaunchAppDomainModules)
            {
                cosmosConfig.PrintModulePreparatory = EditorGUILayout.Toggle("PrintModulePreparatory", cosmosConfig.PrintModulePreparatory);
            }
            cosmosConfig.ResourceLoadMode = (ResourceLoadMode)EditorGUILayout.EnumPopup("ResourceLoadMode", cosmosConfig.ResourceLoadMode);
            switch (cosmosConfig.ResourceLoadMode)
            {
                case ResourceLoadMode.QuarkAsset:
                    {
                        cosmosConfig.QuarkAssetLoadMode = (QuarkAssetLoadMode)EditorGUILayout.EnumPopup("QuarkAssetLoadMode", cosmosConfig.QuarkAssetLoadMode);
                        switch (cosmosConfig.QuarkAssetLoadMode)
                        {
                            case QuarkAssetLoadMode.BuiltAssetBundle:
                                {
                                    cosmosConfig.QuarkAssetBundleUrl = EditorGUILayout.TextField("QuarkAssetBundleUrl", cosmosConfig.QuarkAssetBundleUrl);
                                }
                                break;
                        }
                    }
                    break;
            }
            targetObject.ApplyModifiedProperties();
        }
    }
}