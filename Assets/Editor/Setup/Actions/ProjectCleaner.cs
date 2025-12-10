using System.IO;
using UnityEngine;
using UnityEditor;

namespace Antigravity.Setup.Actions
{
    public static class ProjectCleaner
    {
        public static void Apply(ProjectSetupConfig config)
        {
            if (config == null) return;

            int deletedCount = 0;
            foreach (var path in config.JunkPaths)
            {
                if (AssetDatabase.IsValidFolder(path) || File.Exists(path))
                {
                    if (AssetDatabase.DeleteAsset(path))
                    {
                        Debug.Log($"<b>Project Setup</b>: Deleted {path}");
                        deletedCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"<b>Project Setup</b>: Cleanup complete. Deleted {deletedCount} items.");
        }
    }
}
