﻿using UnityEngine;
using System;
namespace Cosmos.Scene
{
    //================================================
    /*
     * 1、场景加载模块；
     */
    //================================================
    [Module]
    internal sealed class SceneManager : Module, ISceneManager
    {
        ISceneHelper sceneHelper;

        /// <summary>
        /// 异步设置场景加载helper；
        /// </summary>
        /// <param name="sceneHelper">自定义实现的ISceneHelper</param>
        public async void SetHelperAsync(ISceneHelper sceneHelper)
        {
            if (sceneHelper != null)
                await new WaitUntil(() => { return sceneHelper.IsLoading == false; });
            this.sceneHelper = sceneHelper;
        }
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        public void LoadScene(ISceneInfo sceneInfo)
        {
            sceneHelper.LoadScene(sceneInfo);
        }
        /// <summary>
        ///  异步加载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="loadedCallback">加载完毕后的回调</param>
        /// <returns>协程对象</returns>
        public Coroutine LoadSceneAsync(ISceneInfo sceneInfo, Action loadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.LoadSceneAsync(sceneInfo, null, null, loadedCallback));
        }
        /// <summary>
        ///  异步加载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="progressCallback">加载场景进度回调</param>
        /// <param name="loadedCallback">场景加载完毕回调</param>
        /// <returns>协程对象</returns>
        public Coroutine LoadSceneAsync(ISceneInfo sceneInfo, Action<float> progressCallback, Action loadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.LoadSceneAsync(sceneInfo, progressCallback, loadedCallback));
        }
        /// <summary>
        /// 异步加载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="loadedPredicate">场景加载完成的条件</param>
        /// <param name="loadedCallback">场景加载完毕回调</param>
        /// <returns>协程对象</returns>
        public Coroutine LoadSceneAsync(ISceneInfo sceneInfo, Func<bool> loadedPredicate, Action loadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.LoadSceneAsync(sceneInfo, null, loadedPredicate, loadedCallback));
        }
        /// <summary>
        /// 异步加载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="progressCallback">加载场景进度回调</param>
        /// <param name="loadedPredicate">场景加载完成的条件</param>
        /// <param name="loadedCallback">场景加载完毕回调</param>
        /// <returns>协程对象</returns>
        public Coroutine LoadSceneAsync(ISceneInfo sceneInfo, Action<float> progressCallback, Func<bool> loadedPredicate, Action loadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.LoadSceneAsync(sceneInfo, progressCallback, loadedPredicate, loadedCallback));
        }
        /// <summary>
        /// 异步卸载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="progressCallback">卸载场景的进度</param>
        /// <param name="unLoadedPredicate">卸载场景完成的条件</param>
        /// <param name="unLoadedCallback">场景卸载完毕后的回调</param>
        /// <returns>协程对象</returns>
        public Coroutine UnLoadSceneAsync(ISceneInfo sceneInfo, Action<float> progressCallback, Func<bool> unLoadedPredicate, Action unLoadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.UnLoadSceneAsync(sceneInfo, progressCallback, unLoadedPredicate, unLoadedCallback));
        }
        /// <summary>
        /// 异步卸载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="progressCallback">卸载场景的进度</param>
        /// <param name="unLoadedCallback">场景卸载完毕后的回调</param>
        /// <returns>协程对象</returns>
        public Coroutine UnLoadSceneAsync(ISceneInfo sceneInfo, Action<float> progressCallback, Action unLoadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.UnLoadSceneAsync(sceneInfo, progressCallback, unLoadedCallback));
        }
        /// <summary>
        ///  异步卸载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="unLoadedPredicate">卸载场景完成的条件</param>
        /// <param name="unLoadedCallback">场景卸载完毕后的回调</param>
        /// <returns>协程对象</returns>
        public Coroutine UnLoadSceneAsync(ISceneInfo sceneInfo, Func<bool> unLoadedPredicate, Action unLoadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.UnLoadSceneAsync(sceneInfo, null, unLoadedPredicate, unLoadedCallback));
        }
        /// <summary>
        /// 异步卸载；
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="unLoadedCallback">场景卸载完毕后的回调</param>
        /// <returns>协程对象</returns>
        public Coroutine UnLoadSceneAsync(ISceneInfo sceneInfo, Action unLoadedCallback = null)
        {
            if (sceneHelper == null)
                throw new ArgumentNullException($"{this.GetType()}: SceneHelper is invalid !");
            return Utility.Unity.StartCoroutine(sceneHelper.UnLoadSceneAsync(sceneInfo, null, null, unLoadedCallback));
        }
        protected override void OnInitialization()
        {
            sceneHelper = new DefaultSceneHelper();
        }
    }
}