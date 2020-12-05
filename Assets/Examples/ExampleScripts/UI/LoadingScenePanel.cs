﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cosmos.UI;
using Cosmos;
using Cosmos.Scene;
/// <summary>
/// 显示进度的脚本
/// </summary>
public class LoadingScenePanel : UILogicResident
{
    Text txtProgress;
    Slider sldProgress;
    ISceneManager sceneManager;

    protected override void OnInitialization()
    {
        txtProgress = GetUIForm<Text>("TxtProgress");
        sldProgress = GetUIForm<Slider>("SldProgress");
        sceneManager = GameManager.GetModule<ISceneManager>();
        LoadLevel();
    }
    void LoadLevel()
    {
        sldProgress.value = 0;
        if (Utility.Text.IsNumeric(LevelLoadInfo.TargetLevel))
        {
            int index = int.Parse(LevelLoadInfo.TargetLevel);
            sceneManager.LoadSceneAsync(index, UpdateSlider);
        }
        else
            sceneManager.LoadSceneAsync(LevelLoadInfo.TargetLevel, UpdateSlider);
    }
    void UpdateSlider(float value)
    {
        sldProgress.value = value * 100;
        txtProgress.text = (int)sldProgress.value + "%";
        if (value >= 0.9)
            sldProgress.value = 100;
    }
}
