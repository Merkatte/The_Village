using System;
using Cysharp.Threading.Tasks;
using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using UnityEngine;


namespace Game.Manager.Scene
{
    /// <summary>
    /// ISceneTransitionService 의 구체 구현입니다.
    ///
    /// 실제 씬 로딩은 ISceneLoader 추상화를 통해 수행하여
    /// Unity 런타임 없이도 단위 테스트가 가능합니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - ISceneLoader 를 생성자 주입(Constructor Injection)으로 받음 (DIP)
    ///   - MonoBehaviour 에 의존하지 않아 EditMode 테스트 가능
    ///   - null 방어 코드로 잘못된 의존성 주입을 조기에 감지
    /// </summary>
    public class SceneTransitionManager : ISceneTransitionService
    {
        private readonly ISceneLoader _sceneLoader;

        /// <inheritdoc/>
        public SceneType CurrentScene { get; private set; } = SceneType.Bootstrap;

        /// <inheritdoc/>
        public SceneType? PendingTarget { get; private set; }

        /// <inheritdoc/>
        public event Action<SceneType> OnSceneTransitioned;

        /// <summary>
        /// SceneTransitionManager 생성자.
        /// </summary>
        /// <param name="sceneLoader">씨 로딩을 담당하는 구현체 (생성자 주입)</param>
        /// <exception cref="ArgumentNullException">sceneLoader 가 null 인 경우</exception>
        public SceneTransitionManager(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader
                ?? throw new ArgumentNullException(nameof(sceneLoader),
                    "[SceneTransitionManager] ISceneLoader 는 null 이 될 수 없습니다.");
        }

        /// <summary>지정한 씨으로 동기 전환합니다.</summary>
        public void TransitionTo(SceneType target)
        {
            if (CurrentScene == target) return;
            CurrentScene = target;
            _sceneLoader.LoadScene(target);
            OnSceneTransitioned?.Invoke(target);
        }

        /// <summary>로딩 씨을 경유하는 페이드 전환 시작점입니다.</summary>
        public void TransitionToWithLoading(SceneType target)
        {
            if (CurrentScene == target) return;
            CurrentScene = target;
            OnSceneTransitioned?.Invoke(target);
            RunTransitionAsync(target).Forget();
        }

        /// <summary>
        /// 페이드 아웃 → PendingTarget 저장 → LoadingScene 로드
        ///
        /// ─ 중요 ─────────────────────────────────────────────
        ///   LoadSceneAsync(target) 를 여기서 호출하면 안 됩니다!
        ///   이후 LoadScene(Loading) 동기 호출이 Unity 내부에서
        ///   이전 AsyncOperation 을 취소하기 때문입니다.
        ///   LoadingSceneController.Awake 에서 PendingTarget 을 읽고
        ///   거기서 LoadSceneAsync 를 시작합니다.
        /// </summary>
        private async UniTaskVoid RunTransitionAsync(SceneType target)
        {
            // 1. 현재 씨 페이드 아웃
            if (ServiceLocator.IsRegistered<ISceneFader>())
                await ServiceLocator.Get<ISceneFader>().FadeToBlackAsync(0.5f);

            // 2. 0.2초 대기 (Loading 씨이 나타나기 전 잠긄)
            await UniTask.Delay(200);

            // 3. 목적지 저장 — LoadingSceneController 가 읽어가 LoadSceneAsync 를 시작함
            PendingTarget = target;

            // 4. Loading 씨 동기 로드 (이 순간 LoadingSceneController.Awake 호출됨)
            _sceneLoader.LoadScene(SceneType.Loading);
        }
    }
}