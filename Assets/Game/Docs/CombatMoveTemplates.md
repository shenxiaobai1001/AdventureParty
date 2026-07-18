# 武器基础战斗招式模板（Combat Move Templates）

> **用途：** 定义「每条武艺 / 每种战斗姿态」在自动战斗与玩家操控中共用的招式槽。  
> AI 意图检定选的是**槽位**，再播表内动画；不要为 AI 另做一套。  
> **填表方式：** 只填仍为空的「攻击 / 格挡 / 踢 / 大招」等列；**闪避·翻滚已预填或走共享，不必再抄名字。**  
> **相关：** [`CombatSystemOverview.md`](./CombatSystemOverview.md) · 预览场景 `WeaponAttackPreview` · 动画包 `RPG Character Mecanim Animation Pack`

**最后更新：** 2026-07-18  
**状态：** 槽位语义已定稿；**运行时数据已落地** `Resources_moved/Config/CombatMoveSlots.csv` + `CombatMoveSlotConfigData`（按姿态查询，非按武艺）；双手三族格挡 clip 仍可补；播放层尚未接线。

---

## 0. 怎么填

1. 打开预览场景对应武器组，挑好看的 clip。  
2. 把 **完整资产名**（不含扩展名）填进仍为空的格，例：`RPG-Character@2Hand-Sword-Attack3`。  
3. **`melee.attack_a` / `melee.attack_b` 各固定 1 条**（不要写 Attack1…N 池）。  
4. 「无匹配动画」：填 `—` 或 `UI_ONLY`。  
5. 大招解锁统一 **`art>=66`**；无大招写 `—`。  
6. **闪避 / 翻滚：**  
   - 巨剑 / 重武器 / 长兵 → 表内**已写死本族资源**，不用再填。  
   - 单手剑·锤斧·双持·持盾主手·远程 → **`→ SHARED_ARMED`**（§1.5 已预填）。  
   - 武术 → **`→ SHARED_UNARMED`**（§1.5 已预填）。  
7. **法杖**不单独做模板，动画与决策都走 **长兵 §3.3**（持杖时 Animator 仍可用 Staff，招式槽同一套）。

**填写约定**

| 列          | 含义                                     |
| ---------- | -------------------------------------- |
| 槽位 ID      | 代码 / AI 用的稳定 key                       |
| 槽位名        | 中文展示                                   |
| 动画资产名      | 已预填或待你填；`→ SHARED_*` 表示引用 §1.5         |
| Animator 族 | `TWOHANDSWORD` / `ARMED` / `UNARMED` … |
| 解锁         | `default` / `art>=66` / `—`            |
| AI 权重提示    | 伪回合意图提示                                |

---

## 1. 通用槽位字典

### 1.1 近战通用

| 槽位 ID               | 槽位名      | 语义            | 备注                 |
| ------------------- | -------- | ------------- | ------------------ |
| `melee.attack_a`    | 普通攻击 A   | **固定一招**      | 与 B 组成稳定两连         |
| `melee.attack_b`    | 普通攻击 B   | **固定一招**      | 高灵巧窗口内可 A→B        |
| `melee.block`       | 格挡       | 举防；与防御检定配合    | 格挡=招架。有盾→强制盾 Block |
| `melee.guard_break` | 破防 / 正蹬  | 破防或正面踢        | 常为 Kick / 指定突刺     |
| `melee.dodge`       | 闪避       | 短位移           | 双手本族预填；单手/远程/空手→共享 |
| `melee.roll`        | 翻滚       | 更大位移；**持盾禁用** | 同上                 |
| `melee.reckless`    | 舍身（狂战窗口） | 状态+组合，非单动画    | 见 §1.4             |
| `melee.special`     | 大招       | 单段强力招         | **`art>=66`**      |

方向子键：`dodge.back/left/right` · `roll.forward/back/left/right`

### 1.2 远程通用

