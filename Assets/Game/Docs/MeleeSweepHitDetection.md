# Melee Sweep Hit Detection

近战出伤以 **动画 `Hit` 事件开窗 + 刀刃/肢胶囊扫掠** 为准。远程武器不加近战扫掠。

## 自动做了什么

- 角色：`CombatHealth` + `CombatHurtbox`（身体 Trigger）+ `MeleeAttackController` + 武术手脚 `MeleeSweepSource`
- 拔出近战武器时：在武器实例上加 `MeleeSweepSource`，默认用 **Renderer 包围盒最长轴** 估 tip/root
- `Hit` 事件 → 开窗约 0.14s → 每 FixedUpdate 在帧间扫掠 → 碰到 `CombatHurt` 层 Hurtbox 则扣血并播受击

## 你需要手动调的情况

若刀看起来「空挥也出伤」或「砍中不出伤」，在对应 **武器 Item** 上勾选覆盖：

路径：`Assets/Game/Data/Weapons/Items/{Category}/*.asset`  
字段：`SyntyWeaponItemData`

| 字段 | 含义 |
|------|------|
| `overrideMeleeSweep` | 勾选后使用下面三项，不再用网格自动估 |
| `meleeSweepLocalRoot` | 刀身近握把端（武器实例本地坐标） |
| `meleeSweepLocalTip` | 刀尖端 |
| `meleeSweepRadius` | 胶囊半径 |

也可在 Play 时选中场景里手持武器上的 `MeleeSweepSource`，看橙色 Gizmo（选中物体时），直接改 `localRoot` / `localTip` / `radius`。  
**注意：** 仅改场景实例会在下次重生武器时丢失；要持久化请改 Item 的 override，或以后做到 prefab 上。

### 建议调法

1. 装备并拔刀，选中手上武器
2. Scene 视图看橙线是否贴合刀刃
3. 调 tip 到刀尖、root 到护手附近，半径略大于刃厚
4. 数值满意后抄到该武器 Item 的 override 字段

弓 / 弩 / 枪 / 盾：自动 `contributesToMelee = false`，无需调扫掠。

武术：手脚在 `Hand_*` / `Foot_*`（或 `Ball_*`）下的 `LimbHit_*`，一般够用；踢腿打不准则调对应 `MeleeSweepSource` 的 tip 长度。
