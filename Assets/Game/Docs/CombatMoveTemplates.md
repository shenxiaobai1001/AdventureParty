# 战斗招式模板（2026-07-22 大改后）

## 槽位字典

| Slot | 说明 |
| --- | --- |
| `melee.attack_1/2/3` | 普通三连击；防御/闪避打断后从 1 重来 |
| `melee.dash_attack` | 突进（各包 MoveAttack；没有则无此招） |
| `melee.block` | 格挡 |
| `melee.dodge.*` / `melee.roll.*` | 闪避 / 翻滚（持盾禁翻滚） |
| `react.hit.*` / `react.block_hit` / `react.block_break` | 反应 |
| `ranged.fire` / `ranged.reload` | 远程 |

已删除：`melee.guard_break`、`melee.reckless`、`melee.special`。

## 动画模板 ← 左右手负载（≠ 武器类型）

| 负载 | CombatMoveStance | 控制器 |
| --- | --- | --- |
| 空手 | MartialArts | Warrior Karate |
| 仅一把单手剑/锤/斧 | OneHandSingle | **RPG**（R1–R3 + SHARED_ARMED） |
| 单手 + 盾 | SwordShield | Warrior Knight |
| 双开刃（两把剑，含短刃） | DualBlades | Warrior Ninja |
| 其它双持（含斧/锤） | DualHeavy | Warrior Swordsman |
| 巨剑 / 巨锤巨斧 / 长枪 / 法杖 | GreatSword / HeavyWeapon2H / Spear / Staff | 对应 Warrior |
| 弓 / 弩 | RangedBow / RangedCrossbow | Archer / Crossbow |
| 火枪 / 投掷 | RangedRifle / Pistol / Throwing | **RPG** |

运行时表：`CombatMoveSlots.csv`。武器=武艺见 `WeaponProficiencyType`。
