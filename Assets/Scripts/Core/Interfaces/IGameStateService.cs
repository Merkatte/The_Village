using System;
using Game.Core.Enums;

namespace Game.Core.Interfaces
{
    /// <summary>
    /// 게임 전체 상태(마을/던전)를 관리하는 서비스 인터페이스입니다.
    ///
    /// 상태 변경 이력을 이벤트로 알려 다른 시스템이 반응할 수 있게 합니다.
    /// (Observer 패턴 + Open/Closed Principle)
    ///
    /// 구현체: GameStateManager
    /// 테스트 대역: FakeGameStateService
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>
        /// 현재 게임 상태를 반환합니다.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// 상태가 변경될 때 발생하는 이벤트입니다.
        /// 매개변수: (이전 상태, 새 상태)
        /// </summary>
        event Action<GameState, GameState> OnStateChanged;

        /// <summary>
        /// 게임 상태를 지정된 값으로 변경합니다.
        /// 현재 상태와 동일한 값으로의 변경은 무시됩니다.
        /// </summary>
        /// <param name="newState">전환할 새 상태</param>
        void ChangeState(GameState newState);
    }
}
