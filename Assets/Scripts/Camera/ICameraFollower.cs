using UnityEngine;

namespace Game.Camera
{
    /// <summary>
    /// 카메라 팔로우 로직의 인터페이스입니다.
    /// MonoBehaviour 없이 순수 C# 로 테스트 가능합니다. (DIP)
    /// </summary>
    public interface ICameraFollower
    {
        /// <summary>
        /// 타겟 위치를 받아 카메라가 위치해야 할 좌표를 반환합니다.
        /// 범위(Bounds) 밖이면 경계 좌표를 반환합니다.
        /// </summary>
        /// <param name="targetPosition">추적할 대상의 월드 위치</param>
        /// <returns>카메라가 이동해야 할 XY 좌표</returns>
        Vector2 CalculatePosition(Vector2 targetPosition);

        /// <summary>카메라가 이동 가능한 최소 XY 좌표</summary>
        Vector2 BoundsMin { get; }

        /// <summary>카메라가 이동 가능한 최대 XY 좌표</summary>
        Vector2 BoundsMax { get; }
    }
}
