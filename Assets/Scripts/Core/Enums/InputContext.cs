namespace Game.Core.Enums
{
    /// <summary>
    /// 현재 어떤 입력 맥락(Context)이 활성화되어 있는지를 정의합니다.
    ///
    /// InputManager 는 이 값을 기준으로
    /// 어떤 IInputReader 를 활성화할지 결정합니다.
    ///
    /// Gameplay : 던전/마을에서 플레이어가 직접 조작하는 상태
    /// UI       : 인벤토리, 메뉴 등 UI 가 포커스를 가진 상태
    /// </summary>
    public enum InputContext
    {
        None = -1,
        Gameplay,
        UI
    }
}
