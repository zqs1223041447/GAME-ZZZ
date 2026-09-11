using UnityEngine;

namespace Game.Runtime.Core
{
    public sealed class SliceHud
    {
        Texture2D _white;
        GUIStyle _title;
        GUIStyle _body;
        GUIStyle _small;
        GUIStyle _center;
        GUIStyle _tiny;
        GUIStyle _caption;
        GUIStyle _clip;
        GUIStyle _panelBg;
        GUIStyle _slotN;
        GUIStyle _slotH;
        GUIStyle _slotP;
        GUIStyle _slotSel;
        GUIStyle _slotSubtleN;
        GUIStyle _slotSubtleH;
        GUIStyle _pageTitle;
        GUIStyle _navIdle;
        GUIStyle _orbText;
        Rect _flashRect;
        float _flashUntil;
        /// <summary>全局等比缩放（设计空间=1920x1080 基准，方案页布局）；设计坐标=屏幕坐标/_scale。</summary>
        float _scale = 1f;
        bool _styles;

        Vector2 _invScroll;
        SliceTooltipModel.Card _tipCard;
        int _tipPri = -1;
        bool _tipSet;
        /// <summary>每帧至多一张正式 Tooltip 的确定性优先级（工作令 二十二）：面板 &gt; 抽屉 &gt; 底栏 &gt; 顶栏/导航；同优先级先到先得（稳定）。</summary>
        public const int TipPriTopNav = 0, TipPriBottomBar = 1, TipPriDrawer = 2, TipPriPanel = 3;
        /// <summary>编辑器视觉验证辅助：DebugHover=true 时 GUI 指针钉在 DebugHoverPoint（设计空间），正常输入零影响。</summary>
        public static bool DebugHover;
        public static Vector2 DebugHoverPoint;

        /// <summary>编辑器验证用单语句入口（unity-cli eval）：钉指针到设计空间点。</summary>
        public static bool DebugHoverAt(Vector2 designPoint)
        {
            DebugHover = true;
            DebugHoverPoint = designPoint;
            return true;
        }

        public static bool DebugHoverOff()
        {
            DebugHover = false;
            return false;
        }

        /// <summary>编辑器视觉验证辅助（与 DebugHover 同一约定）：钉住天赋树的缩放与居中节点。</summary>
        public static bool DebugTreeView;
        public static float DebugTreeZoom = 1f;
        public static int DebugTreeFocus = -1;

        /// <summary>编辑器验证用单语句入口（unity-cli eval）：钉住天赋树视图。</summary>
        public static bool DebugTreeAt(float zoom, int focusNode)
        {
            DebugTreeView = true;
            DebugTreeZoom = zoom;
            DebugTreeFocus = focusNode;
            return true;
        }

        public static bool DebugTreeOff()
        {
            DebugTreeView = false;
            DebugTreeFocus = -1;
            return false;
        }

        SupportId _picked;
        SupportId _drag;
        bool _dragging;
        Vector2 _dragStart;
        Rect _topBar;
        Rect _nav;
        Rect _skillHud;
        Rect _tray;
        Rect _panel;
        Rect _drawer;
        Rect _craftResult;

        // ---- 天赋树视图状态（平移/缩放；DrawPoeTree 在 BeginGroup 内绘制，故指针需换算到组内局部坐标） ----
        bool _treeInit;
        Vector2 _treePan;
        float _treeZoom = 1f;
        /// <summary>天赋树默认缩放（可读优先；整树概览由玩家滚轮缩小）。</summary>
        public const float DefaultTreeZoom = 0.45f;
        bool _treeDrag;
        Vector2 _treeDragLast;
        Vector2 _treeOrigin;
        string _treeToast;
        bool _treeToastOk;
        Vector2 _treeToastAt;
        float _treeToastUntil;

        /// <summary>天赋树组内的局部指针（GUI 组会平移绘制，但 Event.mousePosition 仍是设计空间坐标）。</summary>
        Vector2 TreePointer
        {
            get
            {
                Vector2 p = Pointer;
                return new Vector2(p.x - _treeOrigin.x, p.y - _treeOrigin.y);
            }
        }

        public bool ShouldBlockWorld(SliceSession s)
        {
            bool panelOpen = s != null && s.Panel != SlicePanel.None;
            return BlocksWorldInput(panelOpen, _dragging, _topBar, _nav, _skillHud, _tray, _drawer,
                new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y) / _scale);
        }

        /// <summary>纯函数：给定设计空间区域与 GUI 点，判定是否吞掉世界点击（R2 抽屉纳入；供几何测试）。</summary>
        public static bool BlocksWorldInput(bool panelOpen, bool dragging, Rect topBar, Rect nav, Rect skillHud,
            Rect tray, Rect drawer, Vector2 guiPoint)
        {
            if (panelOpen || dragging)
                return true;
            return topBar.Contains(guiPoint) || nav.Contains(guiPoint) || skillHud.Contains(guiPoint) ||
                   tray.Contains(guiPoint) || drawer.Contains(guiPoint);
        }

        /// <summary>全局等比缩放因子（1920×1080 设计基准；供缩放几何测试与 Draw 共用）。</summary>
        public static float DesignScale(float screenW, float screenH)
        {
            return Mathf.Clamp(Mathf.Min(screenW / 1920f, screenH / 1080f), 0.4f, 1.4f);
        }

