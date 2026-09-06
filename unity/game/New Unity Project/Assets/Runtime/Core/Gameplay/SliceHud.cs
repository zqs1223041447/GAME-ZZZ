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
            if (s != null && s.Panel != SlicePanel.None)
                return true;
            if (_dragging)
                return true;
            Vector2 gui = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            return _topBar.Contains(gui) || _nav.Contains(gui) || _skillHud.Contains(gui) ||
                   _tray.Contains(gui) || _panel.Contains(gui);
        }

        public void Draw(ArenaDirector director)
        {
            ArenaSim sim = director.Sim;
            SliceSession s = sim.Session;
            if (s == null)
                return;

            EnsureStyles();
            _tooltip = null;

            DrawTop(s);
            DrawNav(s, sim);
            DrawBars(s);
            DrawSkillHud(s);
            DrawSupportTray(s);

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
            DrawTooltip();
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
            _styles = true;
        }

        void DrawTop(SliceSession s)
        {
            _topBar = new Rect(12, 10, 560, 78);
            Fill(_topBar, SlicePalette.Panel);
            Bar(_topBar.x, _topBar.y, _topBar.width, 3, SlicePalette.PanelEdge);
            Label(new Rect(_topBar.x + 12, _topBar.y + 8, 360, 20), s.StatusCopy, _title);
            Label(new Rect(_topBar.x + 12, _topBar.y + 30, 360, 18),
                "稳定度 " + s.Stability + "    收益 x" + s.RewardMultiplier.ToString("0.00") +
                "    废料 " + s.Scrap + "    蚀刻剂 " + s.Etching +
                "    天赋点 " + s.Unspent + "/" + s.TotalPoints, _small);
            Clipped(new Rect(_topBar.x + 12, _topBar.y + 50, 536, 22),
                s.LastMessage + (string.IsNullOrEmpty(s.LastLoot) ? "" : "  ·  " + s.LastLoot),
                _small);
        }

        void DrawNav(SliceSession s, ArenaSim sim)
        {
            _nav = new Rect(Screen.width - 292, 10, 280, 36);
            Fill(_nav, SlicePalette.Panel);
            float x = _nav.x + 4;
            if (NavBtn(new Rect(x, 14, 88, 28), "角色", s.Panel == SlicePanel.Build))
                s.Panel = s.Panel == SlicePanel.Build ? SlicePanel.None : SlicePanel.Build;
            if (NavBtn(new Rect(x + 92, 14, 88, 28), "地图", s.Panel == SlicePanel.Map))
                s.Panel = s.Panel == SlicePanel.Map ? SlicePanel.None : SlicePanel.Map;
            if (NavBtn(new Rect(x + 184, 14, 88, 28), "制作", s.Panel == SlicePanel.Craft))
                s.Panel = s.Panel == SlicePanel.Craft ? SlicePanel.None : SlicePanel.Craft;
        }

        void DrawBars(SliceSession s)
        {
            float life = s.MaxLife > 0f ? s.Life / s.MaxLife : 0f;
            float mana = s.MaxMana > 0f ? s.Mana / s.MaxMana : 0f;
            DrawMeter(14, 94, 280, 16, life, SlicePalette.Life,
                "生命 " + Mathf.CeilToInt(s.Life) + "/" + Mathf.CeilToInt(s.MaxLife));
            DrawMeter(14, 114, 280, 14, mana, SlicePalette.Mana,
                "法力 " + Mathf.CeilToInt(s.Mana) + "/" + Mathf.CeilToInt(s.MaxMana));
        }

        void DrawSkillHud(SliceSession s)
        {
            float w = 178f;
            float total = w * 3f + 16f;
            _skillHud = new Rect((Screen.width - total) * 0.5f, Screen.height - 118, total, 104);
            Fill(_skillHud, SlicePalette.Panel);
            Bar(_skillHud.x, _skillHud.y, _skillHud.width, 3, SlicePalette.PanelEdge);
            DrawSkillCell(s, SkillId.Melee, new Rect(_skillHud.x + 8, _skillHud.y + 10, w - 8, 86));
            DrawSkillCell(s, SkillId.Projectile, new Rect(_skillHud.x + w + 12, _skillHud.y + 10, w - 8, 86));
            DrawSkillCell(s, SkillId.Area, new Rect(_skillHud.x + w * 2 + 16, _skillHud.y + 10, w - 8, 86));
        }

        void DrawSkillCell(SliceSession s, SkillId skill, Rect r)
        {
            bool sel = s.SelectedSkill == skill;
            Fill(r, sel ? new Color(0.16f, 0.17f, 0.14f, 1f) : new Color(0.12f, 0.13f, 0.15f, 1f));
            Bar(r.x, r.y, 4, r.height, sel ? SlicePalette.PanelEdge : SlicePalette.Dim);
            if (Click(new Rect(r.x, r.y, r.width, 28)))
                s.SelectedSkill = skill;

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
            Fill(r, closed ? new Color(0.08f, 0.08f, 0.09f, 1f) : new Color(0.18f, 0.18f, 0.20f, 1f));
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
                e.Use();
                return;
            }

            if (!over || e.type != EventType.MouseDown || e.button != 0)
                return;

            if (_picked != SupportId.None)
            {
                PlaceSupport(s, skill, index, _picked);
                e.Use();
                return;
            }

            if (filled != SupportId.None)
            {
                string err;
                if (!s.TrySetSupport(skill, index, SupportId.None, out err) && err != null)
                    s.LastMessage = err;
                _picked = filled;
                e.Use();
            }
        }

        void DrawSupportTray(SliceSession s)
        {
            _tray = new Rect(12, Screen.height - 118, 210, 104);
            Fill(_tray, SlicePalette.Panel);
            Bar(_tray.x, _tray.y, _tray.width, 3, SlicePalette.PanelEdge);
            Label(new Rect(_tray.x + 8, _tray.y + 6, 194, 16), "辅助", _small);
            int n = 0;
            for (int i = 1; i <= 6; i++)
            {
                int col = n % 2;
                int row = n / 2;
                Rect g = new Rect(_tray.x + 8 + col * 100, _tray.y + 24 + row * 26, 96, 24);
                DrawGem(s, (SupportId)i, g);
                n++;
            }
        }

        void DrawGem(SliceSession s, SupportId id, Rect r)
        {
            SupportDef def = SupportCatalog.Get(id);
            bool used = IsLinked(s, id);
            bool pick = _picked == id;
            Color bg = pick ? new Color(0.32f, 0.28f, 0.12f, 1f) : new Color(0.16f, 0.16f, 0.18f, 1f);
            if (def.ChangesMechanism)
                Bar(r.x, r.y, 4, r.height, SlicePalette.Cinder);
            Fill(r, bg);
            string label = def.Name;
            Clipped(new Rect(r.x + 8, r.y, r.width - 10, r.height), used ? label + "*" : label, _clip, def.Name + "  " + def.Desc);
            Event e = Event.current;
            if (e == null || !r.Contains(e.mousePosition))
                return;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                _picked = id;
                _drag = id;
                _dragging = false;
                _dragStart = e.mousePosition;
                e.Use();
            }
        }

        void DrawBuild(SliceSession s, ArenaSim sim)
        {
            _panel = new Rect(12, 140, 760, 430);
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
            _panel = new Rect(12, 140, 560, 340);
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
            if (r.xMax > Screen.width)
                r.x = Screen.width - r.width - 8;
            if (r.yMax > Screen.height)
                r.y = Screen.height - r.height - 8;
            Fill(r, new Color(0.06f, 0.06f, 0.07f, 0.96f));
            Bar(r.x, r.y, r.width, 2, SlicePalette.PanelEdge);
            Label(new Rect(r.x + 6, r.y + 4, r.width - 12, r.height - 8), _tooltip, _body);
        }

        void DrawBanner(string text, Color c)
        {
            Rect r = new Rect(Screen.width * 0.5f - 220, 86, 440, 36);
            Fill(r, c);
            Label(r, text, _center);
        }

        void PanelChrome(Rect r, string title)
        {
            Fill(r, SlicePalette.Panel);
            Bar(r.x, r.y, r.width, 3, SlicePalette.PanelEdge);
            Label(new Rect(r.x + 12, r.y + 6, r.width - 24, 22), title, _title);
        }

        void DrawMeter(float x, float y, float w, float h, float u, Color c, string label)
        {
            if (u < 0f) u = 0f;
            if (u > 1f) u = 1f;
            Fill(new Rect(x, y, w, h), new Color(0.08f, 0.08f, 0.08f, 0.9f));
            Fill(new Rect(x, y, w * u, h), c);
            Label(new Rect(x + 6, y - 1, w - 8, h + 2), label, _small);
        }

        bool NavBtn(Rect r, string text, bool on)
        {
            Fill(r, on ? new Color(0.28f, 0.24f, 0.12f, 1f) : new Color(0.16f, 0.16f, 0.18f, 1f));
            if (on)
                Bar(r.x, r.y, r.width, 2, SlicePalette.PanelEdge);
            Label(r, text, _center);
            return Click(r);
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
