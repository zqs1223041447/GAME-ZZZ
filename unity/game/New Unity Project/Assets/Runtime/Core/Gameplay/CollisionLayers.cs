using UnityEngine;

namespace Game.Runtime.Core
{
    public static class CollisionLayers
    {
        public const string PlayerName = "Player";
        public const string MonsterName = "Monster";
        public const int PlayerFallback = 6;
        public const int MonsterFallback = 7;

        public static int Player
        {
            get
            {
                int i = LayerMask.NameToLayer(PlayerName);
                return i >= 0 ? i : PlayerFallback;
            }
        }

        public static int Monster
        {
            get
            {
                int i = LayerMask.NameToLayer(MonsterName);
                return i >= 0 ? i : MonsterFallback;
            }
        }

        public static void Apply()
        {
            int player = Player;
            int monster = Monster;
            Physics.IgnoreLayerCollision(player, monster, true);
            Physics.IgnoreLayerCollision(monster, monster, true);
        }
    }
}
