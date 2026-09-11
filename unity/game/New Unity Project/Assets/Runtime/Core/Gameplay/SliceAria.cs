using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// 导演输入素材层：Aria GUI — Full Fantasy RPG UI Kit（Honeti）。
    /// 2026-09-10 Director Attestation：本机已下载素材包全部已授权（S5U_ASSET_ADMISSION §7/§8）。
    /// 单一职责：Resources/UI/AriaGUI 纹理的懒加载 + 静态缓存 + 缺失回退（返回 null，
    /// 调用方退回 SliceHudIcons/SliceSkin 程序化合成——回退路径永不失效）。
    /// 选材 import（不整包）：清单以 DeclaredKeys 为单一真相（测试锁定全部可加载且无多余文件）。
    /// 染色契约：Icons/* 为纯白/灰阶模板，绘制端用 GUI.color 染色；Buttons/Panels/Frames/Bars/Cursors
    /// 的原图带色（金/棕/绿/米白），直接绘制或按需染色。
    /// </summary>
    public static class SliceAria
    {
        public const string Root = "UI/AriaGUI/";

        static readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

        /// <summary>加载失败返回 null（不抛异常），调用方必须允许程序化回退。</summary>
        public static Texture2D Get(string relativePath)
        {
            if (_cache.TryGetValue(relativePath, out var t) && t != null)
                return t;
            t = Resources.Load<Texture2D>(Root + relativePath);
            _cache[relativePath] = t;
            return t;
        }

        // ================= 装备槽类型图标（512 源，绘制端缩放；白色模板由 GUI.color 染色） =================

        public static Texture2D EqWeapon { get { return Get("Icons/Sword"); } }
        public static Texture2D EqHelmet { get { return Get("Icons/Helmet"); } }
        public static Texture2D EqBody { get { return Get("Icons/ChestArmor"); } }
        public static Texture2D EqGloves { get { return Get("Icons/Gloves"); } }
        public static Texture2D EqBoots { get { return Get("Icons/Boots"); } }
        public static Texture2D EqBelt { get { return Get("Icons/Belt"); } }

        // ================= 生命/法力球（金环覆层 + 球底 + 内芯） =================

        /// <summary>生命/法力球金环覆层（FrameRoundGold；叠加在程序化球体外缘之上）。</summary>
        public static Texture2D OrbRing { get { return Get("Frames/FrameRoundGold"); } }

        /// <summary>球底渐变圆（BubbleBg；暗底衬备选，本批仅登记不强制使用）。</summary>
        public static Texture2D OrbBg { get { return Get("Bars/BubbleBg"); } }
        public static Texture2D OrbFillTex { get { return Get("Bars/BubbleFill"); } }

        /// <summary>球体外框（BubbleFrame；带金边的完整圆框，可替代金环覆层）。</summary>
        public static Texture2D OrbFrame { get { return Get("Bars/BubbleFrame"); } }

        /// <summary>球体把手/刻度（BubbleHandle；细长条，用于可拖动球体或刻度标记）。</summary>
        public static Texture2D OrbHandle { get { return Get("Bars/BubbleHandle"); } }

        // ================= 面板 / 框架（九宫格底衬与描边） =================

        /// <summary>主面板金框（FrameGold；九宫格，86²）。</summary>
        public static Texture2D PanelFrame { get { return Get("Panels/FrameGold"); } }

        /// <summary>面板投影衬底（SimplePanelShadow；九宫格，制造浮起层次）。</summary>
        public static Texture2D PanelShadow { get { return Get("Panels/SimplePanelShadow"); } }

        /// <summary>水平分隔饰线（Separator；金色，标题/行分隔）。</summary>
        public static Texture2D Separator { get { return Get("Panels/Separator"); } }

        /// <summary>竖向分隔饰线（SeparatorV；列分隔或侧栏边线）。</summary>
        public static Texture2D SeparatorV { get { return Get("Panels/SeparatorV"); } }

        /// <summary>简约九宫框（FrameSimple；素色描边，任意尺寸缩放的背包/列表框）。</summary>
        public static Texture2D FrameSimple { get { return Get("Frames/FrameSimple"); } }

        /// <summary>华丽金框（FrameCaroGold；带纹饰，用于弹窗/角色面板外框）。</summary>
        public static Texture2D FrameCaroGold { get { return Get("Frames/FrameCaroGold"); } }

        /// <summary>素面九宫面板底（SimplePanel；纯色可染色，做背包/列表衬底）。</summary>
        public static Texture2D PanelSimple { get { return Get("Panels/SimplePanel"); } }

        /// <summary>雕纹九宫面板底（PanelCaro；带角饰的衬底）。</summary>
        public static Texture2D PanelCaro { get { return Get("Panels/PanelCaro"); } }

        /// <summary>内阴影覆层（InnerShadow；叠加在面板内缘，压出凹陷纵深）。</summary>
        public static Texture2D PanelInset { get { return Get("Panels/InnerShadow"); } }

        /// <summary>渐变描边（GradientStroke；九宫，用于次级卡片高光边）。</summary>
        public static Texture2D PanelGradientStroke { get { return Get("Panels/GradientStroke"); } }

        /// <summary>渐变描边变体（GradientStroke02；更素的次级描边）。</summary>
        public static Texture2D PanelGradientStroke02 { get { return Get("Panels/GradientStroke02"); } }

        /// <summary>圆形/方形徽章（Badge；稀有度/等级角标）。</summary>
        public static Texture2D Badge { get { return Get("Panels/Badge"); } }

        /// <summary>徽章变体（Badge02；第二个角标样式）。</summary>
        public static Texture2D Badge02 { get { return Get("Panels/Badge02"); } }

        /// <summary>宽标题横幅（SectionBig；380×64，面板/清单分区标题）。</summary>
        public static Texture2D SectionBig { get { return Get("Panels/SectionBig"); } }

        /// <summary>窄标题横幅（SectionSmall；200×64，子分区标题）。</summary>
        public static Texture2D SectionSmall { get { return Get("Panels/SectionSmall"); } }

        // ================= 按钮状态（正常/悬停/按下/禁用；原图带色） =================

        public static Texture2D ButtonBrown { get { return Get("Buttons/ButtonBrown"); } }
        public static Texture2D ButtonBrownHover { get { return Get("Buttons/ButtonBrownHover"); } }
        public static Texture2D ButtonBrownDown { get { return Get("Buttons/ButtonBrownDown"); } }
        public static Texture2D ButtonInactive { get { return Get("Buttons/ButtonInactive"); } }

        /// <summary>绿色按钮·正常（确认/正向操作）。</summary>
        public static Texture2D ButtonGreen { get { return Get("Buttons/ButtonGreen"); } }

        /// <summary>绿色按钮·悬停。</summary>
        public static Texture2D ButtonGreenHover { get { return Get("Buttons/ButtonGreenHover"); } }

        /// <summary>绿色按钮·按下。</summary>
        public static Texture2D ButtonGreenDown { get { return Get("Buttons/ButtonGreenDown"); } }

        /// <summary>红色按钮·正常（危险/取消操作）。</summary>
        public static Texture2D ButtonRed { get { return Get("Buttons/ButtonRed"); } }

        /// <summary>红色按钮·悬停。</summary>
        public static Texture2D ButtonRedHover { get { return Get("Buttons/ButtonRedHover"); } }

        /// <summary>红色按钮·按下。</summary>
        public static Texture2D ButtonRedDown { get { return Get("Buttons/ButtonRedDown"); } }

        /// <summary>中性九宫按钮底（ButtonRegular；米白可染色，通用次级按钮）。</summary>
        public static Texture2D ButtonNeutral { get { return Get("Buttons/ButtonRegular"); } }

        /// <summary>方形选中框（FrameSelection；格子/图标选中态描边）。</summary>
        public static Texture2D SelectionFrame { get { return Get("Buttons/FrameSelection"); } }

        /// <summary>圆形选中框（FrameSelectionCircle；技能/圆形槽位选中态）。</summary>
        public static Texture2D SelectionFrameRound { get { return Get("Buttons/FrameSelectionCircle"); } }

        /// <summary>方形悬停辉光（SelectionGlow；叠加在选中框外部）。</summary>
        public static Texture2D SelectionGlow { get { return Get("Buttons/SelectionGlow"); } }

        /// <summary>圆形悬停辉光（SelectionGlowCircle；圆形槽位外辉光）。</summary>
        public static Texture2D SelectionGlowRound { get { return Get("Buttons/SelectionGlowCircle"); } }

        // ================= 条 / 滚动 / 滑杆 =================

        /// <summary>水平滚动槽底（ScrollBarBgH）。</summary>
        public static Texture2D ScrollBarBgH { get { return Get("Bars/ScrollBarBgH"); } }

        /// <summary>垂直滚动槽底（ScrollBarBgV）。</summary>
        public static Texture2D ScrollBarBgV { get { return Get("Bars/ScrollBarBgV"); } }

        /// <summary>水平滚动把手（ScrollHandleH）。</summary>
        public static Texture2D ScrollHandleH { get { return Get("Bars/ScrollHandleH"); } }

        /// <summary>垂直滚动把手（ScrollHandleV）。</summary>
        public static Texture2D ScrollHandleV { get { return Get("Bars/ScrollHandleV"); } }

        /// <summary>滑杆槽底（SliderBg；设置/音量等数值滑杆）。</summary>
        public static Texture2D SliderBg { get { return Get("Bars/SliderBg"); } }

        /// <summary>滑杆把手（SliderHandle）。</summary>
        public static Texture2D SliderHandle { get { return Get("Bars/SliderHandle"); } }

        // ================= 光标（原图带金/绿描边） =================

        /// <summary>默认光标。</summary>
        public static Texture2D CursorDefault { get { return Get("Cursors/CursorDefault"); } }

        /// <summary>可点击/指向光标。</summary>
        public static Texture2D CursorPointer { get { return Get("Cursors/CursorPointer"); } }

        /// <summary>帮助光标。</summary>
        public static Texture2D CursorHelp { get { return Get("Cursors/CursorHelp"); } }

        /// <summary>禁止光标。</summary>
        public static Texture2D CursorNotAllowed { get { return Get("Cursors/CursorNotAllowed"); } }

        /// <summary>文本输入光标。</summary>
        public static Texture2D CursorText { get { return Get("Cursors/CursorText"); } }

        /// <summary>等待光标。</summary>
        public static Texture2D CursorWait { get { return Get("Cursors/CursorWait"); } }

        // ================= 物品图标 · 武器（白色模板，绘制端染色） =================

        /// <summary>单手斧（Ax）。</summary>
        public static Texture2D IconAxe { get { return Get("Icons/Ax"); } }

        /// <summary>双刃斧（Ax02）。</summary>
        public static Texture2D IconAxe2 { get { return Get("Icons/Ax02"); } }

        /// <summary>匕首（Knife）。</summary>
        public static Texture2D IconKnife { get { return Get("Icons/Knife"); } }

        /// <summary>成对匕首（Knives）。</summary>
        public static Texture2D IconKnives { get { return Get("Icons/Knives"); } }

        /// <summary>战锤（Hammer）。</summary>
        public static Texture2D IconHammer { get { return Get("Icons/Hammer"); } }

        /// <summary>法杖（Wand）。</summary>
        public static Texture2D IconWand { get { return Get("Icons/Wand"); } }

        /// <summary>成对法杖（Wands）。</summary>
        public static Texture2D IconWands { get { return Get("Icons/Wands"); } }

        /// <summary>箭袋（Quiver；弹药类）。</summary>
        public static Texture2D IconQuiver { get { return Get("Icons/Quiver"); } }

        /// <summary>弹弓（Slingshot；远程武器）。</summary>
        public static Texture2D IconSling { get { return Get("Icons/Slingshot"); } }

        /// <summary>圆盾（Shield）。</summary>
        public static Texture2D IconShield { get { return Get("Icons/Shield"); } }

        /// <summary>盾牌变体（Shield02）。</summary>
        public static Texture2D IconShield2 { get { return Get("Icons/Shield02"); } }

        /// <summary>弓（Bow）。</summary>
        public static Texture2D IconBow { get { return Get("Icons/Bow"); } }

        /// <summary>交叉双剑（Swords；战斗/职业图标）。</summary>
        public static Texture2D IconSwords { get { return Get("Icons/Swords"); } }

        // ================= 物品图标 · 护甲与饰品 =================

        /// <summary>护肩（ShoulderArmor）。</summary>
        public static Texture2D IconPauldrons { get { return Get("Icons/ShoulderArmor"); } }

        /// <summary>胫甲（ShinArmor）。</summary>
        public static Texture2D IconGreaves { get { return Get("Icons/ShinArmor"); } }

        /// <summary>护膝（KneeArmor）。</summary>
        public static Texture2D IconKneeArmor { get { return Get("Icons/KneeArmor"); } }

        /// <summary>裤子（Pants；腿部护甲）。</summary>
        public static Texture2D IconPants { get { return Get("Icons/Pants"); } }

        /// <summary>面罩（Balaclava；头部护甲）。</summary>
        public static Texture2D IconBalaclava { get { return Get("Icons/Balaclava"); } }

        /// <summary>背包（Backpack；容器/负重）。</summary>
        public static Texture2D IconBackpack { get { return Get("Icons/Backpack"); } }

        /// <summary>护身符（Amulet；颈部饰品）。</summary>
        public static Texture2D IconAmulet { get { return Get("Icons/Amulet"); } }

        /// <summary>戒指（Ring）。</summary>
        public static Texture2D IconRing { get { return Get("Icons/Ring"); } }

        /// <summary>王冠（Crown；任务/稀有奖励）。</summary>
        public static Texture2D IconCrown { get { return Get("Icons/Crown"); } }

        /// <summary>勋章（Medal；成就）。</summary>
        public static Texture2D IconMedal { get { return Get("Icons/Medal"); } }

        /// <summary>身份牌（DogTag；任务道具）。</summary>
        public static Texture2D IconDogTag { get { return Get("Icons/DogTag"); } }

        // ================= 物品图标 · 资源 / 货币 / 容器 =================

        /// <summary>宝石（Gem；镶嵌/货币）。</summary>
        public static Texture2D IconGem { get { return Get("Icons/Gem"); } }

        /// <summary>金币（Coins；货币）。</summary>
        public static Texture2D IconCoins { get { return Get("Icons/Coins"); } }

        /// <summary>卷轴（Scroll；配方/法卷）。</summary>
        public static Texture2D IconScroll { get { return Get("Icons/Scroll"); } }

        /// <summary>钥匙（Key；开门/上锁容器）。</summary>
        public static Texture2D IconKey { get { return Get("Icons/Key"); } }

        /// <summary>包裹（Package；快递/奖励箱）。</summary>
        public static Texture2D IconPackage { get { return Get("Icons/Package"); } }

        /// <summary>宝箱（Chest；战利品容器）。</summary>
        public static Texture2D IconChest { get { return Get("Icons/Chest"); } }

        /// <summary>上锁（Lock；未解锁槽位）。</summary>
        public static Texture2D IconLock { get { return Get("Icons/Lock"); } }

        /// <summary>开锁（LockOpen；已解锁槽位）。</summary>
        public static Texture2D IconLockOpen { get { return Get("Icons/LockOpen"); } }

        // ================= 物品图标 · 消耗品（药水） =================

        /// <summary>药水瓶 1（Potion01；通用消耗品）。</summary>
        public static Texture2D IconPotion1 { get { return Get("Icons/Potion01"); } }

        /// <summary>药水瓶 2（Potion02）。</summary>
        public static Texture2D IconPotion2 { get { return Get("Icons/Potion02"); } }

        /// <summary>药水瓶 3（Potion03）。</summary>
        public static Texture2D IconPotion3 { get { return Get("Icons/Potion03"); } }

        /// <summary>药水瓶 4（Potion04）。</summary>
        public static Texture2D IconPotion4 { get { return Get("Icons/Potion04"); } }

        /// <summary>药水瓶 5（Potion05）。</summary>
        public static Texture2D IconPotion5 { get { return Get("Icons/Potion05"); } }

        /// <summary>药水瓶 6（Potion06）。</summary>
        public static Texture2D IconPotion6 { get { return Get("Icons/Potion06"); } }

        /// <summary>火焰药水（PotionFire）。</summary>
        public static Texture2D IconPotionFire { get { return Get("Icons/PotionFire"); } }

        /// <summary>寒霜药水（PotionCold）。</summary>
        public static Texture2D IconPotionCold { get { return Get("Icons/PotionCold"); } }

        /// <summary>雷电药水（PotionBolt）。</summary>
        public static Texture2D IconPotionBolt { get { return Get("Icons/PotionBolt"); } }

        /// <summary>流水药水（PotionWater）。</summary>
        public static Texture2D IconPotionWater { get { return Get("Icons/PotionWater"); } }

        /// <summary>力量药水（PotionStrength）。</summary>
        public static Texture2D IconPotionStrength { get { return Get("Icons/PotionStrength"); } }

        /// <summary>护盾药水（PotionShield）。</summary>
        public static Texture2D IconPotionShield { get { return Get("Icons/PotionShield"); } }

        /// <summary>生命药水（PotionHeart）。</summary>
        public static Texture2D IconPotionHeart { get { return Get("Icons/PotionHeart"); } }

        // ================= 技能 / 元素图标 =================

        /// <summary>火焰（Fire；火系技能/伤害类型）。</summary>
        public static Texture2D IconFire { get { return Get("Icons/Fire"); } }

        /// <summary>寒霜（Cold；冰系技能/伤害类型）。</summary>
        public static Texture2D IconCold { get { return Get("Icons/Cold"); } }

        /// <summary>流水（Water；水系技能/伤害类型）。</summary>
        public static Texture2D IconWater { get { return Get("Icons/Water"); } }

        /// <summary>闪电（Bolt；雷系技能/伤害类型）。</summary>
        public static Texture2D IconBolt { get { return Get("Icons/Bolt"); } }

        /// <summary>电击（Electricity；雷电系变体）。</summary>
        public static Texture2D IconLightning { get { return Get("Icons/Electricity"); } }

        /// <summary>剧毒（Poison；毒系技能/伤害类型）。</summary>
        public static Texture2D IconPoison { get { return Get("Icons/Poison"); } }

        /// <summary>风暴（Storm；范围天气系技能）。</summary>
        public static Texture2D IconStorm { get { return Get("Icons/Storm"); } }

        /// <summary>自然（Leaf；德鲁伊/自然系技能）。</summary>
        public static Texture2D IconLeaf { get { return Get("Icons/Leaf"); } }

        /// <summary>太阳（Sun；神圣/光明系技能）。</summary>
        public static Texture2D IconSun { get { return Get("Icons/Sun"); } }

        /// <summary>月亮（Moon；暗影/月神系技能）。</summary>
        public static Texture2D IconMoon { get { return Get("Icons/Moon"); } }

        // ================= 图标 · 战斗 / 状态 / 界面 =================

        /// <summary>炸弹（Bomb；投掷物）。</summary>
        public static Texture2D IconBomb { get { return Get("Icons/Bomb"); } }

        /// <summary>炸药（Dynamite；爆破技能）。</summary>
        public static Texture2D IconDynamite { get { return Get("Icons/Dynamite"); } }

        /// <summary>星标（Star；收藏/稀有度）。</summary>
        public static Texture2D IconStar { get { return Get("Icons/Star"); } }

        /// <summary>无限（Infinity；无尽模式/持续效果）。</summary>
        public static Texture2D IconInfinity { get { return Get("Icons/Infinity"); } }

        /// <summary>心形（Heart；生命/好感）。</summary>
        public static Texture2D IconHeart { get { return Get("Icons/Heart"); } }

        /// <summary>医疗包（HealthKit；治疗/补给）。</summary>
        public static Texture2D IconHealthKit { get { return Get("Icons/HealthKit"); } }

        /// <summary>力量（Strength；属性/增益）。</summary>
        public static Texture2D IconStrength { get { return Get("Icons/Strength"); } }

        /// <summary>准星（Crosshair；瞄准/精准属性）。</summary>
        public static Texture2D IconCrosshair { get { return Get("Icons/Crosshair"); } }

        /// <summary>骷髅（Skull02；死亡/亡灵）。</summary>
        public static Texture2D IconSkull { get { return Get("Icons/Skull02"); } }

        /// <summary>§8 台账选材单一真相（相对 Root；测试锁定全部可加载且无多余文件）。</summary>
        public static readonly string[] DeclaredKeys =
        {
            // Icons — 装备槽（12）
            "Icons/Sword", "Icons/Helmet", "Icons/ChestArmor", "Icons/Gloves", "Icons/Boots", "Icons/Belt",
            "Icons/Swords", "Icons/Bow", "Icons/Shield02", "Icons/PotionHeart", "Icons/Ring", "Icons/Skull02",
            // Icons — 武器 / 护甲 / 饰品（20）
            "Icons/Ax", "Icons/Ax02", "Icons/Knife", "Icons/Knives", "Icons/Hammer", "Icons/Wand", "Icons/Wands",
            "Icons/Quiver", "Icons/Slingshot", "Icons/Shield",
            "Icons/ShoulderArmor", "Icons/ShinArmor", "Icons/KneeArmor", "Icons/Pants", "Icons/Balaclava",
            "Icons/Backpack", "Icons/Amulet", "Icons/Crown", "Icons/Medal", "Icons/DogTag",
            // Icons — 资源 / 容器（8）
            "Icons/Gem", "Icons/Coins", "Icons/Scroll", "Icons/Key", "Icons/Package", "Icons/Chest",
            "Icons/Lock", "Icons/LockOpen",
            // Icons — 消耗品（12）
            "Icons/Potion01", "Icons/Potion02", "Icons/Potion03", "Icons/Potion04", "Icons/Potion05", "Icons/Potion06",
            "Icons/PotionFire", "Icons/PotionCold", "Icons/PotionBolt", "Icons/PotionWater", "Icons/PotionStrength",
            "Icons/PotionShield",
            // Icons — 技能 / 元素（10）
            "Icons/Fire", "Icons/Cold", "Icons/Water", "Icons/Bolt", "Icons/Electricity", "Icons/Poison",
            "Icons/Storm", "Icons/Leaf", "Icons/Sun", "Icons/Moon",
            // Icons — 战斗 / 状态（8）
            "Icons/Bomb", "Icons/Dynamite", "Icons/Star", "Icons/Infinity", "Icons/Heart", "Icons/HealthKit",
            "Icons/Strength", "Icons/Crosshair",
            // Frames（3）
            "Frames/FrameRoundGold", "Frames/FrameCaroGold", "Frames/FrameSimple",
            // Panels（13）
            "Panels/FrameGold", "Panels/SimplePanelShadow", "Panels/Separator", "Panels/SimplePanel", "Panels/PanelCaro",
            "Panels/InnerShadow", "Panels/GradientStroke", "Panels/GradientStroke02", "Panels/SeparatorV",
            "Panels/Badge", "Panels/Badge02", "Panels/SectionBig", "Panels/SectionSmall",
            // Buttons（15）
            "Buttons/ButtonBrown", "Buttons/ButtonBrownHover", "Buttons/ButtonBrownDown", "Buttons/ButtonInactive",
            "Buttons/ButtonGreen", "Buttons/ButtonGreenHover", "Buttons/ButtonGreenDown",
            "Buttons/ButtonRed", "Buttons/ButtonRedHover", "Buttons/ButtonRedDown",
            "Buttons/ButtonRegular", "Buttons/FrameSelection", "Buttons/FrameSelectionCircle",
            "Buttons/SelectionGlow", "Buttons/SelectionGlowCircle",
            // Bars（10）
            "Bars/BubbleBg", "Bars/BubbleFill", "Bars/BubbleFrame", "Bars/BubbleHandle",
            "Bars/ScrollBarBgH", "Bars/ScrollBarBgV", "Bars/ScrollHandleH", "Bars/ScrollHandleV",
            "Bars/SliderBg", "Bars/SliderHandle",
            // Cursors（6）
            "Cursors/CursorDefault", "Cursors/CursorPointer", "Cursors/CursorHelp", "Cursors/CursorNotAllowed",
            "Cursors/CursorText", "Cursors/CursorWait"
        };
    }
}
