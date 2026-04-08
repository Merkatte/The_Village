using System;
using Game.Core.Enums;

namespace Game.Core.Interfaces
{
    /// <summary>
    /// 씬 전환 기능을 추상화하는 서비스 인터페이스입니다.
    ///
    /// 고수준 모듈(Bootstrap, UI 등)이 구체적인 씬 로딩 방식에
    /// 의존하지 않도록 추상화합니다. (Dependency Inversion Principle)
    ///
    /// 구현체: SceneTransitionManager
    /// 테스트 대역: FakeSceneTransitionService
    /// </summary>
    public interface ISceneTransitionService
    {
        /// <summary>현재 활성화된 씬 타입</summary>
        SceneType CurrentScene { get; }

        /// <summary>씬 전환 완료 이벤트. 매개변수: 전환된 새 씬 타입</summary>
        event Action<SceneType> OnSceneTransitioned;

        /// <summary>
        /// 지정한 씬으로 즉시 동기 전환합니다.
        /// 현재 씬과 동일한 씬으로의 요청은 무시됩니다.
        /// </summary>
        void TransitionTo(SceneType target);

        /// <summary>
        /// 로딩 씬을 경유하여 대상 씬으로 페이드 전환합니다.
        ///
        /// ─ 흐름 ─────────────────────────────────────────────
        ///   1. 현재 씬 페이드 아웃 (ISceneFader)
        ///   2. 0.2초 대기
        ///   3. PendingTarget = target 저장
        ///   4. LoadScene(Loading) 동기 전환
        ///   5. LoadingSceneController.Awake 가 PendingTarget 을 읽어
        ///      LoadSceneAsync(target) 를 직접 시작합니다.
        /// </summary>
        void TransitionToWithLoading(SceneType target);

        /// <summary>
        /// LoadingSceneController 가 읽어갈 목적지 씬 타입입니다.
        /// LoadingSceneController 가 직접 LoadSceneAsync 를 시작합니다.
        /// </summary>
        SceneType? PendingTarget { get; }
    }
}