using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Antigravity.Setup.Actions
{
    public static class SceneSetup
    {
        private const string BaseSceneFolder = "Assets/Project/Scenes";
        private const string BaseSceneName = "BasicScene";
        private const string SampleScenePath = "Assets/Project/Scenes/SampleScene.unity"; // Updated path

        public static void Apply(ProjectSetupConfig config)
        {
            if (!Directory.Exists(BaseSceneFolder))
                Directory.CreateDirectory(BaseSceneFolder);

            // 1. Scene Creation (Copy & Delete SampleScene)
            var scenePath = $"{BaseSceneFolder}/{BaseSceneName}.unity";

            if (File.Exists(SampleScenePath))
            {
                // Copy SampleScene to BasicScene
                AssetDatabase.CopyAsset(SampleScenePath, scenePath);
                AssetDatabase.Refresh();
                
                // Delete SampleScene
                AssetDatabase.DeleteAsset(SampleScenePath);
                Debug.Log($"<b>Project Setup</b>: Copied SampleScene to {scenePath} and deleted original.");
            }
            else if (!File.Exists(scenePath))
            {
                // Fallback: Create new if BasicScene doesn't exist and SampleScene is gone
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
                Debug.Log($"<b>Project Setup</b>: Created new BasicScene at {scenePath} (SampleScene not found).");
            }

            // Open the BasicScene
            EditorSceneManager.OpenScene(scenePath);

            // 2. Hierarchy Setup
            SetupHierarchy();

            // 3. URP Setup
            SetupURP();
            
            // 4. Styling
            ProjectStyler.ApplyHierarchyStyles();

            // 5. Save
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            Debug.Log($"<b>Project Setup</b>: Base scene setup complete.");
        }

        private static void SetupHierarchy()
        {
            // Create Roots
            var uiRoot = GetOrCreateGO("UI");
            var camRoot = GetOrCreateGO("Cameras");
            var levelRoot = GetOrCreateGO("Level");

            // Setup Camera
            var mainCam = GameObject.Find("Main Camera");
            if (mainCam == null)
            {
                mainCam = new GameObject("Main Camera");
                mainCam.AddComponent<Camera>().tag = "MainCamera";
            }
            mainCam.transform.SetParent(camRoot.transform);
            
            // Setup Light
            var light = GameObject.Find("Directional Light");
            if (light == null)
            {
                light = new GameObject("Directional Light");
                var l = light.AddComponent<Light>();
                l.type = LightType.Directional;
            }
            light.transform.SetParent(levelRoot.transform);

            // Setup Global Volume
            var volGO = GameObject.Find("Global Volume");
            if (volGO == null)
            {
                volGO = new GameObject("Global Volume");
                volGO.AddComponent<Volume>().isGlobal = true;
            }
            volGO.transform.SetParent(camRoot.transform);
            
            SetupVolumeProfile(volGO.GetComponent<Volume>());
        }

        private static void SetupVolumeProfile(Volume vol)
        {
            // Create or Load Profile
            var profilePath = "Assets/Project/DefaultVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
            }
            vol.sharedProfile = profile;

            // Add Overrides
            // 1. Tonemapping
            if (!profile.TryGet(out Tonemapping tonemapping)) tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.value = TonemappingMode.Neutral;
            tonemapping.mode.overrideState = true;

            // 2. Bloom
            if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 1f;
            bloom.threshold.overrideState = true;
            bloom.intensity.value = 0.25f;
            bloom.intensity.overrideState = true;
            bloom.scatter.value = 0.5f;
            bloom.scatter.overrideState = true;
            bloom.tint.value = Color.white;
            bloom.tint.overrideState = true;
            bloom.clamp.value = 65472f;
            bloom.clamp.overrideState = true;
            bloom.highQualityFiltering.value = true;
            bloom.highQualityFiltering.overrideState = true;

            // 3. Vignette
            if (!profile.TryGet(out Vignette vignette)) vignette = profile.Add<Vignette>(true);
            vignette.color.value = Color.black;
            vignette.color.overrideState = true;
            vignette.center.value = new Vector2(0.5f, 0.5f);
            vignette.center.overrideState = true;
            vignette.intensity.value = 0.2f;
            vignette.intensity.overrideState = true;
            vignette.smoothness.value = 0.2f;
            vignette.smoothness.overrideState = true;
            vignette.rounded.value = false;
            vignette.rounded.overrideState = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        private static void SetupURP()
        {
#if UNITY_2019_3_OR_NEWER
            // Find URP Asset
            var rpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset"); // Example path
            if (rpAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = rpAsset;
                Debug.Log("<b>Project Setup</b>: Assigned Render Pipeline Asset.");
            }
#endif
        }

        private static GameObject GetOrCreateGO(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go : new GameObject(name);
        }
    }
}
