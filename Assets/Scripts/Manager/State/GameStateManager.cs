using System;
using Game.Core.Enums;
using Game.Core.Interfaces;

namespace Game.Manager.State
{
    /// <summary>
    /// IGameStateService 의 구체 구현입니다.
    ///
    /// 게임 전반의 흐름 상태(마을/던전)를 추적하고
    /// 상태 변경 시 이벤트를 발행합니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - MonoBehaviour 에 의존하지 않으므로 EditMode 에서 단위 테스트 가능
    ///   - 상태 변경 로직을 캡슐화하여 외부에서 CurrentState 를 직접 수정 불가
    ///   - IGameStateService 인터페이스로만 외부에 노출 (DIP)
    /// </summary>
    public class GameStateManager : IGameStateService
    {
        /// <inheritdoc/>
        public GameState CurrentState { get; private set; } = GameState.None;

        /// <inheritdoc/>
        public event Action<GameState, GameState> OnStateChanged;

        /// <summary>
        /// 게임 상태를 변경합니다.
        /// 동일한 상태로의 전환은 이벤트를 발행하지 않고 즉시 반환합니다.
        /// </summary>
        /// <param name="newState">전환할 새 상태</param>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            var previousState = CurrentState;
            CurrentState = newState;

            // 구독자들에게 상태 변경을 알립니다.
            OnStateChanged?.Invoke(previousState, newState);
        }
    }
}