| 槽位 ID           | 槽位名 | 语义   | 备注                        |
| --------------- | --- | ---- | ------------------------- |
| `ranged.fire`   | 射击  | 主输出  | 投掷=掷出                     |
| `ranged.reload` | 装弹  | 空仓必经 | 无动画 → `UI_ONLY`           |
| `ranged.kick`   | 踢腿  | 贴身应急 |                           |
| `ranged.dodge`  | 闪避  | 保命位移 | **→ SHARED_ARMED**，不必分武艺填 |

### 1.3 伪回合意图 → 槽

| 意图        | 常映射槽                                          |
| --------- | --------------------------------------------- |
| 进攻 / 对攻   | `melee.attack_a/b` · `ranged.fire`            |
| 防御 / 格挡   | `melee.block`（持盾权重更高且动画强制盾）                   |
| 压制 / 破防   | `melee.guard_break`                           |
| 舍身狂战      | `melee.reckless` → 链 A/B 组合                   |
| 感知躲开 / 拉开 | `melee.dodge` · `melee.roll` · `ranged.dodge` |
| 贴身风筝失败    | `ranged.kick` 后 `ranged.dodge`                |
| 一锤定音      | `melee.special`（art≥66）                       |

### 1.4 定稿规则

| #   | 议题     | 定稿                                                                                                               |
| --- | ------ | ---------------------------------------------------------------------------------------------------------------- |
| 1   | 平 A    | `attack_a` / `attack_b` **各固定一条**                                                                                |
| 2   | 格挡     | 同一槽；默认主手 Block；**有盾一律盾 Block**                                                                                   |
| 3   | 舍身     | 狂战状态+组合（默认 A→B）；无视自身硬直、不防御；结束后长惩罚（惩罚数值 TBD）                                                                      |
| 4   | 盾+主手闪避 | 走**主手表** = SHARED_ARMED；盾表不填闪避；持盾禁翻滚                                                                             |
| 5   | 法杖     | **不单独开表**；与 **长兵 Polearm** 共用 §3.3                                                                               |
| 6   | 大招     | 统一 **`art>=66`**                                                                                                 |
| 7   | 闪避翻滚填写 | 双手三族**文档已引用包内资源**；单手/空手/远程**只引用共享表**                                                                             |
| 8   | 单手动画模板 | **剑 / 锤 / 斧共用同一套动画**。真正要拆的是握法：**单持一把** vs **双手各一把（双持）** → **普攻 / 大招**分两张姿态表。武艺 XP（长剑 vs 锤斧）仍分开结算。 |
| 9   | 单手格挡 / 破防 | **格挡**单持与双持都走 **Armed Block**（有盾→盾 Block）。**破防**走通用 Armed 踢击（§1.5 `SHARED_ARMED_KICK`），单持/双持不必再各填一套。 |

### 1.5 共享闪避 / 翻滚（已预填，单手·远程·空手用）

#### SHARED_ARMED（长剑 / 双持 / 锤斧 / 持盾时主手闪避；远程闪避）

| 槽位 ID                | 动画资产名                                |
| -------------------- | ------------------------------------ |
| `melee.dodge.back`   | `RPG-Character@Armed-Dodge-Backward` |
| `melee.dodge.left`   | `RPG-Character@Armed-Dodge-Left`     |
| `melee.dodge.right`  | `RPG-Character@Armed-Dodge-Right`    |
| `melee.roll.forward` | `RPG-Character@Armed-Roll-Forward`   |
| `melee.roll.back`    | `RPG-Character@Armed-Roll-Backward`  |
| `melee.roll.left`    | `RPG-Character@Armed-Roll-Left`      |
| `melee.roll.right`   | `RPG-Character@Armed-Roll-Right`     |

> 持盾时只用闪避三条，翻滚禁用。远程 `ranged.dodge.*` 同样引用上表闪避三行。

#### SHARED_ARMED_KICK / BLOCK（单手单持·双持共用）

