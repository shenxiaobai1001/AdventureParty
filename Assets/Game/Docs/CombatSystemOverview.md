# 战斗系统总览（Combat System Overview）

> 本文档是 AdventureParty 战斗相关设计与落地的**唯一索引**。  
> 改规则先改本文档 + CSV，再改代码。聊天结论以本文档为准。

**最后更新：** 2026-07-15  
**状态：** 数据层已对齐四层模型；武艺线「匕首」已替换为「武术」；招式槽模板见 `CombatMoveTemplates.md`，运行时表 `CombatMoveSlots.csv` 已落地（播放层尚未接线）；自动交战 AI 与 XP 结算尚未接入运行时。

---

## 1. 四层模型

| 层       | 中文                | 代码                      | 职责                  | 不负责    |
| ------- | ----------------- | ----------------------- | ------------------- | ------ |
| ① 体质属性  | 力量 / 韧性 / 灵巧 / 精准 | `BodyAttributeType`     | 底子：负重、血量、攻速、要害等基础数值 | 临场决策   |
| ② 武器·技艺 | 巨剑…投掷（10 线）       | `WeaponProficiencyType` | 发展方向：该武器伤害/损耗与招式潜力  | “聪明地打” |
| ③ 战斗属性  | 攻击 / 防御 / 感知      | `FightAttributeType`    | 临场大脑：欲望、成功率、反应与战术选择 | 纸面数值底子 |
| ④ 风格    | （后做）              | TBD                     | 多样化：涌现 + 玩家选定 + 可进化 | —      |

**一句话：**  
体质 + 武艺 = 学院派的纸面水平；战斗属性 = 江湖生死里练出的临场；风格 = 打法个性。

学院派巨剑 vs 高战斗属性单手剑：前者可更重、更疼，但抢攻/识破/抓窗口会输给后者——自动战斗必须靠第 ③ 层。

> **闪避不是战斗属性。** 是否选择闪避/拉开由 **感知** 决策；能否及时完成由 **灵巧** 影响时机。面板若仍留有 `dodge` 节点，脚本忽略不绑定。

---

## 2. 命名对照

| 中文     | 枚举                                  | UI 节点（状态面板）                              | 旧名（已废弃）          |
| ------ | ----------------------------------- | ---------------------------------------- | ---------------- |
| 力量     | `BodyAttributeType.Strength`        | `Power`                                  | Attack           |
| 韧性     | `BodyAttributeType.Toughness`       | `Tough`                                  | Defense          |
| 灵巧     | `BodyAttributeType.Agility`         | `Flexible`                               | —                |
| 精准     | `BodyAttributeType.Precision`       | `Accurate`                               | —                |
| 武艺·巨剑等 | `WeaponProficiencyType.*`           | Greatsword / … / **MartialArts** / Throw | 旧「匕首」武艺 → **武术** |
| 武艺·武术  | `WeaponProficiencyType.MartialArts` | `MartialArts`（或仍叫 `Dagger` 的旧节点）         | Dagger           |
| 战斗·攻击  | `FightAttributeType.Offense`        | `ATK`                                    | —                |
| 战斗·防御  | `FightAttributeType.Defense`        | `Defense`                                | —                |
| 战斗·感知  | `FightAttributeType.Awareness`      | `Perception`                             | —                |

> **禁止**再把体质层叫作 Attack/Defense，以免与战斗属性冲突。

存档位置：`CharacterEntry.combatProficiency` → `CombatProficiencyProfile`  
（含 `attributes` 体质、`weaponProficiencies` 武艺、`fightAttributes` 战斗属性）

---

## 3. 体质属性细则

### 力量 Strength

- **效果：** 负重上限↑；接触式攻击基础伤害↑。  
  **重武器 / 巨剑 / 锤斧**：力量提高其攻击速度，至 soft-cap 后不再因力量提升，转而更吃灵巧。  
  **长剑 / 长兵等：** 力量不提高攻速。
- **成长：** 负重活动。负重 **> 上限 × 3** → 无法行动。
- **力量对决（见 §5.1）：** 进攻动作**已经成功命中/碰防**之后，用双方力量做碾压判定（破盾、完美防反压、攻速惩罚等）。

