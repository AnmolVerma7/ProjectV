using UnityEngine;
using UnityEditor;
using System.Linq;
using VFolders;
using VHierarchy; // Ensure we can access VHierarchy types

namespace Antigravity.Setup.Actions
{
    public static class ProjectStyler
    {
        public static void Apply(ProjectSetupConfig config)
        {
            // --- VFolders Initialization ---
            if (VFolders.VFolders.data == null)
            {
                var guid = AssetDatabase.FindAssets("t:VFoldersData").FirstOrDefault();
                if (!string.IsNullOrEmpty(guid))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    VFolders.VFolders.data = AssetDatabase.LoadAssetAtPath<VFoldersData>(path);
                }
                else
                {
                    var newData = ScriptableObject.CreateInstance<VFoldersData>();
                    var folder = "Assets/Unity Imported Assets/vFolders";
                    if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Unity Imported Assets", "vFolders");
                    AssetDatabase.CreateAsset(newData, $"{folder}/VFolders Data.asset");
                    VFolders.VFolders.data = newData;
                    Debug.Log("<b>Project Setup</b>: Created VFolders Data.");
                }
            }

            // --- VHierarchy Initialization ---
            if (VHierarchy.VHierarchy.data == null)
            {
                var guid = AssetDatabase.FindAssets("t:VHierarchyData").FirstOrDefault();
                if (!string.IsNullOrEmpty(guid))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    VHierarchy.VHierarchy.data = AssetDatabase.LoadAssetAtPath<VHierarchyData>(path);
                }
                else
                {
                    var newData = ScriptableObject.CreateInstance<VHierarchyData>();
                    var folder = "Assets/Unity Imported Assets/vHierarchy";
                    if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Unity Imported Assets", "vHierarchy");
                    AssetDatabase.CreateAsset(newData, $"{folder}/VHierarchy Data.asset");
                    VHierarchy.VHierarchy.data = newData;
                    Debug.Log("<b>Project Setup</b>: Created VHierarchy Data.");
                }
            }

            // --- Apply Styles ---
            if (VFolders.VFolders.data != null) ApplyFolderStyles();
            else Debug.LogError("<b>Project Setup</b>: VFolders Data missing. Folder styling skipped.");

            if (VHierarchy.VHierarchy.data != null) ApplyHierarchyStyles();
            else Debug.LogError("<b>Project Setup</b>: VHierarchy Data missing. Hierarchy styling skipped.");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ApplyFolderStyles()
        {
            const int COLOR_BUILTIN = 6;
            const int COLOR_IMPORTED = 7;

            VFolders.VFolders.SetColor("Assets/Unity Built-In", COLOR_BUILTIN, recursive: false);
            VFolders.VFolders.SetColor("Assets/Unity Imported Assets", COLOR_IMPORTED, recursive: false);
            VFolders.VFolders.SetColor("Assets/Unity Input System", COLOR_IMPORTED, recursive: false);
            
            Debug.Log("<b>Project Setup</b>: Folder styling applied.");
        }

        public static void ApplyHierarchyStyles()
        {
            // Colors
            const int BLACK = 1;
            const int GREEN = 5;
            const int BLUE = 7;

            // Find objects
            var ui = GameObject.Find("UI");
            var cameras = GameObject.Find("Cameras");
            var level = GameObject.Find("Level");
            var mainCam = GameObject.Find("Main Camera");
            var globalVol = GameObject.Find("Global Volume");
            var dirLight = GameObject.Find("Directional Light");

            // Apply Rules
            if (ui)
            {
                VHierarchy.VHierarchy.SetColor(ui, BLACK);
                VHierarchy.VHierarchy.SetIcon(ui, "Canvas Icon");
            }

            if (cameras)
            {
                VHierarchy.VHierarchy.SetColor(cameras, BLUE);
                VHierarchy.VHierarchy.SetIcon(cameras, "Camera Icon");
            }

            if (level)
            {
                VHierarchy.VHierarchy.SetColor(level, GREEN);
                VHierarchy.VHierarchy.SetIcon(level, "Terrain Icon");
            }

            if (mainCam) VHierarchy.VHierarchy.SetIcon(mainCam, "Camera Icon");
            if (globalVol) VHierarchy.VHierarchy.SetIcon(globalVol, "GameObject Icon");
            if (dirLight) VHierarchy.VHierarchy.SetIcon(dirLight, "Light Icon");

            Debug.Log("<b>Project Setup</b>: Hierarchy styling applied.");
        }
    }
}