| 槽位 ID | 动画资产名 | 说明 |
| --- | --- | --- |
| `melee.guard_break` | `RPG-Character@Armed-Attack-Kick-R1` | 通用破防踢；单持/双持同一条 |
| `melee.block`（无盾） | `RPG-Character@Armed-Block-R` | 单持默认右持格挡；左持可用 `Armed-Block-L` |
| `melee.block`（双持无盾） | `RPG-Character@Armed-Block-Dual` | 仍属 Armed 防御族，控制器按 `Side=Dual` 进此态 |

> 有盾时 `melee.block` **强制** `RPG-Character@Armed-Shield-Block`（§3.6），不读上表。

#### SHARED_UNARMED（武术）

| 槽位 ID                | 动画资产名                                  |
| -------------------- | -------------------------------------- |
| `melee.dodge.back`   | `RPG-Character@Unarmed-Dodge-Backward` |
| `melee.dodge.left`   | `RPG-Character@Unarmed-Dodge-Left`     |
| `melee.dodge.right`  | `RPG-Character@Unarmed-Dodge-Right`    |
| `melee.roll.forward` | `RPG-Character@Unarmed-Roll-Forward`   |
| `melee.roll.back`    | `RPG-Character@Unarmed-Roll-Backward`  |
| `melee.roll.left`    | `RPG-Character@Unarmed-Roll-Left`      |
| `melee.roll.right`   | `RPG-Character@Unarmed-Roll-Right`     |

---

## 2. 武艺 ↔ 动画族速查

| 武艺           | 建议 Animator 族               | 预览组         | 近战/远程 | 招式动画模板                        | 闪避翻滚             |
| ------------ | --------------------------- | ----------- | ----- | ----------------------------- | ---------------- |
| GreatSword   | `2Hand-Sword`               | 01_2H_Sword | 近战    | §3.1 本表                       | 本族（已预填）          |
| HeavyWeapon  | `2Hand-Axe`                 | 03_2H_Axe   | 近战    | §3.2 本表                       | 本族（已预填）          |
| Polearm（含法杖） | `2Hand-Spear`（杖可用 Staff 网格） | 02_2H_Spear | 近战    | §3.3 本表                       | 本族（已预填）          |
| Longsword    | `ARMED` + 剑网格               | 08 / 14     | 近战    | **单持 → §3.4A**；**双持 → §3.4B** | → SHARED_ARMED   |
| HammerAxe    | `ARMED` + 锤/斧网格             | 09 / 14     | 近战    | **同上（与长剑共用姿态表）**              | → SHARED_ARMED   |
| MartialArts  | `UNARMED`                   | 16          | 近战    | §3.5                          | → SHARED_UNARMED |
| Shield       | `Armed-Shield`              | 15          | 近战    | §3.6（格挡优先）；主手仍走 §3.4A         | → SHARED_ARMED   |
| BowCrossbow  | Bow / Crossbow              | 05 / 06     | 远程    | §4.1                          | → SHARED_ARMED   |
| Firearm      | Shooting / Pistol           | 07 / 13     | 远程    | §4.2                          | → SHARED_ARMED   |
| Throwing     | Item / ARMED                | 12          | 远程    | §4.3                          | → SHARED_ARMED   |

> **单手姿态判定（运行时）：**  
> 
> - **单持：** 仅主手（或仅副手）有一件 1H 兵器；可搭配盾（盾只覆盖 `melee.block`）。  
> - **双持：** 左右手各持一件 1H 兵器（剑剑 / 锤锤 / 混持均用 **§3.4B** 同一套 Dual 动画）。  
>   短刃物品归长剑武艺，动画仍走 §3.4。

---

## 3. 近战武艺填表

### 3.1 巨剑 · GreatSword