### 韧性 Toughness

- **效果：** 生命上限↑；硬直↓；受伤概率↓；负面状态恢复↑。
- **成长：** 挨打；受伤后**不治疗**、走自然恢复过程时。

### 灵巧 Agility

- **效果：** 攻击速度 / 防御动作速度 / 战斗移速↑；弓弩·火药·投掷装弹↑。
- **成长：** 不负重（轻装）战斗；使用远程或轻武器作战。
- **伪回合：** 「一回合」= 一次**攻击意图窗口**（持续进攻的一段交换）。灵巧足够高时，同一意图窗口内可自然打出**多次攻击**（不是无限，受窗口时长与动画约束；高灵巧表现为压制连打）。

### 精准 Precision

- **效果：** 要害概率与要害伤害↑；远程准度↑。
- **成长：** 暴击累计；经常使用远程作战。

---

## 4. 武器·技艺细则

**统一效果（所有武艺）：**

- 使用该武艺造成的伤害↑
- 对该武器的损耗↓
- **成长统一：勤加使用**（命中 / 参与交战 / 盾则含格挡）

**分武艺特长 / 弱点：**  
写在 `WeaponProficiencyConfig.csv` 的 `strengths` / `weaknesses`。  
**现阶段只记录，不做战斗互动**（后续再做相克与情境加成）。

| 武艺  | 特长                           | 弱点 / 特殊规则                              |
| --- | ---------------------------- | -------------------------------------- |
| 巨剑  | 攻击范围广、能破防、拥有堪比盾牌的防御力         | 起手慢、易被抓收招                              |
| 重武器 | 破防、单发高                       | 间隔较久                                   |
| 长兵  | 近战攻击距离远                      | 贴身劣势                                   |
| 弓弩  | 远程属性均衡                       | 需要装弹、低精准打不准，培养周期长                      |
| 盾   | 可达成**完美防御**（本次 0 伤，见 §5.2）   | 降低主手武器伤害                               |
| 长剑  | 攻击与攻速整体均衡；**短刃/匕首物品也归此武艺**   | 对重甲乏力；**吃负重惩罚**（负重越高越难发挥）              |
| 锤斧  | 比起长剑攻速略低但能破甲                 | 几乎没有防御能力                               |
| 武术  | 空手招式池大（对应 `Unarmed` 动画）；无需兵器 | 距离短、对持械/重甲吃亏                           |
| 火药  | 单发高伤害、高止动                    | 装填很慢                                   |
| 投掷  | 伤害随投掷武艺提升                    | **投的是背包/武器栏里的兵器**；丢光就没得投，AI 需自行捡回或转入近战 |

> **已移除「匕首」武艺线。** 匕首等短刃物品的 `WeaponCategory` 仍可为 `Dagger1H`（库存外形），但 `proficiencyType` 默认归 **长剑**；空手交战涨 **武术**（`MartialArts`）。

---

## 5. 战斗属性细则（自动交战大脑）

| 属性               | 作用                          |
| ---------------- | --------------------------- |
| **攻击 Offense**   | 进攻欲望与起手/对攻意图成功率             |
| **防御 Defense**   | 格挡、招架、硬抗交换的选择与成功率           |
| **感知 Awareness** | 察觉起手、选择闪避/拉开/风筝/追击、多人目标决策质量 |

### 5.1 交换顺序（伪回合）

1. **意图层（战斗属性）：** 双方决定进攻 / 防御 / 闪避拉开等，并做成功率检定。  
2. **动作层（灵巧等）：** 能否在窗口内摆出姿态、同一攻击意图内打几下。  
3. **接触层（武艺 + 体质）：** 命中后结算伤害、要害等。  
4. **力量对决（体质·力量）：** 仅在进攻动作**已成功接触**（打中对方兵器/盾/身）后触发。  

### 5.2 力量对决公式（已定稿口径）

