using Cysharp.Threading.Tasks;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using UnityEngine;

namespace Game.Loading
{
    /// <summary>
    /// ISceneFader 구현체입니다.
    /// 씬의 배경 SpriteRenderer 알파값을 조절하여 페이드 효과를 냅니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Town씬, Dungeon씬 등 각 씬의 검은 배경 오브젝트에 부착합니다.
    ///   [SerializeField] background 에 SpriteRenderer 를 연결하세요.
    ///
    /// ─ 씬 입장 시 동작 ────────────────────────────────────────
    ///   Awake 에서 ServiceLocator 에 등록하고,
    ///   Start 에서 자동으로 FadeFromBlack(씬 진입 시 밝아짐)을 실행합니다.
    ///   Inspector 에서 배경 알파를 1(검은 상태)로 설정해두세요.
    ///
    /// ─ 씬 종료 시 동작 ────────────────────────────────────────
    ///   OnDestroy 에서 ServiceLocator 등록을 해제합니다.
    ///   SceneTransitionManager 가 FadeToBlack 을 먼저 기다린 뒤 씬을 전환합니다.
    /// </summary>
    public class SceneFader : MonoBehaviour, ISceneFader
    {
        [Header("페이드 대상")]
        [Tooltip("화면을 덮는 검은 SpriteRenderer 를 연결합니다.")]
        [SerializeField] private SpriteRenderer background;

        [Header("페이드 시간")]
        [Tooltip("씬 진입 시 밝아지는 시간 (초)")]
        [SerializeField] private float fadeInDuration = 0.5f;

        // ─ 생명주기 ────────────────────────────────────────────

        private void Awake()
        {
            // 로딩 씬에서 바로 사용할 수 있도록 Awake 에서 등록합니다.
            ServiceLocator.Register<ISceneFader>(this);
        }

        private void Start()
        {
            // 씬 진입 시 검은 화면에서 서서히 밝아집니다.
            FadeFromBlackAsync(fadeInDuration).Forget();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ISceneFader>();
        }

        // ─ ISceneFader 구현 ────────────────────────────────────

        /// <summary>화면을 서서히 검게 만듭니다. (알파 → 1)</summary>
        public async UniTask FadeToBlackAsync(float duration)
        {
            await AnimateAlpha(background.color.a, 1f, duration);
        }

        /// <summary>화면을 서서히 밝게 만듭니다. (알파 → 0)</summary>
        public async UniTask FadeFromBlackAsync(float duration)
        {
            await AnimateAlpha(background.color.a, 0f, duration);
        }

        // ─ 내부 ────────────────────────────────────────────────

        /// <summary>지정한 시간 동안 알파값을 보간합니다.</summary>
        private async UniTask AnimateAlpha(float from, float to, float duration)
        {
            if (background == null) return;

            float elapsed = 0f;
            Color color   = background.color;

            while (elapsed < duration)
            {
                elapsed  += Time.deltaTime;
                color.a   = Mathf.Lerp(from, to, elapsed / duration);
                background.color = color;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // 최종값을 정확히 맞춥니다.
            color.a = to;
            background.color = color;
        }
    }
}
