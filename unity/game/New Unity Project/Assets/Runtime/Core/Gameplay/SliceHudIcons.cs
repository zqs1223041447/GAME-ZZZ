using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S5U-WO-02（Dark ARPG Skin Foundation）图标/饰件合成层。
    /// 与 SliceSkin 同一模式：运行期一次性确定性程序合成 → 静态缓存 → 后续帧零 GC；
    /// 零外部资源、零 Resources 契约变更、无每帧纹理生成。
    /// 视觉语言：暗铁/黑石底 + 哑金描边 + 米白符文（与 SliceSkin 金线体系一致）。
    /// 离线溯源渲染（同一算法）= docs/reviews/S5U/asset-source/*.png（Editor/S5UAssetForge）；
    /// 准入台账=docs/reviews/S5U/S5U_ASSET_ADMISSION.md §6（Source=GAME-ZZZ ORIGINAL）。
    /// 注意：Support pip 全部使用同一中性金属贴图（恒等映射），颜色不承载任何 SupportId 语义
    /// （≠Socket Color 机制；测试锁定）。
    /// </summary>
    public static class SliceHudIcons
    {
        static Texture2D _glyphMelee, _glyphProjectile, _glyphArea;
        static Texture2D _pipOn, _pipOff, _pipClosed;
        static Texture2D _globeFrame;
        static Texture2D _globeSheen;
        static Texture2D _slotFrame;
        static Texture2D _separator;
        static Texture2D _eqWeapon, _eqHelmet, _eqBody, _eqGloves, _eqBoots, _eqBelt;
        static bool _built;

        public static void Ensure()
        {
            if (_built && _glyphMelee != null && _glyphProjectile != null && _glyphArea != null &&
                _pipOn != null && _pipOff != null && _pipClosed != null &&
                _globeFrame != null && _globeSheen != null && _slotFrame != null && _separator != null &&
                _eqWeapon != null && _eqHelmet != null && _eqBody != null &&
                _eqGloves != null && _eqBoots != null && _eqBelt != null)
                return;
            _glyphMelee = BuildGlyphMelee();
            _glyphProjectile = BuildGlyphProjectile();
            _glyphArea = BuildGlyphArea();
            _pipOn = BuildPip(true, false);
            _pipOff = BuildPip(false, false);
            _pipClosed = BuildPip(false, true);
            _globeFrame = BuildGlobeFrame();
            _globeSheen = BuildGlobeSheen();
            _slotFrame = BuildSlotFrame();
            _separator = BuildSeparator();
            _eqWeapon = BuildEqWeapon();
            _eqHelmet = BuildEqHelmet();
            _eqBody = BuildEqBody();
            _eqGloves = BuildEqGloves();
            _eqBoots = BuildEqBoots();
            _eqBelt = BuildEqBelt();
            _built = true;
        }

        public static Texture2D GlyphMelee { get { Ensure(); return _glyphMelee; } }
        public static Texture2D GlyphProjectile { get { Ensure(); return _glyphProjectile; } }
        public static Texture2D GlyphArea { get { Ensure(); return _glyphArea; } }
        public static Texture2D PipOn { get { Ensure(); return _pipOn; } }
        public static Texture2D PipOff { get { Ensure(); return _pipOff; } }
        public static Texture2D PipClosed { get { Ensure(); return _pipClosed; } }
        public static Texture2D GlobeFrame { get { Ensure(); return _globeFrame; } }
        /// <summary>S5U-F1 V-09：球内实体感覆层（内圈暗缘+左上高光弧；256²，透明底）。</summary>
        public static Texture2D GlobeSheen { get { Ensure(); return _globeSheen; } }
        public static Texture2D SlotFrame { get { Ensure(); return _slotFrame; } }
        public static Texture2D Separator { get { Ensure(); return _separator; } }
        public static Texture2D EqWeapon { get { Ensure(); return SliceAria.EqWeapon != null ? SliceAria.EqWeapon : _eqWeapon; } } // Aria 优先，程序化回退
        public static Texture2D EqHelmet { get { Ensure(); return SliceAria.EqHelmet != null ? SliceAria.EqHelmet : _eqHelmet; } }
        public static Texture2D EqBody { get { Ensure(); return SliceAria.EqBody != null ? SliceAria.EqBody : _eqBody; } }
        public static Texture2D EqGloves { get { Ensure(); return SliceAria.EqGloves != null ? SliceAria.EqGloves : _eqGloves; } }
        public static Texture2D EqBoots { get { Ensure(); return SliceAria.EqBoots != null ? SliceAria.EqBoots : _eqBoots; } }
        public static Texture2D EqBelt { get { Ensure(); return SliceAria.EqBelt != null ? SliceAria.EqBelt : _eqBelt; } }

        /// <summary>S5U-WO-03：装备槽类型符文（generic 槽位图形，不伪装具体物品外观；恒等映射）。</summary>
        public static Texture2D EquipGlyph(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Helmet: return EqHelmet;
                case EquipSlot.Body: return EqBody;
                case EquipSlot.Gloves: return EqGloves;
                case EquipSlot.Boots: return EqBoots;
                case EquipSlot.Belt: return EqBelt;
                default: return EqWeapon;
            }
        }

        /// <summary>Support pip 恒等映射（全部 SupportId 同一贴图：中性金属，颜色零语义）。</summary>
        public static Texture2D PipFor(SupportId id, bool filled, bool closed)
        {
            return closed ? PipClosed : (filled ? PipOn : PipOff);
        }

        public static Texture2D GlyphFor(SkillId skill)
        {
            if (skill == SkillId.Melee)
                return GlyphMelee;
            if (skill == SkillId.Projectile)
                return GlyphProjectile;
            if (skill == SkillId.Area)
                return GlyphArea;
            return GlyphMelee;
        }

        // ================= S6P-WO-05：poedb 真实美术优先于程序化图形 =================
        // 优先级 poedb（PoE 原始美术）→ Aria（通用图形）→ 程序化合成。任一层缺失都不影响可用性。

        /// <summary>技能槽图标：优先 poedb 技能宝石图，缺失回退程序化符文。</summary>
        public static Texture2D SkillGlyph(SkillId skill)
        {
            Texture2D art = SlicePoeArt.SkillArt(skill);
            return art != null ? art : GlyphFor(skill);
        }

        /// <summary>技能槽是否使用真实美术（绘制端据此决定是否染色——宝石图自带配色）。</summary>
        public static bool SkillGlyphIsArt(SkillId skill)
        {
            return SlicePoeArt.SkillArt(skill) != null;
        }

        /// <summary>物品图标：优先 poedb 物品美术（按槽位 + 稀有度），缺失回退 Aria/程序化槽位符号。</summary>
        public static Texture2D ItemIcon(EquipSlot slot, Rarity rarity)
        {
            Texture2D art = SlicePoeArt.ItemArt(slot, rarity);
            return art != null ? art : EquipGlyph(slot);
        }

        /// <summary>辅助宝石图标：优先 poedb 辅助宝石图，缺失回退程序化宝石底。</summary>
        public static Texture2D SupportGem(SupportId id)
        {
            return SlicePoeArt.SupportArt(id);
        }

        // ---------------- 共用色板（与 SliceSkin 对齐） ----------------

        static readonly Color32 Ink = new Color32(0x14, 0x11, 0x0C, 0xFF);     // 暗描边
        static readonly Color32 Cream = new Color32(0xE8, 0xDF, 0xC8, 0xFF);    // 米白符文
        static readonly Color32 Gold = new Color32(0xF2, 0xC7, 0x39, 0xFF);     // Rare 金
        static readonly Color32 GoldMid = new Color32(0xB9, 0x9A, 0x2E, 0xFF);  // 中金
        static readonly Color32 GoldDim = new Color32(0x8A, 0x6D, 0x1F, 0xFF);  // 暗金
        static readonly Color32 Iron = new Color32(0x26, 0x24, 0x22, 0xFF);     // 暗铁
        static readonly Color32 IronHi = new Color32(0x3C, 0x39, 0x35, 0xFF);   // 铁亮部
        static readonly Color32 IronLo = new Color32(0x14, 0x13, 0x12, 0xFF);   // 铁暗部
        static readonly Color32 SocketDim = new Color32(0x4E, 0x4A, 0x44, 0xFF);

        // ---------------- 技能符文（64²，透明底） ----------------

        static Texture2D BuildGlyphMelee()
        {
            var px = Blank(64);
            // 巨剑：斜 45° 剑身 + 横护手 + 握柄 + 圆铆
            Stroke(px, 64, 64, 46, 18, 22, 42, 6.5f, Ink);
            Stroke(px, 64, 64, 47, 17, 21, 43, 4f, Cream);
            BladeEdge(px, 44, 14, 18, 40);
            Stroke(px, 64, 64, 14, 30, 30, 46, 5f, Ink);   // 护手（垂直于剑身）
            Stroke(px, 64, 64, 14, 30, 30, 46, 2.5f, GoldMid);
            Stroke(px, 64, 64, 19, 45, 13, 51, 4.5f, Ink); // 握柄
            Stroke(px, 64, 64, 19, 45, 13, 51, 2f, Cream);
            Dot(px, 11, 53, 3.4f, Ink);
            Dot(px, 11, 53, 1.8f, Gold);
            return Make(px, 64);
        }

        static void BladeEdge(Color32[] px, int x0, int y0, int x1, int y1)
        {
            // 剑刃中线高光（细白线，叠于剑身）
            Stroke(px, 64, 64, x0, y0, x1, y1, 1.2f, new Color32(0xFF, 0xF6, 0xE0, 0xFF));
        }

        static Texture2D BuildGlyphProjectile()
        {
            var px = Blank(64);
            // 矢：箭杆 + 箭头（三角） + 尾羽（两撇）
            Stroke(px, 64, 64, 18, 46, 40, 24, 5f, Ink);
            Stroke(px, 64, 64, 19, 45, 40, 24, 2.5f, Cream);
            FillTri(px, 38, 26, 52, 12, 46, 30, Ink);
            FillTri(px, 40, 26, 50, 16, 45, 28, Gold);
            Stroke(px, 64, 64, 12, 38, 18, 44, 3.5f, Ink);
            Stroke(px, 64, 64, 16, 34, 22, 40, 3.5f, Ink);
            Stroke(px, 64, 64, 12, 38, 18, 44, 1.5f, Cream);
            Stroke(px, 64, 64, 16, 34, 22, 40, 1.5f, Cream);
            return Make(px, 64);
        }

        static Texture2D BuildGlyphArea()
        {
            var px = Blank(64);
            // 新星环：主环 + 八向刺 + 中心核
            Ring(px, 32, 32, 15f, 4.5f, Ink);
            Ring(px, 32, 32, 15f, 2.2f, Cream);
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI / 4f;
                float c = Mathf.Cos(a), s = Mathf.Sin(a);
                Stroke(px, 64, 64, 32f + c * 19f, 32f + s * 19f, 32f + c * 27f, 32f + s * 27f, 4f, Ink);
                Stroke(px, 64, 64, 32f + c * 20f, 32f + s * 20f, 32f + c * 26f, 32f + s * 26f, 1.8f, Cream);
            }
            Dot(px, 32, 32, 4.5f, Ink);
            Dot(px, 32, 32, 2.6f, Gold);
            return Make(px, 64);
        }

        // ---------------- Support pip（20²；中性金属恒等映射） ----------------

        // ---------------- 装备槽类型符文（64²，透明底；S5U-WO-03） ----------------

        static Texture2D BuildEqWeapon()
        {
            var px = Blank(64);
            // 立式巨剑（垂直剑身+剑尖+横护手+握柄+金铆）——与技能斜剑区分
            Stroke(px, 64, 64, 32, 12, 32, 44, 7f, Ink);
            Stroke(px, 64, 64, 32, 13, 32, 43, 4f, Cream);
            Stroke(px, 64, 64, 32, 10, 32, 14, 5f, Ink);
            FillTri(px, 28, 14, 32, 6, 36, 14, Ink);
            FillTri(px, 30, 13, 32, 8, 34, 13, Cream);
            Stroke(px, 64, 64, 20, 44, 44, 44, 6f, Ink);
            Stroke(px, 64, 64, 20, 44, 44, 44, 2.6f, GoldMid);
            Stroke(px, 64, 64, 32, 47, 32, 55, 5f, Ink);
            Stroke(px, 64, 64, 32, 47, 32, 55, 2f, Cream);
            Dot(px, 32, 58, 3.6f, Ink);
            Dot(px, 32, 58, 1.9f, Gold);
            return Make(px, 64);
        }

        static Texture2D BuildEqHelmet()
        {
            var px = Blank(64);
            // 头盔：圆顶+帽檐+眼缝+护鼻
            Stroke(px, 64, 64, 14, 38, 22, 18, 5.5f, Ink);
            Stroke(px, 64, 64, 22, 18, 42, 18, 5.5f, Ink);
            Stroke(px, 64, 64, 42, 18, 50, 38, 5.5f, Ink);
            Stroke(px, 64, 64, 15, 38, 23, 20, 2.4f, Cream);
            Stroke(px, 64, 64, 23, 20, 41, 20, 2.4f, Cream);
            Stroke(px, 64, 64, 41, 20, 49, 38, 2.4f, Cream);
            Stroke(px, 64, 64, 12, 40, 52, 40, 5f, Ink);
            Stroke(px, 64, 64, 13, 40, 51, 40, 2f, GoldMid);
            Stroke(px, 64, 64, 32, 40, 32, 54, 4f, Ink);
            Stroke(px, 64, 64, 32, 41, 32, 53, 1.6f, Cream);
            Stroke(px, 64, 64, 20, 44, 28, 44, 3.5f, IronLo);
            Stroke(px, 64, 64, 36, 44, 44, 44, 3.5f, IronLo);
            return Make(px, 64);
        }

        static Texture2D BuildEqBody()
        {
            var px = Blank(64);
            // 胸甲：肩线+梯形甲身+中脊金线
            Stroke(px, 64, 64, 14, 14, 24, 10, 5f, Ink);
            Stroke(px, 64, 64, 50, 14, 40, 10, 5f, Ink);
            Stroke(px, 64, 64, 14, 14, 18, 34, 5f, Ink);
            Stroke(px, 64, 64, 50, 14, 46, 34, 5f, Ink);
            Stroke(px, 64, 64, 18, 34, 22, 52, 5f, Ink);
            Stroke(px, 64, 64, 46, 34, 42, 52, 5f, Ink);
            Stroke(px, 64, 64, 22, 52, 42, 52, 5f, Ink);
            Stroke(px, 64, 64, 15, 15, 23, 11, 2.2f, Cream);
            Stroke(px, 64, 64, 49, 15, 41, 11, 2.2f, Cream);
            Stroke(px, 64, 64, 16, 17, 20, 34, 2.2f, Cream);
            Stroke(px, 64, 64, 48, 17, 44, 34, 2.2f, Cream);
            Stroke(px, 64, 64, 20, 35, 24, 51, 2.2f, Cream);
            Stroke(px, 64, 64, 44, 35, 40, 51, 2.2f, Cream);
            Stroke(px, 64, 64, 24, 52, 40, 52, 2.2f, Cream);
            Stroke(px, 64, 64, 32, 12, 32, 52, 2.6f, GoldMid);
            return Make(px, 64);
        }

        static Texture2D BuildEqGloves()
        {
            var px = Blank(64);
            // 手套：掌+四指并拢+拇指外张+护腕
            Stroke(px, 64, 64, 20, 26, 44, 26, 5f, Ink);
            Stroke(px, 64, 64, 20, 26, 20, 46, 5f, Ink);
            Stroke(px, 64, 64, 44, 26, 44, 34, 5f, Ink);
            Stroke(px, 64, 64, 44, 34, 52, 40, 5f, Ink);
            Stroke(px, 64, 64, 52, 40, 48, 46, 5f, Ink);
            Stroke(px, 64, 64, 48, 46, 20, 46, 5f, Ink);
            Stroke(px, 64, 64, 22, 27, 22, 45, 2.2f, Cream);
            Stroke(px, 64, 64, 22, 28, 43, 28, 2.2f, Cream);
            Stroke(px, 64, 64, 43, 28, 43, 33, 2.2f, Cream);
            Stroke(px, 64, 64, 18, 48, 46, 48, 6f, Ink);
            Stroke(px, 64, 64, 19, 48, 45, 48, 2.4f, GoldMid);
            return Make(px, 64);
        }

        static Texture2D BuildEqBoots()
        {
            var px = Blank(64);
            // 靴：靴筒+脚尖+厚底（双靴剪影：主靴+后靴暗影）
            Stroke(px, 64, 64, 16, 10, 38, 10, 5.5f, Ink);
            Stroke(px, 64, 64, 16, 10, 16, 38, 5.5f, Ink);
            Stroke(px, 64, 64, 16, 38, 30, 38, 5.5f, Ink);
            Stroke(px, 64, 64, 30, 38, 30, 48, 5.5f, Ink);
            Stroke(px, 64, 64, 30, 48, 46, 48, 5.5f, Ink);
            Stroke(px, 64, 64, 46, 48, 46, 42, 5.5f, Ink);
            Stroke(px, 64, 64, 46, 42, 38, 38, 5.5f, Ink);
            Stroke(px, 64, 64, 38, 38, 38, 10, 5.5f, Ink);
            Stroke(px, 64, 64, 17, 11, 17, 37, 2.2f, Cream);
            Stroke(px, 64, 64, 17, 37, 29, 37, 2.2f, Cream);
            Stroke(px, 64, 64, 29, 37, 29, 47, 2.2f, Cream);
            Stroke(px, 64, 64, 29, 47, 45, 47, 2.2f, Cream);
            Stroke(px, 64, 64, 37, 12, 37, 34, 3f, IronHi);
            Stroke(px, 64, 64, 12, 52, 50, 52, 6f, Ink);
            Stroke(px, 64, 64, 13, 52, 49, 52, 2.2f, GoldDim);
            return Make(px, 64);
        }

        static Texture2D BuildEqBelt()
        {
            var px = Blank(64);
            // 腰带：横带+方扣（金框+针）+带尾
            Stroke(px, 64, 64, 8, 30, 56, 30, 6f, Ink);
            Stroke(px, 64, 64, 8, 38, 56, 38, 6f, Ink);
            FillTri(px, 8, 27, 8, 41, 2, 34, IronLo);
            Stroke(px, 64, 64, 9, 30, 55, 30, 2.4f, Cream);
            Stroke(px, 64, 64, 9, 38, 55, 38, 2.4f, Cream);
            Stroke(px, 64, 64, 26, 22, 40, 22, 5f, Ink);
            Stroke(px, 64, 64, 40, 22, 40, 46, 5f, Ink);
            Stroke(px, 64, 64, 40, 46, 26, 46, 5f, Ink);
            Stroke(px, 64, 64, 26, 46, 26, 22, 5f, Ink);
            Stroke(px, 64, 64, 26, 22, 40, 22, 2.2f, Gold);
            Stroke(px, 64, 64, 40, 22, 40, 46, 2.2f, Gold);
            Stroke(px, 64, 64, 40, 46, 26, 46, 2.2f, Gold);
            Stroke(px, 64, 64, 26, 46, 26, 22, 2.2f, Gold);
            Stroke(px, 64, 64, 33, 26, 33, 42, 3f, Ink);
            Stroke(px, 64, 64, 33, 27, 33, 41, 1.6f, Cream);
            return Make(px, 64);
        }

        static Texture2D BuildPip(bool filled, bool closed)
        {
            const int s = 20;
            var px = Blank(s);
            if (closed)
            {
                Dot(px, 10, 10, 4.5f, SocketDim);
                return Make(px, s);
            }
            // 中性暗金属窝座（所有 SupportId 共用同一外观）
            Dot(px, 10, 10, 8f, IronLo);
            Dot(px, 10, 10, 7f, Iron);
            Dot(px, 9, 9, 5.5f, IronHi);
            if (filled)
            {
                Dot(px, 10, 10, 4.2f, Ink);
                Dot(px, 10, 10, 3f, GoldMid);
                Dot(px, 9, 9, 1.6f, Gold);
            }
            else
            {
                Dot(px, 10, 10, 3.2f, IronLo);
            }
            return Make(px, s);
        }

        /// <summary>S5U-F1 V-09：球内覆层 256²（fill 半径 0.40 映射）——外缘 5px 内阴影 + 左上高光弧（resource vessel 实体感）。</summary>
        static Texture2D BuildGlobeSheen()
        {
            const int s = 256;
            var px = new Color32[s * s];
            float c = (s - 1) * 0.5f;
            float rFill = s * 0.40f;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float band = rFill - r;
                    if (band >= 0f && band < 5f)
                    {
                        // 内圈暗缘（fill 外缘内阴影）
                        px[y * s + x] = new Color32(6, 5, 4, (byte)(255f * 0.62f * (1f - band / 5f)));
                        continue;
                    }
                    if (r < rFill * 0.93f && r > rFill * 0.42f && dx < -rFill * 0.12f && dy < -rFill * 0.12f)
                    {
                        // 左上高光弧（玻璃反射）
                        float arc = 1f - Mathf.Abs(r - rFill * 0.70f) / (rFill * 0.26f);
                        if (arc > 0f)
                            px[y * s + x] = new Color32(255, 246, 224, (byte)(255f * 0.22f * arc));
                    }
                }
            }
            return Make(px, s);
        }

        // ---------------- 球框（256²，环形装饰升级版） ----------------

        static Texture2D BuildGlobeFrame()
        {
            const int s = 256;
            var px = new Color32[s * s];
            float c = (s - 1) * 0.5f;
            float rOut = s * 0.485f;
            float rIn = s * 0.385f;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color32 col = new Color32(0, 0, 0, 0);
                    if (r <= rOut)
                    {
                        float a = 1f;
                        if (r > rOut - 1.5f)
                            a = (rOut - r) / 1.5f;
                        if (r < rIn - 1.5f)
                            a = Mathf.Max(0f, (r - (rIn - 1.5f)) / 1.5f);
                        if (a > 0f)
                        {
                            if (r >= rIn + 2f && r <= rOut - 3f)
                            {
                                // 暗铁环带 + 斜向磨痕
                                float n = 0.78f + 0.44f * VNoise(x / 48f, y / 48f, 71, 6);
                                float hi = Mathf.Max(0f, 1f - Mathf.Abs(dy + dx * 0.55f) / 40f) * 0.35f;
                                byte rr = (byte)Mathf.Clamp((Iron.r + hi * 90f) * n, 0f, 255f);
                                byte gg = (byte)Mathf.Clamp((Iron.g + hi * 82f) * n, 0f, 255f);
                                byte bb = (byte)Mathf.Clamp((Iron.b + hi * 70f) * n, 0f, 255f);
                                col = new Color32(rr, gg, bb, 0xFF);
                            }
                            else if (r > rOut - 3f)
                            {
                                col = r > rOut - 1.6f ? Ink : GoldDim; // 外缘金环
                            }
                            else if (r >= rIn && r < rIn + 2f)
                            {
                                col = GoldMid; // 内缘金描
                            }
                            else
                            {
                                col = Ink; // 内唇暗线
                            }
                            col.a = (byte)(255f * Mathf.Clamp01(a));
                        }
                    }
                    px[y * s + x] = col;
                }
            }
            // 八向金钉（45° 步进，环带中线）
            for (int k = 0; k < 8; k++)
            {
                float ang = k * Mathf.PI / 4f + Mathf.PI / 8f;
                float cx = c + Mathf.Cos(ang) * (rIn + rOut) * 0.5f;
                float cy = c + Mathf.Sin(ang) * (rIn + rOut) * 0.5f;
                for (int dy = -3; dy <= 3; dy++)
                {
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        int x = Mathf.RoundToInt(cx) + dx;
                        int y = Mathf.RoundToInt(cy) + dy;
                        if (x < 0 || y < 0 || x >= s || y >= s)
                            continue;
                        int d2 = dx * dx + dy * dy;
                        if (d2 <= 3)
                            px[y * s + x] = Gold;
                        else if (d2 <= 9)
                            px[y * s + x] = GoldDim;
                    }
                }
            }
            return Make(px, s);
        }

        // ---------------- 技能槽框（128² 九宫格；显示 96px） ----------------

        static Texture2D BuildSlotFrame()
        {
            const int s = 128;
            var px = new Color32[s * s];
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float n = 0.82f + 0.34f * VNoise(x / 34f, y / 34f, 83, 5);
                    px[y * s + x] = new Color32(
                        (byte)(Iron.r * n), (byte)(Iron.g * n), (byte)(Iron.b * n), 0xFF);
                }
            }
            Edge(px, s, 0, Ink);
            EdgeTopLeft(px, s, 1, IronHi);
            EdgeBottomRight(px, s, 1, IronLo);
            RectLine(px, s, 4, GoldDim);
            RectLine(px, s, 5, GoldMid);
            // 内槽区压暗（图标衬底）
            for (int y = 12; y < s - 12; y++)
            {
                for (int x = 12; x < s - 12; x++)
                {
                    var c0 = px[y * s + x];
                    px[y * s + x] = new Color32((byte)(c0.r * 0.55f), (byte)(c0.g * 0.55f), (byte)(c0.b * 0.55f), 0xFF);
                }
            }
            return Make(px, s);
        }

        // ---------------- 分隔饰线（256×6） ----------------

        static Texture2D BuildSeparator()
        {
            var px = Blank(256, 6);
            for (int x = 0; x < 256; x++)
            {
                px[2 * 256 + x] = GoldDim;
                px[3 * 256 + x] = GoldDim;
            }
            for (int dy = 0; dy < 6; dy++)
            {
                for (int dx = -5; dx <= 5; dx++)
                {
                    int x = 128 + dx;
                    if (x < 0 || x >= 256)
                        continue;
                    if (Mathf.Abs(dx) + Mathf.Abs(dy - 2.5f) <= 4.2f)
                        px[dy * 256 + x] = Gold;
                }
            }
            return Make(px, 256, 6);
        }

        // ---------------- 光栅基元（与 SliceSkin 噪声一致） ----------------

        static Color32[] Blank(int s)
        {
            return Blank(s, s);
        }

        static Color32[] Blank(int w, int h)
        {
            return new Color32[w * h];
        }

        static float VNoise(float u, float v, int seed, int period)
        {
            float x = u * period;
            float y = v * period;
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float fx = x - x0;
            float fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float a = Lattice(x0, y0, seed, period);
            float b = Lattice(x0 + 1, y0, seed, period);
            float c = Lattice(x0, y0 + 1, seed, period);
            float d = Lattice(x0 + 1, y0 + 1, seed, period);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        static float Lattice(int x, int y, int seed, int period)
        {
            int px = ((x % period) + period) % period;
            int py = ((y % period) + period) % period;
            unchecked
            {
                int h = px * 374761393 + py * 668265263 + seed * 1274126177;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
            }
        }

        /// <summary>抗锯齿粗描边（暗底线→细亮线两段调用）。</summary>
        static void Stroke(Color32[] px, int w, int h, float x0, float y0, float x1, float y1, float width, Color32 c)
        {
            float len = Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            if (len < 0.01f)
                return;
            float dx = (x1 - x0) / len;
            float dy = (y1 - y0) / len;
            float half = width * 0.5f;
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(x0, x1) - half - 1));
            int maxX = Mathf.Min(w - 1, Mathf.CeilToInt(Mathf.Max(x0, x1) + half + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(y0, y1) - half - 1));
            int maxY = Mathf.Min(h - 1, Mathf.CeilToInt(Mathf.Max(y0, y1) + half + 1));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float vx = x - x0;
                    float vy = y - y0;
                    float t = vx * dx + vy * dy;
                    t = Mathf.Clamp(t, 0f, len);
                    float cxp = x0 + dx * t;
                    float cyp = y0 + dy * t;
                    float d = Mathf.Sqrt((x - cxp) * (x - cxp) + (y - cyp) * (y - cyp));
                    float a = half + 0.5f - d;
                    if (a <= 0f)
                        continue;
                    Blend(ref px[y * w + x], c, Mathf.Clamp01(a));
                }
            }
        }

        static void Dot(Color32[] px, float cx, float cy, float r, Color32 c)
        {
            int size = (int)Mathf.Sqrt(px.Length);
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(cx + r + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(cy + r + 1));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float a = r + 0.5f - d;
                    if (a <= 0f)
                        continue;
                    Blend(ref px[y * size + x], c, Mathf.Clamp01(a));
                }
            }
        }

        static void Ring(Color32[] px, float cx, float cy, float r, float width, Color32 c)
        {
            int size = (int)Mathf.Sqrt(px.Length);
            int lo = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(cx, cy) - r - width - 2));
            int hi = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(cx, cy) + r + width + 2));
            for (int y = lo; y <= hi; y++)
            {
                for (int x = lo; x <= hi; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float a = 1f - Mathf.Abs(d - r) / (width * 0.5f + 0.5f);
                    if (a <= 0f)
                        continue;
                    Blend(ref px[y * size + x], c, Mathf.Clamp01(a));
                }
            }
        }

        static void FillTri(Color32[] px, float ax, float ay, float bx, float by, float cx, float cy, Color32 c)
        {
            int size = (int)Mathf.Sqrt(px.Length);
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ax, Mathf.Min(bx, cx)) - 1));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(ax, Mathf.Max(bx, cx)) + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ay, Mathf.Min(by, cy)) - 1));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(ay, Mathf.Max(by, cy)) + 1));
            float d = (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
            if (Mathf.Abs(d) < 0.001f)
                return;
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w1 = ((bx - ax) * (y - ay) - (by - ay) * (x - ax)) / d;
                    float w2 = ((cx - bx) * (y - by) - (cy - by) * (x - bx)) / d;
                    float w3 = 1f - w1 - w2;
                    if (w1 < -0.02f || w2 < -0.02f || w3 < -0.02f)
                        continue;
                    Blend(ref px[y * size + x], c, 1f);
                }
            }
        }

        static void Blend(ref Color32 dst, Color32 src, float a)
        {
            if (a >= 1f && src.a == 255)
            {
                dst = src;
                return;
            }
            float sa = src.a / 255f * a;
            float da = dst.a / 255f;
            float oa = sa + da * (1f - sa);
            if (oa <= 0f)
                return;
            byte r = (byte)Mathf.Clamp((src.r * sa + dst.r * da * (1f - sa)) / oa, 0f, 255f);
            byte g = (byte)Mathf.Clamp((src.g * sa + dst.g * da * (1f - sa)) / oa, 0f, 255f);
            byte b = (byte)Mathf.Clamp((src.b * sa + dst.b * da * (1f - sa)) / oa, 0f, 255f);
            dst = new Color32(r, g, b, (byte)(oa * 255f));
        }

        static void Edge(Color32[] px, int size, int inset, Color32 c)
        {
            for (int i = inset; i < size - inset; i++)
            {
                px[inset * size + i] = c;
                px[(size - 1 - inset) * size + i] = c;
                px[i * size + inset] = c;
                px[i * size + (size - 1 - inset)] = c;
            }
        }

        static void EdgeTopLeft(Color32[] px, int size, int inset, Color32 c)
        {
            for (int i = inset; i < size - inset; i++)
            {
                px[inset * size + i] = c;
                px[i * size + inset] = c;
            }
        }

        static void EdgeBottomRight(Color32[] px, int size, int inset, Color32 c)
        {
            for (int i = inset; i < size - inset; i++)
            {
                px[(size - 1 - inset) * size + i] = c;
                px[i * size + (size - 1 - inset)] = c;
            }
        }

        static void RectLine(Color32[] px, int size, int inset, Color32 c)
        {
            Edge(px, size, inset, c);
        }

        static Texture2D Make(Color32[] px, int size)
        {
            return Make(px, size, size);
        }

        static Texture2D Make(Color32[] px, int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            t.SetPixels32(px);
            t.Apply();
            return t;
        }
    }
}
