﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Cosmos.QuarkAsset;
namespace Cosmos.Test
{
    public class QuarkAssetLoader : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void RuntimeCallback()
        {
            Debug.Log("RuntimeInitializeOnLoadMethod");
        }
        void Start()
        {
            var go = QuarkUtility.LoadAsset<GameObject>("YBot_LM_Local");
            if (go != null)
                Instantiate(go);
            else
                Debug.LogError("go null");
        }
    }
}