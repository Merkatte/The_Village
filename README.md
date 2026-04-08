# The Village

![Unity](https://img.shields.io/badge/Unity-6-black?logo=unity) ![C#](https://img.shields.io/badge/C%23-purple?logo=csharp) ![Status](https://img.shields.io/badge/status-in%20progress-yellow) ![License](https://img.shields.io/badge/license-MIT-green)

> 🇰🇷 [한국어로 보기](README.ko.md)

A 2D village and dungeon exploration game built with Unity 6 and C#.

---

## Project Status

This project is currently **in active development**. Systems are being incrementally designed and implemented. The table below reflects the current state.

| System | Status |
|--------|--------|
| Core / ServiceLocator / Input | ✅ Implemented & verified |
| Player (movement, harvest, equipment) | ✅ Implemented & verified |
| Inventory | ✅ Implemented & verified |
| Dungeon (spawn, harvest pipeline) | ✅ Implemented & verified |
| Enemy AI (FSM, movement, combat) | 🔧 Script implemented / Editor asset setup in progress |
| Shop / Currency | 🔧 Script implemented / Editor asset setup in progress |
| UI (Inventory / Shop / Sell popups) | 🔧 Script implemented / Prefab setup in progress |
| Level design / Content | 🔲 Not started |
| Enemy AI Phase 6 (Ranged attack) | 🔲 Not started |
| Overall game loop / balancing | 🔲 Not started |

---

## Development Approach

This project was developed using an **AI-assisted workflow leveraging Claude Code's Agent, Plan Mode, and Skills features** — not just prompting for code generation.

### AI Workflow

| Mode | Usage |
|------|-------|
| **Agent Teammates** | Used Claude Code's experimental Agent Teammates feature (Explore, Plan) to analyze the codebase and draft implementation plans. Provided specifications to each teammate and reviewed results. |
| **AI Assist** | Iterative implement → review → revise within a single session. Manually reviewed generated code and redirected when it deviated from architectural rules. |
| **Plan Mode** | Reviewed and approved implementation plans before execution, acting as a gatekeeping step between design and code generation. |
| **Skills** | Automated repetitive tasks (commits, context updates, etc.) via custom slash commands. |

### My Role

- Designed all module structures and interfaces (no agent made these decisions)
- Wrote specifications and managed context passed to each agent
- Maintained `AI_Context/` folder directly — architecture docs, coding rules, change history
- Reviewed agent output and issued correction instructions against architectural standards
- Approved and adjusted implementation plans via Plan Mode before any code was written

---

## Tech Stack

| | |
|---|---|
| Engine | Unity 6 |
| Language | C# |
| Async | UniTask |
| Input | Unity New Input System |
| Testing | Unity Test Framework (EditMode) |
| Data | CSV + ScriptableObject |

---

## Architecture

### DI — ServiceLocator Pattern

Adopted the ServiceLocator pattern to manage services independently of the MonoBehaviour lifecycle.  
Global services are registered in `GameBootstrap`; scene-specific services are registered and released in each scene's `XxxSceneInitializer`.

```
ServiceLocator
  ├── IInventory
  ├── ICurrencyService
  ├── IItemCatalogRepository
  ├── ISpriteRepository
  ├── IGameStateService
  ├── ISceneTransitionService
  ├── InputManager
  └── IUIManager
```

### MonoBehaviour Bridge Pattern

MonoBehaviour handles Unity lifecycle concerns only. **All logic is delegated to pure C# classes**, enabling EditMode unit testing without Unity.

```
PlayerController (MonoBehaviour — input subscription, Transform update)
    └── PlayerMover (pure C# — movement vector calculation)

EnemyController (MonoBehaviour — bridge)
    ├── EnemyBrain   (pure C# — FSM state transitions)
    ├── EnemyMover   (pure C# — movement calculation)
    ├── EnemyHealth  (pure C# — HP management)
    └── EnemyAttacker (pure C# — attack strategy)
```

### Dependency Flow

```
GameBootstrap
  └─ ServiceLocator
       ├─ ItemCatalogRepository   ← ItemCatalog.csv
       ├─ SpriteRepository        ← Resources/Sprites/Items/{itemId}
       ├─ GameStateManager
       ├─ InputManager
       ├─ SceneTransitionManager
       ├─ Inventory
       ├─ CurrencyManager
       └─ [Dungeon Scene] DungeonSceneInitializer
            ├─ DropTableRepository
            ├─ SpawnPointRepository → HarvestableSpawner
            └─ EnemyRepository → EnemySpawner
                 └─ EnemyController (Brain / Mover / Health / Attacker)

TownSceneInitializer
  └─ ShopService (IItemCatalogRepository + ICurrencyService + IInventory)
```

---

## Implemented Systems

| System | Description |
|--------|-------------|
| **Inventory** | Slot-based inventory. Automatically stacks items implementing `IStackable`. |
| **Shop** | Buy/sell popup. Integrated with `CurrencyManager` (Gold). Configurable sell price ratio. |
| **Enemy AI** | 5-state FSM (`Idle → Alert → Combat → Attacking → Dead`). Includes wander, search, and retreat logic. |
| **Dungeon** | CSV-based resource spawn and harvest hold pipeline. `HarvestController → DropManager → IInventory`. |
| **UI System** | Popup stack based on `PopupBase`. Data-driven shortcut keys via `UIShortcutHandler`. |
| **Player** | Movement, tool equip, harvest hold, death handling. |
| **Scene Transition** | Fade in/out with loading scene. Async initialization via UniTask to prevent freezing. |
| **Currency** | `ICurrencyService` — manages multiple currencies (Gold / SkillPoint). |

---

## Project Structure

```
Assets/Scripts/
├── Camera/         Camera follow system
├── Core/           Common infrastructure (Enum · Interface · ServiceLocator · CsvReader)
├── Data/           ScriptableObject-based config data
├── Dungeon/        Resource harvest · spawn · death handling
├── Editor/         Editor-only utilities
├── Enemy/          Enemy AI (FSM · movement · health · combat · CSV data)
├── Graphic/        Sprite loading and repository
├── Inventory/      Slot-based inventory
├── Item/           Item data model · catalog · factory
├── Loading/        Scene fade · loading sequence
├── Manager/        Bootstrap · input · scene transition · state · currency
├── Player/         Movement · equipment · harvest · health
├── Shop/           Shop service logic
├── Town/           Town scene triggers
└── Tests/          EditMode unit tests (27 cases)
```

Each module is separated by Assembly Definition, enforcing compile boundaries and dependency direction.

```
Game.Core → Game.Manager → Game.Player
                         → Game.Tests.EditMode (test only, excluded from runtime)
```

---

## Design Decisions

**ServiceLocator**  
Required dependency management independent of the MonoBehaviour lifecycle, without an external DI framework like Zenject. Each scene's Initializer explicitly registers and unregisters services, preventing memory leaks on scene transitions.

<details>
<summary>View ServiceLocator code</summary>

```csharp
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<TInterface>(TInterface implementation)
    {
        if (implementation == null)
            throw new ArgumentNullException(nameof(implementation),
                $"[ServiceLocator] '{typeof(TInterface).Name}' implementation is null.");

        _services[typeof(TInterface)] = implementation;
    }

    public static TInterface Get<TInterface>()
    {
        if (_services.TryGetValue(typeof(TInterface), out var service))
            return (TInterface)service;

        throw new InvalidOperationException(
            $"[ServiceLocator] '{typeof(TInterface).Name}' is not registered. " +
            "Ensure GameBootstrap.RegisterServices() has been called first.");
    }

    public static void Unregister<TInterface>()
    {
        var key = typeof(TInterface);
        if (!_services.ContainsKey(key))
            throw new InvalidOperationException(
                $"[ServiceLocator] '{key.Name}' is not registered.");
        _services.Remove(key);
    }

    public static void Clear() => _services.Clear();
}
```

</details>

---

**MonoBehaviour Bridge Pattern — Pure C# Presenter**  
UI logic is delegated to a pure C# `InventoryPresenter`, enabling EditMode testing without Unity. Communication with the View goes through the `IInventoryView` interface, fully decoupling logic from the actual UI.

<details>
<summary>View InventoryPresenter code</summary>

```csharp
public sealed class InventoryPresenter : IDisposable
{
    private readonly IInventory        _inventory;
    private readonly ISpriteRepository _spriteRepository;
    private          IInventoryView    _view;

    public InventoryPresenter(IInventory inventory, ISpriteRepository spriteRepository)
    {
        _inventory        = inventory        ?? throw new ArgumentNullException(nameof(inventory));
        _spriteRepository = spriteRepository ?? throw new ArgumentNullException(nameof(spriteRepository));

        _inventory.OnInventoryChanged += OnInventoryChanged;
    }

    public void Bind(IInventoryView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        Refresh();
    }

    public void Unbind() => _view = null;

    public void Dispose()
    {
        _inventory.OnInventoryChanged -= OnInventoryChanged;
        _view = null;
    }

    private void OnInventoryChanged() => Refresh();

    private void Refresh()
    {
        if (_view == null) return;

        var viewModels = new SlotViewModel[_inventory.SlotCount];
        for (int i = 0; i < _inventory.SlotCount; i++)
        {
            var item = _inventory.GetSlot(i);
            if (item == null) { viewModels[i] = SlotViewModel.Empty; continue; }

            var icon         = _spriteRepository.GetSprite(item.ItemId);
            var quantityText = item is IStackable s && s.Quantity > 1
                ? s.Quantity.ToString() : string.Empty;

            viewModels[i] = new SlotViewModel(icon, item.ItemName, quantityText);
        }
        _view.Refresh(viewModels);
    }
}
```

</details>

---

**Enemy AI FSM**  
5-state FSM: `Idle(wander) → Alert(search) → Combat(chase) → Attacking → Dead`. When the player exits AlertRange, the enemy searches the last known position before returning to spawn.  
State decision (`EnemyBrain`) and execution (`EnemyController`) are separated for independent testability.

<details>
<summary>View EnemyBrain FSM code</summary>

```csharp
// Responsible for state decisions only. Movement and attack execution handled by EnemyController. (SRP)
public sealed class EnemyBrain
{
    private readonly EnemyData _data;

    public EnemyBrain(EnemyData data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public EnemyState Tick(EnemyState current, float distance, bool isAttacking)
    {
        if (current == EnemyState.Dead) return EnemyState.Dead;

        switch (current)
        {
            case EnemyState.Idle:
                return distance <= _data.AlertRange ? EnemyState.Alert : EnemyState.Idle;

            case EnemyState.Alert:
                if (distance <= _data.CombatRange) return EnemyState.Combat;
                if (distance >  _data.AlertRange)  return EnemyState.Idle;
                return EnemyState.Alert;

            case EnemyState.Combat:
                if (distance <= _data.AttackRange) return EnemyState.Attacking;
                if (distance >  _data.CombatRange) return EnemyState.Alert;
                return EnemyState.Combat;

            case EnemyState.Attacking:
                if (isAttacking)                   return EnemyState.Attacking;
                if (distance <= _data.AttackRange) return EnemyState.Attacking;
                return EnemyState.Combat;

            default: return EnemyState.Idle;
        }
    }
}
```

</details>


