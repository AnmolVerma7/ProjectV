using System.IO;
using UnityEngine;
using UnityEditor;

namespace Antigravity.Setup.Actions
{
    public static class FolderCreator
    {
        public static void Apply(ProjectSetupConfig config)
        {
            if (config == null) return;

            foreach (var rule in config.Folders)
            {
                EnsureDir(rule.Root);
                foreach (var sub in rule.SubFolders)
                {
                    EnsureDir(Path.Combine(rule.Root, sub));
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("<b>Project Setup</b>: Folders created.");
        }

        private static void EnsureDir(string path)
        {
            path = path.Replace('\\', '/');
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
