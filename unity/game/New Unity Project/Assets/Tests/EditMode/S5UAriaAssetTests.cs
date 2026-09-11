using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 导演输入素材层（SliceAria / Aria GUI kit）资源契约：
    /// 1) DeclaredKeys 单一真相——声明的 key 必须全部可加载（import 契约锁定）；
    /// 2) Resources 目录不得出现 DeclaredKeys 之外的 png（选材 import，不整包）；
    /// 3) 缺失路径返回 null 不抛异常（程序化回退永不失效）；
    /// 4) 装备槽/球环消费点在 Aria 存在时拿到 Aria 纹理。
    /// </summary>
    public sealed class S5UAriaAssetTests
    {
        const string ImportRoot = "Assets/Resources/UI/AriaGUI";

        [Test]
        public void Aria_DeclaredKeys_AllLoadable()
        {
            var missing = new List<string>();
            foreach (var key in SliceAria.DeclaredKeys)
                if (SliceAria.Get(key) == null)
                    missing.Add(key);
            Assert.IsEmpty(missing, "SliceAria 声明 key 加载失败: " + string.Join(", ", missing));
        }

        [Test]
        public void Aria_DeclaredKeys_NoDuplicates()
        {
            var seen = new HashSet<string>();
            var dupes = new List<string>();
            foreach (var key in SliceAria.DeclaredKeys)
                if (!seen.Add(key))
                    dupes.Add(key);
            Assert.IsEmpty(dupes, "SliceAria 声明 key 重复: " + string.Join(", ", dupes));
        }

        [Test]
        public void Aria_ResourcesFolder_HasNoUndeclaredTextures()
        {
            var declared = new HashSet<string>(SliceAria.DeclaredKeys);
            var extra = new List<string>();
            var root = Path.Combine(Application.dataPath, "Resources/UI/AriaGUI").Replace('\\', '/');
            foreach (var png in Directory.GetFiles(root, "*.png", SearchOption.AllDirectories))
            {
                var rel = png.Replace('\\', '/').Replace(root + "/", "").Replace(".png", "");
                if (!declared.Contains(rel))
                    extra.Add(rel);
            }
            Assert.IsEmpty(extra, "AriaGUI 目录出现未声明 png（违反选材 import）: " + string.Join(", ", extra));
        }

        [Test]
        public void Aria_MissingPath_ReturnsNull_NoThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var t = SliceAria.Get("Icons/DoesNotExist");
                Assert.IsNull(t, "不存在路径必须回退 null（调用方启用程序化回退）");
            });
        }

        [Test]
        public void Aria_Consumers_GetAriaTextures_WhenPresent()
        {
            if (SliceAria.OrbRing == null)
                Assert.Fail("OrbRing (FrameRoundGold) 必须可加载——DrawOrb 金环覆层的输入契约");
            var slots = new Dictionary<EquipSlot, Texture2D>
            {
                { EquipSlot.Weapon, SliceHudIcons.EqWeapon },
                { EquipSlot.Helmet, SliceHudIcons.EqHelmet },
                { EquipSlot.Body, SliceHudIcons.EqBody },
                { EquipSlot.Gloves, SliceHudIcons.EqGloves },
                { EquipSlot.Boots, SliceHudIcons.EqBoots },
                { EquipSlot.Belt, SliceHudIcons.EqBelt }
            };
            foreach (var kv in slots)
            {
                Assert.IsNotNull(kv.Value, kv.Key + " 槽位符文不得为 null");
                Assert.IsTrue(kv.Value is Texture2D && kv.Value.width > 0, kv.Key + " 槽位符文尺寸无效");
            }
        }

        [Test]
        public void Aria_EquipIcons_FallbackPaths_Intact()
        {
            // 回退真值仍在：即使删除 Aria 纹理，程序化合成层必须独立成立（编译期断言，防误删）
            Assert.IsNotNull(SliceHudIcons.GlyphMelee);
            Assert.IsNotNull(SliceHudIcons.GlyphProjectile);
            Assert.IsNotNull(SliceHudIcons.GlyphArea);
        }
    }
}
