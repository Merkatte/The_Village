using System;

namespace Game.Manager.Interfaces.Input
{
    /// <summary>
    /// UI 입력(확인, 취소, 메뉴 탐색 등)을 추상화하는 인터페이스입니다.
    ///
    /// ─ 위치 결정 이유 ─────────────────────────────────────────
    ///   IPlayerInputReader 와 같은 레이어(Game.Manager)에 배치하여
    ///   입력 관련 인터페이스를 한 레이어에서 관리합니다.
    ///
    /// 구현체: UIInputReader
    /// 테스트 대역: FakeUIInputReader
    /// </summary>
    public interface IUIInputReader
    {
        /// <summary>
        /// 확인(Submit) 입력이 발생할 때 발행됩니다. (예: 스페이스, 컨트롤러 A버튼)
        /// </summary>
        event Action OnSubmitPerformed;

        /// <summary>
        /// 취소(Cancel) 입력이 발생할 때 발행됩니다. (예: ESC, 컨트롤러 B버튼)
        /// </summary>
        event Action OnCancelPerformed;

        /// <summary>
        /// 입력 수신을 활성화합니다.
        /// InputManager 가 컨텍스트 전환 시 호출합니다.
        /// </summary>
        void Enable();

        /// <summary>
        /// 입력 수신을 비활성화합니다.
        /// InputManager 가 컨텍스트 전환 시 호출합니다.
        /// </summary>
        void Disable();
    }
}
