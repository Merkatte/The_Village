using System.Collections.Generic;

namespace Game.Dungeon.Spawn
{
    /// <summary>
    /// 스폰 지점 저장소 인터페이스입니다.
    /// </summary>
    public interface ISpawnPointRepository
    {
        /// <summary>특정 ID의 스폰 지점 데이터를 반환합니다. 없으면 null.</summary>
        SpawnPointData GetById(string spawnPointId);

        /// <summary>전체 스폰 지점 목록을 반환합니다.</summary>
        IReadOnlyList<SpawnPointData> GetAll();
    }
}
