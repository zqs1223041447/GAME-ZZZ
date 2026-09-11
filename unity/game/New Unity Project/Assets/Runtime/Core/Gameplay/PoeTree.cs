using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>天赋树节点类型（与 passive_tree.json 的 kind 字段一一对应）。</summary>
    public enum PoeNodeKind : byte
    {
        Normal = 0,
        Notable = 1,
        Keystone = 2,
        Mastery = 3,
        Jewel = 4,
        Start = 5
    }

    /// <summary>一颗天赋节点（真实 PoE 数据：坐标由 group + orbit 几何推导，与官方页面一致）。</summary>
    [Serializable]
    public struct PoeNode
    {
        public int index;
        /// <summary>官方 skill id（排障/对照 PoEDB 用；不是本域的索引）。</summary>
        public int skill;
        /// <summary>1 = 树上可见但不可点（时光珠宝类显著点，官方数据里本就没有连线）。</summary>
        public int locked;
        public string name;
        public int kind;
        public float x;
        public float y;
        public int group;
        public int orbit;
        public int orbitIndex;
        /// <summary>图标文件主干名（Resources/UI/PoE/Icons/&lt;icon&gt;.png）。</summary>
        public string icon;
        /// <summary>PoE 原始词条文本，换行分隔（tooltip 真值）。</summary>
        public string stats;
        /// <summary>专精节点（kind=3）的可选效果列表，换行分隔。空串=非专精。生效必须经显式选择，禁止默认首条。</summary>
        public string choices;
        /// <summary>相邻节点索引（无向，已去重升序）。</summary>
        public int[] links;

        public PoeNodeKind Kind { get { return (PoeNodeKind)kind; } }
    }

    /// <summary>天赋簇（绘制底衬圆/半圆；radius = 内容半径，用于把底衬缩放到刚好包住节点）。</summary>
    [Serializable]
    public struct PoeGroup
    {
        public int index;
        public float x;
        public float y;
        /// <summary>1=小圆 2=中圆 3=宽椭圆（对应官方 PSGroupBackground1/2/3 贴图）。</summary>
        public int bg;
        /// <summary>1=半幅贴图（只画半边）。</summary>
        public int half;
        public float radius;
        public int nodes;
    }

    /// <summary>贴图图集切片表（JsonUtility 无字典支持，故坐标以平行数组发布）。</summary>
    [Serializable]
    public class PoeAtlas
    {
        public string file;
        public int w;
        public int h;
        public string[] names;
        public int[] rx;
        public int[] ry;
        public int[] rw;
        public int[] rh;

        public int IndexOf(string name)
        {
            if (names == null || name == null)
                return -1;
            for (int i = 0; i < names.Length; i++)
                if (names[i] == name)
                    return i;
            return -1;
        }

        /// <summary>切片 UV。官方 sprites 表的坐标是「网页图像」原点（左上），Unity 纹理 UV 原点在左下——V 必须翻转，
        /// 否则整张图集会被镜像取样（表现为节点框消失、簇底衬变成乱纹）。</summary>
        public bool TryRect(string name, out Rect rect)
        {
            int i = IndexOf(name);
            if (i < 0)
            {
                rect = default;
                return false;
            }
            rect = new Rect(rx[i] / (float)w, 1f - (ry[i] + rh[i]) / (float)h,
                rw[i] / (float)w, rh[i] / (float)h);
            return true;
        }

        /// <summary>切片像素宽（绘制尺寸推导用；缺名返回 0）。</summary>
        public float PixelW(string name)
        {
            int i = IndexOf(name);
            return i < 0 ? 0f : rw[i];
        }

        public float PixelH(string name)
        {
            int i = IndexOf(name);
            return i < 0 ? 0f : rh[i];
        }
    }

    [Serializable]
    public class PoeChrome
    {
        public PoeAtlas frame;
        public PoeAtlas group;
        public PoeAtlas line;
        public PoeAtlas background;
        public PoeAtlas mastery;
        public PoeAtlas jewel;
    }

    [Serializable]
    public class PoeMeta
    {
        public string source;
        public string art;
        public int nodeCount;
        public int groupCount;
        public int start;
        public int totalPoints;
        public float minX;
        public float minY;
        public float maxX;
        public float maxY;
    }

    [Serializable]
    public class PoeTreeData
    {
        public PoeMeta meta;
        public PoeNode[] nodes;
        public PoeGroup[] groups;
        public PoeChrome chrome;
    }

    /// <summary>
    /// 真实 PoE 天赋树数据层（S5U 导演插入周期）：Resources/UI/PoE/passive_tree.json 懒加载 + 静态缓存。
    /// 数据来源=官方天赋树页面内嵌 tree 数据（节点/坐标/连线/词条真值），美术=PoEDB 同源 CDN。
    /// 只读、无 gameplay 状态；缺失资源返回 null，调用方必须允许回退（永不抛异常）。
    /// </summary>
    public static class PoeTree
    {
        /// <summary>官方 orbit 几何常量（sprites/constants）：弧度半径与每轨槽位数，用于圆弧连线与定位。</summary>
        public static readonly float[] OrbitRadii = { 0f, 82f, 162f, 335f, 493f, 662f, 846f };
        public static readonly float[] SkillsPerOrbit = { 1f, 6f, 16f, 16f, 40f, 72f, 72f };

        public const string Root = "UI/PoE/";
        public const string DataPath = Root + "passive_tree";
        public const string IconDir = Root + "Icons/";
        public const string ChromeDir = Root + "Chrome/";

        static PoeTreeData _data;
        static bool _loaded;
        static readonly Dictionary<string, Texture2D> _icons = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Texture2D> _chrome = new Dictionary<string, Texture2D>();

        public static PoeTreeData Data
        {
            get
            {
                if (!_loaded)
                {
                    _loaded = true;
                    var asset = Resources.Load<TextAsset>(DataPath);
                    if (asset != null)
                    {
                        try { _data = JsonUtility.FromJson<PoeTreeData>(asset.text); }
                        catch (Exception) { _data = null; }
                    }
                }
                return _data;
            }
        }

        public static bool Ready { get { return Data != null && Data.nodes != null && Data.nodes.Length > 0; } }

        public static PoeNode[] Nodes { get { var d = Data; return d == null ? null : d.nodes; } }
        public static PoeGroup[] Groups { get { var d = Data; return d == null ? null : d.groups; } }

        public static int Count
        {
            get
            {
                var n = Nodes;
                return n == null ? 0 : n.Length;
            }
        }

        public static PoeNode Get(int index)
        {
            var n = Nodes;
            if (n == null || index < 0 || index >= n.Length)
                return default;
            return n[index];
        }

        /// <summary>起始节点（官方页面里 Scion 的起点，位于树心）。缺数据时回退 0。</summary>
        public static int StartIndex
        {
            get
            {
                var d = Data;
                if (d == null || d.meta == null)
                    return 0;
                if (d.meta.start < 0 || d.meta.start >= Count)
                    return 0;
                return d.meta.start;
            }
        }



        /// <summary>节点图标（PoEDB 同源 PNG；缺失返回 null）。</summary>
        public static Texture2D Icon(string stem)
        {
            if (string.IsNullOrEmpty(stem))
                return null;
            Texture2D t;
            if (_icons.TryGetValue(stem, out t) && t != null)
                return t;
            t = Resources.Load<Texture2D>(IconDir + stem);
            _icons[stem] = t;
            return t;
        }

        /// <summary>图集贴图（Chrome/&lt;file&gt;；缺失返回 null）。
        /// 注意：Resources.Load 的路径不带扩展名，数据里记的是文件名（frame.png），故先去掉扩展名。</summary>
        public static Texture2D Chrome(string file)
        {
            if (string.IsNullOrEmpty(file))
                return null;
            int dot = file.LastIndexOf('.');
            string stem = dot > 0 ? file.Substring(0, dot) : file;
            Texture2D t;
            if (_chrome.TryGetValue(stem, out t) && t != null)
                return t;
            t = Resources.Load<Texture2D>(ChromeDir + stem);
            _chrome[stem] = t;
            return t;
        }
    }
}
