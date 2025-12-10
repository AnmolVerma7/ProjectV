using System.Collections.Generic;
using UnityEngine;

namespace Antigravity.Setup
{
    [CreateAssetMenu(fileName = "ProjectSetupConfig", menuName = "Antigravity/Project Setup Config")]
    public class ProjectSetupConfig : ScriptableObject
    {
        [Header("Folder Structure")]
        [Tooltip("Root folders and their subfolders")]
        public List<FolderRule> Folders = new List<FolderRule>
        {
            new FolderRule { Root = "Assets/Project", SubFolders = new List<string> {
                "Animation", "Art", "Audio", "Materials", "Models", "Prefabs", 
                "Scenes", "Scripts/Runtime", "Scripts/Editor", "Scripts/ScriptableObjects", 
                "Shaders", "UI", "VFX" 
            }},
            new FolderRule { Root = "Assets/Unity Input System", SubFolders = new List<string>() },
            new FolderRule { Root = "Assets/Unity Built-In", SubFolders = new List<string>() },
            new FolderRule { Root = "Assets/Unity Imported Assets", SubFolders = new List<string>() }
        };

        [Header("Packages")]
        public List<string> UnityRegistryPackages = new List<string>
        {
            "com.unity.render-pipelines.universal",
            "com.unity.shadergraph",
            "com.unity.postprocessing",
            "com.unity.ai.navigation",
            "com.unity.probuilder"
        };

        public List<string> GitPackages = new List<string>
        {
            "git+https://github.com/KyleBanks/scene-ref-attribute"
        };

        [Header("Asset Store Imports")]
        [Tooltip("Relative paths from default Asset Store cache")]
        public List<AssetStorePackage> AssetStorePackages = new List<AssetStorePackage>
        {
            new AssetStorePackage { Category = "kubacho lab/Editor ExtensionsUtilities", PackageNames = new List<string> {
                "vFolders 2.unitypackage", "vHierarchy 2.unitypackage", "vInspector 2.unitypackage", "vTabs 2.unitypackage"
            }}
        };

        [Header("Cleanup")]
        public List<string> JunkPaths = new List<string>
        {
            "Assets/TutorialInfo",
            "Assets/Readme.asset"
        };
    }

    [System.Serializable]
    public class FolderRule
    {
        public string Root;
        public List<string> SubFolders;
    }

    [System.Serializable]
    public class AssetStorePackage
    {
        public string Category;
        public List<string> PackageNames;
    }
}
