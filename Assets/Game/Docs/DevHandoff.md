# 开发交接备忘（Dev Handoff）

> **给另一台电脑上的 Cursor / 自己：** 先读本文，再读文末「相关文档」。  
> **最后更新：** 2026-07-25  
> **分支：** `main`

---

## 1. 一句话现状

**近战闭环已通：** 装备 → 拔刀 → 右键 NPC 攻击 → 扫掠/锁定出伤 → 受击；**不动 NPC 战斗模拟**（弱/一般/强发信号）+ 玩家 `CombatBrain`（有信号防/闪，无信号进攻）+ XP 已接。  
下一步：验各武艺招式、抽 `CombatMotor`、加深 Brain 意图对抗；移动 AI 暂缓，等 Brain 稳后再套同一套。

---

## 2. 本周做了什么（2026-07-22 → 07-25）

### 2.1 装备 / UI

- `WeaponEquipService` 智能装/卸；`UIEquip` 右键装备面板（ClickBlocker 全透明；卸下按钮常显）
- 装备持久化：`CharacterInventoryData` 左右手；角标 `EquippedMarker`
- Tag：`Teammate` / `NPC`；相机左键选队友
- `UIFloatOperation`：右键角色 → 队友占位 / 他人 `btn_Atk` 开战

### 2.2 动画 / 姿态

- `CombatAnimControllerCatalog`：按姿态切 Warrior / RPG Controller（**Editor 仍用 AssetDatabase，进包需改 Resources/Addressables**）
- `PlayerStanceController`：E 拔收；换武 sheath→unsheath
- 弓左持 / 盾专用 socket；火枪不再强扭 X+90
- 攻击走 `CombatMovePlayer` + Hit 动画事件

### 2.3 出伤 / 受伤

- `MeleeAttackController`：`Hit` 开窗 + 刀刃扫掠；锁定目标触及辅助
- `CombatHealth` / `CombatHurtbox`（层 `CombatHurt`）
- `CombatEngageService`：进战斗姿态、追到近战距、再轻攻击
- 文档：`MeleeSweepHitDetection.md`（武器 tip/root 手调说明）

### 2.4 战斗模拟（不动 AI）

- `CombatSimOpponent`：挂 NPC；弱/一般/强 → 属性快照；定时发 `CombatSimAttackSignal`
- `CombatBrain`：可复用决策模板（日后 NPC 直接套）
- `CombatSimXp`：攻/防/闪涨武艺与体质/战斗属性
- 菜单：`Game/Combat/Add CombatSimOpponent To Selected`
- 缺口：`CombatSimAndBrainGaps.md`

### 2.5 武器资源精简

- 每类 Item **保留 1 把**；其余在 `Data/Weapons/Items/_Backup`、`Prefabs/Weapons/World/_Backup`
- 菜单：`Game/Weapons/Archive Extra Weapons…` / `Game/Combat/Duplicate Selected Hero As NPC`

### 2.6 本地商包（不进 Git）

| 路径 | 说明 |
| --- | --- |
| `Assets/Synty/` | 已在 `.gitignore` |
| `Assets/Newanimaton/` | Warrior Mecanim 包，**本周起忽略**；各机本地导入 |

---

## 3. 接下来建议顺序

| 优先级 | 任务 | 说明 |
| --- | --- | --- |
| P0 | 各武艺招式肉眼验收 | CSV + Hit 事件 + 扫掠 tip |
| P1 | 抽 `CombatMotor` | Brain 只下命令，玩家键/AI 共用 |
| P2 | Brain 同时意图对抗 | 文档 §5.1 缩水版 |
| P3 | Controller 运行时加载 | 去掉纯 AssetDatabase |
| P4 | 真格挡窗 / 力量碾压 | 文档 §5.2 |
| P5 | 移动 AI | 套同一 `CombatBrain` + Motor |

---

## 4. 另一台电脑怎么接

1. `git pull`  
2. 本地准备：`Assets/Synty/` + `Assets/Newanimaton/ExplosiveLLC/`（Warrior 包）  
3. Unity 打开工程，等编译  
4. Cursor 丢给 Agent：`Assets/Game/Docs/DevHandoff.md`  
5. 建议开场白：  
   > 读 `DevHandoff.md`。Main 里给 NPC 加 `CombatSimOpponent`，测右键攻击与 Brain。下一步验武器招式或抽 CombatMotor。

快速自测：

1. Teammate 拔刀装备近战  
2. NPC 挂 `CombatSimOpponent`，Strength=Normal  
3. 右键 NPC → 攻击 → 应见出伤 + `[CombatSim]` / `[CombatBrain]` 日志  

---

## 5. 相关路径速查

| 用途 | 路径 |
| --- | --- |
| 本备忘 | `Assets/Game/Docs/DevHandoff.md` |
| 战斗总览 | `Assets/Game/Docs/CombatSystemOverview.md` |
| 扫掠出伤 | `Assets/Game/Docs/MeleeSweepHitDetection.md` |
| Sim/Brain 缺口 | `Assets/Game/Docs/CombatSimAndBrainGaps.md` |
| 招式表 | `Assets/Game/Docs/CombatMoveTemplates.md` |
| 槽位 CSV | `Assets/Game/Resources_moved/Config/CombatMoveSlots.csv` |
| 战斗脚本 | `Assets/Game/Scripts/Combat/`（含 `Sim/`） |
| 浮层 UI | `UIFloatOperationPanel.cs` / `UIEquipPanel.cs` |

---

## 6. 维护约定

换机器或阶段性收工时，**先改本文件 §1–§3**，再提交推送，避免下一台只靠聊天记录猜进度。
