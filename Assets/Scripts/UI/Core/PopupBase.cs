using UnityEngine;

namespace Game.UI.Core
{
    /// <summary>
    /// 모든 팝업 UI 의 기반 MonoBehaviour 입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - Instantiate / Destroy 생명주기는 UIManager 가 전담합니다.
    ///   - OnOpen()  : 팝업 열릴 때 서비스 획득·구독 등 초기화 작업
    ///   - OnClose() : Destroy 직전 리소스 해제·구독 해제 작업
    ///
    /// ─ 프리팹 규칙 ────────────────────────────────────────────
    ///   - 팝업 프리팹 루트에는 반드시 Canvas 컴포넌트를 포함하세요.
    ///     (Render Mode: Screen Space - Overlay 권장)
    ///   - 팝업마다 독립 Canvas 를 가지므로 Sorting Order 를 통해
    ///     팝업 간 렌더링 순서를 제어할 수 있습니다.
    /// </summary>
    public abstract class PopupBase : MonoBehaviour
    {
        /// <summary>
        /// ESC 키로 이 팝업을 닫을 수 있는지 여부입니다.
        /// 사망 팝업처럼 반드시 버튼으로만 닫아야 하는 팝업은 false 로 재정의하세요.
        /// </summary>
        public virtual bool IsEscClosable => true;

        /// <summary>
        /// UIManager 가 팝업을 인스턴스화한 직후 호출합니다.
        /// 서비스 획득, 이벤트 구독 등 초기화 로직을 여기에 구현하세요.
        /// </summary>
        public virtual void OnOpen() { }

        /// <summary>
        /// UIManager 가 팝업을 파괴하기 직전에 호출합니다.
        /// 이벤트 구독 해제, Presenter Dispose 등 정리 로직을 여기에 구현하세요.
        /// </summary>
        public virtual void OnClose() { }
    }
}