采用 **力量差碾压**，不用「力量 × 战斗攻击」乘区——避免战斗属性再次污染纸面碾压，让「学院大力士」与「江湖高战斗属性」各管一层。

```
gap = Attacker.Strength - Defender.Strength
threshold = max(CrushMinAbsolute, Defender.Strength * CrushRatio)
// 建议初值：CrushMinAbsolute = 12，CrushRatio = 0.15（策划可调 CSV）

if gap >= threshold:
  进攻方力量碾压成立
elif gap <= -threshold:
  防守方力量碾压成立
else:
  势均力敌：盾/招架走常规结果，不额外破防也不完美反压
```

**进攻方碾压成立且对方在盾挡/招架：**  
打破本次防御 → 视为破防接触；对方再按装备、韧性等判断是否硬直。

**防守方碾压成立且己方成功做出防御意图：**  
**完美防御**（本次 **0 伤害**），并对进攻方施加一段 **攻速惩罚**。

双方力量差不多：常规格挡/招架，不触发上述碾压效果。

### 5.3 体质 × 攻防情境

| 交叉         | 规则                                            |
| ---------- | --------------------------------------------- |
| 力量 × 进攻接触后 | 见 §5.2 进攻碾压（破盾/破招架）                           |
| 力量 × 防御成功后 | 见 §5.2 防守碾压（完美防御 + 对方攻速惩罚）                    |
| 灵巧 × 攻击意图  | 同一伪回合攻击窗口内可连续多击；灵巧越高连打越自然                     |
| 灵巧 × 防御意图  | 只影响**摆出防御的速度**；高灵巧即使刚出完手也可能赶上防御，低灵巧可能判到了却摆不出来 |
| 灵巧 × 闪避动作  | 感知决定「要不要闪」；灵巧决定「闪得是否及时」（高：刀未到人已走；低：还没动完已挨刀）   |
| 精准 × 造成伤害时 | 判定是否暴击 / 要害                                   |
| 精准 × 防御成功时 | 触发 **完美防御**（本次 0 伤，即使对方带破防/重武）                |

> **完美防御**统一含义：本次交换 **不吃任何伤害**。来源可以是「力量防守碾压」或「精准在成功防御上的触发」，效果口径相同。

### 5.4 模拟对局（设计用例 / 未来 AI 逻辑）

A：高体质高巨剑武艺、**低战斗属性**  
B：中等体质武艺、**高战斗属性**单手剑  

1. 双方都想抢攻 → B 的高防御/感知挡或躲过 A 第一下（或仅吃到格挡伤害）。  
2. A 收招窗口 → B 高攻速 + 感知立刻反击。  
3. 若 A 韧性/装备顶不住 → 硬直，被 B 持续压制。  
4. 若 A 抗住且其 **战斗·攻击 > 战斗·防御** 倾向对攻 → B 感知再次读招格挡/规避。  
5. 远程高感知角色被贴身 → 优先跑路拉扯，而不是傻站射击。

---

## 6. 配置表清单

路径：`Assets/Game/Resources_moved/Config/`

| 文件                                  | 内容                                |
| ----------------------------------- | --------------------------------- |
| `BodyAttributesConfig.csv`          | 体质介绍 / 训练 / 等级影响文案                |
| `BodyAttributeGainConfig.csv`       | 体质成长行为 → XP                       |
| `FightAttributesConfig.csv`         | 战斗属性介绍 / 训练 / 影响（仅攻/防/感知）         |
| `FightAttributeGainConfig.csv`      | 战斗属性成长行为 → XP                     |
| `WeaponProficiencyConfig.csv`       | 武艺介绍、统一系数、优缺点文案                   |
| `WeaponProficiencyGainConfig.csv`   | 武艺“勤加使用” XP                       |
| `WeaponProficiencyLevelEffects.csv` | 武艺每级数值（伤害/损耗/特长项）                 |
| `WeaponItems.csv`                   | 武器实例 → category / proficiencyType |
| `CombatAttributesConfig.csv`        | **兼容旧名**，内容与 Body 表同步             |
| `CombatAttributeGainConfig.csv`     | **兼容旧名**，请优先改 Body 表              |

