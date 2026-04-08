# 아키텍처 — Item · Inventory · Shop

> 최종 업데이트: 2026-04-02

---

## Item — 아이템 데이터 모델

### 기반
| 파일 | 설명 |
|------|------|
| `Item/ItemData.cs` | abstract 클래스. 모든 아이템 공통 기반 (`ItemId`, `ItemName`, `ItemGrade`, `MaxStackSize`, `Value`) |
| `Item/ItemFactory.cs` | `ItemCatalogEntry.ItemType` 에 따라 적절한 `ItemData` 구현체를 생성하는 정적 팩토리 |

### Catalog (아이템 카탈로그)
| 파일 | 설명 |
|------|------|
| `Item/Catalog/ItemCatalogEntry.cs` | CSV 1행 데이터 클래스. 공통 6열 + 타입별 선택 6열 |
| `Item/Catalog/IItemCatalogRepository.cs` | 카탈로그 저장소 인터페이스 (`GetEntry`, `GetEntryByResourceType`, `GetAll`) |
| `Item/Catalog/ItemCatalogRepository.cs` | ItemId / ResourceType 이중 인덱스 구현체 |
| `Item/Catalog/CsvItemCatalogParser.cs` | CSV 파서. 필수 6열 + 선택 6열 |
| `Item/Catalog/ItemCatalogLoader.cs` | `TextAsset` → `IItemCatalogRepository` 변환 정적 로더 |

### Resource (자원 아이템)
| 파일 | 설명 |
|------|------|
| `Item/Resource/ResourceItem.cs` | `ItemData` + `IStackable` 구현. 채취 자원 아이템 (`ResourceType`, `Quantity`) |

### Equipment (장비)
| 파일 | 설명 |
|------|------|
| `Item/Equipment/IEquipment.cs` | 장비 인터페이스 + `EquipmentSlot` enum (Weapon, Armor 등) |
| `Item/Equipment/Weapon.cs` | `ItemData` + `IEquipment` 구현. 공격력 수치 보유 |
| `Item/Equipment/Armor.cs` | `ItemData` + `IEquipment` 구현. 방어력 수치 보유 |

### Tool (도구)
| 파일 | 설명 |
|------|------|
| `Item/Tool/ITool.cs` | 도구 인터페이스 (`ToolType`, 효율 배율 등) |
| `Item/Tool/HarvestTool.cs` | `ItemData` + `ITool` 구현. 채취 도구 (도끼, 곡괭이) |
| `Item/Tool/FarmTool.cs` | `ItemData` + `ITool` 구현. 농기구 (낫, 물뿌리개) |

---

## Inventory — 인벤토리 시스템

| 파일 | 설명 |
|------|------|
| `Inventory/IInventory.cs` | 인벤토리 서비스 인터페이스 (`TryAddItem(ItemData)`, `GetSlot`, `RemoveAt`, `MoveItem`, `OnInventoryChanged`) |
| `Inventory/Inventory.cs` | `IInventory` 구현체. `ItemData[]` 슬롯 배열로 위치 영속성 보장. `IStackable` 아이템 자동 합산 |

> 슬롯 인덱스 = 아이템 위치. 인벤토리를 열고 닫아도 위치 유지.
> UI 드래그 시 `MoveItem(from, to)` 으로 두 슬롯 교환.
> `IStackable` + `MaxStackSize > 1` 인 아이템은 동일 ItemId 슬롯에 자동 합산, 넘치면 새 슬롯 배치.

---

## Shop — 상점 시스템

| 파일 | 설명 |
|------|------|
| `Shop/ShopResult.cs` | 구입·판매 결과 Enum (`Success`, `NotEnoughGold`, `InventoryFull`, `InvalidItem`) |
| `Shop/ShopItemInfo.cs` | 상점 판매 항목 불변 데이터 (`ItemId`, `ItemName`, `BuyPrice`) |
| `Shop/IShopService.cs` | 상점 서비스 인터페이스 (`ShopItems`, `GetSellPrice`, `TryBuy`, `TrySell`) |
| `Shop/ShopService.cs` | `IShopService` 구현체. 골드 소비·환불, 인벤토리 추가·제거 처리 |

> `ShopService` 는 `TownSceneInitializer` 에서 `IShopService` 로 등록합니다.
> `ShopTab.cs` (Buy/Sell 탭 Enum)는 삭제되었습니다. 상점 UI는 구매 전용입니다.
> 판매는 별도 `SellUI` 팝업(`PopupType.Sell`)으로 분리되었습니다.