| 槽位 ID                | 槽位名     | 动画资产名                                                                                | Animator 族    | 解锁      | AI 权重提示 |
| -------------------- | ------- | ------------------------------------------------------------------------------------ | ------------- | ------- | ------- |
| `melee.attack_a`     | 普通攻击 A  | `RPG-Character@2Hand-Sword-Attack5`                                                  | `2Hand-Sword` | default | 常用进攻    |
| `melee.attack_b`     | 普通攻击 B  | `RPG-Character@2Hand-Sword-Attack4`                                                  | `2Hand-Sword` | default | 连段 / 变招 |
| `melee.block`        | 格挡      |                                                                                      | `2Hand-Sword` | default | 防御意图    |
| `melee.guard_break`  | 破防 / 正蹬 | `RPG-Character@2Hand-Sword-Attack9` / `RPG-Character@2Hand-Sword-Attack-Kick-L1`     | `2Hand-Sword` | default | 对方格挡时加压 |
| `melee.dodge.back`   | 闪避·后    | `RPG-Character@2Hand-Sword-Dodge-Backward`                                           | `2Hand-Sword` | default | 感知躲开    |
| `melee.dodge.left`   | 闪避·左    | `RPG-Character@2Hand-Sword-Dodge-Left`                                               | `2Hand-Sword` | default |         |
| `melee.dodge.right`  | 闪避·右    | `RPG-Character@2Hand-Sword-Dodge-Right`                                              | `2Hand-Sword` | default |         |
| `melee.roll.forward` | 翻滚·前    | `RPG-Character@2Hand-Sword-Roll-Forward`                                             | `2Hand-Sword` | default | 拉近 / 突进 |
| `melee.roll.back`    | 翻滚·后    | `RPG-Character@2Hand-Sword-Roll-Backward`                                            | `2Hand-Sword` | default | 拉开      |
| `melee.roll.left`    | 翻滚·左    | `RPG-Character@2Hand-Sword-Roll-Left`                                                | `2Hand-Sword` | default |         |
| `melee.roll.right`   | 翻滚·右    | `RPG-Character@2Hand-Sword-Roll-Right`                                               | `2Hand-Sword` | default |         |
| `melee.reckless`     | 舍身狂战    | `RPG-Character@2Hand-Sword-Attack6` + `RPG-Character@2Hand-Sword-Attack8` 循环两次       | —             | default | 见 §1.4  |
| `melee.special`      | 大招      | 跳起 `RPG-Character@2Hand-Sword-Air-Attack1` + `RPG-Character@2Hand-Sword-Air-Attack1` | `2Hand-Sword` | art>=66 |         |

**策划备注：**

---

### 3.2 重武器 · HeavyWeapon

| 槽位 ID                | 槽位名     | 动画资产名                                                                        | Animator 族  | 解锁      | AI 权重提示 |
| -------------------- | ------- | ---------------------------------------------------------------------------- | ----------- | ------- | ------- |
| `melee.attack_a`     | 普通攻击 A  | `RPG-Character@2Hand-Axe-Attack5`                                            | `2Hand-Axe` | default |         |
| `melee.attack_b`     | 普通攻击 B  | `RPG-Character@2Hand-Axe-Attack2`                                            | `2Hand-Axe` | default |         |
| `melee.block`        | 格挡      |                                                                              | `2Hand-Axe` | default | 防御意图    |
| `melee.guard_break`  | 破防 / 正蹬 | `RPG-Character@2Hand-Axe-Attack1` / `RPG-Character@2Hand-Axe-Attack-Kick-R1` | `2Hand-Axe` | default | 破防特长优先  |
| `melee.dodge.back`   | 闪避·后    | `RPG-Character@2Hand-Axe-Dodge-Backward`                                     | `2Hand-Axe` | default |         |
| `melee.dodge.left`   | 闪避·左    | `RPG-Character@2Hand-Axe-Dodge-Left`                                         | `2Hand-Axe` | default |         |
| `melee.dodge.right`  | 闪避·右    | `RPG-Character@2Hand-Axe-Dodge-Right`                                        | `2Hand-Axe` | default |         |
| `melee.roll.forward` | 翻滚·前    | `RPG-Character@2Hand-Axe-Roll-Forward`                                       | `2Hand-Axe` | default |         |
| `melee.roll.back`    | 翻滚·后    | `RPG-Character@2Hand-Axe-Roll-Backward`                                      | `2Hand-Axe` | default |         |
| `melee.roll.left`    | 翻滚·左    | `RPG-Character@2Hand-Axe-Roll-Left`                                          | `2Hand-Axe` | default |         |
| `melee.roll.right`   | 翻滚·右    | `RPG-Character@2Hand-Axe-Roll-Right`                                         | `2Hand-Axe` | default |         |
| `melee.reckless`     | 舍身狂战    | `RPG-Character@2Hand-Axe-Attack6` + `RPG-Character@2Hand-Axe-Attack1` 两个循环   | —           | default | 见 §1.4  |
| `melee.special`      | 大招      | `RPG-Character@2Hand-Axe-Attack4` 播完停住再旋转角色做龙卷风式攻击                           | `2Hand-Axe` | art>=66 |         |

