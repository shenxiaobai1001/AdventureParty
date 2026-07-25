# 战斗模拟（Combat Sim）与角色战斗 AI 缺口

## 已落地

| 组件 | 作用 |
|------|------|
| `CombatSimOpponent` | 挂 NPC；玩家右键攻击后开局；**不移动**；按弱/一般/强发攻击信号 |
| `CombatSimAttackSignal` | 含伤害、攻方属性快照、前摇/反应窗 |
| `CombatBrain` | 玩家侧决策：**有信号→防御/闪避，无信号→进攻**；可作日后 NPC 模板 |
| `CombatSimXp` | 攻/防/闪成功与失败涨武艺与体质/战斗属性 |

### 用法

1. 选中 NPC → 菜单 `Game/Combat/Add CombatSimOpponent To Selected`
2. Inspector 设 `Strength` = Weak / Normal / Strong
3. Play：左键选队友 → 右键 NPC → 攻击 → 模拟交战开始
4. Console 可见 `[CombatSim]` / `[CombatBrain]` / `[CombatSimXp]`

弱/一般/强大致等级：体/战 ≈ 3 / 8 / 14，武艺略低一档。

---

## 还差什么（完善角色战斗 AI → 再套给 NPC）

按优先级：

### A. 决策层（CombatBrain 深化）

1. **意图对抗**：双方同时报进攻/防御/闪避，用 Offense/Defense/Awareness 做成功率，而不是只对「对方已发出的信号」反应  
2. **连招窗口**：灵巧决定同一意图内打几下（文档伪回合动作层）  
3. **收招惩罚 / 抓 whiff**：对方攻击落空后的反击权重（`punishWhiff`）  
4. **手动优先**：玩家键位覆盖 AI（已有开关，需接到 UI/默认键）

### B. 执行层（CombatMotor，建议新建）

玩家与 AI **共用**执行接口，Brain 只下命令：

- EnterCombat / Face / AttackLight / StartBlock / DodgeDir / Stagger  
- 避免 Brain 里直接调一堆组件（现在略耦合）

### C. 接触与防御深化

1. 格挡 vs 扫掠：真格挡窗（非仅模拟掷骰）  
2. 力量碾压破防 / 完美防御（文档 §5.2）  
3. 武器相克（CSV strengths/weaknesses 尚未进战斗）  
4. 受击方向（front/back）与硬直表

### D. 成长与存档

1. `HeroCombatProficiency` 绑到真正的 `CharacterEntry` 存档（现在可运行时新建）  
2. WeaponGain CSV 列名 `trigger/xp` 与代码 `actionKey/baseXp` 对齐  
3. UI 状态面板实时刷新等级

### E. 再交给 NPC 时

1. NPC 挂同一套 `CombatBrain` + `CombatMotor`  
2. 用 `CombatSimStrength` 或档案初始化属性  
3. **再加**移动/索敌（你明确先不做）  
4. 去掉或降级 `CombatSimOpponent` 纯发信器，改为 Brain 自己起手进攻

### F. 武器动作完善（你提到的并行线）

与 AI 独立，但影响 Brain 手感：

- 各武艺 attack_1/2/3、block、dodge、hit react 槽位验收  
- 扫掠 tip/root 按武器 override  
- Hit 事件覆盖率补齐

---

## 建议下一刀

1. 抽 `CombatMotor`（玩家键 + Brain 共用）  
2. Brain 意图对抗（同时决策）  
3. 一边验武器招式表  

移动 AI 等 Brain 稳定后再套。
