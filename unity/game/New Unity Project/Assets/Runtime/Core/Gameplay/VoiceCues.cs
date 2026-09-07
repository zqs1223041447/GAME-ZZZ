using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// 玩家人声提示（独立于 AudioEvents 战斗 SFX 五键，不共享查找表）。
    /// 查找路径 Resources/Audio/Voice/<键>；未配置=静音。
    /// 冷却由调用方按键给定：Cast 1.5s（防 0.12s 施法轮转每发都播）、Hit 0.6s、Death 每次进入 Dead 由调用门保证一次。
    /// </summary>
    public static class VoiceCues
    {
        static readonly Dictionary<string, AudioClip> _table = new Dictionary<string, AudioClip>
        {
            { "Cast", null },
            { "Hit", null },
            { "Death", null }
        };
        static readonly HashSet<string> _probed = new HashSet<string>();
        static readonly Dictionary<string, float> _lastPlay = new Dictionary<string, float>();
        static AudioSource _source;

        public static void Play(string key, float cooldownSec)
        {
            AudioClip clip = ClipFor(key);
            if (clip == null)
                return;
            float now = Time.unscaledTime;
            float last;
            if (_lastPlay.TryGetValue(key, out last) && now - last < cooldownSec)
                return;
            _lastPlay[key] = now;
            Source().PlayOneShot(clip);
        }

        static AudioClip ClipFor(string key)
        {
            if (!_probed.Contains(key))
            {
                _probed.Add(key);
                _table[key] = Resources.Load<AudioClip>("Audio/Voice/" + key);
            }
            return _table[key];
        }

        static AudioSource Source()
        {
            if (_source == null)
            {
                var go = new GameObject("VoiceCue");
                if (Application.isPlaying)
                    Object.DontDestroyOnLoad(go);
                _source = go.AddComponent<AudioSource>();
                _source.playOnAwake = false;
            }
            return _source;
        }
    }
}
