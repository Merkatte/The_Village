using System;
using UnityEngine;

namespace Game.Manager.Interfaces.Input
{
    /// <summary>
    /// 플레이어 게임플레이 입력(이동, 공격 등)을 추상화하는 인터페이스입니다.
    ///
    /// ─ 위치 결정 이유 ─────────────────────────────────────────
    ///   Vector2(UnityEngine 타입)를 사용하므로 Game.Core(noEngineReferences:true) 에
    ///   둘 수 없습니다. Unity 타입에 의존하는 인터페이스는 Game.Manager 레이어에 배치합니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 구현체(PlayerInputReader)가 New Input System 세부 사항을 캡슐화 (DIP)
    ///   - 테스트 시 FakePlayerInputReader 로 교체 가능 (LSP)
    ///
    /// 구현체: PlayerInputReader
    /// 테스트 대역: FakePlayerInputReader
    /// </summary>
    public interface IPlayerInputReader
    {
        /// <summary>
        /// WASD / 왼쪽 스틱 이동 입력이 시작되거나 변경될 때 발생합니다.
        /// 매개변수: 입력 방향 벡터 (정규화되지 않은 원시 값)
        /// </summary>
        event Action<Vector2> OnMovePerformed;

        /// <summary>
        /// 이동 입력이 해제(키업)될 때 발생합니다.
        /// </summary>
        event Action OnMoveCancelled;

        /// <summary>
        /// 채취 키를 누르기 시작할 때 발생합니다.
        /// 홀드 판정은 HarvestController 에서 처리합니다.
        /// </summary>
        event Action OnHarvestStarted;

        /// <summary>
        /// 채취 키를 뗄 때 발생합니다.
        /// </summary>
        event Action OnHarvestCancelled;

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
