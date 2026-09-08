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
        GUIStyle _clip;
        GUIStyle _panelBg;
        GUIStyle _slotN;
        GUIStyle _slotH;
        GUIStyle _slotP;
        GUIStyle _slotSel;
        GUIStyle _orbText;
        Rect _flashRect;
        float _flashUntil;
        /// <summary>全局等比缩放（设计空间=1920x1080 基准，方案页布局）；设计坐标=屏幕坐标/_scale。</summary>
        float _scale = 1f;
        bool _styles;

        Vector2 _invScroll;
        string _tooltip;
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

        static readonly Vector2[] TreePos =
        {
            new Vector2(0.50f, 0.06f),
            new Vector2(0.20f, 0.26f),
            new Vector2(0.50f, 0.26f),
            new Vector2(0.80f, 0.26f),
            new Vector2(0.10f, 0.48f),
            new Vector2(0.50f, 0.48f),
            new Vector2(0.30f, 0.48f),
            new Vector2(0.08f, 0.72f),
            new Vector2(0.70f, 0.48f),
            new Vector2(0.50f, 0.72f),
            new Vector2(0.32f, 0.72f),
            new Vector2(0.16f, 0.92f),
            new Vector2(0.70f, 0.72f),
            new Vector2(0.88f, 0.72f),
            new Vector2(0.90f, 0.48f),
            new Vector2(0.90f, 0.92f)
        };

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
                _tooltip = null;

                DrawTop(s);
                DrawNav(s, sim);
                DrawSkillHud(s);
                DrawSupportTray(s);
                DrawDrawer(s);

                if (s.Panel == SlicePanel.Build)
                    DrawBuild(s, sim);
                else if (s.Panel == SlicePanel.Map)
                    DrawMap(s, sim);
                else if (s.Panel == SlicePanel.Craft)
                    DrawCraft(s);
                else
                    _panel = default;

                if (s.State == MapState.Dead)
                    DrawBanner(SliceCopy.DeadRespec, new Color(0.55f, 0.12f, 0.10f, 0.92f));
                else if (s.State == MapState.Cleared)
                    DrawBanner(SliceCopy.Cleared, new Color(0.12f, 0.38f, 0.18f, 0.92f));

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

        public void HandleKeys(SliceSession s, ArenaSim sim)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                s.Panel = s.Panel == SlicePanel.Build ? SlicePanel.None : SlicePanel.Build;
            if (Input.GetKeyDown(KeyCode.F6))
                s.Panel = s.Panel == SlicePanel.Map ? SlicePanel.None : SlicePanel.Map;
            if (Input.GetKeyDown(KeyCode.F8))
                s.Panel = s.Panel == SlicePanel.Craft ? SlicePanel.None : SlicePanel.Craft;
            if (Input.GetKeyDown(KeyCode.Escape))
                s.Panel = SlicePanel.None;
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
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Text }
            };
            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Text }
            };
            _small = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                clipping = TextClipping.Clip,
                normal = { textColor = SlicePalette.Dim }
            };
            _center = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                wordWrap = true,
                normal = { textColor = SlicePalette.Text }
            };
            _clip = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = SlicePalette.Text }
            };
            // Phase3 换皮（SliceSkin 程序化石质贴图；border 必须与贴图九宫格边框一致）
            RectOffset pb = new RectOffset(SliceSkin.PanelBorder, SliceSkin.PanelBorder, SliceSkin.PanelBorder, SliceSkin.PanelBorder);
            RectOffset sb = new RectOffset(SliceSkin.SlotBorder, SliceSkin.SlotBorder, SliceSkin.SlotBorder, SliceSkin.SlotBorder);
            _panelBg = new GUIStyle { border = pb, normal = { background = SliceSkin.Panel } };
            _slotN = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotNormal } };
            _slotH = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotHover } };
            _slotP = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotPress } };
            _slotSel = new GUIStyle { border = sb, normal = { background = SliceSkin.SlotSelected } };
            _orbText = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                normal = { textColor = SliceSkin.TextCream }
            };
            _styles = true;
        }

        void DrawTop(SliceSession s)
        {
            _topBar = new Rect(12, 10, 560, 84);
            PanelBg(_topBar);
            Label(new Rect(_topBar.x + 16, _topBar.y + 14, 360, 20), s.StatusCopy, _title);
            Label(new Rect(_topBar.x + 16, _topBar.y + 38, 528, 16),
                "稳定度 " + s.Stability + "    收益 x" + s.RewardMultiplier.ToString("0.00") +
                "    废料 " + s.Scrap + "    蚀刻剂 " + s.Etching +
                "    天赋点 " + s.Unspent + "/" + s.TotalPoints, _small);
            Clipped(new Rect(_topBar.x + 16, _topBar.y + 58, 528, 20),
                s.LastMessage + (string.IsNullOrEmpty(s.LastLoot) ? "" : "  ·  " + s.LastLoot),
                _small);
        }

        void DrawNav(SliceSession s, ArenaSim sim)
        {
            _nav = new Rect(Dw() - 296, 10, 284, 44);
            PanelBg(_nav);
            float x = _nav.x + 6;
            if (NavBtn(new Rect(x, _nav.y + 8, 88, 28), "角色", s.Panel == SlicePanel.Build))
                s.Panel = s.Panel == SlicePanel.Build ? SlicePanel.None : SlicePanel.Build;
            if (NavBtn(new Rect(x + 92, _nav.y + 8, 88, 28), "地图", s.Panel == SlicePanel.Map))
                s.Panel = s.Panel == SlicePanel.Map ? SlicePanel.None : SlicePanel.Map;
            if (NavBtn(new Rect(x + 184, _nav.y + 8, 88, 28), "制作", s.Panel == SlicePanel.Craft))
                s.Panel = s.Panel == SlicePanel.Craft ? SlicePanel.None : SlicePanel.Craft;
        }

        void DrawSkillHud(SliceSession s)
        {
            const float cellW = 178f;
            const float orbD = 92f;
            const float gemW = 84f;
            const float gemH = 44f;
            const float gemGap = 6f;
            float cells = cellW * 3f + 16f;
            float stripW = 4f * gemW + 3f * gemGap;
            float total = 10f + orbD * 2f + 10f + cells + 14f + stripW + 10f;
            float x0 = Mathf.Max(12f, (Dw() - total) * 0.5f);
            _skillHud = new Rect(x0, Dh() - 118, total, 104);
            PanelBg(_skillHud);
            float life = s.MaxLife > 0f ? s.Life / s.MaxLife : 0f;
            float mana = s.MaxMana > 0f ? s.Mana / s.MaxMana : 0f;
            DrawOrb(new Rect(x0 + 10f, _skillHud.y + 6f, orbD, orbD), life, SlicePalette.Life, s.Life, s.MaxLife);
            DrawOrb(new Rect(x0 + 10f + orbD + 10f, _skillHud.y + 6f, orbD, orbD), mana, SlicePalette.Mana, s.Mana, s.MaxMana);
            float cx = x0 + 10f + orbD * 2f + 10f;
            DrawSkillCell(s, SkillId.Melee, new Rect(cx + 8f, _skillHud.y + 9f, cellW - 8f, 86f));
            DrawSkillCell(s, SkillId.Projectile, new Rect(cx + cellW + 12f, _skillHud.y + 9f, cellW - 8f, 86f));
            DrawSkillCell(s, SkillId.Area, new Rect(cx + cellW * 2f + 16f, _skillHud.y + 9f, cellW - 8f, 86f));
            _tray = new Rect(x0 + 10f + orbD * 2f + 10f + cells + 14f, _skillHud.y + 5f, stripW, 94f);
            DrawSupportTray(s);
        }

        void DrawSkillCell(SliceSession s, SkillId skill, Rect r)
        {
            bool sel = s.SelectedSkill == skill;
            Event e = Event.current;
            bool hover = e != null && r.Contains(e.mousePosition);
            bool press = hover && e != null && e.type == EventType.MouseDown && e.button == 0;
            GUI.Box(r, GUIContent.none, press ? _slotP : (sel ? _slotSel : (hover ? _slotH : _slotN)));
            if (Click(new Rect(r.x, r.y, r.width, 28)))
            {
                s.SelectedSkill = skill;
                ClickFlash(r);
            }

            string name = SliceSession.SkillDisplayName(skill);
            string key = SliceSession.SkillHotkey(skill);
            Label(new Rect(r.x + 10, r.y + 4, r.width - 36, 22), name, _title);
            Label(new Rect(r.x + r.width - 22, r.y + 4, 20, 20), key, _small);

            SupportId[] arr = s.SupportsOf(skill);
            int cap = s.SupportCapacity(skill);
            if (arr == null)
                return;
            float sx = r.x + 10;
            for (int i = 0; i < arr.Length; i++)
            {
                Rect sock = new Rect(sx + i * 80, r.y + 32, 76, 46);
                DrawSocket(s, skill, i, cap, arr[i], sock);
            }
        }

        void DrawSocket(SliceSession s, SkillId skill, int index, int cap, SupportId filled, Rect r)
        {
            bool closed = index >= cap;
            Event e = Event.current;
            bool hover = !closed && e != null && r.Contains(e.mousePosition);
            Color old = GUI.color;
            if (closed)
                GUI.color = new Color(0.55f, 0.55f, 0.58f, 1f);
            GUI.Box(r, GUIContent.none, hover ? _slotH : _slotN);
            GUI.color = old;
            string text;
            string tip;
            if (closed)
            {
                text = "无孔";
                tip = "该技能装备孔不足";
            }
            else if (filled == SupportId.None)
            {
                text = "空";
                tip = "拖入或点击辅助";
            }
            else
            {
                SupportDef def = SupportCatalog.Get(filled);
                text = def.Name;
                tip = def.Name + "  " + def.Desc;
            }

            Clipped(new Rect(r.x + 4, r.y + 6, r.width - 8, 34), text, _center, tip);
            if (closed)
                return;
            HandleSocketInput(s, skill, index, filled, r);
        }

        void HandleSocketInput(SliceSession s, SkillId skill, int index, SupportId filled, Rect r)
        {
            Event e = Event.current;
            if (e == null)
                return;
            bool over = r.Contains(e.mousePosition);
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

        void DrawSupportTray(SliceSession s)
        {
            // Proposal Round1：辅助 tray 并入底栏右侧（_tray 由 DrawSkillHud 设定）；拾取/拖拽输入流不变
            const float gw = 84f;
            const float gh = 44f;
            const float gap = 6f;
            const int cols = 4;
            for (int i = 1; i <= SupportCatalog.Count; i++)
            {
                int idx0 = i - 1;
                Rect g = new Rect(_tray.x + (idx0 % cols) * (gw + gap), _tray.y + (idx0 / cols) * (gh + gap), gw, gh);
                DrawGem(s, (SupportId)i, g);
            }
        }

        void DrawGem(SliceSession s, SupportId id, Rect r)
        {
            SupportDef def = SupportCatalog.Get(id);
            bool used = IsLinked(s, id);
            bool pick = _picked == id;
            Event e = Event.current;
            bool hover = e != null && r.Contains(e.mousePosition);
            GUI.Box(r, GUIContent.none, pick ? _slotSel : (hover ? _slotH : _slotN));
            if (def.ChangesMechanism)
                Bar(r.x, r.y, 4, r.height, SlicePalette.Cinder);
            string label = def.Name;
            Clipped(new Rect(r.x + 8, r.y + 12, r.width - 10, r.height - 14), used ? label + "*" : label, _clip, def.Name + "  " + def.Desc);
            e = Event.current;
            if (e == null || !r.Contains(e.mousePosition))
                return;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                _picked = id;
                _drag = id;
                _dragging = false;
                _dragStart = e.mousePosition;
                ClickFlash(r);
                e.Use();
            }
        }

        /// <summary>R2 右侧装备抽屉：常驻列=标题+2×2 迷你槽+Build/Craft tab。单渲染器原则：Build/Craft 内容在列左侧面板迁移呈现（move, not duplicate），列永远可见。</summary>
        void DrawDrawer(SliceSession s)
        {
            float dw = Dw();
            _drawer = SliceDrawerLayout.Column(dw);
            PanelBg(_drawer);
            Label(new Rect(_drawer.x + 8, _drawer.y + 6, _drawer.width - 16, 18), "装备", _small);
            DrawMiniSlot(s, EquipSlot.Weapon, SliceDrawerLayout.Slot(0, dw));
            DrawMiniSlot(s, EquipSlot.Body, SliceDrawerLayout.Slot(1, dw));
            DrawMiniSlot(s, EquipSlot.Helmet, SliceDrawerLayout.Slot(2, dw));
            DrawMiniSlot(s, EquipSlot.Boots, SliceDrawerLayout.Slot(3, dw));
            if (NavBtn(SliceDrawerLayout.Tab(0, dw), "角色", s.Panel == SlicePanel.Build))
                s.Panel = s.Panel == SlicePanel.Build ? SlicePanel.None : SlicePanel.Build;
            if (NavBtn(SliceDrawerLayout.Tab(1, dw), "制作", s.Panel == SlicePanel.Craft))
                s.Panel = s.Panel == SlicePanel.Craft ? SlicePanel.None : SlicePanel.Craft;
        }

        /// <summary>迷你装备槽：只读现有 canonical 装备状态（EquipSlot/Inventory），点击=打开 Build 面板；无新图标资源（文字+程序化槽位皮肤）。</summary>
        void DrawMiniSlot(SliceSession s, EquipSlot slot, Rect r)
        {
            Event e = Event.current;
            bool hover = e != null && r.Contains(e.mousePosition);
            bool press = hover && e != null && e.type == EventType.MouseDown && e.button == 0;
            GUI.Box(r, GUIContent.none, press ? _slotP : (hover ? _slotH : _slotN));
            int idx = s.Equipped[(int)slot];
            bool has = idx >= 0 && idx < s.InventoryCount;
            Label(new Rect(r.x + 8, r.y + 6, r.width - 14, 16), SliceSession.SlotName(slot), _small);
            if (has)
            {
                ItemInstance it = s.Inventory[idx];
                GUI.color = it.Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Ordinary;
                Clipped(new Rect(r.x + 8, r.y + 24, r.width - 14, 18),
                    SliceSession.CleanBaseName(it.BaseName), _clip, SliceSession.DescribeItem(it));
                GUI.color = Color.white;
            }
            else
            {
                Label(new Rect(r.x + 8, r.y + 24, r.width - 14, 18), "空", _small);
            }
            if (Click(r))
            {
                if (s.Panel != SlicePanel.Build)
                    s.Panel = SlicePanel.Build;
                ClickFlash(r);
            }
        }

        void DrawBuild(SliceSession s, ArenaSim sim)
        {
            _panel = SliceDrawerLayout.BuildPanel(Dw());
            PanelChrome(_panel, "角色  ·  " + s.StatusCopy);
            DrawGearRow(s, new Rect(_panel.x + 12, _panel.y + 32, 736, 118));
            DrawInventory(s, new Rect(_panel.x + 12, _panel.y + 154, 360, 260));
            DrawTree(s, new Rect(_panel.x + 380, _panel.y + 154, 368, 260));
        }

        void DrawGearRow(SliceSession s, Rect r)
        {
            float w = (r.width - 18) / 4f;
            DrawSlotCard(s, EquipSlot.Weapon, new Rect(r.x, r.y, w, r.height));
            DrawSlotCard(s, EquipSlot.Body, new Rect(r.x + w + 6, r.y, w, r.height));
            DrawSlotCard(s, EquipSlot.Helmet, new Rect(r.x + (w + 6) * 2, r.y, w, r.height));
            DrawSlotCard(s, EquipSlot.Boots, new Rect(r.x + (w + 6) * 3, r.y, w, r.height));
        }

        void DrawSlotCard(SliceSession s, EquipSlot slot, Rect r)
        {
            int idx = s.Equipped[(int)slot];
            bool has = idx >= 0 && idx < s.InventoryCount;
            Color edge = SlicePalette.Ordinary;
            if (has)
                edge = s.Inventory[idx].Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Ordinary;
            Fill(r, new Color(0.13f, 0.13f, 0.15f, 1f));
            Bar(r.x, r.y, 5, r.height, edge);
            Label(new Rect(r.x + 10, r.y + 4, r.width - 14, 16), SliceSession.SlotName(slot), _small);
            if (!has)
            {
                Label(new Rect(r.x + 10, r.y + 24, r.width - 14, 20), "空", _body);
                return;
            }

            DrawItemBody(s.Inventory[idx], new Rect(r.x + 10, r.y + 20, r.width - 16, r.height - 24));
            if (Click(r))
                s.SelectedInv = idx;
        }

        void DrawItemBody(ItemInstance it, Rect r)
        {
            Color rc = it.Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Ordinary;
            GUI.color = rc;
            Label(new Rect(r.x, r.y, r.width, 16), SliceSession.RarityWord(it.Rarity), _small);
            GUI.color = Color.white;
            Clipped(new Rect(r.x, r.y + 14, r.width, 16), SliceSession.CleanBaseName(it.BaseName) + "  " + it.SocketCount + "孔", _body);
            float y = r.y + 32;
            for (int i = 0; i < it.AffixCount && y < r.yMax - 12; i++)
            {
                Clipped(new Rect(r.x, y, r.width, 14), SliceSession.AffixLine(it, i), _small);
                y += 14;
            }
        }

        void DrawInventory(SliceSession s, Rect r)
        {
            Fill(r, new Color(0.11f, 0.11f, 0.13f, 1f));
            Label(new Rect(r.x + 8, r.y + 4, r.width - 16, 18), "背包（点击装备）", _small);
            Rect view = new Rect(r.x + 6, r.y + 24, r.width - 12, r.height - 30);
            _invScroll = GUI.BeginScrollView(view, _invScroll, new Rect(0, 0, view.width - 16, s.InventoryCount * 46f));
            for (int i = 0; i < s.InventoryCount; i++)
            {
                Rect row = new Rect(0, i * 46f, view.width - 18, 44);
                bool sel = i == s.SelectedInv;
                ItemInstance it = s.Inventory[i];
                Color edge = it.Rarity == Rarity.Rare ? SlicePalette.Rare : SlicePalette.Ordinary;
                Fill(row, sel ? new Color(0.20f, 0.19f, 0.14f, 1f) : new Color(0.14f, 0.14f, 0.16f, 1f));
                Bar(row.x, row.y, 4, row.height, edge);
                Clipped(new Rect(row.x + 10, row.y + 2, row.width - 14, 16),
                    SliceSession.RarityWord(it.Rarity) + "  " + SliceSession.CleanBaseName(it.BaseName), _clip);
                Clipped(new Rect(row.x + 10, row.y + 18, row.width - 14, 22),
                    ItemAffixSummary(it), _small, SliceSession.DescribeItem(it));
                if (GUI.Button(row, GUIContent.none, GUIStyle.none))
                {
                    s.SelectedInv = i;
                    string err;
                    if (!s.TryEquip(i, out err) && err != null)
                        s.LastMessage = err;
                }
            }

            GUI.EndScrollView();
        }

        void DrawTree(SliceSession s, Rect r)
        {
            Fill(r, new Color(0.11f, 0.11f, 0.13f, 1f));
            Label(new Rect(r.x + 8, r.y + 4, r.width - 16, 16), "天赋  已点金 / 可点绿 / 锁住灰", _small);
            Rect area = new Rect(r.x + 6, r.y + 22, r.width - 12, r.height - 28);
            for (int i = 0; i < PassiveCatalog.Count; i++)
            {
                int[] links = PassiveCatalog.Get(i).Links;
                if (links == null)
                    continue;
                Vector2 a = NodeCenter(area, i);
                for (int k = 0; k < links.Length; k++)
                {
                    if (links[k] <= i)
                        continue;
                    DrawLine(a, NodeCenter(area, links[k]), new Color(0.35f, 0.33f, 0.28f, 1f), 2f);
                }
            }

            for (int i = 0; i < PassiveCatalog.Count; i++)
            {
                PassiveNode n = PassiveCatalog.Get(i);
                Rect nr = NodeRect(area, i, n);
                NodeUiState st = s.NodeState(i);
                Color c = SlicePalette.NodeLock;
                if (st == NodeUiState.Allocated)
                    c = n.Mechanic ? SlicePalette.Cinder : n.Notable ? SlicePalette.Notable : SlicePalette.NodeOn;
                else if (st == NodeUiState.Available)
                    c = SlicePalette.NodeAvail;
                Fill(nr, new Color(c.r * 0.25f, c.g * 0.25f, c.b * 0.25f, 1f));
                Bar(nr.x, nr.y, nr.width, 3, c);
                string label = NodeLabel(n);
                Clipped(new Rect(nr.x + 4, nr.y + 6, nr.width - 8, nr.height - 8), label, _center, n.Name + "  " + n.Desc);
                if (Click(nr))
                {
                    string err;
                    if (!s.TryAllocate(i, out err) && err != null)
                        s.LastMessage = err;
                }
            }
        }

        static string NodeLabel(PassiveNode n)
        {
            if (n.Mechanic || n.Notable)
                return n.Name + "\n" + n.Desc;
            return n.Name;
        }

        static Vector2 NodeCenter(Rect area, int i)
        {
            Vector2 p = TreePos[i];
            return new Vector2(area.x + p.x * area.width, area.y + p.y * area.height);
        }

        static Rect NodeRect(Rect area, int i, PassiveNode n)
        {
            Vector2 c = NodeCenter(area, i);
            float w = n.Mechanic || n.Notable ? 86f : 70f;
            float h = n.Mechanic || n.Notable ? 40f : 32f;
            return new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h);
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
                if ((e.mousePosition - _dragStart).sqrMagnitude > 16f)
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
            Vector2 m = Event.current.mousePosition;
            Rect g = new Rect(m.x - 48, m.y - 12, 96, 24);
            Fill(g, new Color(0.20f, 0.18f, 0.10f, 0.9f));
            Label(g, SupportCatalog.Get(_drag).Name, _center);
        }

        void DrawTooltip()
        {
            if (string.IsNullOrEmpty(_tooltip))
                return;
            Vector2 m = Event.current.mousePosition;
            Vector2 size = _body.CalcSize(new GUIContent(_tooltip));
            float w = Mathf.Min(280f, size.x + 16f);
            float h = _body.CalcHeight(new GUIContent(_tooltip), w - 12f) + 10f;
            Rect r = new Rect(m.x + 14, m.y + 16, w, h);
            if (r.xMax > Dw())
                r.x = Dw() - r.width - 8;
            if (r.yMax > Dh())
                r.y = Dh() - r.height - 8;
            Fill(r, new Color(0.06f, 0.06f, 0.07f, 0.96f));
            Bar(r.x, r.y, r.width, 2, SlicePalette.PanelEdge);
            Label(new Rect(r.x + 6, r.y + 4, r.width - 12, r.height - 8), _tooltip, _body);
        }

        void DrawBanner(string text, Color c)
        {
            Rect r = new Rect(Dw() * 0.5f - 220, 86, 440, 36);
            Fill(r, c);
            Label(r, text, _center);
        }

        void PanelChrome(Rect r, string title)
        {
            PanelBg(r);
            Label(new Rect(r.x + 16, r.y + 10, r.width - 32, 22), title, _title);
        }

        /// <summary>PoE 式双球：底部按比例填充（Group 裁剪）+ 石环金边框 + 球心数值。</summary>
        void DrawOrb(Rect r, float u, Color c, float cur, float max)
        {
            if (u < 0f) u = 0f;
            if (u > 1f) u = 1f;
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
            GUI.DrawTexture(new Rect(0f, 0f, d, d), SliceSkin.OrbRing);
            GUI.EndGroup();
            Label(new Rect(r.x, r.y + r.height * 0.5f - 8f, r.width, 16f),
                Mathf.CeilToInt(cur) + "/" + Mathf.CeilToInt(max), _orbText);
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
            bool hover = e != null && r.Contains(e.mousePosition);
            bool press = hover && e != null && e.type == EventType.MouseDown && e.button == 0;
            GUI.Box(r, GUIContent.none, on ? _slotSel : (press ? _slotP : (hover ? _slotH : _slotN)));
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
            Clipped(r, text, style, text);
        }

        void Clipped(Rect r, string text, GUIStyle style, string tip)
        {
            Label(r, text, style);
            Vector2 size = style.CalcSize(new GUIContent(text));
            bool trunc = size.x > r.width - 2f || text.IndexOf('\n') < 0 && size.x > r.width;
            if (r.Contains(Event.current.mousePosition) && (trunc || tip != text))
                _tooltip = tip;
        }

        bool Click(Rect r)
        {
            Event e = Event.current;
            if (e == null || e.type != EventType.MouseDown || e.button != 0)
                return false;
            if (!r.Contains(e.mousePosition))
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
