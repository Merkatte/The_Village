using System.Collections.Generic;
using Game.Enemy.Core;

namespace Game.Enemy.Data
{
    /// <summary>
    /// 적 수치 데이터 저장소 인터페이스입니다.
    ///
    /// 구현체: EnemyRepository
    /// 사용처: EnemyLoader → DungeonSceneInitializer → ServiceLocator 등록
    /// </summary>
    public interface IEnemyRepository
    {
        /// <summary>
        /// EnemyId 로 EnemyData 를 조회합니다.
        /// </summary>
        /// <param name="enemyId">조회할 EnemyId (대소문자 무시)</param>
        /// <returns>찾으면 EnemyData, 없으면 null</returns>
        EnemyData GetById(string enemyId);

        /// <summary>
        /// 등록된 모든 EnemyData 를 반환합니다.
        /// </summary>
        IReadOnlyList<EnemyData> GetAll();
    }
}
