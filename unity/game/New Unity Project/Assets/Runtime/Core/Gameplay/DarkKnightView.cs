using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Runtime.Core
{
    public static class DarkKnightView
    {
        // 别名指向统一路径契约（S3-M2 单一真相源），保留旧公开名以兼容既有引用
        public const string ResourcesNameDarkKnight = RuntimeResourcePaths.PlayerDarkKnight;

        public const float TargetHeight = 1.16f;

        public static GameObject TryMount(Transform playerRoot)
        {
            GameObject prefab = Resources.Load<GameObject>(ResourcesNameDarkKnight);
            if (prefab == null)
            {
                // 旧 Eve 回退已删除（曾用名）：无 DarkKnight 预制体即回退胶囊视图（避免同名预制体复活已剔除旧模）
                GameLog.Info("Arena", "no DarkKnight prefab -> capsule visual");
                return null;
            }

            GameObject go = Object.Instantiate(prefab, playerRoot, false);
            go.name = "DarkKnight";
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            StripBlockingPhysics(go);
            QuietAnimator(go);
            FitToCapsule(go, TargetHeight);
            PrepareRenderers(go);
            return go;
        }

        public static void QuietAnimator(GameObject go)
        {
            Animator anim = FindAnimator(go);
            if (anim == null)
                return;
            anim.applyRootMotion = false;
            RuntimeAnimatorController ctrl = anim.runtimeAnimatorController;
            if (ctrl == null)
            {
                anim.enabled = false;
                return;
            }

            AnimationClip[] clips = ctrl.animationClips;
            bool hasMotion = false;
            if (clips != null)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null)
                    {
                        hasMotion = true;
                        break;
                    }
                }
            }

            if (!hasMotion)
                anim.enabled = false;
        }

        public static void FitToCapsule(GameObject go, float targetHeight)
        {
            if (go == null || targetHeight <= 0.01f)
                return;

            // 身高以身体包围盒为准，排除武器（刀随动画垂到地下会污染整体包围盒）
            Bounds b;
            if (!TryWorldBounds(go, out b))
                return;

            float height = b.size.y;
            if (height < 1e-5f)
                return;

            float scale = targetHeight / height;
            Vector3 s = go.transform.localScale;
            go.transform.localScale = new Vector3(s.x * scale, s.y * scale, s.z * scale);

            if (!TryWorldBounds(go, out b))
                return;

            Transform parent = go.transform.parent;
            float groundY = parent != null ? parent.position.y : 0f;
            float lift = groundY - b.min.y;
            if (Mathf.Abs(lift) > 1e-4f)
                go.transform.position += new Vector3(0f, lift, 0f);

            GameLog.Info("Arena",
                "DarkKnight fitted body height " + targetHeight.ToString("0.00") +
                " from " + height.ToString("0.000") +
                " scale x" + scale.ToString("0.00"));
        }

        static bool IsWeaponRenderer(Renderer r)
        {
            // 长刀（Bip_Weapon_R 挂点），不参与身高/贴地计算
            return r != null && r.name == "longblade";
        }

        // 每帧贴地校正：动画胯骨高度与挂载时静止姿势不同，一次性补偿会失准
        public static void UpdateGround(GameObject model, float groundY)
        {
            if (model == null)
                return;
            Bounds b;
            if (!TryWorldBounds(model, out b))
                return;
            float lift = groundY - b.min.y;
            if (Mathf.Abs(lift) > 1e-4f)
                model.transform.position += new Vector3(0f, lift, 0f);
        }

        static bool TryWorldBounds(GameObject go, out Bounds bounds)
        {
            Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            bounds = new Bounds(go.transform.position, Vector3.zero);
            for (int i = 0; i < rs.Length; i++)
            {
                if (rs[i] == null || !rs[i].enabled || IsWeaponRenderer(rs[i]))
                    continue;
                if (!any)
                {
                    bounds = rs[i].bounds;
                    any = true;
                }
                else
                    bounds.Encapsulate(rs[i].bounds);
            }

            return any && bounds.size.y > 1e-6f;
        }

        static void PrepareRenderers(GameObject go)
        {
            Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rs.Length; i++)
            {
                rs[i].shadowCastingMode = ShadowCastingMode.Off;
                rs[i].receiveShadows = true;
                var skin = rs[i] as SkinnedMeshRenderer;
                if (skin == null)
                    continue;
                skin.updateWhenOffscreen = true;
                skin.quality = SkinQuality.Auto;
            }
        }

        public static Animator FindAnimator(GameObject dk)
        {
            if (dk == null)
                return null;
            Animator anim = dk.GetComponent<Animator>();
            if (anim == null)
                anim = dk.GetComponentInChildren<Animator>();
            return anim;
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
