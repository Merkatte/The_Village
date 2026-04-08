using UnityEngine;
using Game.Core.Enums;

namespace Game.Core.Interfaces
{
    /// <summary>
    /// 실제 씬 로딩 메커니즘을 추상화하는 인터페이스입니다.
    ///
    /// 이 인터페이스를 통해 SceneTransitionManager는
    /// UnityEngine.SceneManagement 에 직접 의존하지 않습니다.
    /// → 테스트 시 FakeSceneLoader로 교체 가능 (DIP / LSP 적용)
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>
        /// 지정된 SceneType에 해당하는 Unity 씬을 로드합니다.
        /// </summary>
        /// <param name="sceneType">로드할 씬 타입</param>
        void LoadScene(SceneType sceneType);
        AsyncOperation LoadSceneAsync(SceneType sceneType);
    }
}