        public void Draw(ArenaDirector director)
        {
            ArenaSim sim = director.Sim;
            SliceSession s = sim.Session;
            if (s == null)
                return;

            // 全局等比缩放：布局按 1920x1080 设计空间书写（方案页基准），任意窗口按比例缩放
            _scale = DesignScale(Screen.width, Screen.height);
            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(_scale, _scale), Vector2.zero);
            try
            {
                EnsureStyles();
                _tipSet = false;
                _tipPri = -1;

                // S5U-F1 V-03：阅读页面打开时压暗世界（cached 白纹理+暗色，零每帧纹理分配；不全遮死、不改 gameplay）
                if (s != null && s.Panel != SlicePanel.None)
                    Fill(new Rect(0f, 0f, Dw(), Dh()), new Color(0.02f, 0.02f, 0.015f, 0.52f));

                DrawTop(s);
                DrawNav(s, sim);
                DrawSkillHud(s);
                if (s.BagOpen)
                {
                    DrawDrawer(s);
                }
                else
                {
                    // 面板关闭：不占位、不吞世界点击（底栏已按 BagOpen=false 重新居中）
                    _drawer = default;
                    _tray = default;
                }

                if (s.Panel == SlicePanel.Build)
                    DrawBuild(s, sim);
                else if (s.Panel == SlicePanel.Map)
                    DrawMap(s, sim);
                else if (s.Panel == SlicePanel.Craft)
                    DrawCraft(s);
                else
                    _panel = default;

                // 天赋树是全屏独占表面：横幅（世界状态）不再压在上面
                if (s.Panel != SlicePanel.Build)
                {
                    if (s.State == MapState.Dead)
                        DrawBanner(SliceCopy.DeadRespec, new Color(0.55f, 0.12f, 0.10f, 0.92f));
                    else if (s.State == MapState.Cleared)
                        DrawBanner(SliceCopy.Cleared, new Color(0.12f, 0.38f, 0.18f, 0.92f));
                }

                EndDragIfNeeded(s);
                DrawDragGhost();
                DrawFlash();
                DrawTooltip();
            }
            finally
            {
                GUI.matrix = oldMatrix;
            }
        }

        /// <summary>设计空间宽/高（方案页 1920x1080 基准坐标）。</summary>
        float Dw()
        {
            return Screen.width / _scale;
        }

        float Dh()
        {
            return Screen.height / _scale;
        }

        /// <summary>GUI 指针（当前 GUI 空间）；DebugHover 时钉在验证点。</summary>
        Vector2 Pointer
        {
            get { return DebugHover ? DebugHoverPoint : Event.current.mousePosition; }
        }

        public void HandleKeys(SliceSession s, ArenaSim sim)
        {
            // 天赋树开关（导演：天赋树通过快捷键打开和关闭）；P 与 Tab 同义，Tab 保留给老习惯
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Tab))
                s.Panel = s.Panel == SlicePanel.Build ? SlicePanel.None : SlicePanel.Build;
            // 背包开关（导演 2026-09-11：背包改成快捷键开关式）
            if (Input.GetKeyDown(KeyCode.I))
                s.BagOpen = !s.BagOpen;
            if (Input.GetKeyDown(KeyCode.F6))
                s.Panel = s.Panel == SlicePanel.Map ? SlicePanel.None : SlicePanel.Map;
            if (Input.GetKeyDown(KeyCode.F8))
                s.Panel = s.Panel == SlicePanel.Craft ? SlicePanel.None : SlicePanel.Craft;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                s.Panel = SlicePanel.None;
                s.BagOpen = false;
            }
            if (Input.GetKeyDown(KeyCode.R) && s.Panel != SlicePanel.None)
            {
                string err;
                if (!s.TryRespec(out err) && err != null)
                    s.LastMessage = err;
            }

            if (s.Panel == SlicePanel.Map && Input.GetKeyDown(KeyCode.Return))
            {
                string err;
                if (s.OnMap)
                    s.ExitMap(sim, false);
                else if (!s.TryEnterMap(sim, out err) && err != null)
                    s.LastMessage = err;
            }
        }

        void EnsureStyles()
        {
            if (_styles)
                return;
            _white = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
            // 2026-09-10 导演：字号整体上调（旧 11px 档在缩放下被图标挤断），并给每级留足行高。
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Text }
            };
            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = true,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Text }
            };
            _small = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Dim }
            };
            _center = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                wordWrap = true,
                normal = { textColor = SlicePalette.Text }
            };
            _tiny = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                normal = { textColor = SliceSkin.TextCream }
            };
            _caption = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Dim }
            };
            _clip = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = SlicePalette.Text }
            };
            // S5U-F1 V-07：页面主标题层级（20-22）；V-05 导航降权态
            _pageTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                normal = { textColor = SliceSkin.TextCream }
            };
            _navIdle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Dim }
            };
            // Phase3 换皮（SliceSkin 程序化石质贴图；border 必须与贴图九宫格边框一致）
            RectOffset pb = new RectOffset(SliceSkin.PanelBorder, SliceSkin.PanelBorder, SliceSkin.PanelBorder, SliceSkin.PanelBorder);
            RectOffset sb = new RectOffset(SliceSkin.SlotBorder, SliceSkin.SlotBorder, SliceSkin.SlotBorder, SliceSkin.SlotBorder);
            _panelBg = new GUIStyle { border = pb, normal = { background = SliceSkin.Panel } };
            _slotN = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotNormal } };
            _slotH = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotHover } };
            _slotP = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotPress } };
            _slotSel = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotSelected } };
            // S5U-F1 V-02：Cell 层级（暗铁无金；hover=铁棱微亮）
            _slotSubtleN = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotSubtle } };
            _slotSubtleH = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotSubtleHover } };
            _orbText = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                normal = { textColor = SliceSkin.TextCream }
            };
            _styles = true;
        }

        void DrawTop(SliceSession s)
        {
            // 2026-09-10 导演：状态条加宽加高——旧 470×64 在 14px 字号下三行互相挤压、且被右侧导航压边。
            _topBar = new Rect(12, 10, 620, 84);
            GUI.Box(_topBar, GUIContent.none, _slotSubtleN);
            Label(new Rect(_topBar.x + 16, _topBar.y + 8, _topBar.width - 32, 24), s.StatusCopy, _title);
            Label(new Rect(_topBar.x + 16, _topBar.y + 36, _topBar.width - 32, 20),
                "稳定度 " + s.Stability + "    收益 x" + s.RewardMultiplier.ToString("0.00") +
                "    废料 " + s.Scrap + "    蚀刻剂 " + s.Etching +
                "    天赋点 " + s.Unspent + "/" + s.TotalPoints, _small);
            Clipped(new Rect(_topBar.x + 16, _topBar.y + 58, _topBar.width - 32, 20),
                s.LastMessage + (string.IsNullOrEmpty(s.LastLoot) ? "" : "  ·  " + s.LastLoot), _small);
        }

        void DrawNav(SliceSession s, ArenaSim sim)
        {
            // 2026-09-10 导演：背包面板贴右；2026-09-11 导演：改为快捷键开关式（I 开关 / Esc 关闭）
            _nav = new Rect(_topBar.xMax + 10f, 10, 354, 40);
            GUI.Box(_nav, GUIContent.none, _slotSubtleN);
            float x = _nav.x + 7;
            if (NavBtnSmall(new Rect(x, _nav.y + 7, 80, 26), "天赋 P", s.Panel == SlicePanel.Build))
                s.Panel = s.Panel == SlicePanel.Build ? SlicePanel.None : SlicePanel.Build;
            if (NavBtnSmall(new Rect(x + 86, _nav.y + 7, 80, 26), "地图 F6", s.Panel == SlicePanel.Map))
                s.Panel = s.Panel == SlicePanel.Map ? SlicePanel.None : SlicePanel.Map;
            if (NavBtnSmall(new Rect(x + 172, _nav.y + 7, 80, 26), "制作 F8", s.Panel == SlicePanel.Craft))
                s.Panel = s.Panel == SlicePanel.Craft ? SlicePanel.None : SlicePanel.Craft;
            if (NavBtnSmall(new Rect(x + 258, _nav.y + 7, 82, 26), "背包 I", s.BagOpen))
                s.BagOpen = !s.BagOpen;
        }

        /// <summary>S5U-F1 V-05：小型导航按钮（inactive=暗铁+暗字；active=金字金框；点击语义同 NavBtn）。</summary>
        bool NavBtnSmall(Rect r, string text, bool on)
        {
            Event e = Event.current;
            bool hover = e != null && r.Contains(Pointer);
            bool press = hover && e != null && e.type == EventType.MouseDown && e.button == 0;
            GUI.Box(r, GUIContent.none, on ? _slotSel : (press ? _slotP : (hover ? _slotSubtleH : _slotSubtleN)));
            GUI.color = on ? SliceSkin.Gold : SlicePalette.Dim;
            Label(r, text, _navIdle);
            GUI.color = Color.white;
            bool clicked = Click(r);
            if (clicked)
                ClickFlash(r);
            return clicked;
        }

        /// <summary>S5U-WO-02 战斗底栏布局单一来源（测试锚点）。S5U-F1 V-01：统一 action assembly——
        /// 外框 Frame 包裹 Life | support tray | QWE | Mana 连续整体；Frame= strongest 框层级。</summary>
        /// <summary>底栏主动技能槽数（Q W E R T，与 PoE 同类 ARPG 的默认技能栏一致）。</summary>
        public const int SkillSlots = 5;
        /// <summary>为后续药剂预留的槽数（当前无药剂内容，恒为空槽呈现）。</summary>
        public const int FlaskSlots = 4;
        /// <summary>主动技能槽热键标签（索引 0..4）。</summary>
        public static readonly string[] SlotHotkeys = { "Q", "W", "E", "R", "T" };
        /// <summary>预留药剂槽热键标签（PoE 式数字键）。</summary>
        public static readonly string[] FlaskHotkeys = { "1", "2", "3", "4" };

        public struct CombatBarLayout
        {
            public Rect Bar, Frame, LifeOrb, ManaOrb, FlaskRow;
            /// <summary>主动技能槽行（Q/W/E/R/T 顺序，5 个）。</summary>
            public Rect SlotQ, SlotW, SlotE, SlotR, SlotT;
            /// <summary>预留药剂槽（1/2/3/4）。</summary>
            public Rect Flask1, Flask2, Flask3, Flask4;

            public Rect SkillSlot(int i)
            {
                switch (i)
                {
                    case 0: return SlotQ;
                    case 1: return SlotW;
                    case 2: return SlotE;
                    case 3: return SlotR;
                    default: return SlotT;
                }
            }

            public Rect FlaskSlot(int i)
            {
                switch (i)
                {
                    case 0: return Flask1;
                    case 1: return Flask2;
                    case 2: return Flask3;
                    default: return Flask4;
                }
            }
        }

        /// <summary>2026-09-10 导演：底栏只保留主动技能，Q/W/E 扩展为 Q/W/E/R/T，并为后续药剂预留槽位；
        /// 辅助宝石托盘已迁出底栏（移入背包面板）。</summary>
        public static CombatBarLayout CombatBarRects(float dw, float dh)
        {
            return CombatBarRects(dw, dh, true);
        }

        /// <summary>背包面板开关参与底栏布局：关闭时底栏可用整幅宽度（导演 2026-09-11 背包改快捷键开关）。</summary>
        public static CombatBarLayout CombatBarRects(float dw, float dh, bool bagOpen)
        {
            // 背包面板开启时贴右占 SliceDrawerLayout.PanelW：底栏在剩余宽度里居中，不与面板相互遮挡；
            // 极窄设计空间（小窗钳到 0.4 缩放）等比缩小，保证永不出界/永不压面板。
            float nominalOrb = 128f;
            const float nominalSlot = 92f, nominalSlotGap = 8f;
            const float nominalFlaskW = 54f, nominalFlaskH = 78f, nominalFlaskGap = 6f;
            float nominalSlotsW = nominalSlot * SkillSlots + nominalSlotGap * (SkillSlots - 1);
            float nominalFlasksW = nominalFlaskW * FlaskSlots + nominalFlaskGap * (FlaskSlots - 1);
            float nominalTotal = nominalOrb + 14f + nominalSlotsW + 18f + nominalFlasksW + 14f + nominalOrb;
            float availW = dw - (bagOpen ? SliceDrawerLayout.PanelW : 0f);
            // 左右各留 12 的外框余量后仍放不下就等比缩小（k 已把余量算进去，x0 的 12px 下限不会再顶出去）
            float k = Mathf.Min(1f, Mathf.Max(0.2f, (availW - 24f) / nominalTotal));

            float orbD = nominalOrb * k;
            float slotSize = nominalSlot * k;
            float slotGap = nominalSlotGap * k;
            float flaskW = nominalFlaskW * k;
            float flaskH = nominalFlaskH * k;
            float flaskGap = nominalFlaskGap * k;
            float barH = orbD + 16f;
            float slotsW = slotSize * SkillSlots + slotGap * (SkillSlots - 1);
            float flasksW = flaskW * FlaskSlots + flaskGap * (FlaskSlots - 1);
            float total = orbD + 14f * k + slotsW + 18f * k + flasksW + 14f * k + orbD;
            float x0 = Mathf.Max(12f, (Mathf.Max(total, availW) - total) * 0.5f);
            CombatBarLayout L;
            L.Frame = new Rect(x0 - 10f, dh - barH - 28f, total + 20f, barH + 28f);
            L.Bar = new Rect(x0, dh - barH - 8f, total, barH);
            float y = L.Bar.y + 8f;
            L.LifeOrb = new Rect(x0, y, orbD, orbD);
            float sx = x0 + orbD + 14f;
            float sy = L.Bar.y + (barH - slotSize) * 0.5f;
            L.SlotQ = new Rect(sx, sy, slotSize, slotSize);
            L.SlotW = new Rect(sx + (slotSize + slotGap), sy, slotSize, slotSize);
            L.SlotE = new Rect(sx + (slotSize + slotGap) * 2f, sy, slotSize, slotSize);
            L.SlotR = new Rect(sx + (slotSize + slotGap) * 3f, sy, slotSize, slotSize);
            L.SlotT = new Rect(sx + (slotSize + slotGap) * 4f, sy, slotSize, slotSize);
            float fx = sx + slotsW + 18f;
            float fy = L.Bar.y + (barH - flaskH) * 0.5f;
            L.Flask1 = new Rect(fx, fy, flaskW, flaskH);
            L.Flask2 = new Rect(fx + (flaskW + flaskGap), fy, flaskW, flaskH);
            L.Flask3 = new Rect(fx + (flaskW + flaskGap) * 2f, fy, flaskW, flaskH);
            L.Flask4 = new Rect(fx + (flaskW + flaskGap) * 3f, fy, flaskW, flaskH);
            L.FlaskRow = new Rect(fx, fy, flasksW, flaskH);
            L.ManaOrb = new Rect(fx + flasksW + 14f, y, orbD, orbD);
            return L;
        }

        /// <summary>S5U-WO-02 战斗底栏（S5U-F1 V-01 统一 action assembly）：
        /// 外框 plate → LIFE 球 ─ 辅助 tray ─ [Q][W][E] 图标槽 ─ MANA 球 连续整体；
        /// 语义冻结：连接源唯一真值（LinkBadgeText=LinkSourceLabel 同源紧凑态）/容量/支持孔 1:1/拖放与点击流不变。</summary>
        void DrawSkillHud(SliceSession s)
        {
            var L = CombatBarRects(Dw(), Dh(), s.BagOpen);
            _skillHud = L.Frame;
            PanelBg(L.Frame); // Outer 框层级（strongest）
            DrawOrb(L.LifeOrb,
                s.MaxLife > 0f ? s.Life / s.MaxLife : 0f, SlicePalette.Life, s.Life, s.MaxLife, true);
            DrawSkillCell(s, SkillId.Melee, L.SlotQ, 0);
            DrawSkillCell(s, SkillId.Projectile, L.SlotW, 1);
            DrawSkillCell(s, SkillId.Area, L.SlotE, 2);
            DrawSkillCell(s, SkillId.IceSpear, L.SlotR, 3);
            DrawSkillCell(s, SkillId.Fireball, L.SlotT, 4);
            DrawFlaskSlots(L);
            DrawOrb(L.ManaOrb,
                s.MaxMana > 0f ? s.Mana / s.MaxMana : 0f, SlicePalette.Mana, s.Mana, s.MaxMana, false);
        }

        /// <summary>预留的主动技能槽（尚无技能内容）：只呈现空槽+热键，不提供任何点击语义。</summary>
        void DrawEmptySkillCell(Rect frame, int slot)
        {
            Event e = Event.current;
            bool hover = e != null && frame.Contains(Pointer);
            GUI.Box(frame, GUIContent.none, hover ? _slotSubtleH : _slotSubtleN);
            if (hover)
                RequestTip(SliceTooltipModel.TextCard("空技能槽 " + SlotHotkeys[slot], "尚未装配技能。"), TipPriBottomBar);
        }

        /// <summary>预留药剂槽（导演：为后续药剂留位置）：恒为空槽+数字热键，不作假内容。</summary>
        void DrawFlaskSlots(CombatBarLayout L)
        {
            // Aria 药剂图标（导演输入素材包）；缺失时退回同包的心脏瓶，再缺失则只留空槽
            Texture2D potion = SliceAria.IconPotion1;
            if (potion == null)
                potion = SliceAria.Get("Icons/PotionHeart");
            for (int i = 0; i < FlaskSlots; i++)
            {
                Rect r = L.FlaskSlot(i);
                Event e = Event.current;
                bool hover = e != null && r.Contains(Pointer);
                GUI.Box(r, GUIContent.none, hover ? _slotSubtleH : _slotSubtleN);
                if (potion != null)
                {
                    GUI.color = new Color(1f, 1f, 1f, hover ? 0.38f : 0.24f);
                    GUI.DrawTexture(new Rect(r.x + 9f, r.y + 20f, r.width - 18f, r.height - 34f), potion);
                    GUI.color = Color.white;
                }
                Rect hb = new Rect(r.x + 4f, r.y + 3f, 18f, 16f);
                Fill(hb, new Color(0.05f, 0.045f, 0.04f, 0.92f));
                Label(hb, FlaskHotkeys[i], _tiny);
                if (hover)
                    RequestTip(SliceTooltipModel.TextCard("空药剂槽 " + FlaskHotkeys[i], "尚未装配药剂。"), TipPriBottomBar);
            }
        }

        /// <summary>S5U-WO-02：图标优先技能槽（96² 框 + 符文 + 热键徽章 + 紧凑连接徽章 + 支持孔 pip 行 + 弱化名注）。
        /// 悬停=完整连接真值 tooltip（LinkSourceLabel/组划分/支持清单——S5 合同不降级）。</summary>
        void DrawSkillCell(SliceSession s, SkillId skill, Rect frame, int slot)
        {
            bool sel = s.SelectedSkill == skill;
            Event e = Event.current;
            bool hover = e != null && frame.Contains(Pointer);
            bool press = hover && e != null && e.type == EventType.MouseDown && e.button == 0
                && !PipsRect(frame).Contains(Pointer);
            // S5U-F1 V-02/V-08：Cell 层级=暗铁；selected 才用金；连接徽章降权（技能身份=符文第一）
            GUI.Box(frame, GUIContent.none, press ? _slotP : (sel ? _slotSel : (hover ? _slotSubtleH : _slotSubtleN)));
            // 图标/Aria 描金槽框：帧成为技能身份的第一层，而非被程序化方框压住
            var skillFrame = SliceAria.ButtonBrown;
            if (skillFrame != null)
                NineSlice(new Rect(frame.x + 2f, frame.y + 2f, frame.width - 4f, frame.height - 4f), skillFrame, 10f);
            Rect glyph = new Rect(frame.x + frame.width * 0.5f - 25f, frame.y + 8f, 50f, 50f);
            bool artGlyph = SliceHudIcons.SkillGlyphIsArt(skill);
            // S6P-WO-05：poedb 宝石图自带配色，不再按状态整体染色（只做轻微明度区分），否则暗掉宝石本色
            var iconTint = artGlyph
                ? (sel ? Color.white : new Color(1f, 1f, 1f, 0.92f))
                : (sel ? Color.white : (hover ? new Color(1f, 1f, 1f, 0.96f) : new Color(0.80f, 0.78f, 0.72f, 1f)));
            GUI.color = iconTint;
            GUI.DrawTexture(glyph, SliceHudIcons.SkillGlyph(skill));
            GUI.color = Color.white;
            Rect hb = new Rect(frame.x + frame.width - 24f, frame.y + 4f, 20f, 17f);
            Fill(hb, new Color(0.05f, 0.045f, 0.04f, 0.92f));
            Label(hb, SlotHotkeys[slot], _tiny);
            Rect badge = new Rect(frame.x + 6f, frame.y + 60f, frame.width - 12f, 18f);
            Fill(badge, new Color(0.05f, 0.045f, 0.04f, 0.85f));
            GUI.color = sel ? SliceSkin.TextCream : SlicePalette.Dim;
            Label(badge, s.LinkBadgeText(skill), _tiny);
            GUI.color = Color.white;
            // 支持孔 pip 行（1:1 映射原 socket 语义；容量外=closed）
            SupportId[] arr = s.SupportsOf(skill);
            if (arr != null)
            {
                int cap = s.SupportCapacity(skill);
                for (int i = 0; i < arr.Length; i++)
                {
                    var pip = new Rect(frame.x + 6f + i * 22f, frame.y + 79f, 18f, 18f);
                    DrawSocketPip(s, skill, i, cap, arr[i], pip);
                }
            }
            // 名注（弱化，语义保留）：贴在底栏外框之内，不再与下一屏内容抢位置
            var cap0 = new Rect(frame.x - 21f, frame.yMax + 2f, 134f, 18f);
            Label(cap0, SliceSession.SkillDisplayName(skill), _caption);
            if (Click(new Rect(frame.x + 4f, frame.y + 4f, frame.width - 8f, 54f)))
            {
                s.SelectedSkill = skill;
                ClickFlash(frame);
            }
            // pip 行悬停时让位给支持孔 tooltip（同优先级后写胜出，须显式排除）
            if (hover && !PipsRect(frame).Contains(Pointer) && (e == null || e.type == EventType.Repaint))
                RequestSkillCellTip(s, skill);
            // 框级拖放落点（未命中具体 pip 时=第一个有效孔）
            if (_dragging && e != null && e.type == EventType.MouseUp && frame.Contains(Pointer)
                && !PipsRect(frame).Contains(Pointer))
            {
                int idx = FirstOpenSocketIndex(s, skill);
                PlaceSupport(s, skill, idx, _drag);
                ClickFlash(frame);
                e.Use();
            }
        }

        /// <summary>pip 行命中区（含容差；供框级落点排除）。</summary>
        static Rect PipsRect(Rect frame)
        {
            return new Rect(frame.x + 4f, frame.y + 77f, frame.width - 8f, 17f);
        }

        static int FirstOpenSocketIndex(SliceSession s, SkillId skill)
        {
            SupportId[] arr = s.SupportsOf(skill);
            if (arr == null)
                return 0;
            int cap = Mathf.Min(s.SupportCapacity(skill), arr.Length);
            for (int i = 0; i < cap; i++)
                if (arr[i] == SupportId.None)
                    return i;
            return 0;
        }

        /// <summary>技能槽悬停 tooltip：连接源完整真值（S5 合同承载点）。</summary>
        void RequestSkillCellTip(SliceSession s, SkillId skill)
        {
            SupportId[] arr = s.SupportsOf(skill);
            string sup = "无辅助";
            if (arr != null)
            {
                var names = new System.Collections.Generic.List<string>(arr.Length);
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i] != SupportId.None)
                        names.Add(SupportCatalog.Get(arr[i]).Name);
                if (names.Count > 0)
                    sup = "辅助：" + string.Join(" / ", names.ToArray());
            }
            // TextCard(body) 是单元素 Subtitle 语义槽；多行真值须显式拆进 Body[]（15px/行裁剪）
            var card = SliceTooltipModel.TextCard(
                SliceSession.SkillDisplayName(skill) + "  " + SliceSession.SkillHotkey(skill), null);
            card.Body = new[] { s.LinkSourceLabel(skill), sup, "左键选择技能 · 拖辅助入孔" };
            RequestTip(card, TipPriBottomBar);
        }

        /// <summary>S5U-WO-02：支持孔 pip（closed/empty/filled 三态语义与旧 DrawSocket 一致；中性金属恒等映射≠Socket Color）。</summary>
        void DrawSocketPip(SliceSession s, SkillId skill, int index, int cap, SupportId filled, Rect r)
        {
            bool closed = index >= cap;
            Event e = Event.current;
            bool over = e != null && r.Contains(Pointer);
            GUI.color = closed ? new Color(1f, 1f, 1f, 0.5f) : (over ? Color.white : new Color(0.93f, 0.93f, 0.93f, 1f));
            GUI.DrawTexture(r, SliceHudIcons.PipFor(filled, filled != SupportId.None, closed));
            GUI.color = Color.white;
            if (over)
            {
                if (closed)
                    RequestTip(SliceTooltipModel.TextCard("无孔", "该技能装备孔不足"), TipPriBottomBar);
                else if (filled == SupportId.None)
                    RequestTip(SliceTooltipModel.TextCard("空", "拖入或点击辅助"), TipPriBottomBar);
                else
                    RequestTip(SliceTooltipModel.SupportCard(filled, s), TipPriBottomBar);
            }
            if (closed)
                return;
            HandleSocketInput(s, skill, index, filled, r);
        }

        void HandleSocketInput(SliceSession s, SkillId skill, int index, SupportId filled, Rect r)
        {
            Event e = Event.current;
            if (e == null)
                return;
            bool over = r.Contains(Pointer);
            if (_dragging && e.type == EventType.MouseUp && over)
            {
                PlaceSupport(s, skill, index, _drag);
                ClickFlash(r);
                e.Use();
                return;
            }

            if (!over || e.type != EventType.MouseDown || e.button != 0)
                return;

            if (_picked != SupportId.None)
            {
                PlaceSupport(s, skill, index, _picked);
                ClickFlash(r);
                e.Use();
                return;
            }

            if (filled != SupportId.None)
            {
                string err;
                if (!s.TrySetSupport(skill, index, SupportId.None, out err) && err != null)
                    s.LastMessage = err;
                _picked = filled;
                ClickFlash(r);
                e.Use();
            }
        }

        /// <summary>2026-09-10 导演：辅助宝石托盘迁出底栏，改在背包面板“辅助宝石”区呈现
        /// （底栏只保留主动技能 + 预留药剂槽）。拾取/拖拽/落点输入流一字未改。</summary>
        void DrawSupportTray(SliceSession s, Rect area)
        {
            _tray = area;
            const float gap = 6f;
            int n = SupportCatalog.Count;
            if (n <= 0)
                return;
            float gw = (area.width - gap * (n - 1)) / n;
            float gh = area.height;
            for (int i = 1; i <= n; i++)
            {
                Rect g = new Rect(area.x + (i - 1) * (gw + gap), area.y, gw, gh);
                DrawGem(s, (SupportId)i, g);
            }
        }

        void DrawGem(SliceSession s, SupportId id, Rect r)
        {
            SupportDef def = SupportCatalog.Get(id);
            bool used = IsLinked(s, id);
            bool pick = _picked == id;
            Event e = Event.current;
            bool hover = e != null && r.Contains(Pointer);
            GUI.Box(r, GUIContent.none, pick ? _slotSel : (hover ? _slotSubtleH : _slotSubtleN));
            if (def.ChangesMechanism)
                Bar(r.x, r.y, 4, r.height, SlicePalette.Cinder);
            // S6P-WO-05：poedb 辅助宝石图（真实 PoE 宝石美术）；缺失则退回纯文字格
            Texture2D gem = SliceHudIcons.SupportGem(id);
            float textX = r.x + 10f;
            float textW = r.width - 14f;
            if (gem != null)
            {
                float gs = Mathf.Min(34f, r.height - 8f);
                var gr = new Rect(r.x + 8f, r.y + (r.height - gs) * 0.5f, gs, gs);
                GUI.color = used ? Color.white : new Color(1f, 1f, 1f, 0.82f);
                GUI.DrawTexture(gr, gem);
                GUI.color = Color.white;
                textX = gr.xMax + 6f;
                textW = r.xMax - textX - 4f;
            }
            string label = def.Name;
            GUI.color = used ? SliceSkin.TextCream : SlicePalette.Dim;
            // 托盘已迁入宽面板：一格一个宝石名，字号与行高按新尺寸给足，不再截断
            Clipped(new Rect(textX, r.y + (r.height - 20) * 0.5f, textW, 20), used ? label + "*" : label, _clip);
            GUI.color = Color.white;
            if (hover)
                RequestTip(SliceTooltipModel.SupportCard(id, s), TipPriDrawer); // 兼容性=canonical runtime（工作令 十六/十七）
            e = Event.current;
            if (e == null || !r.Contains(Pointer))
                return;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                _picked = id;
                _drag = id;
                _dragging = false;
                _dragStart = Pointer;
                ClickFlash(r);
                e.Use();
            }
        }

        /// <summary>背包面板（2026-09-10 导演：完全贴右边、上下通顶、扩大格子显示量）。
        /// 单渲染器原则不变：装备六槽 / 辅助宝石托盘 / 背包网格三区共用一块常驻面板，
        /// 拖拽拾取与点击流一字未改，只是搬了家。</summary>
        void DrawDrawer(SliceSession s)
        {
            float dw = Dw(), dh = Dh();
            _drawer = SliceDrawerLayout.Shell(dw, dh);
            PanelBg(_drawer);
            // 导演输入（Aria）：描金九宫窗框（纹理缺失=程序化 PanelBg 独立成立）
            var ariaFrame = SliceAria.PanelFrame;
            if (ariaFrame != null)
                NineSlice(_drawer, ariaFrame, 14f);
            var head = SliceDrawerLayout.ShellHeader(dw, dh);
            Label(new Rect(head.x + SliceDrawerLayout.PadX, head.y + 8, 220, 26), "背包", _pageTitle);
            GUI.DrawTexture(new Rect(head.x + SliceDrawerLayout.PadX, head.y + head.height - 4,
                head.width - 2f * SliceDrawerLayout.PadX, 3), SliceHudIcons.Separator);
            if (NavBtnSmall(SliceDrawerLayout.ShellTab(0, dw, dh), "天赋 P", s.Panel == SlicePanel.Build))
                s.Panel = s.Panel == SlicePanel.Build ? SlicePanel.None : SlicePanel.Build;
            if (NavBtnSmall(SliceDrawerLayout.ShellTab(1, dw, dh), "制作 F8", s.Panel == SlicePanel.Craft))
                s.Panel = s.Panel == SlicePanel.Craft ? SlicePanel.None : SlicePanel.Craft;
            if (NavBtnSmall(new Rect(head.xMax - SliceDrawerLayout.PadX - 76f, head.y + 6, 76, 26), "关闭 I", false))
                s.BagOpen = false;
            // 装备区（恒 6 槽；3 列 × 2 行；含第二连接配置条——冻结的构筑功能，随槽卡保留）
            Label(SliceDrawerLayout.EquipLabel(dw, dh), "装备", _title);
            for (int i = 0; i < SliceDrawerLayout.DisplayOrder.Length; i++)
                DrawSlotCard(s, SliceDrawerLayout.DisplayOrder[i], SliceDrawerLayout.ShellSlot(i, dw, dh));
            // 辅助宝石区（2026-09-10：自底栏迁入；拾取→点孔/拖放的输入流不变）
            Label(SliceDrawerLayout.TrayLabel(dw, dh), "辅助宝石（点选后装配到技能孔）", _title);
            DrawSupportTray(s, SliceDrawerLayout.TrayArea(dw, dh));
            // 背包区（12 列满幅格子；顺序=库存真值；零网格机制）
            Label(SliceDrawerLayout.InvLabel(dw, dh),
                "背包  " + s.InventoryCount + "/" + s.Inventory.Length + "（点击装备）", _title);
            DrawInventoryGrid(s, SliceDrawerLayout.ShellInvView(dw, dh));
            // 底部反馈条
            Clipped(SliceDrawerLayout.ShellFooter(dw, dh),
                s.LastMessage + (string.IsNullOrEmpty(s.LastLoot) ? "" : "  ·  " + s.LastLoot), _small);
        }

        /// <summary>九宫描画（Aria 描金窗框；border=纹理角部像素）。UV 原点=左下，纹理对称故无需翻转。</summary>
        static void NineSlice(Rect r, Texture2D t, float b)
        {
            float uw = b / t.width, vh = b / t.height;
            float iw = 1f - 2f * uw, ih = 1f - 2f * vh;
            GUI.DrawTextureWithTexCoords(new Rect(r.x, r.y, b, b), t, new Rect(0f, 0f, uw, vh));
            GUI.DrawTextureWithTexCoords(new Rect(r.xMax - b, r.y, b, b), t, new Rect(1f - uw, 0f, uw, vh));
            GUI.DrawTextureWithTexCoords(new Rect(r.x, r.yMax - b, b, b), t, new Rect(0f, 1f - vh, uw, vh));
            GUI.DrawTextureWithTexCoords(new Rect(r.xMax - b, r.yMax - b, b, b), t, new Rect(1f - uw, 1f - vh, uw, vh));
            GUI.DrawTextureWithTexCoords(new Rect(r.x + b, r.y, r.width - 2f * b, b), t, new Rect(uw, 0f, iw, vh));
            GUI.DrawTextureWithTexCoords(new Rect(r.x + b, r.yMax - b, r.width - 2f * b, b), t, new Rect(uw, 1f - vh, iw, vh));
            GUI.DrawTextureWithTexCoords(new Rect(r.x, r.y + b, b, r.height - 2f * b), t, new Rect(0f, vh, uw, ih));
            GUI.DrawTextureWithTexCoords(new Rect(r.xMax - b, r.y + b, b, r.height - 2f * b), t, new Rect(1f - uw, vh, uw, ih));
            GUI.DrawTextureWithTexCoords(new Rect(r.x + b, r.y + b, r.width - 2f * b, r.height - 2f * b), t, new Rect(uw, vh, iw, ih));
        }

        /// <summary>S5U-F1 回归锚（V-15）：滚动视口内 cell 的屏幕（设计空间）矩形——内容坐标→视口坐标换算单一来源；
        /// DrawInventoryGrid 与回归测试共用，坐标永不再漂移。</summary>
        public static Rect InvCellScreenRect(Rect view, Vector2 scroll, int index)
        {
            Rect cell = SliceDrawerLayout.ShellInvCell(index);
            return new Rect(view.x + cell.x - scroll.x, view.y + cell.y - scroll.y, cell.width, cell.height);
        }

        /// <summary>背包呈现网格（S-10/S-11/S-13 + 2026-09-10 导演指令"D2 式满幅格子"）：
        /// 1 物品=1 格（78×70），顺序=库存真值；图标优先格=稀有度染底+同色 1px 框+居中 44² 类型图标
        /// （多件同类并排：格底/框色=稀有度辨识，图标=槽位辨识，名称/词缀=悬停 ItemCard tooltip）；
        /// 选中/hover 独立状态（selected &gt; hover）；点击流=既有 SelectInv+TryEquip（零新机制）。</summary>
        void DrawInventoryGrid(SliceSession s, Rect view)
        {
            // 2026-09-10 导演：满幅格网——整块容量全部画成格子（空格也是可见槽位），而非只画已有物品。
            int cells = s.Inventory.Length;
            float contentH = SliceDrawerLayout.ShellInvContentHeight(cells);
            _invScroll = GUI.BeginScrollView(view, _invScroll, new Rect(0, 0, view.width - 18, contentH));
            for (int i = 0; i < cells; i++)
            {
                Rect cell = SliceDrawerLayout.ShellInvCell(i);
                // 悬停/tooltip 用屏幕（设计）坐标：cell 是内容坐标，Pointer 是设计空间
                // （旧列表实现此处的坐标从未对齐——S5U-WO-03 修复；换算单一来源=InvCellScreenRect 回归锚）
                Rect cellScreen = InvCellScreenRect(view, _invScroll, i);
                Event e = Event.current;
                bool hover = e != null && cellScreen.Contains(Pointer);
                if (i >= s.InventoryCount)
                {
                    // 空槽：只给一层极弱铁框，不喧宾夺主
                    GUI.Box(cell, GUIContent.none, hover ? _slotSubtleH : _slotSubtleN);
                    continue;
                }

                ItemInstance it = s.Inventory[i];
                bool sel = i == s.SelectedInv;
                bool equipped = false;
                for (int k = 0; k < s.Equipped.Length; k++)
                    if (s.Equipped[k] == i) { equipped = true; break; }
                bool rare = it.Rarity == Rarity.Rare;
                GUI.Box(cell, GUIContent.none, sel ? _slotSel : (hover ? _slotSubtleH : _slotSubtleN));
                // 稀有度染底（Rare=暗金雾 / Ordinary=中性极弱）+同色 1px 内框：拥挤网格的第一辨识层
                Fill(cell, rare ? new Color(0.95f, 0.78f, 0.22f, 0.12f) : new Color(0.45f, 0.47f, 0.52f, 0.08f));
                Color rim = rare ? SlicePalette.Rare : new Color(0.55f, 0.58f, 0.64f, 0.5f);
                Fill(new Rect(cell.x, cell.y, cell.width, 1f), rim);
                Fill(new Rect(cell.x, cell.yMax - 1f, cell.width, 1f), rim);
                Fill(new Rect(cell.x, cell.y, 1f, cell.height), rim);
                Fill(new Rect(cell.xMax - 1f, cell.y, 1f, cell.height), rim);
                // 居中物品美术（poedb 真实物品图优先；缺失回退 Aria/程序化槽位符号）
                GUI.color = equipped ? Color.white : new Color(0.88f, 0.88f, 0.88f, 1f);
                GUI.DrawTexture(FitAspect(cell, SliceHudIcons.ItemIcon(it.Slot, it.Rarity),
                        Mathf.Min(cell.width, cell.height) - 8f),
                    SliceHudIcons.ItemIcon(it.Slot, it.Rarity));
                GUI.color = Color.white;
                if (equipped)
                {
                    var badge = new Rect(cell.x + 2f, cell.y + 2f, cell.width - 4f, 16f);
                    Fill(badge, new Color(0.05f, 0.045f, 0.04f, 0.85f));
                    Label(badge, "已装备", _tiny);
                }
                if (hover)
                    RequestTip(SliceTooltipModel.ItemCard(it, s), TipPriPanel); // 候选 vs canonical 同槽已装备（工作令 七-十）
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none))
                {
                    s.SelectedInv = i;
                    string err;
                    if (!s.TryEquip(i, out err) && err != null)
                        s.LastMessage = err;
                }
            }
            GUI.EndScrollView();
        }

        /// <summary>按纹理原始宽高比在给定矩形内等比缩放居中（PoE 物品图非正方：武器 1×3、腰带 2×1——
        /// 强行塞进方框会拉伸变形）。纹理缺失时退化为原矩形的方形内缩。</summary>
        static Rect FitAspect(Rect box, Texture2D tex, float maxSide)
        {
            float side = Mathf.Min(maxSide, Mathf.Min(box.width, box.height));
            float aw = side;
            float ah = side;
            if (tex != null && tex.width > 0 && tex.height > 0)
            {
                float k = tex.width / (float)tex.height;
                if (k >= 1f)
                    ah = side / k;
                else
                    aw = side * k;
            }
            return new Rect(box.x + (box.width - aw) * 0.5f, box.y + (box.height - ah) * 0.5f, aw, ah);
        }

        void DrawBuild(SliceSession s, ArenaSim sim)
        {
            _panel = SliceDrawerLayout.BuildPanel(Dw());
            _panel = new Rect(0f, 0f, Dw(), Dh());
            DrawPoeTree(s);
        }

        /// <summary>装备卡（背包面板“装备”区，3 列 × 2 行）：槽位符号 + 名称 + 稀有度真值 + 第二连接配置条。
        /// 2026-09-10 导演：天赋页上方的装备区已删除，装备呈现唯一落点=此处。</summary>
        void DrawSlotCard(SliceSession s, EquipSlot slot, Rect r)
        {
            int idx = s.Equipped[(int)slot];
            bool has = idx >= 0 && idx < s.InventoryCount;
            Color edge = SlicePalette.Ordinary;
            if (has)
                edge = s.Inventory[idx].Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Ordinary;
            Fill(r, new Color(0.13f, 0.13f, 0.15f, 1f));
            Bar(r.x, r.y, 5, r.height, edge);
            Label(new Rect(r.x + 14, r.y + 6, r.width - 20, 22), SliceSession.SlotName(slot), _title);
            GUI.color = has ? Color.white : new Color(1f, 1f, 1f, 0.28f);
            GUI.DrawTexture(FitAspect(new Rect(r.x + 14, r.y + 32, 44, 44),
                    has ? SliceHudIcons.ItemIcon(slot, s.Inventory[idx].Rarity) : SliceHudIcons.EquipGlyph(slot), 44f),
                has ? SliceHudIcons.ItemIcon(slot, s.Inventory[idx].Rarity) : SliceHudIcons.EquipGlyph(slot));
            GUI.color = Color.white;
            if (!has)
            {
                Label(new Rect(r.x + 66, r.y + 44, r.width - 74, 24), "空", _body);
                return;
            }

            ItemInstance it = s.Inventory[idx];
            // S5-WO-03：第二连接配置条=卡底有界呈现（资格=SecondaryLinkConfigurable 与域同源）；
            // 配置条物品行转紧凑单行，词缀详情看 Tooltip（无新屏/无重构）
            bool rebind = SliceSession.SecondaryLinkConfigurable(it);
            DrawItemBody(it, new Rect(r.x + 66, r.y + 30, r.width - 74, rebind ? 22f : 56f), rebind);
            if (rebind)
                DrawRebindStrip(s, idx, it, new Rect(r.x + 12, r.y + r.height - 26, r.width - 24, 22));
            if (r.Contains(Pointer))
                RequestTip(SliceTooltipModel.ItemCard(it, s), TipPriPanel); // 已装备本体=「已装备」卡
            if (Click(r))
                s.SelectedInv = idx;
        }

        /// <summary>S5-WO-03：第二连接配置条（候选=RebindCandidates；可用性=PreviewReassignError 同一校验内核；
        /// 写入唯一走 TryReassignLink；不可用候选=灰显+hover 原因+点击=确定性可见拒绝，状态零改动）。</summary>
        void DrawRebindStrip(SliceSession s, int idx, ItemInstance it, Rect r)
        {
            Fill(r, new Color(0.10f, 0.10f, 0.12f, 1f));
            Label(new Rect(r.x + 2, r.y + 3, 58, 16), "第二连接", _small);
            float bx = r.x + 62;
            SkillId[] cands = s.RebindCandidates(it);
            for (int i = 0; i < cands.Length; i++)
            {
                SkillId skill = cands[i];
                Rect b = new Rect(bx + i * 38, r.y + 1, 34, r.height - 2);
                string reason = s.PreviewReassignError(it, skill);
                bool on = it.LinkSkill1 == skill;
                bool usable = reason == null;
                bool clicked = false;
                if (usable)
                {
                    clicked = NavBtn(b, SliceSession.SkillDisplayName(skill), on);
                    if (clicked && !on)
                    {
                        string err;
                        if (!s.TryReassignLink(idx, skill, out err) && err != null)
                            s.LastMessage = err;
                    }
                }
                else
                {
                    Event e = Event.current;
                    bool hover = e != null && b.Contains(Pointer);
                    Color old = GUI.color;
                    GUI.color = new Color(0.58f, 0.58f, 0.62f, 1f);
                    GUI.Box(b, GUIContent.none, hover ? _slotSubtleH : _slotSubtleN);
                    GUI.color = old;
                    Label(b, SliceSession.SkillDisplayName(skill), _center);
                    if (hover)
                        RequestTip(SliceTooltipModel.TextCard("第二连接不可用", reason), TipPriPanel);
                    if (Click(b))
                        s.LastMessage = reason; // 确定性可见拒绝（不静默、无异常文本）
                }
            }
            Rect cb = new Rect(bx + cands.Length * 38, r.y + 1, 30, r.height - 2);
            if (it.LinkSkill1 != SkillId.None)
            {
                if (NavBtn(cb, "无", false))
                {
                    string err;
                    if (!s.TryReassignLink(idx, SkillId.None, out err) && err != null)
                        s.LastMessage = err;
                }
            }
            else
            {
                GUI.Box(cb, GUIContent.none, _slotSubtleN);
                Label(cb, "无", _center);
            }
        }

        void DrawItemBody(ItemInstance it, Rect r, bool compact = false)
        {
            if (compact)
            {
                // S5-WO-03：第二连接配置条占用卡底时的单行物品体（词缀详情看 Tooltip）
                Clipped(new Rect(r.x, r.y, r.width, 20),
                    SliceSession.RarityWord(it.Rarity) + " " + SliceSession.CleanBaseName(it.BaseName) + " " + it.SocketCount + "孔", _small);
                return;
            }
            Color rc = it.Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Ordinary;
            GUI.color = rc;
            Label(new Rect(r.x, r.y, r.width, 16), SliceSession.RarityWord(it.Rarity), _small);
            GUI.color = Color.white;
            Clipped(new Rect(r.x, r.y + 16, r.width, 18), SliceSession.CleanBaseName(it.BaseName) + "  " + it.SocketCount + "孔", _body);
            float y = r.y + 36;
            for (int i = 0; i < it.AffixCount && y < r.yMax - 14; i++)
            {
                Clipped(new Rect(r.x, y, r.width, 16), SliceSession.AffixLine(it, i), _small);
                y += 16;
            }
        }

        // ============================ 真实 PoE 天赋树（全屏表面） ============================

        /// <summary>天赋树视口 = 全屏减去顶部标题条（天赋树是独占全屏表面，与 PoE 一致）。</summary>
        public static Rect PoeTreeViewport(float dw, float dh)
        {
            const float headerH = 46f;
            return new Rect(0f, headerH, dw, Mathf.Max(160f, dh - headerH));
        }

        /// <summary>全屏天赋树：真实 PoE 布局/图标/连线/词条，支持平移缩放、悬停词条、点击加点。
        /// 开关走快捷键 P（与角色页同一 Panel 语义）。</summary>
        void DrawPoeTree(SliceSession s)
        {
            float dw = Dw(), dh = Dh();
            // 全屏表面：暗底 + 细金线围边（不套九宫面板——贴图会被整屏拉伸成糊状）
            Fill(_panel, new Color(0.035f, 0.04f, 0.05f, 0.99f));
            Fill(new Rect(0f, 0f, dw, 2f), SliceSkin.GoldDim);
            Fill(new Rect(0f, dh - 2f, dw, 2f), SliceSkin.GoldDim);
            Fill(new Rect(0f, 0f, 2f, dh), SliceSkin.GoldDim);
            Fill(new Rect(dw - 2f, 0f, 2f, dh), SliceSkin.GoldDim);

            var head = new Rect(0f, 0f, dw, 46f);
            Fill(head, new Color(0.07f, 0.065f, 0.055f, 0.96f));
            Fill(new Rect(0f, head.yMax - 2f, dw, 2f), SliceSkin.GoldDim);

            int count = PassiveCatalog.Count;
            int allocated = 0;
            for (int i = 0; i < s.Allocated.Length; i++)
                if (s.Allocated[i])
                    allocated++;

            Label(new Rect(18f, 8f, 320f, 30f), "天赋树", _pageTitle);
            Label(new Rect(140f, 12f, dw - 460f, 24f),
                "天赋点 " + s.Unspent + " / " + s.TotalPoints + "     已点亮 " + allocated + " / " + count +
                "     金=已点亮 · 亮金框=可点亮且真生效 · 冷蓝框=可点亮但当前 0 效果 · 暗=当前不可用（悬停看原因）",
                _small);
            Label(new Rect(140f, 30f, dw - 460f, 20f),
                "滚轮缩放 · 拖拽平移 · 悬停看词条 · 点击加点 · P 关闭（连线：金=已通 · 灰=未点亮）",
                _caption);
            if (NavBtnSmall(new Rect(dw - 176f, 8f, 92f, 30f), "R 重构", false))
            {
                string err;
                if (!s.TryRespec(out err) && err != null)
                    s.LastMessage = err;
            }
            if (NavBtnSmall(new Rect(dw - 76f, 8f, 60f, 30f), "关闭", false))
                s.Panel = SlicePanel.None;

            Rect view = PoeTreeViewport(dw, dh);
            _treeOrigin = new Vector2(view.x, view.y);
            GUI.BeginGroup(view);
            Rect local = new Rect(0f, 0f, view.width, view.height);
            DrawPoeTreeCanvas(s, local);
            GUI.EndGroup();
            _treeOrigin = Vector2.zero;
        }

        /// <summary>视口内画布（局部坐标）：簇底衬 → 连线 → 节点 → tooltip。</summary>
        void DrawPoeTreeCanvas(SliceSession s, Rect local)
        {
            var groups = PoeTree.Groups;
            var nodes = PoeTree.Nodes;
            if (nodes == null || nodes.Length == 0)
            {
                Label(local, "天赋数据缺失（Resources/UI/PoE/passive_tree.json）", _center);
                return;
            }

            if (!_treeInit)
            {
                // 导演 2026-09-11：默认视图不再"整树缩到一粒一粒"（1920×1080 下 FitZoom≈0.044，
                // 节点只有 2px、连线挤成一团，肉眼看起来就是"没连线、点不动"）。
                // 改为以可读缩放对准起点所在簇；整树概览仍可用滚轮缩小。
                _treeZoom = Mathf.Clamp(DefaultTreeZoom, PoeTreeView.MinZoom, PoeTreeView.MaxZoom);
                _treePan = CentreOn(PoeTree.Get(SliceSession.StartNode), local, _treeZoom);
                _treeInit = true;
            }

            if (DebugTreeView)
            {
                _treeZoom = Mathf.Clamp(DebugTreeZoom, PoeTreeView.MinZoom, PoeTreeView.MaxZoom);
                _treePan = DebugTreeFocus >= 0 && DebugTreeFocus < nodes.Length
                    ? CentreOn(nodes[DebugTreeFocus], local, _treeZoom)
                    : PoeTreeView.FitPan(local, _treeZoom);
            }
            else
            {
                HandleTreeInput(local);
            }

            var frameAtlas = PoeTree.Data.chrome == null ? null : PoeTree.Data.chrome.frame;
            Texture2D frameTex = frameAtlas == null ? null : PoeTree.Chrome(frameAtlas.file);

            // 1) 簇底衬
            var groupAtlas = PoeTree.Data.chrome == null ? null : PoeTree.Data.chrome.group;
            Texture2D groupTex = groupAtlas == null ? null : PoeTree.Chrome(groupAtlas.file);
            if (groupTex != null)
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    PoeGroup g = groups[i];
                    string sprite = g.bg == 1 ? "PSGroupBackground1" : (g.bg == 3 ? "PSGroupBackground3" : "PSGroupBackground2");
                    Rect uv;
                    if (!groupAtlas.TryRect(sprite, out uv))
                        continue;
                    float sw = groupAtlas.PixelW(sprite), sh = groupAtlas.PixelH(sprite);
                    float radiusPx = g.radius * _treeZoom;
                    Vector2 gc = PoeTreeView.ScreenOf(new Vector2(g.x, g.y), _treePan, _treeZoom);
                    if (gc.x + radiusPx < local.x || gc.x - radiusPx > local.xMax ||
                        gc.y + radiusPx < local.y || gc.y - radiusPx > local.yMax)
                        continue;
                    Rect dr = PoeTreeView.GroupRect(g, sw, sh, _treePan, _treeZoom);
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    GUI.DrawTextureWithTexCoords(dr, groupTex, uv);
                    GUI.color = Color.white;
                }
            }

            // 2) 连线（同簇同轨=圆弧，其余=直线；颜色按已点亮程度分三级）
            for (int i = 0; i < nodes.Length; i++)
            {
                PoeNode n = nodes[i];
                int[] links = n.links;
                if (links == null)
                    continue;
                bool nOn = s.Allocated[i];
                for (int k = 0; k < links.Length; k++)
                {
                    int j = links[k];
                    if (j <= i)
                        continue;
                    PoeNode m = nodes[j];
                    bool mOn = s.Allocated[j];
                    bool mAvail = !mOn && s.CanAllocate(j);
                    bool nAvail = !nOn && s.CanAllocate(i);
                    // 导演 2026-09-11：「人眼看他们不是通过线连接起来的」——旧配色在贴合缩放下只有 ~0.9px
                    // 且 alpha 0.55 的近黑灰，几乎不可见。连接线必须始终可读，并区分三种状态。
                    Color c = nOn && mOn
                        ? new Color(0.98f, 0.84f, 0.42f, 1.00f)      // 两端已点亮：金
                        : (nOn || mOn ? new Color(0.80f, 0.68f, 0.36f, 1.00f)   // 一端已点亮
                                      : (nAvail || mAvail ? new Color(0.62f, 0.62f, 0.58f, 0.95f) // 通向可点亮节点
                                                          : new Color(0.44f, 0.44f, 0.50f, 0.90f))); // 其余：可读灰
                    if (!SegmentVisible(n, m, local, 60f))
                        continue;
                    // 线宽与缩放解耦：整树贴合视图下也保证 >= 1.7 设计像素（缩到 0.4 设计缩放仍可见）
                    float lw = Mathf.Max(1.7f, 2.2f * _treeZoom + 0.8f);
                    // 一律用**节点真实坐标**画直线：canonical 的 group 中心与节点坐标不同系，
                    // 旧实现按 orbit 公式重算弧线端点，实测 1457/2787 条连线因此悬空、根本不碰节点
                    // （导演 2026-09-11「看不出是线连起来的」根因）。
                    DrawLine(PoeTreeView.ScreenOf(new Vector2(n.x, n.y), _treePan, _treeZoom),
                        PoeTreeView.ScreenOf(new Vector2(m.x, m.y), _treePan, _treeZoom), c, lw);
                }
            }

            // 3) 节点（图标 = PoE 原图；框 = 官方 frame 图集三态）
            int hovered = -1;
            for (int i = 0; i < nodes.Length; i++)
            {
                PoeNode n = nodes[i];
                if (!PoeTreeView.Visible(n, _treePan, _treeZoom, local, 20f))
                    continue;
                Rect nr = PoeTreeView.NodeRect(n, _treePan, _treeZoom);
                NodeUiState st = LockedState(n, s.NodeState(i));
                bool yields = PassiveSupport.YieldsModifiers(s.NodeTruth(i).Effect);
                if (frameTex != null)
                {
                    Rect uv;
                    if (frameAtlas.TryRect(FrameSprite(n.Kind, st), out uv))
                    {
                        GUI.color = FrameTint(n.Kind, st, yields);
                        GUI.DrawTextureWithTexCoords(nr, frameTex, uv);
                        GUI.color = Color.white;
                    }
                }
                var icon = PoeTree.Icon(n.icon);
                if (icon != null)
                {
                    float s2 = nr.width * 0.66f;
                    GUI.color = IconTint(st, yields);
                    GUI.DrawTexture(new Rect(nr.x + (nr.width - s2) * 0.5f, nr.y + (nr.height - s2) * 0.5f, s2, s2), icon);
                    GUI.color = Color.white;
                }
                else if (frameTex == null)
                {
                    // 数据/图集双双缺失时的兜底：至少让结构可读（永不空白）
                    Color cc = st == NodeUiState.Allocated ? SlicePalette.NodeOn
                        : st == NodeUiState.Available ? SlicePalette.NodeAvail : SlicePalette.NodeLock;
                    Fill(nr, new Color(cc.r * 0.3f, cc.g * 0.3f, cc.b * 0.3f, 0.9f));
                }

                if (nr.Contains(TreePointer))
                    hovered = i;
            }

            // 名称：缩放到能看清时给 notable/keystone/mastery 打名字（PoE 同款行为）
            if (_treeZoom > 0.28f)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    PoeNode n = nodes[i];
                    if (n.Kind != PoeNodeKind.Notable && n.Kind != PoeNodeKind.Keystone && n.Kind != PoeNodeKind.Mastery)
                        continue;
                    if (!PoeTreeView.Visible(n, _treePan, _treeZoom, local, 20f))
                        continue;
                    Rect nr = PoeTreeView.NodeRect(n, _treePan, _treeZoom);
                    GUI.color = s.Allocated[i] ? SliceSkin.Gold : SlicePalette.Dim;
                    Label(new Rect(nr.x - 46f, nr.yMax, nr.width + 92f, 20f), n.name, _caption);
                    GUI.color = Color.white;
                }
            }

            if (hovered >= 0)
            {
                PoeNode n = nodes[hovered];
                RequestTip(NodeCard(s, hovered, n), TipPriPanel);
                if (ClickLocal(NodeClickRect(n)))
                {
                    string err;
                    if (s.TryAllocate(hovered, out err))
                    {
                        TreeToast("点亮 " + n.name, true);
                    }
                    else
                    {
                        // 导演 2026-09-11：点击不能"点了没反应"——拒绝必须就地、可见、带原因
                        if (err != null)
                            s.LastMessage = err;
                        TreeToast(err ?? "该节点当前不可点亮", false);
                    }
                }
            }

            DrawTreeToast(local);
        }

        /// <summary>树内就地提示（点击成功/被拒都在指针附近显示，不再只写状态条）。</summary>
        void TreeToast(string text, bool ok)
        {
            _treeToast = text;
            _treeToastOk = ok;
            _treeToastAt = TreePointer;
            _treeToastUntil = Time.unscaledTime + (ok ? 1.6f : 2.6f);
        }

        void DrawTreeToast(Rect local)
        {
            if (string.IsNullOrEmpty(_treeToast) || Time.unscaledTime >= _treeToastUntil)
                return;
            const float w = 420f, h = 30f;
            float x = Mathf.Clamp(_treeToastAt.x + 16f, local.x + 6f, Mathf.Max(local.x + 6f, local.xMax - w - 6f));
            float y = Mathf.Clamp(_treeToastAt.y - h - 8f, local.y + 6f, Mathf.Max(local.y + 6f, local.yMax - h - 6f));
            var r = new Rect(x, y, w, h);
            Fill(r, new Color(0.06f, 0.06f, 0.05f, 0.94f));
            Fill(new Rect(r.x, r.y, 3f, r.height), _treeToastOk ? SliceSkin.Gold : new Color(0.78f, 0.30f, 0.26f, 1f));
            GUI.color = _treeToastOk ? SliceSkin.Gold : new Color(0.93f, 0.72f, 0.68f, 1f);
            Label(new Rect(r.x + 10f, r.y + 5f, r.width - 16f, 20f), _treeToast, _small);
            GUI.color = Color.white;
        }

        /// <summary>把某节点摆到视口正中的平移量（验证视图与“跟随当前构筑”共用）。</summary>
        static Vector2 CentreOn(PoeNode n, Rect local, float zoom)
        {
            return new Vector2(local.x + local.width * 0.5f - n.x * zoom,
                local.y + local.height * 0.5f - n.y * zoom);
        }

        /// <summary>节点 tooltip：真实名称 + 当前状态（**放在正文第一行，不得被词条挤掉**）+ 真实词条（专精=全部可选效果）。
        /// 正文按行拆进 Body[]：Subtitle 槽只放得下一行，多行字符串在那里会被裁掉（route-only 的
        /// 「0 效果」披露曾经因此不可见）。</summary>
        static SliceTooltipModel.Card NodeCard(SliceSession s, int index, PoeNode n)
        {
            var lines = new System.Collections.Generic.List<string>();

            // S6P-WO-04A2 §20：可点/不可点读**通行**真值、"是否真生效"读**效果**真值，UI 不自己判定。
            NodeUiState st = LockedState(n, s.NodeState(index));
            string blocked = s.NodeBlockReason(index);
            bool yields = PassiveSupport.YieldsModifiers(s.NodeTruth(index).Effect);
            if (st == NodeUiState.Allocated)
            {
                if (blocked != null)
                    lines.Add("已点亮　·　" + blocked);
                else if (yields)
                    lines.Add("已点亮　·　点击无效果，可用 R 重构");
                else
                {
                    // route-only 的 disclosure 拆成两行：单行超过 tooltip 宽（BaseW 360）会被裁掉
                    lines.Add("已点亮（路径）　·　可用 R 重构");
                    lines.Add("该节点当前 0 游戏效果（引擎兑现不了它的承诺）");
                }
            }
            else if (st == NodeUiState.Available)
            {
                if (yields)
                    lines.Add("可点亮　·　消耗 1 天赋点");
                else
                {
                    lines.Add("可点亮（路径）　·　消耗 1 天赋点");
                    lines.Add("该节点当前无法兑现任何效果（0 效果）");
                }
            }
            else if (n.locked != 0)
                lines.Add("不可点　·　该类显著点只能由时光珠宝授予");
            else if (blocked != null)
                lines.Add("当前不可点亮　·　" + blocked);
            else
                lines.Add("未连接　·　需与已点亮节点相连");

            if (n.Kind == PoeNodeKind.Mastery)
            {
                if (string.IsNullOrEmpty(n.choices))
                    lines.Add("（无生效词条）");
                else
                {
                    lines.Add("可选效果（本轮尚未开放显式选择）：");
                    AppendLines(lines, n.choices);
                }
            }
            else if (string.IsNullOrEmpty(n.stats))
                lines.Add("（无词条）");
            else
                AppendLines(lines, n.stats);

            var card = SliceTooltipModel.TextCard(n.name + "　【" + KindName(n.Kind) + "】", null);
            card.Body = lines.ToArray();
            return card;
        }

        static void AppendLines(System.Collections.Generic.List<string> lines, string text)
        {
            int start = 0;
            while (start < text.Length)
            {
                int nl = text.IndexOf('\n', start);
                string line;
                if (nl < 0) { line = text.Substring(start); start = text.Length; }
                else { line = text.Substring(start, nl - start); start = nl + 1; }
                if (line.Length > 0)
                    lines.Add(line);
            }
        }

        static string KindName(PoeNodeKind k)
        {
            switch (k)
            {
                case PoeNodeKind.Notable: return "显著";
                case PoeNodeKind.Keystone: return "基石";
                case PoeNodeKind.Mastery: return "专精";
                case PoeNodeKind.Jewel: return "珠宝孔";
                case PoeNodeKind.Start: return "起点";
                default: return "小点";
            }
        }

        /// <summary>时光珠宝类节点（官方数据无连线）恒为不可点态，避免“看起来能点却点不动”。</summary>
        static NodeUiState LockedState(PoeNode n, NodeUiState st)
        {
            return n.locked != 0 && st != NodeUiState.Allocated ? NodeUiState.Locked : st;
        }

        static string FrameSprite(PoeNodeKind kind, NodeUiState st)
        {
            if (kind == PoeNodeKind.Start)
                return st == NodeUiState.Allocated ? "PSSkillFrameActive" : "PSSkillFrame";
            string baseName;
            switch (kind)
            {
                case PoeNodeKind.Keystone: baseName = "KeystoneFrame"; break;
                case PoeNodeKind.Notable: baseName = "NotableFrame"; break;
                case PoeNodeKind.Jewel: baseName = "JewelFrame"; break;
                default: baseName = "PSSkillFrame"; break;
            }
            if (baseName == "PSSkillFrame")
                return st == NodeUiState.Allocated ? "PSSkillFrameActive"
                    : (st == NodeUiState.Available ? "PSSkillFrameHighlighted" : "PSSkillFrame");
            return st == NodeUiState.Allocated ? baseName + "Allocated"
                : (st == NodeUiState.Available ? baseName + "CanAllocate" : baseName + "Unallocated");
        }

        static Color FrameTint(PoeNodeKind kind, NodeUiState st, bool yields)
        {
            if (st == NodeUiState.Allocated)
            {
                if (!yields)
                    return new Color(0.72f, 0.70f, 0.66f, 1f);   // 路径节点：已点亮但 0 效果，压暗以免当成有效点
                return kind == PoeNodeKind.Keystone ? new Color(1f, 0.86f, 0.45f, 1f) : Color.white;
            }
            if (st == NodeUiState.Available)
                return yields
                    ? new Color(1f, 0.93f, 0.66f, 1f)            // 可点亮且真生效：金亮（导演：一眼看出哪些能点）
                    : new Color(0.66f, 0.78f, 0.86f, 1f);        // 可点亮但 0 效果（route-only）：冷灰蓝，与有效点区分
            return new Color(0.40f, 0.40f, 0.45f, 1f);           // 当前不可用：明显压暗（旧的 0.52 灰分不出来）
        }

        static Color IconTint(NodeUiState st, bool yields)
        {
            if (st == NodeUiState.Allocated)
                return yields ? Color.white : new Color(0.78f, 0.78f, 0.74f, 1f);
            if (st == NodeUiState.Available)
                return yields ? new Color(1f, 0.97f, 0.82f, 1f) : new Color(0.72f, 0.82f, 0.90f, 1f);
            return new Color(0.38f, 0.39f, 0.43f, 0.85f);
        }

        /// <summary>组内局部坐标的点击判定（天赋树在 BeginGroup 里绘制，指针必须先换算到组内，
        /// 否则命中区整体偏掉视口原点——这正是旧背包网格踩过的坐标坑）。</summary>
        bool ClickLocal(Rect r)
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.MouseDown || e.button != 0)
                return false;
            if (!r.Contains(TreePointer))
                return false;
            e.Use();
            return true;
        }

        /// <summary>节点命中区（略放宽，便于密集视图下点击；组内局部坐标）。</summary>
        Rect NodeClickRect(PoeNode n)
        {
            Rect r = PoeTreeView.NodeRect(n, _treePan, _treeZoom);
            float pad = Mathf.Max(2f, r.width * 0.1f);
            return new Rect(r.x - pad, r.y - pad, r.width + 2f * pad, r.height + 2f * pad);
        }

        bool SegmentVisible(PoeNode a, PoeNode b, Rect local, float pad)
        {
            Vector2 pa = PoeTreeView.ScreenOf(new Vector2(a.x, a.y), _treePan, _treeZoom);
            Vector2 pb = PoeTreeView.ScreenOf(new Vector2(b.x, b.y), _treePan, _treeZoom);
            float minX = Mathf.Min(pa.x, pb.x) - pad, maxX = Mathf.Max(pa.x, pb.x) + pad;
            float minY = Mathf.Min(pa.y, pb.y) - pad, maxY = Mathf.Max(pa.y, pb.y) + pad;
            return maxX >= local.x && minX <= local.xMax && maxY >= local.y && minY <= local.yMax;
        }

        /// <summary>天赋树输入：滚轮锚点缩放、左键拖拽平移（点在空白处才是平移，点节点=加点）。</summary>
        void HandleTreeInput(Rect local)
        {
            Event e = Event.current;
            if (e == null)
                return;
            Vector2 lp = TreePointer;
            if (e.type == EventType.ScrollWheel && local.Contains(lp))
            {
                float nz = Mathf.Clamp(_treeZoom * (1f - e.delta.y * 0.06f), PoeTreeView.MinZoom, PoeTreeView.MaxZoom);
                _treePan = PoeTreeView.ZoomAround(_treePan, _treeZoom, nz, lp);
                _treeZoom = nz;
                e.Use();
                return;
            }
            if (e.type == EventType.MouseDown && e.button == 0 && local.Contains(lp))
            {
                _treeDrag = true;
                _treeDragLast = lp;
            }
            else if (_treeDrag && e.type == EventType.MouseDrag && e.button == 0)
            {
                _treePan += lp - _treeDragLast;
                _treeDragLast = lp;
                e.Use();
            }
            else if (_treeDrag && e.type == EventType.MouseUp && e.button == 0)
            {
                _treeDrag = false;
            }
        }

        void DrawMap(SliceSession s, ArenaSim sim)
        {
            _panel = new Rect(12, 140, 480, 300);
            PanelChrome(_panel, "地图 灰烬庭院  ·  " + s.StatusCopy);
            Label(new Rect(_panel.x + 16, _panel.y + 32, 448, 18), "进图前勾选词缀，稳定度与收益立即更新。", _small);
            float y = _panel.y + 54;
            for (int i = 0; i < MapAffixCatalog.All.Length; i++)
            {
                MapAffixDef a = MapAffixCatalog.All[i];
                Rect row = new Rect(_panel.x + 16, y, 448, 36);
                Fill(row, s.MapAffixOn[i] ? new Color(0.22f, 0.18f, 0.10f, 1f) : new Color(0.14f, 0.14f, 0.16f, 1f));
                Fill(new Rect(row.x + 10, row.y + 10, 16, 16), s.MapAffixOn[i] ? SlicePalette.PanelEdge : new Color(0.25f, 0.25f, 0.27f, 1f));
                if (s.MapAffixOn[i])
                    Label(new Rect(row.x + 10, row.y + 8, 16, 18), "✓", _center);
                Label(new Rect(row.x + 36, row.y + 2, 400, 16), a.Name + "   稳定 -" + a.StabilityCost + "   收益 +" + a.RewardAdd.ToString("0.00"), _body);
                Label(new Rect(row.x + 36, row.y + 18, 400, 14), a.Desc, _small);
                if (Click(row))
                    s.ToggleMapAffix(i);
                y += 40;
            }

            Label(new Rect(_panel.x + 16, y + 4, 448, 22),
                "稳定度 " + s.Stability + "     收益 x" + s.RewardMultiplier.ToString("0.00"), _title);

            Rect btn = new Rect(_panel.x + 16, _panel.yMax - 48, 448, 32);
            if (s.OnMap)
            {
                if (NavBtn(btn, "出图", true))
                {
                    s.ExitMap(sim, false);
                    s.Panel = SlicePanel.None;
                }
            }
            else if (NavBtn(btn, "进入地图", false))
            {
                string err;
                if (!s.TryEnterMap(sim, out err) && err != null)
                    s.LastMessage = err;
                else
                    s.Panel = SlicePanel.None;
            }
        }

        void DrawCraft(SliceSession s)
        {
            _panel = SliceDrawerLayout.CraftPanel(Dw());
            PanelChrome(_panel, "制作  ·  " + s.StatusCopy);
            Label(new Rect(_panel.x + 16, _panel.y + 32, 528, 18),
                "废料 " + s.Scrap + "    蚀刻剂 " + s.Etching, _title);

            _craftResult = new Rect(_panel.x + 16, _panel.y + 56, 528, 96);
            Fill(_craftResult, new Color(0.13f, 0.13f, 0.15f, 1f));
            if (s.SelectedInv >= 0 && s.SelectedInv < s.InventoryCount)
            {
                ItemInstance it = s.Inventory[s.SelectedInv];
                Color edge = it.Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Ordinary;
                Bar(_craftResult.x, _craftResult.y, 5, _craftResult.height, edge);
                DrawItemBody(it, new Rect(_craftResult.x + 12, _craftResult.y + 6, _craftResult.width - 20, _craftResult.height - 10));
                if (_craftResult.Contains(Pointer))
                    RequestTip(SliceTooltipModel.ItemCard(it, s), TipPriPanel);
            }
            else
                Label(new Rect(_craftResult.x + 12, _craftResult.y + 12, 500, 40), "在角色面板点选一件装备。", _body);

            if (NavBtn(new Rect(_panel.x + 16, _panel.y + 160, 250, 32), "随机制作   废料 " + s.Scrap, false))
            {
                string err;
                if (!s.TryRandomCraft(s.SelectedInv, out err) && err != null)
                    s.LastMessage = err;
            }

            if (NavBtn(new Rect(_panel.x + 278, _panel.y + 160, 250, 32), "定向制作   蚀刻剂 " + s.Etching, false))
            {
                string err;
                if (!s.TryDirectedCraft(s.SelectedInv, (AffixId)s.CraftAffixPick, out err) && err != null)
                    s.LastMessage = err;
            }

            Label(new Rect(_panel.x + 16, _panel.y + 200, 528, 16), "定向写入：", _small);
            for (int i = 0; i < AffixCatalog.Count; i++)
            {
                int col = i % 5;
                int row = i / 5;
                Rect b = new Rect(_panel.x + 16 + col * 106, _panel.y + 220 + row * 28, 100, 24);
                AffixDef def = AffixCatalog.Get((AffixId)i);
                if (NavBtn(b, def.Name, s.CraftAffixPick == i))
                    s.CraftAffixPick = i;
            }
        }

        void PlaceSupport(SliceSession s, SkillId skill, int index, SupportId id)
        {
            string err;
            if (!s.TrySetSupport(skill, index, id, out err) && err != null)
                s.LastMessage = err;
            _picked = SupportId.None;
            _drag = SupportId.None;
            _dragging = false;
        }

        void EndDragIfNeeded(SliceSession s)
        {
            Event e = Event.current;
            if (e == null)
                return;
            if (_drag != SupportId.None && e.type == EventType.MouseDrag)
            {
                if ((Pointer - _dragStart).sqrMagnitude > 16f)
                    _dragging = true;
            }

            if (e.type == EventType.MouseUp)
            {
                if (_dragging)
                {
                    _drag = SupportId.None;
                    _dragging = false;
                }
            }
        }

        void DrawDragGhost()
        {
            if (!_dragging || _drag == SupportId.None)
                return;
            Vector2 m = Pointer;
            Rect g = new Rect(m.x - 48, m.y - 12, 96, 24);
            Fill(g, new Color(0.20f, 0.18f, 0.10f, 0.9f));
            Label(g, SupportCatalog.Get(_drag).Name, _center);
        }

        /// <summary>R3 统一 Tooltip 渲染（单一出口）：SliceSkin 石底金边卡 + Rarity 着色标题 + 对比区 + 操作提示；
        /// 位置=SliceTooltipLayout（pointer 右下/右溢翻左/下溢上翻/钳视口）。不可交互、不新增世界吞区。</summary>
        void DrawTooltip()
        {
            if (!_tipSet)
                return;
            SliceTooltipModel.Card card = _tipCard;
            float w = SliceTooltipLayout.BaseW;
            const float pad = 10f;
            const float titleH = 20f, lineH = 16f, smallH = 15f;
            // S5U-WO-03：层级分隔线（标题块|正文|对比区）——渲染层视觉，不动模型
            bool div1 = card.Body != null && card.Body.Length > 0 &&
                (!string.IsNullOrEmpty(card.Title) || !string.IsNullOrEmpty(card.Subtitle) || !string.IsNullOrEmpty(card.Badge));
            bool div2 = card.Body != null && card.Body.Length > 0 &&
                (!string.IsNullOrEmpty(card.ContextTitle) || (card.Context != null && card.Context.Length > 0));
            float h = pad * 2f;
            if (div1) h += 8f;
            if (div2) h += 8f;
            if (!string.IsNullOrEmpty(card.Title)) h += titleH;
            if (!string.IsNullOrEmpty(card.Subtitle)) h += lineH;
            if (!string.IsNullOrEmpty(card.Badge)) h += lineH;
            if (card.Body != null) h += card.Body.Length * smallH;
            if (!string.IsNullOrEmpty(card.ContextTitle)) h += 18f;
            if (card.Context != null) h += card.Context.Length * smallH;
            if (!string.IsNullOrEmpty(card.Footer)) h += lineH;

            Rect r = SliceTooltipLayout.Place(Pointer, w, h, Dw(), Dh());
            PanelBg(r);
            float y = r.y + pad - 2f;
            float ix = r.x + 12f, iw = w - 24f;
            if (!string.IsNullOrEmpty(card.Title))
            {
                Color old = GUI.color;
                GUI.color = card.Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Text;
                Label(new Rect(ix, y, iw, titleH), card.Title, _title);
                GUI.color = old;
                y += titleH;
            }
            if (!string.IsNullOrEmpty(card.Subtitle))
            {
                Label(new Rect(ix, y, iw, lineH), card.Subtitle, _small);
                y += lineH;
            }
            if (!string.IsNullOrEmpty(card.Badge))
            {
                Color old = GUI.color;
                GUI.color = SlicePalette.Notable;
                Label(new Rect(ix, y, iw, lineH), card.Badge, _small);
                GUI.color = old;
                y += lineH;
            }
            if (div1)
            {
                Fill(new Rect(ix, y + 1, iw, 2), SlicePalette.PanelEdge);
                y += 8f;
            }
            if (card.Body != null)
            {
                for (int i = 0; i < card.Body.Length; i++)
                {
                    Label(new Rect(ix, y, iw, smallH), card.Body[i], _small);
                    y += smallH;
                }
            }
            if (div2)
            {
                Fill(new Rect(ix, y + 1, iw, 2), SlicePalette.PanelEdge);
                y += 8f;
            }
            if (!string.IsNullOrEmpty(card.ContextTitle))
            {
                Color old = GUI.color;
                GUI.color = SlicePalette.PanelEdge;
                Label(new Rect(ix, y, iw, 18f), card.ContextTitle, _small);
                GUI.color = old;
                y += 18f;
                if (card.Context != null)
                {
                    for (int i = 0; i < card.Context.Length; i++)
                    {
                        Label(new Rect(ix, y, iw, smallH), card.Context[i], _small);
                        y += smallH;
                    }
                }
            }
            if (!string.IsNullOrEmpty(card.Footer))
                Label(new Rect(ix, y, iw, lineH), card.Footer, _small);
        }

        void DrawBanner(string text, Color c)
        {
            // 背包面板常驻贴右（PanelW）：横幅只在世界区内居中，不压面板内容
            const float w = 440f;
            float worldW = Mathf.Max(w, Dw() - SliceDrawerLayout.PanelW);
            Rect r = new Rect((worldW - w) * 0.5f, 86, w, 36);
            Fill(r, c);
            Label(r, text, _center);
        }

        void PanelChrome(Rect r, string title)
        {
            PanelBg(r);
            // S5U-F1 V-04/V-07：主导页面标题 20 级（页面=dominant；HUD/导航/状态=subordinate）
            Label(new Rect(r.x + 16, r.y + 8, r.width - 32, 26), title, _pageTitle);
        }

        /// <summary>S5U-WO-02 双球：底部按比例填充（Group 裁剪，资源 mechanics 不变）+ 暗底衬 + 装饰铁环金钉框
        /// + 紧凑数值（SliceHudFormat 通用格式化；底层值零改动）+ 悬停精确值 tooltip。</summary>
        void DrawOrb(Rect r, float u, Color c, float cur, float max, bool isLife)
        {
            if (u < 0f) u = 0f;
            if (u > 1f) u = 1f;
            // 暗底衬（世界对比 + 空态可读）
            GUI.color = new Color(0.07f, 0.06f, 0.05f, 0.88f);
            GUI.DrawTexture(r, SliceSkin.OrbFill);
            GUI.color = Color.white;
            GUI.BeginGroup(r);
            float d = r.width;
            float fh = Mathf.Round(d * u);
            if (fh > 0f)
            {
                GUI.BeginGroup(new Rect(0f, d - fh, d, fh));
                Color old = GUI.color;
                GUI.color = c;
                GUI.DrawTexture(new Rect(0f, -(d - fh), d, d), SliceSkin.OrbFill);
                GUI.color = old;
                GUI.EndGroup();
            }
            GUI.EndGroup();
            GUI.color = new Color(1f, 1f, 1f, 0.9f);
            GUI.DrawTexture(r, SliceHudIcons.GlobeSheen); // S5U-F1 V-09：内圈暗缘+左上高光（实体感）
            GUI.color = Color.white;
            GUI.DrawTexture(r, SliceHudIcons.GlobeFrame);
            var ariaRing = SliceAria.OrbRing; // 导演输入（Aria）：金环覆层；缺失=程序化球框独立成立
            if (ariaRing != null)
                GUI.DrawTexture(r, ariaRing);
            Label(new Rect(r.x, r.y + r.height * 0.5f - 9f, r.width, 18f),
                SliceHudFormat.Compact(cur) + "/" + SliceHudFormat.Compact(max), _orbText);
            if (r.Contains(Pointer))
                RequestTip(SliceTooltipModel.TextCard(isLife ? "生命" : "法力",
                    "当前 " + Mathf.CeilToInt(cur) + " / " + Mathf.CeilToInt(max)), TipPriBottomBar);
        }

        /// <summary>点击效：金色高亮闪（约 0.16s 衰减），命中元素上叠加绘制。</summary>
        void ClickFlash(Rect r)
        {
            _flashRect = r;
            _flashUntil = Time.realtimeSinceStartup + 0.16f;
        }

        void DrawFlash()
        {
            float remain = _flashUntil - Time.realtimeSinceStartup;
            if (remain <= 0f)
                return;
            float k = remain / 0.16f;
            Fill(_flashRect, new Color(1f, 0.87f, 0.45f, 0.30f * k));
        }

        /// <summary>石质面板底（SliceSkin 九宫格；替代旧纯色 Fill+边条）。</summary>
        void PanelBg(Rect r)
        {
            GUI.Box(r, GUIContent.none, _panelBg);
        }

        bool NavBtn(Rect r, string text, bool on)
        {
            Event e = Event.current;
            bool hover = e != null && r.Contains(Pointer);
            bool press = hover && e != null && e.type == EventType.MouseDown && e.button == 0;
            GUI.Box(r, GUIContent.none, on ? _slotSel : (press ? _slotP : (hover ? _slotSubtleH : _slotSubtleN)));
            Label(r, text, _center);
            bool clicked = Click(r);
            if (clicked)
                ClickFlash(r);
            return clicked;
        }

        void Fill(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _white);
            GUI.color = old;
        }

        void Bar(float x, float y, float w, float h, Color c)
        {
            Fill(new Rect(x, y, w, h), c);
        }

        void Label(Rect r, string text, GUIStyle style)
        {
            Color old = GUI.color;
            GUI.color = Color.white;
            GUI.Label(r, text, style);
            GUI.color = old;
        }

        void Clipped(Rect r, string text, GUIStyle style)
        {
            Label(r, text, style);
            Vector2 size = style.CalcSize(new GUIContent(text));
            bool trunc = size.x > r.width - 2f || text.IndexOf('\n') < 0 && size.x > r.width;
            if (trunc && r.Contains(Pointer))
                RequestTip(SliceTooltipModel.TextCard(null, text), TipPriTopNav);
        }

        /// <summary>R3 单一出口：所有正式 Tooltip 经此请求；同优先级先到先得，高优先级覆盖（工作令 二十二）。</summary>
        void RequestTip(SliceTooltipModel.Card card, int pri)
        {
            if (!_tipSet || pri >= _tipPri)
            {
                _tipCard = card;
                _tipPri = pri;
                _tipSet = true;
            }
        }

        bool Click(Rect r)
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.MouseDown || e.button != 0)
                return false;
            if (!r.Contains(Pointer))
                return false;
            e.Use();
            return true;
        }

        static bool IsLinked(SliceSession s, SupportId id)
        {
            return Has(s.QSupports, id) || Has(s.WSupports, id) || Has(s.ESupports, id);
        }

        static bool Has(SupportId[] arr, SupportId id)
        {
            if (arr == null)
                return false;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == id)
                    return true;
            }

            return false;
        }

        static string ItemAffixSummary(ItemInstance it)
        {
            if (it.AffixCount <= 0)
                return "";
            string s = SliceSession.AffixLine(it, 0);
            for (int i = 1; i < it.AffixCount; i++)
                s += " · " + SliceSession.AffixLine(it, i);
            return s;
        }

        void DrawLine(Vector2 a, Vector2 b, Color c, float w)
        {
            Vector2 d = b - a;
            float len = d.magnitude;
            if (len < 1f)
                return;
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            Matrix4x4 old = GUI.matrix;
            GUIUtility.RotateAroundPivot(ang, a);
            Fill(new Rect(a.x, a.y - w * 0.5f, len, w), c);
            GUI.matrix = old;
        }
    }
}
