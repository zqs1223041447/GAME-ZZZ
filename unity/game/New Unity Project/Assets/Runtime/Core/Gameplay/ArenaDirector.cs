using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Runtime.Core
{
    public sealed class ArenaDirector : MonoBehaviour
    {
        public readonly ArenaSim Sim = new ArenaSim();
        public bool EnablePlayerInput = true;
        public string LastPerfPath;

        Camera _camera;
        Transform _playerView;
        MeshRenderer _playerRenderer;
        GameObject _dk;
        Animator _dkAnimator;
        AnimState _lastDkAnim = (AnimState)255;
        bool _dkHasIdle;
        bool _dkHasRun;
        bool _dkHasAttack;
        bool _dkHasCast;
        bool _dkHasHit;
        bool _dkHasDeath;
        float _dkHitLen = 0.4f;
        float _dkHitUntil;
        bool _dkDead;
        float _dkDeathFreezeAt;
        float _lastPlayerFlash;
        GameObject[] _dummyGo;
        MeshRenderer[] _dummyRenderer;
        MeshFilter[] _dummyFilter;
        Transform[] _projView;
        Transform[] _fbView;
        MeshRenderer[] _fbRenderer;
        MeshFilter[] _fbFilter;
        Mesh _meshSphere;
        Mesh _meshCube;
        Mesh _meshCylinder;
        Mesh _meshCapsule;
        Material _lit;
        MaterialPropertyBlock _mpb;
        readonly List<PerfRow> _rows = new List<PerfRow>(4);
        bool _built;
        bool _drawGui = true;
        bool _sampling;
        // 跟拍按贴地后身高 ≈1.19 收一帧：同俯角把 2x 远取景收到人物约占画面高 1/9（全身可辨+近怪在框内）
        static readonly Vector3 CamOffset = new Vector3(0f, 10.6f, -9.3f);
        const float CamFov = 42f;
        const float CamLookY = 0.6f;
        const float ViewBodyScale = 0.58f;
        const float PlayerViewSx = 0.64f;
        const float PlayerViewSy = 0.58f;
        const float PlayerColRadius = 0.32f;
        const float PlayerColHeight = 1.16f;
        bool _lmbFollow;
        float _lmbHeldTime;
        SkillId _heldSkill;
        float _skillHeldTime;
        readonly SliceHud _hud = new SliceHud();
        Transform _ground;

        static readonly Color PlayerColor = new Color(0.25f, 0.85f, 0.95f);
        static readonly Color DummyColor = new Color(0.92f, 0.48f, 0.18f);
        static readonly Color BruteColor = new Color(0.55f, 0.22f, 0.16f);
        static readonly Color StingerColor = new Color(0.35f, 0.72f, 0.28f);
        static readonly Color AshColor = new Color(0.95f, 0.42f, 0.12f);
        static readonly Color WardenColor = new Color(0.55f, 0.22f, 0.72f);
        static readonly Color FireProj = new Color(1f, 0.45f, 0.12f);
        static readonly Color HitColor = Color.white;
        static readonly Color DeathColor = new Color(0.18f, 0.16f, 0.16f);
        static readonly Color CastColor = new Color(1f, 0.86f, 0.25f);
        static readonly Color ProjColor = new Color(1f, 0.92f, 0.35f);

        void Awake()
        {
            BuildIfNeeded();
        }

        void Update()
        {
            if (!_built)
                return;

            PlayerCommand cmd = PlayerCommand.None();
            if (EnablePlayerInput && !_sampling)
            {
                if (Sim.Session != null)
                    _hud.HandleKeys(Sim.Session, Sim);
                bool block = _hud.ShouldBlockWorld(Sim.Session);
                cmd = ReadInput(block);
            }

            Sim.Tick(Time.deltaTime, cmd);
            SyncViews();
        }

        void OnGUI()
        {
            if (!_drawGui || _sampling)
                return;
            if (Sim.Session != null)
            {
                _hud.Draw(this);
                return;
            }

            const int w = 460;
            GUI.Box(new Rect(12, 12, w, 148), "");
            GUI.Label(new Rect(24, 20, w - 24, 128),
                "S1 Arena\n" +
                "按住左键跟着走，松开停；单击仍走到点\n" +
                "按住 Q/W/E 连发，点按一次只放一次\n" +
                "Q 近战   W 弹道   E 范围   F1/F2/F3 密度   F5 采样\n" +
                "Dummy " + Sim.AliveDummyCount + "/" + Sim.SpawnedCount +
                "  phase " + Sim.Caster.Phase +
                "  dt " + (Time.unscaledDeltaTime * 1000f).ToString("F1") + "ms");
        }

        public void BuildIfNeeded()
        {
            if (_built)
                return;

            Sim.Reset();
            CollisionLayers.Apply();
            _mpb = new MaterialPropertyBlock();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            _lit = new Material(shader);
            _lit.enableInstancing = true;

            _camera = Camera.main;
            if (_camera == null)
            {
                var camGo = new GameObject("Main Camera");
                _camera = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }

            _camera.fieldOfView = CamFov;
            _camera.nearClipPlane = 0.15f;

            BuildGround();
            BuildPlayer();

            Mesh cube = PrimitiveMesh(PrimitiveType.Cube);
            Mesh sphere = PrimitiveMesh(PrimitiveType.Sphere);
            _meshCube = cube;
            _meshSphere = sphere;
            _meshCylinder = PrimitiveMesh(PrimitiveType.Cylinder);
            _meshCapsule = PrimitiveMesh(PrimitiveType.Capsule);
            _dummyGo = new GameObject[CombatRules.DummyPoolSize];
            _dummyRenderer = new MeshRenderer[CombatRules.DummyPoolSize];
            _dummyFilter = new MeshFilter[CombatRules.DummyPoolSize];
            for (int i = 0; i < _dummyGo.Length; i++)
            {
                var go = new GameObject("Dummy");
                go.layer = CollisionLayers.Monster;
                go.SetActive(false);
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = cube;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = _lit;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
                _dummyGo[i] = go;
                _dummyRenderer[i] = mr;
                _dummyFilter[i] = mf;
            }

            _projView = new Transform[CombatRules.ProjectilePoolSize];
            for (int i = 0; i < _projView.Length; i++)
            {
                var go = new GameObject("Projectile");
                go.SetActive(false);
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = sphere;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = _lit;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                go.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                ApplyColor(mr, ProjColor);
                _projView[i] = go.transform;
            }

            _fbView = new Transform[CombatRules.FeedbackPoolSize];
            _fbRenderer = new MeshRenderer[CombatRules.FeedbackPoolSize];
            _fbFilter = new MeshFilter[CombatRules.FeedbackPoolSize];
            for (int i = 0; i < _fbView.Length; i++)
            {
                var go = new GameObject("Feedback");
                go.SetActive(false);
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = sphere;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = _lit;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                _fbView[i] = go.transform;
                _fbRenderer[i] = mr;
                _fbFilter[i] = mf;
            }

            _built = true;
            if (Sim.Session == null)
            {
                Sim.Session = new SliceSession();
                Sim.Caster.Defs = Sim.Session.ResolveSkillDef;
            }

            Sim.SpawnDummies(8, CombatRules.ArenaSeed);
            GameLog.Info("Arena", "S2 UI. 角色/地图/制作可全鼠标。Q/W/E 仍可连发。");

            // 测量钩子：默认无操作，仅独立包 -arenaPerf 参数或显式调用时启动
            ArenaPerfHarness.InitFromArgs();
            ArenaPerfHarness.TryStart(this);
        }

        PlayerCommand ReadInput(bool blockWorld)
        {
            bool inMap = Sim.Session != null && Sim.Session.State == MapState.InMap;
            if (!inMap)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    Sim.SpawnDummies(100, CombatRules.ArenaSeed);
                    GameLog.Info("Arena", "spawned 100 Dummy");
                }
                else if (Input.GetKeyDown(KeyCode.F2))
                {
                    Sim.SpawnDummies(200, CombatRules.ArenaSeed);
                    GameLog.Info("Arena", "spawned 200 Dummy");
                }
                else if (Input.GetKeyDown(KeyCode.F3))
                {
                    Sim.SpawnDummies(300, CombatRules.ArenaSeed);
                    GameLog.Info("Arena", "spawned 300 Dummy");
                }
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                StartCoroutine(SampleCurrent());
            }

            if (blockWorld)
                return PlayerCommand.None();

            float aimX;
            float aimZ;
            bool hasAim = ScreenAim(out aimX, out aimZ);
            int hover = -1;
            if (hasAim)
                hover = Sim.Dummies.FindNearestAlive(aimX, aimZ, CombatRules.PickRadius);

            PlayerCommand skillCmd = ReadSkill(hasAim, hover, aimX, aimZ);
            if (skillCmd.Kind != CommandKind.None)
                return skillCmd;

            return ReadMove(hasAim, hover, aimX, aimZ);
        }

        PlayerCommand ReadSkill(bool hasAim, int hover, float aimX, float aimZ)
        {
            SkillId down = SkillId.None;
            if (Input.GetKeyDown(KeyCode.Q))
                down = SkillId.Melee;
            else if (Input.GetKeyDown(KeyCode.W))
                down = SkillId.Projectile;
            else if (Input.GetKeyDown(KeyCode.E))
                down = SkillId.Area;

            if (down != SkillId.None)
            {
                _heldSkill = down;
                _skillHeldTime = 0f;
                return MakeCast(down, hasAim, hover, aimX, aimZ);
            }

            if (_heldSkill == SkillId.None)
                return PlayerCommand.None();

            if (!Input.GetKey(KeyOf(_heldSkill)))
            {
                _heldSkill = SkillId.None;
                _skillHeldTime = 0f;
                return PlayerCommand.None();
            }

            _skillHeldTime += Time.deltaTime;
            if (_skillHeldTime < CombatRules.HoldSkillRepeat)
                return PlayerCommand.None();

            return MakeCast(_heldSkill, hasAim, hover, aimX, aimZ);
        }

        PlayerCommand ReadMove(bool hasAim, int hover, float aimX, float aimZ)
        {
            if (Input.GetMouseButtonDown(0) && hasAim)
            {
                _lmbFollow = false;
                _lmbHeldTime = 0f;
                if (hover >= 0)
                    return PlayerCommand.CastAt(SkillId.Melee, hover, aimX, aimZ);
                return PlayerCommand.MoveTo(aimX, aimZ);
            }

            if (Input.GetMouseButton(0) && hasAim)
            {
                _lmbHeldTime += Time.deltaTime;
                if (_lmbHeldTime >= CombatRules.HoldMoveEngage)
                {
                    _lmbFollow = true;
                    if (hover >= 0)
                        return PlayerCommand.CastAt(SkillId.Melee, hover, aimX, aimZ);
                    return PlayerCommand.MoveTo(aimX, aimZ);
                }

                return PlayerCommand.None();
            }

            if (Input.GetMouseButtonUp(0) && _lmbFollow)
            {
                _lmbFollow = false;
                _lmbHeldTime = 0f;
                return PlayerCommand.Stop();
            }

            if (!Input.GetMouseButton(0))
            {
                _lmbFollow = false;
                _lmbHeldTime = 0f;
            }

            return PlayerCommand.None();
        }

        PlayerCommand MakeCast(SkillId skill, bool hasAim, int hover, float aimX, float aimZ)
        {
            if (!hasAim)
            {
                float yaw = Sim.Player.YawDeg * Mathf.Deg2Rad;
                aimX = Sim.Player.X + Mathf.Sin(yaw) * 2f;
                aimZ = Sim.Player.Z + Mathf.Cos(yaw) * 2f;
            }

            return PlayerCommand.CastAt(skill, hover, aimX, aimZ);
        }

        static KeyCode KeyOf(SkillId skill)
        {
            if (skill == SkillId.Melee)
                return KeyCode.Q;
            if (skill == SkillId.Projectile)
                return KeyCode.W;
            return KeyCode.E;
        }

        bool ScreenAim(out float x, out float z)
        {
            x = 0f;
            z = 0f;
            if (_camera == null)
                return false;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, Vector3.zero);
            float enter;
            if (!plane.Raycast(ray, out enter))
                return false;
            Vector3 p = ray.GetPoint(enter);
            x = p.x;
            z = p.z;
            return true;
        }

        void SyncViews()
        {
            if (_ground != null && Sim.Session != null)
            {
                Color g = Sim.Session.OnMap
                    ? new Color(0.28f, 0.12f, 0.10f)
                    : new Color(0.18f, 0.22f, 0.18f);
                ApplyColor(_ground.GetComponent<MeshRenderer>(), g);
            }

            Vector3 playerPos = new Vector3(Sim.Player.X, 0f, Sim.Player.Z);
            _playerView.SetPositionAndRotation(playerPos, Quaternion.Euler(0f, Sim.Player.YawDeg, 0f));
            float playerFlash = Sim.Session != null ? Sim.Session.HitFlash : 0f;
            if (_dk == null)
                ApplyPlayerVisual(_playerRenderer.transform, _playerRenderer, Sim.Player.Anim, PlayerColor, playerFlash);
            else
            {
                _playerView.localScale = Vector3.one;
                DriveDarkKnight(Sim.Player.Anim);
                DarkKnightView.UpdateGround(_dk, playerPos.y);
            }

            if (_camera != null)
            {
                Vector3 look = new Vector3(Sim.Player.X, CamLookY, Sim.Player.Z);
                _camera.transform.position = look + CamOffset;
                _camera.transform.LookAt(look);
            }

            Dummy[] dummies = Sim.Dummies.Items;
            for (int i = 0; i < _dummyGo.Length; i++)
            {
                Dummy d = dummies[i];
                GameObject go = _dummyGo[i];
                if (!d.Occupied)
                {
                    if (go.activeSelf)
                        go.SetActive(false);
                    continue;
                }

                if (!go.activeSelf)
                    go.SetActive(true);
                _dummyFilter[i].sharedMesh = MeshFor(d.Kind);
                float y = HeightFor(d.Kind, d.Scale);
                go.transform.SetPositionAndRotation(
                    new Vector3(d.X, y, d.Z),
                    Quaternion.Euler(0f, d.YawDeg, 0f));
                Color baseColor = d.Alive ? ColorFor(d) : DeathColor;
                float sx = d.Scale > 0.1f ? d.Scale : 1f;
                ApplyAnimVisual(go.transform, _dummyRenderer[i], d.Anim, baseColor, d.HitFlash, sx, d.Kind);
                if (d.IgniteRemain > 0f && d.Alive)
                    ApplyColor(_dummyRenderer[i], Color.Lerp(baseColor, AshColor, 0.55f));
            }

            Projectile[] projs = Sim.Projectiles.Items;
            for (int i = 0; i < _projView.Length; i++)
            {
                bool on = i < projs.Length && projs[i].Alive;
                GameObject go = _projView[i].gameObject;
                if (go.activeSelf != on)
                    go.SetActive(on);
                if (!on)
                    continue;
                _projView[i].position = new Vector3(projs[i].X, 0.55f, projs[i].Z);
                MeshRenderer pmr = _projView[i].GetComponent<MeshRenderer>();
                if (pmr != null)
                    ApplyColor(pmr, projs[i].HasFire || projs[i].FromFork ? FireProj : ProjColor);
                float ps = projs[i].FromFork ? 0.28f : 0.35f;
                _projView[i].localScale = new Vector3(ps, ps, ps);
            }

            Feedback[] fbs = Sim.Feedback.Items;
            for (int i = 0; i < _fbView.Length; i++)
            {
                bool on = i < fbs.Length && fbs[i].Alive;
                GameObject go = _fbView[i].gameObject;
                if (go.activeSelf != on)
                    go.SetActive(on);
                if (!on)
                    continue;
                ApplyFeedbackVisual(_fbView[i], _fbFilter[i], _fbRenderer[i], fbs[i]);
            }
        }

        void ApplyFeedbackVisual(Transform t, MeshFilter mf, MeshRenderer mr, Feedback fb)
        {
            float u = fb.Duration > 0f ? fb.Age / fb.Duration : 1f;
            if (u < 0f)
                u = 0f;
            if (u > 1f)
                u = 1f;

            if (fb.Kind == FeedbackKind.Area)
            {
                mf.sharedMesh = _meshCylinder;
                float r = fb.Scale > 0.1f ? fb.Scale : 6.4f;
                t.SetPositionAndRotation(new Vector3(fb.X, 0.06f, fb.Z), Quaternion.identity);
                t.localScale = new Vector3(r, 0.04f, r);
                Color c = new Color(1f, 0.55f, 0.12f, 1f);
                c = Color.Lerp(c, new Color(1f, 0.92f, 0.35f), u);
                ApplyColor(mr, c);
                return;
            }

            if (fb.Kind == FeedbackKind.MeleeSwing)
            {
                mf.sharedMesh = _meshCube;
                float pulse = 1f + 0.35f * (1f - u);
                t.SetPositionAndRotation(
                    new Vector3(fb.X, 0.55f, fb.Z),
                    Quaternion.Euler(0f, fb.YawDeg, 0f));
                t.localScale = new Vector3(0.85f * pulse, 0.18f, 0.42f * pulse);
                ApplyColor(mr, Color.Lerp(Color.white, CastColor, u));
                return;
            }

            mf.sharedMesh = _meshSphere;
            float s = fb.Scale > 0.05f ? fb.Scale : 0.45f;
            Color color = HitColor;
            float y = 0.5f;
            if (fb.Kind == FeedbackKind.Death)
            {
                s = 0.28f + u * 0.6f;
                color = DeathColor;
                y = 0.32f;
            }
            else if (fb.Kind == FeedbackKind.Cast)
            {
                s = (0.32f + u * 0.32f) * (fb.Scale > 0.05f ? fb.Scale : 1f);
                color = CastColor;
            }
            else if (fb.Kind == FeedbackKind.Loot)
            {
                s = 0.24f + u * 0.55f;
                color = new Color(0.95f, 0.82f, 0.2f);
                y = 0.7f + u * 0.5f;
            }
            else if (fb.Kind == FeedbackKind.Ignite)
            {
                s = (0.4f + 0.28f * (1f - u));
                color = new Color(1f, 0.35f, 0.05f);
                y = 0.42f;
            }
            else
            {
                s = s * (1.05f + 0.55f * (1f - u));
                color = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.4f), u);
            }

            t.SetPositionAndRotation(new Vector3(fb.X, y, fb.Z), Quaternion.identity);
            t.localScale = new Vector3(s, s, s);
            ApplyColor(mr, color);
        }

        static Color ColorFor(Dummy d)
        {
            switch (d.Kind)
            {
                case EnemyKind.Brute: return SlicePalette.Brute;
                case EnemyKind.Stinger: return SlicePalette.Stinger;
                case EnemyKind.Ashling: return SlicePalette.Ashling;
                case EnemyKind.Warden: return SlicePalette.Warden;
                default: return DummyColor;
            }
        }

        Mesh MeshFor(EnemyKind kind)
        {
            if (kind == EnemyKind.Stinger)
                return _meshSphere;
            if (kind == EnemyKind.Ashling)
                return _meshCylinder;
            if (kind == EnemyKind.Warden)
                return _meshCapsule;
            return _meshCube;
        }

        static float HeightFor(EnemyKind kind, float scale)
        {
            float s = (scale > 0.1f ? scale : 1f) * ViewBodyScale;
            if (kind == EnemyKind.Warden)
                return 0.95f * s;
            if (kind == EnemyKind.Stinger)
                return 0.42f * s;
            if (kind == EnemyKind.Ashling)
                return 0.55f * s;
            return 0.5f * s;
        }

        void ApplyAnimVisual(Transform t, MeshRenderer mr, AnimState anim, Color baseColor, float hitFlash)
        {
            ApplyAnimVisual(t, mr, anim, baseColor, hitFlash, 1f, EnemyKind.Dummy);
        }

        void ApplyAnimVisual(Transform t, MeshRenderer mr, AnimState anim, Color baseColor, float hitFlash, float scale, EnemyKind kind)
        {
            Color c = baseColor;
            float sx = 1f, sy = 1f, sz = 1f;
            switch (anim)
            {
                case AnimState.Run:
                    sy = 1f + 0.08f * Mathf.Sin(Time.time * 10f);
                    break;
                case AnimState.Attack:
                    sz = 1.18f;
                    sx = 1.10f;
                    c = Color.white;
                    break;
                case AnimState.Cast:
                    sx = sy = sz = 1.10f;
                    c = CastColor;
                    break;
                case AnimState.Hit:
                    c = HitColor;
                    sx = sy = sz = 1.08f;
                    break;
                case AnimState.Death:
                    float k = 0.22f;
                    sx = sy = sz = k;
                    c = DeathColor;
                    break;
            }

            if (hitFlash > 0f)
                c = Color.Lerp(c, HitColor, anim == AnimState.Death ? 0.75f : 0.95f);
            if (kind == EnemyKind.Brute)
            {
                sx *= 1.35f;
                sz *= 1.20f;
                sy *= 0.82f;
            }
            else if (kind == EnemyKind.Ashling)
            {
                sx *= 0.72f;
                sz *= 0.72f;
                sy *= 1.35f;
            }
            else if (kind == EnemyKind.Warden)
            {
                sx *= 1.15f;
                sz *= 1.15f;
                sy *= 1.30f;
            }

            float body = scale * ViewBodyScale;
            t.localScale = new Vector3(sx * body, sy * body, sz * body);
            ApplyColor(mr, c);
        }

        void ApplyPlayerVisual(Transform t, MeshRenderer mr, AnimState anim, Color baseColor, float hitFlash)
        {
            Color c = baseColor;
            float sx = 1f, sy = 1f, sz = 1f;
            switch (anim)
            {
                case AnimState.Run:
                    sy = 1f + 0.08f * Mathf.Sin(Time.time * 10f);
                    break;
                case AnimState.Attack:
                    sz = 1.18f;
                    sx = 1.10f;
                    c = Color.white;
                    break;
                case AnimState.Cast:
                    sx = sy = sz = 1.10f;
                    c = CastColor;
                    break;
                case AnimState.Hit:
                    c = HitColor;
                    sx = sy = sz = 1.08f;
                    break;
                case AnimState.Death:
                    sx = sy = sz = 0.22f;
                    c = DeathColor;
                    break;
            }

            if (hitFlash > 0f)
                c = Color.Lerp(c, HitColor, anim == AnimState.Death ? 0.75f : 0.95f);
            t.localScale = new Vector3(sx * PlayerViewSx, sy * PlayerViewSy, sz * PlayerViewSx);
            ApplyColor(mr, c);
        }

        void ApplyColor(MeshRenderer mr, Color color)
        {
            if (mr == null)
                return;
            mr.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", color);
            _mpb.SetColor("_Color", color);
            mr.SetPropertyBlock(_mpb);
        }

        void BuildGround()
        {
            Mesh plane = PrimitiveMesh(PrimitiveType.Plane);
            var go = new GameObject("Ground");
            go.layer = 0;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = plane;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _lit;
            go.transform.localScale = new Vector3(8f, 1f, 8f);
            ApplyColor(mr, new Color(0.18f, 0.22f, 0.18f));
            var col = go.AddComponent<MeshCollider>();
            col.sharedMesh = plane;
            _ground = go.transform;
        }

        void BuildPlayer()
        {
            var root = new GameObject("Player");
            root.layer = CollisionLayers.Player;
            var body = root.AddComponent<CapsuleCollider>();
            body.radius = PlayerColRadius;
            body.height = PlayerColHeight;
            body.center = new Vector3(0f, PlayerColHeight * 0.5f, 0f);
            body.direction = 1;

            GameObject vis = BuildCapsule("CapsuleVisual", PlayerColor, PlayerColRadius, PlayerColHeight);
            vis.layer = CollisionLayers.Player;
            vis.transform.SetParent(root.transform, false);
            vis.transform.localPosition = new Vector3(0f, PlayerColHeight * 0.5f, 0f);
            vis.transform.localRotation = Quaternion.identity;
            _playerView = root.transform;
            _playerRenderer = vis.GetComponent<MeshRenderer>();

            _dk = DarkKnightView.TryMount(root.transform);
            if (_dk != null)
            {
                _playerRenderer.enabled = false;
                _dkAnimator = DarkKnightView.FindAnimator(_dk);
                CacheDarkKnightStates();
                GameLog.Info("Arena", "DarkKnight mounted under Player.");
            }
            else
                GameLog.Info("Arena", "capsule visual (no player mesh)");
        }

        void CacheDarkKnightStates()
        {
            _dkHasIdle = HasDarkKnightState("Idle");
            _dkHasRun = HasDarkKnightState("Run");
            _dkHasAttack = HasDarkKnightState("Attack");
            _dkHasCast = HasDarkKnightState("Cast");
            _dkHasHit = HasDarkKnightState("Hit");
            _dkHasDeath = HasDarkKnightState("Death");
            _dkHitLen = DkClipLength("Hit", 0.4f);
        }

        float DkClipLength(string name, float fallback)
        {
            RuntimeAnimatorController ctrl = _dkAnimator != null ? _dkAnimator.runtimeAnimatorController : null;
            if (ctrl == null || ctrl.animationClips == null)
                return fallback;
            for (int i = 0; i < ctrl.animationClips.Length; i++)
            {
                AnimationClip clip = ctrl.animationClips[i];
                if (clip != null && clip.name == name)
                    return Mathf.Max(0.05f, clip.length);
            }
            return fallback;
        }

        bool HasDarkKnightState(string name)
        {
            if (_dkAnimator == null || _dkAnimator.runtimeAnimatorController == null)
                return false;
            return _dkAnimator.HasState(0, Animator.StringToHash(name));
        }

        // 视图层驱动：Hit/Death 只挂现有受击/死亡信号（Session.HitFlash 上跳沿、MapState.Dead），逻辑状态机不扩
        void DriveDarkKnight(AnimState anim)
        {
            if (_dkAnimator == null || _dkAnimator.runtimeAnimatorController == null)
                return;

            float flash = Sim.Session != null ? Sim.Session.HitFlash : 0f;
            bool dead = Sim.Session != null && Sim.Session.State == MapState.Dead;

            if (dead)
            {
                if (!_dkDead)
                {
                    _dkDead = true;
                    AudioEvents.Play(AudioEventId.Death);
                    if (_dkHasDeath)
                    {
                        _dkAnimator.speed = 1f;
                        _dkAnimator.Play("Death", 0, 0f);
                        _dkDeathFreezeAt = Time.time + DkClipLength("Death", 1f);
                    }
                }
                // 非循环状态播完会被采样绕回：到点即冻结，定格跪倒末帧
                if (_dkDeathFreezeAt > 0f && Time.time >= _dkDeathFreezeAt)
                {
                    _dkAnimator.speed = 0f;
                    _dkDeathFreezeAt = 0f;
                }
                _lastPlayerFlash = flash;
                return;
            }
            _dkDead = false;
            if (_dkAnimator.speed == 0f)
                _dkAnimator.speed = 1f;

            bool hitEdge = flash > 0.02f && _lastPlayerFlash <= 0.02f;
            if (hitEdge)
                AudioEvents.Play(AudioEventId.Hit); // 与 Hit 动画同一处：玩家受击上跳沿
            if (_dkHasHit && hitEdge)
            {
                // 新的受击上跳沿即重播：高攻速下 Hit 可被下一击打断重来
                _dkAnimator.Play("Hit", 0, 0f);
                _dkHitUntil = Time.time + _dkHitLen;
                _lastDkAnim = (AnimState)255; // Hit 播完后强制重放当前逻辑姿态
            }
            _lastPlayerFlash = flash;

            if (Time.time < _dkHitUntil)
                return;

            if (anim == _lastDkAnim)
                return;

            string state = null;
            if (anim == AnimState.Run && _dkHasRun)
                state = "Run";
            else if (anim == AnimState.Attack && _dkHasAttack)
                state = "Attack";
            else if (anim == AnimState.Cast && _dkHasCast)
                state = "Cast";
            else if (_dkHasIdle)
                state = "Idle";

            _lastDkAnim = anim;
            if (state == null)
                return;
            _dkAnimator.Play(state, 0, 0f);
        }

        GameObject BuildCapsule(string name, Color color, float radius, float height)
        {
            Mesh mesh = PrimitiveMesh(PrimitiveType.Capsule);
            var go = new GameObject(name);
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _lit;
            go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            ApplyColor(mr, color);
            return go;
        }

        static Mesh PrimitiveMesh(PrimitiveType type)
        {
            var tmp = GameObject.CreatePrimitive(type);
            Mesh mesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            Collider col = tmp.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
            Destroy(tmp);
            return mesh;
        }

        public IEnumerator SampleDensity(int count, int warmup, int frames)
        {
            BuildIfNeeded();
            EnablePlayerInput = false;
            _drawGui = false;
            _sampling = true;
            Sim.Reset();
            Sim.SpawnDummies(count, CombatRules.ArenaSeed);
            var sampler = new PerfSampler();
            for (int i = 0; i < warmup; i++)
                yield return null;
            sampler.Begin();
            for (int i = 0; i < frames; i++)
            {
                sampler.Sample(Time.unscaledDeltaTime);
                yield return null;
            }

            PerfRow row = sampler.End(count, Sim.AliveDummyCount);
            _rows.Add(row);
            LastPerfPath = PerfSampler.DefaultPath;
            PerfSampler.Write(LastPerfPath, _rows);
            GameLog.Info("Perf",
                "dummy=" + row.DummyCount +
                " frames=" + row.Frames +
                " mainMs.avg=" + row.MainMsAvg.ToString("F2") +
                " p95=" + row.MainMsP95.ToString("F2") +
                " gc/frame=" + row.GcBytesAvg.ToString("F0"));
            _sampling = false;
            _drawGui = true;
            EnablePlayerInput = true;
        }

        IEnumerator SampleCurrent()
        {
            int count = Sim.SpawnedCount;
            if (count <= 0)
                count = Sim.AliveDummyCount;
            if (count <= 0)
                count = 100;
            _rows.Clear();
            yield return SampleDensity(count, 20, 90);
        }
    }
}