**策划备注：** 重武器含巨锤与巨斧，不必再拆表。

---

### 3.3 长兵 · Polearm（矛 / 叉 / **法杖共用本表**）

> 法杖**不再单独开副表**。持杖时仍用本槽位；视觉可用 Staff 网格，闪避翻滚先用矛族资源（与长兵统一）。

| 槽位 ID                | 槽位名     | 动画资产名                                                                             | Animator 族    | 解锁      | AI 权重提示 |
| -------------------- | ------- | --------------------------------------------------------------------------------- | ------------- | ------- | ------- |
| `melee.attack_a`     | 普通攻击 A  | `RPG-Character@2Hand-Spear-Attack5`                                               | `2Hand-Spear` | default | 戳刺保距    |
| `melee.attack_b`     | 普通攻击 B  | `RPG-Character@2Hand-Spear-Attack6`                                               | `2Hand-Spear` | default |         |
| `melee.block`        | 格挡      |                                                                                   | `2Hand-Spear` | default | 防御意图    |
| `melee.guard_break`  | 破防 / 正蹬 | `RPG-Character@2Hand-Spear-Attack10` / `RPG-Character@2Hand-Spear-Attack-Kick-R1` | `2Hand-Spear` | default |         |
| `melee.dodge.back`   | 闪避·后    | `RPG-Character@2Hand-Spear-Dodge-Backward`                                        | `2Hand-Spear` | default |         |
| `melee.dodge.left`   | 闪避·左    | `RPG-Character@2Hand-Spear-Dodge-Left`                                            | `2Hand-Spear` | default |         |
| `melee.dodge.right`  | 闪避·右    | `RPG-Character@2Hand-Spear-Dodge-Right`                                           | `2Hand-Spear` | default |         |
| `melee.roll.forward` | 翻滚·前    | `RPG-Character@2Hand-Spear-Roll-Forward`                                          | `2Hand-Spear` | default |         |
| `melee.roll.back`    | 翻滚·后    | `RPG-Character@2Hand-Spear-Roll-Backward`                                         | `2Hand-Spear` | default |         |
| `melee.roll.left`    | 翻滚·左    | `RPG-Character@2Hand-Spear-Roll-Left`                                             | `2Hand-Spear` | default |         |
| `melee.roll.right`   | 翻滚·右    | `RPG-Character@2Hand-Spear-Roll-Right`                                            | `2Hand-Spear` | default |         |
| `melee.reckless`     | 舍身狂战    | `RPG-Character@2Hand-Spear-Attack7` + `Attack8` + `Attack1`                       | —             | default | 见 §1.4  |
| `melee.special`      | 大招      | `RPG-Character@2Hand-Spear-Attack3` + `RPG-Character@2Hand-Spear-Attack11`        | `2Hand-Spear` | art>=66 |         |

**策划备注：** 法杖与矛共用；若日后杖要独立 Cast 大招再开扩展，不另开近战负模板。

---

### 3.4 单手武装姿态表（长剑 · 锤斧 · 短刃 **共用**）

