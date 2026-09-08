using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// 通用敌人视觉表现层（S3-P5-ART-R3-TROLL-RUNTIME-INTEGRATION）：无 gameplay 权限——
    /// 不决定 AI/伤害/速度/死亡条件/目标选择/攻击冷却/死亡时机。只持有视觉预制体实例与 Animator，
    /// 把视图层（ArenaDirector）读到的通用 gameplay 状态映射为动画状态。
    /// 信号来源全部为已有 gameplay 状态：AnimState（Idle/Run/Hit/Death）、HitFlash、
    /// AttackExecutions（攻击执行序号观察计数，ArenaSim 消费 AttackReady 时 +1）。
    /// 纯 C# 类（非 MonoBehaviour）：不携带任何可写 gameplay 数据，不注册 Update。
    /// </summary>
    public sealed class EnemyVisualPresenter
    {
        readonly GameObject _root;
        readonly Animator _animator;
        readonly bool _hasIdle;
        readonly bool _hasRun;
        readonly bool _hasAttack;
        readonly bool _hasHit;
        readonly bool _hasDeath;
        readonly float _attackLen;
        readonly float _hitLen;
        readonly bool _hasClips;

        float _actionUntil;
        int _lastAttackSerial = int.MinValue;
        bool _observed;
        bool _deathShown;
        string _lastRequestedState;

        EnemyVisualPresenter(GameObject root, Animator animator)
        {
            _root = root;
            _animator = animator;
            RuntimeAnimatorController ctrl = _animator != null ? _animator.runtimeAnimatorController : null;
            _hasClips = ctrl != null && ctrl.animationClips != null && ctrl.animationClips.Length > 0;
            if (ctrl == null)
                return;
            _hasIdle = HasState("Idle");
            _hasRun = HasState("Run");
            _hasAttack = HasState("Attack");
            _hasHit = HasState("Hit");
            _hasDeath = HasState("Death");
            _attackLen = ClipLength("Attack", 0.6f);
            _hitLen = ClipLength("Hit", 0.3f);
        }

        /// <summary>实例化预制体并挂到 gameplay root 下（位置/朝向归零，scale 沿用预制体根节点自带值）。</summary>
        public static EnemyVisualPresenter Mount(GameObject prefab, Transform gameplayRoot)
        {
            if (prefab == null || gameplayRoot == null)
                return null;

            GameObject go = Object.Instantiate(prefab, gameplayRoot, false);
            go.name = "VisualRoot";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            // 保留预制体根节点自带 scale（美术负责整体尺寸，如 FireLion 0.00648），此处不得覆写
            StripBlockingPhysics(go);

            Animator animator = null;
            Animator[] animators = go.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null)
                {
                    animator = animators[i];
                    break;
                }
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
                // 静止时无动画更新需求：非循环状态播完由 Present 重新驱动，不依赖 Update 自转
                if (animator.runtimeAnimatorController == null)
                    animator.enabled = false;
            }

            return new EnemyVisualPresenter(go, animator);
        }

        public GameObject Root
        {
            get { return _root; }
        }

        /// <summary>只读观察：最近一次 Present 请求的动画状态名（测试/QA 验证钩子，默认只读无副作用）。</summary>
        public string LastRequestedState
        {
            get { return _lastRequestedState; }
        }

        public void Show()
        {
            if (_root != null && !_root.activeSelf)
                _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null && _root.activeSelf)
                _root.SetActive(false);
        }

        /// <summary>
        /// 把已有 gameplay 状态映射为视觉动画。优先级：Death &gt; Hit 边沿 &gt; Attack 边沿 &gt; 移动/待机；
        /// Hit/Attack 动作窗口内不被移动姿态打断（与 DarkKnightView 同语义）。
        /// </summary>
        public void Present(bool alive, AnimState anim, float hitFlash, int attackSerial, float now)
        {
            if (_root == null)
                return;

            if (!alive)
            {
                if (!_deathShown)
                {
                    _deathShown = true;
                    if (_hasDeath)
                        RequestState("Death");
                }
                return;
            }
            _deathShown = false;

            bool attackEdge = _observed && attackSerial != _lastAttackSerial;
            _observed = true;
            _lastAttackSerial = attackSerial;
            if (_hasAttack && attackEdge)
            {
                RequestState("Attack");
                _actionUntil = now + _attackLen;
                return;
            }

            bool hitEdge = hitFlash > 0.02f;
            if (_hasHit && hitEdge && anim == AnimState.Hit && _lastRequestedState != "Hit")
            {
                RequestState("Hit");
                _actionUntil = now + _hitLen;
                return;
            }

            if (now < _actionUntil)
                return;

            string state = MoveStateFor(anim);
            if (_hasRun && anim == AnimState.Run)
                RequestState(state);
            else if (_hasIdle && state == "Idle")
                RequestState(state);
            else if (_hasIdle && state == "Run")
                RequestState("Idle"); // 控制器缺 Run 态时回退待机，不伪造
        }

        public void ResetObservation()
        {
            _lastAttackSerial = int.MinValue;
            _observed = false;
            _deathShown = false;
            _actionUntil = 0f;
            _lastRequestedState = null;
        }

        /// <summary>移动姿态映射（纯函数）：Run→"Run"，其余→"Idle"。</summary>
        public static string MoveStateFor(AnimState anim)
        {
            return anim == AnimState.Run ? "Run" : "Idle";
        }

        void RequestState(string state)
        {
            _lastRequestedState = state;
            if (_animator == null || !_animator.enabled || !_hasClips)
                return;
            _animator.speed = 1f;
            _animator.Play(state, 0, 0f);
        }

        bool HasState(string name)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return false;
            return _animator.HasState(0, Animator.StringToHash(name));
        }

        float ClipLength(string name, float fallback)
        {
            RuntimeAnimatorController ctrl = _animator != null ? _animator.runtimeAnimatorController : null;
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

        static void StripBlockingPhysics(GameObject go)
        {
            Rigidbody[] bodies = go.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
                Object.Destroy(bodies[i]);

            CharacterController[] ccs = go.GetComponentsInChildren<CharacterController>(true);
            for (int i = 0; i < ccs.Length; i++)
                Object.Destroy(ccs[i]);

            Collider[] cols = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
                Object.Destroy(cols[i]);
        }
    }
}