加载代码：`CombatProficiencyConfigData.cs` 内各 `*ConfigData` Singleton。

---

## 7. 关键代码索引

| 路径                                           | 作用                                |
| -------------------------------------------- | --------------------------------- |
| `Scripts/Combat/BodyAttributeType.cs`        | 体质枚举                              |
| `Scripts/Combat/FightAttributeType.cs`       | 战斗属性枚举（Offense/Defense/Awareness） |
| `Scripts/Combat/WeaponProficiencyType.cs`    | 武艺枚举                              |
| `Scripts/Combat/CombatProficiencyProfile.cs` | 角色进度存档结构                          |
| `Scripts/Combat/CombatProficiencyRuntime.cs` | 加 XP API                          |
| `Scripts/Combat/CombatLoadoutResolver.cs`    | 双手/双持/副手解析                        |
| `Scripts/Combat/CombatAnimBinding.cs`        | 装武 → Animator                     |
| `Scripts/Combat/WeaponProficiencyMapper.cs`  | Category → 武艺                     |
| `Scripts/UI/UIStatePanel.cs`                 | 状态面板（体质 + 战斗三属性 + 武艺）             |
| `Scripts/UI/CharacterEntry.cs`               | 角色条目含 `combatProficiency`         |
| `Docs/CombatMoveTemplates.md`                | **各武艺基础招式槽 + 动画填写表（AI 同模）**       |
| `Config/CombatMoveSlots.csv`                 | **姿态 × 槽位 → 动画**（运行时）               |
| `CombatMoveSlotConfigData` / `CombatMoveStance` | 槽位加载与按握法解析                       |

**尚未实现：**  
战斗 XP 结算钩子、负重系统、自然回血涨韧性、伪回合交战 AI、风格、武艺优缺点互动、空手武术 AI、投掷捡武 AI。

---

## 8. 实现分期（建议）

| 阶段     | 内容                                 | 完成标准           |
| ------ | ---------------------------------- | -------------- |
| **P0** | 本文档 + 枚举/表/面板改名 + 战斗属性数据槽          | ✅              |
| **P1** | 状态面板战斗属性 ATK/Defense/Perception    | ✅（已移除 Dodge）   |
| **P2** | 负重 / 挨打自愈 / 轻装战 / 暴击 → Body Gain   | 属性会涨           |
| **P3** | 命中交战 → Weapon Art Gain；战后战斗属性 Gain | 武艺与战斗属性会涨      |
| **P4** | 无动画伪回合：意图检定 + 力量对决 + 战斗日志          | 能复现 AB 用例与完美防御 |
| **P5** | 接动画包对招                             | 可见拼斗           |
| **P6** | 武艺优缺点互动 + 空手武术/投掷特殊 AI + 风格        | 深度内容           |

---

## 9. 改规则检查清单

改设计时依次：

1. 更新本文档对应章节  
2. 更新相关 CSV  
3. 如需新枚举值 → 改 `*Type.cs` + `CombatProficiencyProfile.EnsureDefaults`  
4. 如需展示 → 改 `UIStatePanel` 或新 UI  
5. 最后才写结算 / AI 代码  

---

## 10. 开放问题（文档维护区）

- [ ] 力量对重武攻速的 soft-cap 具体等级？  
- [ ] 负重公式、长剑负重惩罚曲线、与「三倍禁止行动」的 HUD？  
- [ ] `CrushMinAbsolute` / `CrushRatio` 正式数值（现文档建议 12 / 0.15）？  
- [x] 战斗属性是否也 1–100、同一 XP 曲线？  
- [x] 闪避是否为独立战斗属性？→ **否**，并入感知决策 + 灵巧时机  
- [x] 力量对决用力量差还是力量×战斗攻击？→ **力量差碾压**  
- [ ] 招式解锁表是否独立 CSV？  
- [ ] 风格：全局槽位数、进化层级命名？  
- [ ] 状态面板 WeaponPanel 标题是否改为「武器·技艺」？  
- [ ] 面板上多余的 `dodge` 节点是否删掉？  