> **动画不按武艺拆。** 剑 / 锤 / 斧 / 短刃在包内走同一套 `ARMED` 逻辑；差别只在手上挂什么网格。  
> **要拆的是握法：** 下面 A = 单持一把，B = 双手各一把。  
> 武艺等级仍按 `Longsword` / `HammerAxe` 分别涨，但选招动画模板按当前握法读 A 或 B。

#### 3.4.A 单持（主手一件 1H；可另持盾）

| 槽位 ID                            | 槽位名     | 动画资产名                         | Animator 族            | 解锁      | AI 权重提示                      |
| -------------------------------- | ------- | ----------------------------- | --------------------- | ------- | ---------------------------- |
| `melee.attack_a`                 | 普通攻击 A  | `RPG-Character@Sword-Attack-R1` | `ARMED` | default | 剑锤斧同一套 |
| `melee.attack_b`                 | 普通攻击 B  | `RPG-Character@Sword-Attack-R3` | `ARMED` | default |  |
| `melee.block`                    | 格挡      | `→ SHARED_ARMED` Block（§1.5） | `Armed` | default | 无盾用 Armed Block；**有盾 → 盾 Block（§3.6）** |
| `melee.guard_break`              | 破防 / 正蹬 | `→ SHARED_ARMED_KICK`（§1.5） | `Armed` | default | 通用踢击，不必另填 |
| `melee.dodge.*` / `melee.roll.*` | 闪避 / 翻滚 | `→ SHARED_ARMED`（§1.5）        | `Armed` | default | 不必再填 |
| `melee.reckless`                 | 舍身狂战    | `COMBO: attack_a→attack_b`    | —       | default | 见 §1.4 |
| `melee.special`                  | 大招      | 连续闪避 + 普攻 A                  | `ARMED` | art>=66 | 单持专用大招 |

**策划备注：** 填 Armed / 1Hand-Sword 系即可代表锤斧；不必为 Mace 再抄一份。格挡与破防已共享，单持表只需关心 A/B/大招。

#### 3.4.B 双持（左右手各一件 1H）

| 槽位 ID                            | 槽位名     | 动画资产名                            | Animator 族          | 解锁      | AI 权重提示 |
| -------------------------------- | ------- | -------------------------------- | ------------------- | ------- | ------- |
| `melee.attack_a`                 | 普通攻击 A  | `RPG-Character@Armed-Attack-Dual3` | `Armed-Attack-Dual*` | default | 双持专用平 A |
| `melee.attack_b`                 | 普通攻击 B  | `RPG-Character@Armed-Attack-Dual1` | `Armed-Attack-Dual*` | default |  |
| `melee.block`                    | 格挡      | `→ SHARED_ARMED` Block Dual（§1.5） | `Armed` | default | **与单持同族 Armed 防御**；双持无盾 |
| `melee.guard_break`              | 破防 / 正蹬 | `→ SHARED_ARMED_KICK`（§1.5） | `Armed` | default | 与单持同一条通用踢 |
| `melee.dodge.*` / `melee.roll.*` | 闪避 / 翻滚 | `→ SHARED_ARMED`                 | `Armed` | default | 与单持相同共享表 |
| `melee.reckless`                 | 舍身狂战    | `COMBO: attack_a→attack_b`       | —       | default | 组合用双持 A/B |
| `melee.special`                  | 大招      | 连续普通攻击 A 三次                      | `ARMED` Dual | art>=66 | 双持专用大招 |

**策划备注：** 双持相对单持，**只需另填普攻 A/B 与大招**；格挡/破防/闪滚全部共享 Armed。XP 仍记在实际武艺（双剑→长剑，双锤→锤斧，混持规则 TBD）。

---

### 3.5 武术 · MartialArts

