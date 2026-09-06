using System.IO;
using UnityEngine;

namespace Game.Runtime.Content
{
    public static class ContentPaths
    {
        public static string ProjectRoot
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
        }

        public static string ValidDirectory
        {
            get { return Path.Combine(ProjectRoot, "ContentData", "valid"); }
        }

        public static string InvalidDirectory
        {
            get { return Path.Combine(ProjectRoot, "ContentData", "invalid"); }
        }
    }
}
