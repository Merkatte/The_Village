namespace Game.Dungeon.Resource
{
    /// <summary>
    /// 난수 생성 인터페이스입니다.
    /// 테스트 시 FakeRandom 으로 교체하여 확률 판정을 완전히 제어할 수 있습니다. (DIP)
    /// </summary>
    public interface IRandom
    {
        /// <summary>0.0 이상 1.0 미만의 실수를 반환합니다.</summary>
        float NextFloat();

        /// <summary>min 이상 max 이하의 정수를 반환합니다.</summary>
        int NextInt(int min, int max);
    }

    /// <summary>
    /// Unity의 Random을 사용하는 실제 난수 생성기입니다.
    /// 런타임 시 ServiceLocator 또는 생성자를 통해 주입됩니다.
    /// </summary>
    public sealed class UnityRandom : IRandom
    {
        /// <inheritdoc/>
        public float NextFloat() => UnityEngine.Random.value;

        /// <inheritdoc/>
        public int NextInt(int min, int max) => UnityEngine.Random.Range(min, max + 1);
    }
}
