using UnityEngine;

namespace Game.Camera
{
    /// <summary>
    /// Main Camera 에 붙이는 MonoBehaviour 입니다.
    /// CameraFollower(ICameraFollower) 에 계산을 위임하고
    /// 결과를 transform.position 에 반영합니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   target       : 추적할 플레이어 Transform
    ///   boundsMin    : 카메라 이동 가능 최소 좌표 (XY)
    ///   boundsMax    : 카메라 이동 가능 최대 좌표 (XY)
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   - LateUpdate 에서 이동하여 플레이어 Update 이후에 카메라가 반응합니다.
    ///   - Z 값(카메라 깊이)은 초기값을 유지합니다.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("추적 대상")]
        [SerializeField] private Transform target;

        [Header("카메라 이동 범위")]
        [SerializeField] private Vector2 boundsMin = new Vector2(-10f, -8f);
        [SerializeField] private Vector2 boundsMax = new Vector2( 10f,  8f);

        /// <summary>이동 로직을 담당하는 순수 C# 객체</summary>
        private ICameraFollower _follower;

        /// <summary>카메라 Z 깊이 — 시작 시 고정</summary>
        private float _cameraZ;

        private void Awake()
        {
            _follower = new CameraFollower(boundsMin, boundsMax);
            _cameraZ  = transform.position.z;
        }

        /// <summary>
        /// LateUpdate 에서 처리하여 플레이어 이동 후 카메라가 반응하도록 합니다.
        /// </summary>
        private void LateUpdate()
        {
            if (!target) return;

            // 타겟 위치를 Bounds 범위로 클램프한 좌표를 계산합니다.
            var pos = _follower.CalculatePosition(target.position);

            // Z 값은 카메라 초기 깊이를 유지합니다.
            transform.position = new Vector3(pos.x, pos.y, _cameraZ);
        }

        /// <summary>씬 에디터에서 범위를 시각적으로 확인할 수 있도록 기즈모를 그립니다.</summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var center = new Vector3(
                (boundsMin.x + boundsMax.x) * 0.5f,
                (boundsMin.y + boundsMax.y) * 0.5f,
                0f);
            var size = new Vector3(
                boundsMax.x - boundsMin.x,
                boundsMax.y - boundsMin.y,
                0f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
