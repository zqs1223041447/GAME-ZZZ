using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S6P-WO-05：被动树视觉纹理常驻 owner。只加载当前 render plan 需要的 individual icons。
    /// 禁止每帧 UnloadUnusedAssets / GC.Collect。
    /// </summary>
    public static class PassiveTreeTextures
    {
        static readonly Dictionary<string, Texture2D> Icons = new Dictionary<string, Texture2D>();
        static readonly HashSet<string> Missing = new HashSet<string>();
        static Texture2D _frame;
        static Texture2D _group;

        public static int ResidentIconCount { get { return Icons.Count; } }
        public static int MissingAttemptCount { get { return Missing.Count; } }
        public static Texture2D FrameAtlas { get { return _frame; } }
        public static Texture2D GroupAtlas { get { return _group; } }

        public static void Sync(PassiveTreeRenderPlan plan)
        {
            if (plan == null)
            {
                ReleaseAll();
                return;
            }
            var chrome = PoeTree.Data != null ? PoeTree.Data.chrome : null;
            if (plan.NeedFrameAtlas && chrome != null && chrome.frame != null)
                _frame = LoadChrome(chrome.frame.file);
            else
                _frame = null;
            if (plan.NeedGroupAtlas && chrome != null && chrome.group != null)
                _group = LoadChrome(chrome.group.file);
            else
                _group = null;

            if (plan.RequiredIconStems == null || plan.RequiredIconStems.Count == 0)
            {
                ReleaseIcons();
                return;
            }
            var drop = new List<string>();
            foreach (var kv in Icons)
            {
                if (!plan.RequiredIconStems.Contains(kv.Key))
                    drop.Add(kv.Key);
            }
            for (int i = 0; i < drop.Count; i++)
                Icons.Remove(drop[i]);
            foreach (string stem in plan.RequiredIconStems)
            {
                if (Icons.ContainsKey(stem) || Missing.Contains(stem))
                    continue;
                Texture2D t = Resources.Load<Texture2D>(PoeTree.IconDir + stem);
                if (t == null)
                    Missing.Add(stem);
                else
                    Icons[stem] = t;
            }
        }

        public static Texture2D Icon(string stem)
        {
            if (string.IsNullOrEmpty(stem))
                return null;
            Texture2D t;
            if (Icons.TryGetValue(stem, out t))
                return t;
            return null;
        }

        public static void ReleaseAll()
        {
            ReleaseIcons();
            _frame = null;
            _group = null;
        }

        static Texture2D LoadChrome(string file)
        {
            if (string.IsNullOrEmpty(file))
                return null;
            int dot = file.LastIndexOf('.');
            string stem = dot > 0 ? file.Substring(0, dot) : file;
            return Resources.Load<Texture2D>(PoeTree.ChromeDir + stem);
        }

        static void ReleaseIcons()
        {
            Icons.Clear();
        }
    }
}
