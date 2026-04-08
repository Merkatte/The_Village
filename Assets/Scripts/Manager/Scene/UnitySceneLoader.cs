using System;
using Game.Core.Enums;
using Game.Core.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Manager.Scene
{
    /// <summary>
    /// UnityEngine.SceneManagement 을 사용하는 ISceneLoader 구현체입니다.
    ///
    /// 씬 이름 규칙: {SceneType}Scene (예: TownScene, DungeonScene)
    /// Build Settings 에 씬이 등록되어 있어야 합니다.
    ///
    /// 이 클래스는 Unity 런타임에서만 동작하며,
    /// 테스트 시에는 FakeSceneLoader 로 대체됩니다. (LSP)
    /// </summary>
    public class UnitySceneLoader : ISceneLoader
    {
        /// <summary>
        /// SceneType 에 해당하는 Unity 씬을 동기적으로 로드합니다.
        /// 씬 이름 형식: "{SceneType}Scene" (예: SceneType.Town → "TownScene")
        /// </summary>
        /// <param name="sceneType">로드할 씬 타입</param>
        public void LoadScene(SceneType sceneType)
        {
            var sceneName = sceneType + "Scene";
            SceneManager.LoadScene(sceneName);
        }
        
        [Obsolete("Current Method is obsoleted. Current method is replaced to Loading Scene.")]
        public AsyncOperation LoadSceneAsync(SceneType sceneType)
        {
            var sceneName = sceneType + "Scene";
            Debug.Log("Scene Name => " + sceneName);
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            return operation;
        }
    }
}
