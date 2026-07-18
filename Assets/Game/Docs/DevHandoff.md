# 开发交接备忘（Dev Handoff）

> **给另一台电脑上的 Cursor / 自己：** 先读本文，再读文末「相关文档」。  
> **最后更新：** 2026-07-18  
> **分支：** `main`

---

## 1. 一句话现状

战斗**设计与招式槽数据**已基本落地；**还不能真正自动打架**。  
下一步不是重做 Animator，而是做「槽位 → 播动画」的播放层。

---

## 2. 最近做了什么

### 2.1 战斗成长四层模型（数据 + UI）

- 体质：力量 / 韧性 / 灵巧 / 精准  
- 武艺：10 线；原「匕首」武艺改为 **武术 MartialArts**（徒手）；物理短刃仍归 **长剑** XP  
- 战斗属性：攻击 / 防御 / 感知（**没有**闪避 Fight 属性）  
- 风格层：后做  
- 配置 CSV：`BodyAttributes*`、`FightAttributes*`、`WeaponProficiency*`  
- 状态面板：`UIStatePanel` 绑定上述数值  

### 2.2 武器栏 / 握法 / 拔刀动画绑定

- `CombatLoadoutResolver`：从武器格解析 `CombatGripMode`（单持 / 双持 / 双手 / 空手…）  
- `CombatAnimBinding` + `PlayerStanceController`：把握法写到 RPG 包 Animator（`Weapon` / `Side` 等）  
- 武器分类、图标、背挂/手持布局等一批武器管线改动  

### 2.3 招式模板（策划表）

- 文档：`Assets/Game/Docs/CombatMoveTemplates.md`  
- 关键约定：  
  - 动画按**握法姿态**，不按武艺分表（剑/锤/斧单手共用）  
  - **单持** vs **双持** 拆普攻 / 大招；格挡与破防踢走 **Armed 共享**  
  - 闪避翻滚：双手三族本表；单手/远程 → `SHARED_ARMED`；武术 → `SHARED_UNARMED`  
  - 法杖与长兵共用；持盾强制盾 Block、禁翻滚  
  - 大招解锁统一 `art>=66`  

### 2.4 招式槽运行时数据（刚做完）

| 文件 | 作用 |
| --- | --- |
| `Assets/Game/Resources_moved/Config/CombatMoveSlots.csv` | 135 行：姿态 × 槽位 → clip / combo |
| `Assets/Game/Scripts/Combat/CombatMoveStance.cs` | 姿态枚举 |
| `Assets/Game/Scripts/Combat/CombatMoveSlotConfigData.cs` | 加载 + `TryResolve`（共享引用、持盾覆盖） |

查询示例：

```csharp
CombatMoveSlotConfigData.Instance.TryResolve(loadout, "melee.attack_a", out var slot);
// slot.animAsset / slot.comboSequence
```

### 2.5 武器攻击预览场景（辅助填表）

- 菜单：`Game → Animation → Build Weapon Attack Preview Scene`  
- 场景：`Assets/Game/Scenes/WeaponAttackPreview.unity`  
- 脚本：`Assets/Game/Scripts/Animation/WeaponAttackPreview*`  

### 2.6 Animator 结论（已对齐，勿重做）

现有 `RPG-Character-Animation-Controller` **够用**：Armed / Dual / 盾 / 2H 各族 / 踢 / 闪滚都在。  
缺的是游戏侧「读槽位 → 调包 API 播」，不是新状态机。

---

## 3. 接下来准备做什么（建议顺序）

| 优先级 | 任务 | 说明 |
| --- | --- | --- |
| **P0** | **播放层** | 薄封装：解析槽位 → 设 `Weapon`/`Side`/`Action` → 触发 Attack/Block/Dodge/Kick；固定 A/B 编号，勿走包内随机 Attack |
| P1 | 补空 clip | 双手三族 `melee.block` 等仍空，可改 CSV 或模板文档 |
| P2 | 舍身 COMBO | `melee.reckless` = 状态 + `attack_a→attack_b` 链，非单 Trigger |
| P3 | 伪回合 AI | 意图检定只从槽位选型（同模） |
| P4 | XP / 伤害结算 | 武艺与体质增长接入真实交战 |
| — | 开放项 | 舍身惩罚；持盾能否舍身；`ranged.aim`；双持混持 XP；无大招是否永久 `—` |

**不要做：** 为战斗另起一套 Animator Controller；按 Longsword/HammerAxe 再拆两套单手动画表。

---

## 4. 另一台电脑怎么接

1. `git pull`（本备忘随本次提交进 `main`）  
2. 用 Unity 打开工程，等脚本编译  
3. 新开 Cursor，把本文件路径丢给 Agent：  
   `Assets/Game/Docs/DevHandoff.md`  
4. 建议开场白：  
   > 读 `DevHandoff.md`，从 **P0 播放层** 继续：把 `CombatMoveSlotConfigData.TryResolve` 接到 RPG Character 的 Attack/Block/Dodge/Kick。

---

## 5. 关键路径速查

| 用途 | 路径 |
| --- | --- |
| 本备忘 | `Assets/Game/Docs/DevHandoff.md` |
| 战斗总览 | `Assets/Game/Docs/CombatSystemOverview.md` |
| 招式填写表 | `Assets/Game/Docs/CombatMoveTemplates.md` |
| 槽位 CSV | `Assets/Game/Resources_moved/Config/CombatMoveSlots.csv` |
| 战斗脚本目录 | `Assets/Game/Scripts/Combat/` |
| 握法绑定 | `CombatAnimBinding.cs` / `CombatLoadoutResolver.cs` |
| RPG 控制器 | `Assets/Game/Animation/RPG Character Mecanim Animation Pack/Animation Controller/RPG-Character-Animation-Controller.controller` |

---

## 6. 维护约定

换机器继续干活或阶段性收工时，**先改本文件的 §2 / §3**，再提交，避免下一台 Cursor 只靠聊天记录猜进度。
