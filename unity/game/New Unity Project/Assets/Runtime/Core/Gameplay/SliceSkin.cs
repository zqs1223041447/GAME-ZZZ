using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// Phase3 正式 UI 换皮层（UI_PROPOSAL_POE_D3 第 1 轮：底栏+双球；导演本轮目标=贴图+点击效）。
    /// 全部贴图运行期程序合成：确定性（整数哈希值噪声，固定种子）、零外部资源、零 Resources 契约变更；
    /// 合成一次静态缓存，后续帧零 GC。色彩基准=方案页：底 #17140F / Rare 金 #F2C739 /
    /// 描边暗金 #B99A2E·#8A6D1F / 正文米白 #E8DFC8；SlicePalette 既有数据色（Life/Mana/Rare 等）不动。
    /// </summary>
    public static class SliceSkin
    {
        public static readonly Color Base = Hex(0x17140F);
        public static readonly Color Gold = Hex(0xF2C739);
        public static readonly Color GoldMid = Hex(0xB99A2E);
        public static readonly Color GoldDim = Hex(0x8A6D1F);
        public static readonly Color BevelLight = Hex(0x453C2C);
        public static readonly Color BevelDark = Hex(0x060504);
        public static readonly Color TextCream = Hex(0xE8DFC8);

        /// <summary>面板九宫格边框宽（贴图同值；GUIStyle.border 必须一致）。</summary>
        public const int PanelBorder = 18;
        /// <summary>槽位九宫格边框宽。</summary>
        public const int SlotBorder = 8;

        static Texture2D _panel;
        static Texture2D _slot;
        static Texture2D _slotHover;
        static Texture2D _slotPress;
        static Texture2D _slotSelected;
        static Texture2D _slotSubtle;
        static Texture2D _slotSubtleHover;
        static Texture2D _orbRing;
        static Texture2D _orbFill;
        static bool _built;

        public static void Ensure()
        {
            // Unity null 语义=被销毁的贴图视为未建：交互式编辑器无域重载时自愈重建（Gate 批处理新域不受影响）
            if (_built && _panel != null && _slot != null && _slotHover != null && _slotPress != null &&
                _slotSelected != null && _slotSubtle != null && _slotSubtleHover != null &&
                _orbRing != null && _orbFill != null)
                return;
            Texture2D stone = BuildStone();
            _panel = BuildPanel(stone);
            _slot = BuildSlot(stone, false, false, false);
            _slotHover = BuildSlot(stone, true, false, false);
            _slotPress = BuildSlot(stone, false, true, false);
            _slotSelected = BuildSlot(stone, false, false, true);
            _slotSubtle = BuildSubtle(stone, false);
            _slotSubtleHover = BuildSubtle(stone, true);
            _orbRing = BuildOrbRing();
            _orbFill = BuildOrbFill();
            _built = true;
        }

        public static Texture2D Panel { get { Ensure(); return _panel; } }
        public static Texture2D SlotNormal { get { Ensure(); return _slot; } }
        public static Texture2D SlotHover { get { Ensure(); return _slotHover; } }
        public static Texture2D SlotPress { get { Ensure(); return _slotPress; } }
        public static Texture2D SlotSelected { get { Ensure(); return _slotSelected; } }
        /// <summary>S5U-F1 V-02：Cell/Slot 层级=暗铁无金框（金只做强调；网格/托盘/次要槽用此层）。</summary>
        public static Texture2D SlotSubtle { get { Ensure(); return _slotSubtle; } }
        public static Texture2D SlotSubtleHover { get { Ensure(); return _slotSubtleHover; } }
        public static Texture2D OrbRing { get { Ensure(); return _orbRing; } }
        public static Texture2D OrbFill { get { Ensure(); return _orbFill; } }

        static Color Hex(int rgb)
        {
            return new Color32((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF), 0xFF);
        }

        /// <summary>整数哈希格点值噪声（周期 wrap 保证可平铺；[0,1]）。</summary>
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

        /// <summary>128² 可平铺暗石底：#17140F 基底 + 三倍频凿纹 + 暗坑 + 亮斑。</summary>
        static Texture2D BuildStone()
        {
            const int s = 128;
            var px = new Color32[s * s];
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float u = x / (float)s;
                    float v = y / (float)s;
                    float n = VNoise(u, v, 11, 4) * 0.55f + VNoise(u, v, 23, 8) * 0.30f + VNoise(u, v, 37, 16) * 0.15f;
                    float k = 0.82f + 0.36f * n;
                    float pit = VNoise(u, v, 51, 32);
                    if (pit < 0.085f)
                        k *= 0.72f;
                    float fleck = VNoise(u, v, 67, 24);
                    if (fleck > 0.955f)
                        k *= 1.22f;
                    Color32 b = Base;
                    px[y * s + x] = new Color32(
                        (byte)Mathf.Clamp(b.r * k, 0f, 255f),
                        (byte)Mathf.Clamp(b.g * k, 0f, 255f),
                        (byte)Mathf.Clamp(b.b * k, 0f, 255f), 0xFF);
                }
            }
            return MakeTex(s, px, wrap: true);
        }

        /// <summary>256² 面板九宫格：石底 + 外缘凿边 + 金线内描（边框区内图案沿边均匀=拉伸安全）。</summary>
        static Texture2D BuildPanel(Texture2D stone)
        {
            const int size = 256;
            var px = new Color32[size * size];
            SampleStoneTiled(stone, px, size, 0.92f);
            // 外缘 1px 全暗 + 第 2px 上左亮/下右暗（凿边）
            Edge(px, size, 0, BevelDark);
            EdgeTopLeft(px, size, 1, BevelLight);
            EdgeBottomRight(px, size, 1, new Color32(0x0C, 0x0A, 0x08, 0xFF));
            // 金线内描：inset 5 起 1.5px（以 2px 画，外圈中金、内圈暗金），四角点亮 Rare 金
            int t0 = 5;
            RectLine(px, size, t0, GoldMid);
            RectLine(px, size, t0 + 1, GoldDim);
            for (int i = 0; i < 3; i++)
            {
                CornerDot(px, size, t0 + i, Gold);
            }
            return MakeTex(size, px);
        }

        /// <summary>S5U-F1 V-02：暗铁 Subtle 层（无金线；hover=微亮铁棱）。金只保留给 Outer/Selected/强调。</summary>
        static Texture2D BuildSubtle(Texture2D stone, bool hover)
        {
            const int size = 96;
            var px = new Color32[size * size];
            float mul = hover ? 0.78f : 0.52f;
            SampleStoneTiled(stone, px, size, mul);
            Edge(px, size, 0, hover ? Hex(0x453E32) : Hex(0x231F19));
            EdgeTopLeft(px, size, 1, hover ? Hex(0x554C3E) : Hex(0x332D25));
            EdgeBottomRight(px, size, 1, Hex(0x050403));
            return MakeTex(size, px);
        }

        /// <summary>96² 槽位九宫格（hover=金线点亮 / press=压暗+反向凿边 / selected=金线+内晕圈）。</summary>
        static Texture2D BuildSlot(Texture2D stone, bool hover, bool press, bool selected)
        {
            const int size = 96;
            var px = new Color32[size * size];
            float mul = press ? 0.62f : (hover ? 1.10f : (selected ? 1.06f : 0.80f));
            SampleStoneTiled(stone, px, size, mul);
            if (press)
            {
                Edge(px, size, 0, BevelDark);
                EdgeTopLeft(px, size, 1, BevelDark);
                EdgeBottomRight(px, size, 1, new Color32(0x2A, 0x25, 0x1C, 0xFF)); // 反向=按下
            }
            else
            {
                Edge(px, size, 0, BevelDark);
                EdgeTopLeft(px, size, 1, BevelLight);
                EdgeBottomRight(px, size, 1, new Color32(0x0C, 0x0A, 0x08, 0xFF));
            }
            Color32 trim = press ? GoldDim : (hover ? Gold : (selected ? Gold : GoldMid));
            int t0 = 3;
            RectLine(px, size, t0, trim);
            if (selected)
                RectLine(px, size, t0 + 2, new Color32(0xF2, 0xC7, 0x39, 0x50));
            return MakeTex(size, px);
        }

        /// <summary>192² 球框（环形，外内透明）：暗石环带+金环描边+四向金钉；抗锯齿 alpha 渐变。</summary>
        static Texture2D BuildOrbRing()
        {
            const int s = 192;
            var px = new Color32[s * s];
            float c = (s - 1) * 0.5f;
            float rOut = s * 0.48f;
            float rIn = s * 0.40f;
            float rTrimOut = rIn + 1.5f;
            float rTrimIn = rIn - 1.0f;
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
                            if (r >= rTrimIn && r <= rTrimOut)
                                col = GoldMid;
                            else
                            {
                                float n = 0.8f + 0.4f * Lattice(x, y, 91, 24);
                                col = new Color32(
                                    (byte)(0x24 * n), (byte)(0x1F * n), (byte)(0x17 * n), 0xFF);
                            }
                            col.a = (byte)(255f * Mathf.Clamp01(a));
                        }
                    }
                    px[y * s + x] = col;
                }
            }
            // 四向金钉（N/E/S/W，r=环带中线）
            for (int k = 0; k < 4; k++)
            {
                float ang = k * Mathf.PI / 2f;
                float cx = c + Mathf.Cos(ang) * (rIn + rOut) * 0.5f;
                float cy = c + Mathf.Sin(ang) * (rIn + rOut) * 0.5f;
                for (int y = -2; y <= 2; y++)
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        int px0 = Mathf.RoundToInt(cx) + x;
                        int py0 = Mathf.RoundToInt(cy) + y;
                        if (px0 < 0 || py0 < 0 || px0 >= s || py0 >= s)
                            continue;
                        if (x * x + y * y <= 4)
                            px[py0 * s + px0] = Gold;
                    }
                }
            }
            return MakeTex(s, px);
        }

        /// <summary>192² 球液（灰阶圆，运行期以 GUI.color 染红/蓝）：上亮下暗+边缘收暗，圆形 AA。</summary>
        static Texture2D BuildOrbFill()
        {
            const int s = 192;
            var px = new Color32[s * s];
            float c = (s - 1) * 0.5f;
            float r = s * 0.40f;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    Color32 col = new Color32(0, 0, 0, 0);
                    if (dist <= r)
                    {
                        float a = dist > r - 1.5f ? (r - dist) / 1.5f : 1f;
                        float v = Mathf.Lerp(0.72f, 0.38f, y / (float)(s - 1));
                        float edge = 1f - 0.30f * Mathf.Pow(dist / r, 2.2f);
                        byte g = (byte)(255f * Mathf.Clamp01(v * edge));
                        col = new Color32(g, g, g, (byte)(255f * Mathf.Clamp01(a)));
                    }
                    px[y * s + x] = col;
                }
            }
            return MakeTex(s, px);
        }

        static void SampleStoneTiled(Texture2D stone, Color32[] dst, int size, float mul)
        {
            Color32[] src = stone.GetPixels32();
            int ss = stone.width;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color32 b = src[(y % ss) * ss + (x % ss)];
                    dst[y * size + x] = new Color32(
                        (byte)(b.r * mul), (byte)(b.g * mul), (byte)(b.b * mul), 0xFF);
                }
            }
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

        /// <summary>inset 处画一圈 1px 矩形线（九宫格边框区内，图案沿边均匀）。</summary>
        static void RectLine(Color32[] px, int size, int inset, Color32 c)
        {
            Edge(px, size, inset, c);
        }

        static void CornerDot(Color32[] px, int size, int inset, Color32 c)
        {
            px[inset * size + inset] = c;
            px[inset * size + (size - 1 - inset)] = c;
            px[(size - 1 - inset) * size + inset] = c;
            px[(size - 1 - inset) * size + (size - 1 - inset)] = c;
        }

        static Texture2D MakeTex(int size, Color32[] px, bool wrap = false)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = wrap ? TextureWrapMode.Repeat : TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            t.SetPixels32(px);
            t.Apply();
            return t;
        }
    }
}
