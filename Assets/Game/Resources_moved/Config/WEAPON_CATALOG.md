# Weapon Catalog (Fantasy Hero + Kingdom)

Non-modular Synty weapon prefabs scanned from:

- `Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Weapons`
- `Assets/Synty/PolygonFantasyKingdom/Prefabs/Weapons` (excludes `Modular/`)

Excluded from catalog: `Modular/` parts, `_Cover` scabbards, arrows, quivers.

## Category → Grid → Icon Render

| 游戏类型 | WeaponCategory | 栏位 | 图标渲染方向 |
|---------|----------------|------|-------------|
| 单手剑/匕首/小斧 | Sword1H / Dagger1H / Misc1H | 6×2 | 横向 |
| 单手锤/钉头锤 | Hammer1H | 6×2 | 横向 |
| 盾牌 | Shield | 4×4 | **竖直** |
| 弓 | Bow | 10×2 | **竖直** |
| 长柄/法杖/权杖/双手斧(Kingdom) | Polearm2H | 10×2 | 横向 |
| 大锤/大剑 | GreatSword2H | 10×2 | 横向 |

## Pack Summary (approx.)

| Pack | Shield | Bow | Polearm2H | GreatSword2H | Sword1H | Hammer1H | Dagger1H | **合计** |
|------|--------|-----|-----------|--------------|---------|----------|----------|--------|
| Hero | 37 | 0 | 5 | 1 | 7 | 2 | 2 | **54** |
| Kingdom | 10 | 4 | 44 | 6 | 18 | 6 | 15 | **103** |
| **总计** | 47 | 4 | 49 | 7 | 25 | 8 | 17 | **157** |

Full list is generated into `WeaponItems.csv` via **Game → Weapon → 1. Generate WeaponItems.csv From Synty**.

## Pipeline

1. `Game/Weapon/1. Generate WeaponItems.csv From Synty`
2. `Game/Icon Studio/Create Or Update Weapon Icon Studio Scene` → Play → **F6** batch icons
3. `Game/Weapon/2. Generate ItemData + World Prefabs`
4. `Game/Weapon/Create Weapon Test Scene` → open `WeaponTest.unity`

Pick up weapons (auto or **R**) → items go to **NormalBack** → drag to **Weapon** grid (10×4).
