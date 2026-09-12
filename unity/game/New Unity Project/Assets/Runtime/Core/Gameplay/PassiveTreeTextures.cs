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
        static int _loadCalls;
        static int _unloadCalls;
        static int _epoch;

        public static int ResourceLoadCalls { get { return _loadCalls; } }
        public static int OwnerUnloadCalls { get { return _unloadCalls; } }
        public static int Epoch { get { return _epoch; } }

        public static int ResidentIconCount { get { return Icons.Count; } }

        public static int ResidentBytes
        {
            get
            {
                int n = 0;
                foreach (var kv in Icons)
                    n += ByteSize(kv.Value);
                n += ByteSize(_frame);
                n += ByteSize(_group);
                return n;
            }
        }

        static int ByteSize(Texture2D t)
        {
            if (t == null)
                return 0;
            return t.width * t.height * 4;
        }
        public static int ResidentChromeCount
        {
            get { return (_frame != null ? 1 : 0) + (_group != null ? 1 : 0); }
        }
        public static int MissingAttemptCount { get { return Missing.Count; } }
        public static Texture2D FrameAtlas { get { return _frame; } }
        public static Texture2D GroupAtlas { get { return _group; } }

        public static bool ResidentIconsMatch(HashSet<string> required)
        {
            if (required == null)
                return Icons.Count == 0;
            if (Icons.Count != required.Count)
                return false;
            foreach (var kv in Icons)
            {
                if (!required.Contains(kv.Key))
                    return false;
            }
            return true;
        }

        public static void Sync(PassiveTreeRenderPlan plan)
        {
            if (plan == null)
            {
                ReleaseAll();
                return;
            }
            var chrome = PoeTree.Data != null ? PoeTree.Data.chrome : null;
            if (plan.NeedFrameAtlas && chrome != null && chrome.frame != null)
            {
                if (_frame == null)
                {
                    _loadCalls++;
                    _frame = LoadChrome(chrome.frame.file);
                }
            }
            else if (_frame != null)
            {
                _unloadCalls++;
                _frame = null;
            }
            if (plan.NeedGroupAtlas && chrome != null && chrome.group != null)
            {
                if (_group == null)
                {
                    _loadCalls++;
                    _group = LoadChrome(chrome.group.file);
                }
            }
            else if (_group != null)
            {
                _unloadCalls++;
                _group = null;
            }

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
            {
                Icons.Remove(drop[i]);
                _unloadCalls++;
            }
            foreach (string stem in plan.RequiredIconStems)
            {
                if (Icons.ContainsKey(stem) || Missing.Contains(stem))
                    continue;
                _loadCalls++;
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
            _unloadCalls += Icons.Count;
            ReleaseIcons();
            _frame = null;
            _group = null;
            Missing.Clear();
            _epoch++;
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
