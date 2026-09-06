using UnityEngine;
using Game.Runtime.Core;

namespace Game.Runtime.Content
{
    public sealed class BootstrapRunner : MonoBehaviour
    {
        public const uint BootstrapSeed = 0xC0FFEE;

        void Start()
        {
            var rng = new SeededRng(BootstrapSeed);
            GameLog.Info("Bootstrap", "SeededRng created with seed 0x" + BootstrapSeed.ToString("X"));

            // Touch the generator so Play-mode logs prove a live sequence.
            GameLog.Info("Bootstrap", "First NextUInt=" + rng.NextUInt());

            string directory = ContentPaths.ValidDirectory;
            if (!ContentDatabase.TryLoadAll(directory, out ContentDatabase db, out var errors))
            {
                GameLog.Error("Bootstrap", "Content load failed from '" + directory + "': " + string.Join("; ", errors));
                return;
            }

            GameLog.Info("Bootstrap", "Content load succeeded from '" + directory + "', count=" + db.Count);
        }
    }
}
