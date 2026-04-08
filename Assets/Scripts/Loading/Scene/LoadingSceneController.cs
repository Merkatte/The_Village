using Cysharp.Threading.Tasks;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Loading.Scene
{
    /// <summary>
    /// 로딩 씨의 전체 흐름을 제어하는 콘트롤러입니다.
    ///
    /// ─ 실행 순서 ────────────────────────────────────────────
    ///   1. ServiceLocator 에서 PendingTarget 을 읽어 앤 AsyncOperation 시작
    ///   2. 배경 1→0 (FadeIn)
    ///   3. 로딩 완료 대기
    ///   4. holdDuration 대기
    ///   5. 배경 0→1 (FadeOut)
    ///   6. allowSceneActivation = true → 목적지 씨 진입
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("로딩 중 표시할 툴팝 텍스트")]
        [SerializeField] private TextMeshPro toolTipText;

        [Header("배경")]
        [Tooltip("화면을 덮는 검은 SpriteRenderer")]
        [SerializeField] private SpriteRenderer background;

        [Header("타이밍")]
        [Tooltip("로딩 완료 후 목적지 씨으로 전환하기 전 대기 시간 (초)")]
        [SerializeField] [Range(0f, 5f)] private float holdDuration = 1f;

        [Tooltip("로딩씨 등장 시 배경이 밝아지는 시간 (초)")]
        [SerializeField] [Range(0.1f, 3f)] private float fadeInDuration = 1f;

        [Tooltip("목적지 씨 전환 전 배경이 검어지는 시간 (초)")]
        [SerializeField] [Range(0.1f, 3f)] private float fadeOutDuration = 0.5f;

        // ─ 생명주기 ────────────────────────────────────────────

        private void Awake()
        {
            var service = ServiceLocator.Get<ISceneTransitionService>();

            if (service.PendingTarget == null)
            {
                Debug.LogError("[LoadingSceneController] PendingTarget 이 null 입니다. " +
                               "TransitionToWithLoading() 을 통해 씨을 전환했는지 확인하세요.");
                return;
            }

            // 특집: LoadingScene 안에서 LoadSceneAsync 를 시작해야 합니다.
            // 이전 씨에서 LoadScene(동기) 호출이 발생하면 Unity가
            // 그 시점에 시작된 AsyncOperation 을 취소하기 때문입니다.
            var target    = service.PendingTarget.Value;
            var sceneName = target.ToString() + "Scene";
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            Debug.Log($"[LoadingSceneController] '{sceneName}' 비동기 로드 시작");
            RunLoadingSequence(operation).Forget();
        }

        // ─ 로딩 시퀀스 ─────────────────────────────────────────

        private async UniTaskVoid RunLoadingSequence(AsyncOperation operation)
        {
            // 1. 배경 1→0 (서서히 밝아지며 툴팝이 보임)
            await FadeAlpha(1f, 0f, fadeInDuration);

            // 2. 목적지 씨 로딩 완료 대기 (progress 0.9 = 100%)
            await WaitForLoading(operation);

            // 3. 로딩 완료 후 잠시 대기
            await UniTask.Delay((int)(holdDuration * 1000));

            // 4. 배경 0→1 (다시 검어집)
            await FadeAlpha(0f, 1f, fadeOutDuration);

            // 5. 목적지 씨으로 교체
            operation.allowSceneActivation = true;
        }

        /// <summary>AsyncOperation progress 가 0.9 이상이 될 때까지 대기합니다.</summary>
        private async UniTask WaitForLoading(AsyncOperation operation)
        {
            while (operation.progress < 0.9f)
                await UniTask.Yield(PlayerLoopTiming.Update);
        }

        /// <summary>지정한 시간 동안 배경 알파값을 보간합니다.</summary>
        private async UniTask FadeAlpha(float from, float to, float duration)
        {
            if (background == null) return;

            float elapsed    = 0f;
            Color color      = background.color;
            color.a          = from;
            background.color = color;

            while (elapsed < duration)
            {
                elapsed         += Time.deltaTime;
                color.a          = Mathf.Lerp(from, to, elapsed / duration);
                background.color = color;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            color.a          = to;
            background.color = color;
        }
    }
}