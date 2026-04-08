using UnityEngine;

namespace Game.Camera
{
    /// <summary>
    /// ICameraFollower 의 구체 구현입니다.
    /// 순수 C# 클래스이므로 MonoBehaviour 없이 테스트 가능합니다.
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   - 타겟이 BoundsMin ~ BoundsMax 내에 있으면 타겟 위치를 그대로 반환합니다.
    ///   - 타겟이 범위를 벗어나면 Mathf.Clamp 로 경계 값으로 고정합니다.
    ///   - Z 값은 관여하지 않습니다. (CameraController 가 유지)
    /// </summary>
    public class CameraFollower : ICameraFollower
    {
        /// <inheritdoc/>
        public Vector2 BoundsMin { get; }

        /// <inheritdoc/>
        public Vector2 BoundsMax { get; }

        /// <summary>
        /// CameraFollower 생성자.
        /// </summary>
        /// <param name="boundsMin">카메라 이동 가능 최소 좌표 (XY)</param>
        /// <param name="boundsMax">카메라 이동 가능 최대 좌표 (XY)</param>
        public CameraFollower(Vector2 boundsMin, Vector2 boundsMax)
        {
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
        }

        /// <inheritdoc/>
        public Vector2 CalculatePosition(Vector2 targetPosition)
        {
            // 타겟 위치를 Bounds 범위로 클램프하여 반환합니다.
            return new Vector2(
                Mathf.Clamp(targetPosition.x, BoundsMin.x, BoundsMax.x),
                Mathf.Clamp(targetPosition.y, BoundsMin.y, BoundsMax.y)
            );
        }
    }
}