| 槽位 ID                            | 槽位名     | 动画资产名                                                                | Animator 族 | 解锁      | AI 权重提示 |
| -------------------------------- | ------- | -------------------------------------------------------------------- | ---------- | ------- | ------- |
| `melee.attack_a`                 | 普通攻击 A  | RPG-Character@Unarmed-Attack-R3/RPG-Character@Unarmed-Attack-R2      | `Unarmed`  | default |         |
| `melee.attack_b`                 | 普通攻击 B  | RPG-Character@Unarmed-Attack-L1/RPG-Character@Unarmed-Attack-Kick-R1 | `Unarmed`  | default |         |
| `melee.block`                    | 格挡      | RPG-Character@Unarmed-Block                                          | `Unarmed`  | default | 徒手架招    |
| `melee.guard_break`              | 破防 / 正蹬 | RPG-Character@Unarmed-Attack-Kick-R2                                 | `Unarmed`  | default | 踢击丰富    |
| `melee.dodge.*` / `melee.roll.*` | 闪避 / 翻滚 | `→ SHARED_UNARMED`（§1.5）                                             | `Unarmed`  | default | 不必再填    |
| `melee.reckless`                 | 舍身狂战    | `COMBO: attack_a→attack_b`                                           | —          | default | 自由人可高权重 |
| `melee.special`                  | 大招      | 跳起来飞踢RPG-Character@Unarmed-Air-Attack1                               | `Unarmed`  | art>=66 |         |

---

### 3.6 盾 · Shield（与单手**单持**主手同时装备）

主要填盾击与格挡。主手进攻仍读 **§3.4A**；闪避 = SHARED_ARMED；翻滚禁用。双持时一般不装备盾。

