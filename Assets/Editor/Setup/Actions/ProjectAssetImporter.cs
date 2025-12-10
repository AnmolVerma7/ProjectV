using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Antigravity.Setup.Actions
{
    public static class ProjectAssetImporter
    {
        public static void Apply(ProjectSetupConfig config)
        {
            if (config == null) return;

            foreach (var group in config.AssetStorePackages)
            {
                foreach (var pkgName in group.PackageNames)
                {
                    ImportFromCache(group.Category, pkgName);
                }
            }
        }

        private static void ImportFromCache(string subfolder, string fileName)
        {
            var root = GetDefaultAssetStorePath();
            var fullPath = Path.Combine(root, subfolder, fileName);

            if (File.Exists(fullPath))
            {
                Debug.Log($"<b>Project Setup</b>: Importing {fileName}...");
                AssetDatabase.ImportPackage(fullPath, false);
            }
            else
            {
                Debug.LogWarning($"<b>Project Setup</b>: Could not find package at {fullPath}");
            }
        }

        private static string GetDefaultAssetStorePath()
        {
#if UNITY_EDITOR_WIN
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appData, "Unity", "Asset Store-5.x");
            return Directory.Exists(path) ? path : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif UNITY_EDITOR_OSX
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(home, "Library", "Unity", "Asset Store-5.x");
            return Directory.Exists(path) ? path : home;
#else
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(home, ".local", "share", "unity3d", "Asset Store-5.x");
            return Directory.Exists(path) ? path : home;
#endif
        }
    }
}
