using System;
using System.Collections.Generic;

namespace Game.Core.Infrastructure
{
    /// <summary>
    /// 의존성 주입 컨테이너 역할을 하는 서비스 로케이터입니다.
    ///
    /// Singleton MonoBehaviour 패턴의 대안으로,
    /// 인터페이스 타입을 키로 사용하여 구현체를 등록·조회합니다.
    ///
    /// ─ 사용 흐름 ──────────────────────────────────────────────
    ///   1. GameBootstrap.Awake() 에서 Register 호출 (서비스 등록)
    ///   2. 각 시스템의 생성자 혹은 초기화 메서드에서 Get 호출 (의존성 수령)
    ///
    /// ─ 주의사항 ───────────────────────────────────────────────
    ///   - 순수 C# 클래스이므로 Unity 라이프사이클에 무관합니다.
    ///   - EditMode 테스트의 [TearDown] 에서 반드시 Clear() 를 호출하세요.
    /// </summary>
    public static class ServiceLocator
    {
        /// <summary>
        /// 인터페이스 타입 → 구현 인스턴스 매핑 딕셔너리
        /// </summary>
        private static readonly Dictionary<Type, object> _services
            = new Dictionary<Type, object>();

        /// <summary>
        /// 서비스를 인터페이스 타입으로 등록합니다.
        /// 동일 타입으로 재등록하면 기존 값을 덮어씁니다.
        /// </summary>
        /// <typeparam name="TInterface">등록할 인터페이스 타입</typeparam>
        /// <param name="implementation">실제 구현 인스턴스</param>
        /// <exception cref="ArgumentNullException">implementation 이 null 인 경우</exception>
        public static void Register<TInterface>(TInterface implementation)
        {
            if (implementation == null)
                throw new ArgumentNullException(nameof(implementation),
                    $"[ServiceLocator] '{typeof(TInterface).Name}' 구현체가 null 입니다.");

            _services[typeof(TInterface)] = implementation;
        }

        /// <summary>
        /// 등록된 서비스를 인터페이스 타입으로 조회합니다.
        /// </summary>
        /// <typeparam name="TInterface">조회할 인터페이스 타입</typeparam>
        /// <returns>등록된 구현 인스턴스</returns>
        /// <exception cref="InvalidOperationException">서비스가 등록되지 않은 경우</exception>
        public static TInterface Get<TInterface>()
        {
            if (_services.TryGetValue(typeof(TInterface), out var service))
                return (TInterface)service;

            throw new InvalidOperationException(
                $"[ServiceLocator] '{typeof(TInterface).Name}' 서비스가 등록되지 않았습니다. " +
                "GameBootstrap.RegisterServices() 가 먼저 실행되었는지 확인하세요.");
        }

        /// <summary>
        /// 해당 인터페이스 타입으로 서비스가 등록되어 있는지 확인합니다.
        /// </summary>
        /// <typeparam name="TInterface">확인할 인터페이스 타입</typeparam>
        /// <returns>등록되어 있으면 true</returns>
        public static bool IsRegistered<TInterface>()
            => _services.ContainsKey(typeof(TInterface));

        /// <summary>
        /// 등록된 모든 서비스를 초기화합니다.
        /// EditMode 테스트의 [TearDown] 또는 게임 재시작 시 호출하세요.
        /// </summary>
        /// <summary>
        /// 특정 서비스를 ServiceLocator 에서 제거합니다.
        /// 던전처럼 특정 씬에서만 필요한 서비스를 씬 종료 시 해제할 때 사용합니다.
        /// </summary>
        public static void Unregister<TInterface>()
        {
            var key = typeof(TInterface);
            if (!_services.ContainsKey(key))
                throw new InvalidOperationException(
                    $"[ServiceLocator] '{key.Name}' 는 등록되어 있지 않습니다.");

            _services.Remove(key);
        }

        
public static void Clear() => _services.Clear();
    }
}
