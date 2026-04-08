using Cysharp.Threading.Tasks;

namespace Game.Core.Interfaces
{
    /// <summary>
    /// 씬 전환 시 화면 페이드 효과를 추상화하는 인터페이스입니다.
    ///
    /// ─ 구현체: SceneFader (MonoBehaviour)
    ///   각 씬의 배경 SpriteRenderer 에 부착하여 ServiceLocator 에 등록합니다.
    ///
    /// ─ 페이드 방향 ────────────────────────────────────────────
    ///   FadeToBlack   : 알파 현재값 → 1 (화면이 검게)
    ///   FadeFromBlack : 알파 현재값 → 0 (화면이 밝아짐)
    /// </summary>
    public interface ISceneFader
    {
        /// <summary>화면을 서서히 검게 만듭니다. (알파 → 1)</summary>
        /// <param name="duration">페이드에 걸리는 시간 (초)</param>
        UniTask FadeToBlackAsync(float duration);

        /// <summary>화면을 서서히 밝게 만듭니다. (알파 → 0)</summary>
        /// <param name="duration">페이드에 걸리는 시간 (초)</param>
        UniTask FadeFromBlackAsync(float duration);
    }
}