| 槽位 ID               | 槽位名     | 动画资产名                              | Animator 族     | 解锁            | AI 权重提示     |
| ------------------- | ------- | ---------------------------------- | -------------- | ------------- | ----------- |
| `melee.attack_a`    | 盾击 A    | RPG-Character@Armed-Shield-Attack2 | `Armed-Shield` | default       | 输出偏低        |
| `melee.attack_b`    | 盾击 B    | RPG-Character@Armed-Shield-Attack3 | `Armed-Shield` | default       | 或 `—`       |
| `melee.block`       | 格挡      | RPG-Character@Armed-Shield-Block   | `Armed-Shield` | default       | **持盾时全局优先** |
| `melee.guard_break` | 破防 / 正蹬 | 盾牌就没有破防招式了                         |                | `—` / default | 盾偏守         |
| `melee.dodge.*`     | 闪避      | `→ SHARED_ARMED`                   | —              | default       | 不必在盾表填      |
| `melee.roll.*`      | 翻滚      | `持盾禁用                              | —              | —             |             |
| `melee.reckless`    | 舍身狂战    | 盾牌没有                               | —              | default       | 持盾是否允许 TBD  |
| `melee.special`     | 大招      | 盾牌没有                               |                | art>=66 或 `—` |             |

---

## 4. 远程武艺填表

### 4.1 弓弩 · BowCrossbow

| 槽位 ID            | 槽位名 | 动画资产名·弓                                | 动画资产名·弩                                     | 解锁      | AI 权重提示          |
| ---------------- | --- | -------------------------------------- | ------------------------------------------- | ------- | ---------------- |
| `ranged.fire`    | 射击  | RPG-Character@2Hand-Bow-Attack5        | RPG-Character@2Hand-Crossbow-Attack2        | default | 主输出              |
| `ranged.reload`  | 装弹  |                                        | RPG-Character@2Hand-Crossbow-Reload         | default | 无 clip→`UI_ONLY` |
| `ranged.kick`    | 踢腿  | RPG-Character@2Hand-Bow-Attack-Kick-L1 | RPG-Character@2Hand-Crossbow-Attack-Kick-R1 | default | 贴身应急             |
| `ranged.dodge.*` | 闪避  | `→ SHARED_ARMED`                       | `→ SHARED_ARMED`                            | default | 不必再填             |

**策划备注：** `ranged.aim` 是否单列 — TBD

---

### 4.2 火药 · Firearm

| 槽位 ID            | 槽位名 | 动画资产名·步枪                                    | 动画资产名·手枪                       | 解锁      | AI 权重提示         |
| ---------------- | --- | ------------------------------------------- | ------------------------------ | ------- | --------------- |
| `ranged.fire`    | 射击  | RPG-Character@2Hand-Shooting-Aiming-Fire    | RPG-Character@Pistol-Attack-R3 | default | 高伤低频            |
| `ranged.reload`  | 装弹  | RPG-Character@2Hand-Shooting-Reload-Rifle   | RPG-Character@Pistol-Reload-L1 | default | 长窗口；无→`UI_ONLY` |
| `ranged.kick`    | 踢腿  | RPG-Character@2Hand-Shooting-Attack-Kick-L1 |                                | default |                 |
| `ranged.dodge.*` | 闪避  | `→ SHARED_ARMED`                            | `→ SHARED_ARMED`               | default | 不必再填            |

---

### 4.3 投掷 · Throwing

| 槽位 ID            | 槽位名   | 动画资产名                        | Animator 族             | 解锁      | AI 权重提示    |
| ---------------- | ----- | ---------------------------- | ---------------------- | ------- | ---------- |
| `ranged.fire`    | 投出    | RPG-Character@Item-Attack-L2 | `1Hand-Item` / `ARMED` | default | 消耗栏内战器     |
| `ranged.reload`  | 「再取物」 | `UI_ONLY` 或拾取动画              | —                      | default | 无弹药→捡物或切近战 |
| `ranged.kick`    | 踢腿    |                              | `Armed` / `Unarmed`    | default |            |
| `ranged.dodge.*` | 闪避    | `→ SHARED_ARMED`             | `Armed`                | default | 不必再填       |

**空闲近战：** 投光后是否临时套用武术/长剑模板 — TBD

---

## 5. 填写进度清单

| 武艺 / 共享                | 你还需要填什么               | 进度  |
| ---------------------- | --------------------- | --- |
| SHARED_ARMED / UNARMED | **已预填**，无需再抄          | ✅   |
| GreatSword             | 格挡（闪滚已填）；可微调          | ☐   |
| HeavyWeapon            | 格挡（闪滚已填）              | ☐   |
| Polearm（含法杖）           | 格挡（闪滚已填）；杖不另表         | ☐   |
| 单手·单持 §3.4A（剑锤斧共用）     | A/B、大招（Block/Kick 已共享） | ✅   |
| 单手·双持 §3.4B（同上共用）      | A/B、大招（Block/Kick 已共享） | ✅   |
| SHARED_ARMED Block/Kick      | Armed 格挡 + 通用踢          | ✅   |
| MartialArts            | 同上                    | ☐   |
| Shield                 | 盾击 + 盾 Block（配 §3.4A） | ☐   |
| BowCrossbow            | 射/装/踢                 | ☐   |
| Firearm                | 同上                    | ☐   |
| Throwing               | 投出等                   | ☐   |

---

## 6. 落地顺序

1. ✅ CSV：`CombatMoveSlots.csv` × 姿态（`OneHandSingle` / `OneHandDual` / `GreatSword2H` …）；查询 API：`CombatMoveSlotConfigData.TryResolve`。武艺只负责 XP。  
2. 双手三族格挡等空 `animAsset` 可继续在 CSV / 本文补。  
3. **下一步 · 播放层**：用现有 RPG 控制器参数（`Weapon`/`Side`/`Action` + Attack/Block/Dodge/Kick Trigger）把已解析槽位播出来——**不必重做 Animator**。  
4. 舍身：状态旗标 + COMBO，非单 Trigger。  
5. 伪回合 AI 只从本模板选型。  

---

## 7. 仍开放

- [ ] 舍身结束后的战斗惩罚具体内容与时长  
- [ ] 持盾时是否允许舍身  
- [ ] 远程是否需要 `ranged.aim`  
- [ ] 无大招资源的武艺是否永久 `—`  
- [ ] 双持混持（如剑+锤）XP 记入哪条武艺  
