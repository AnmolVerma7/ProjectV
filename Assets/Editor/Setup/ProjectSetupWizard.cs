using UnityEngine;
using UnityEditor;
using Antigravity.Setup.Actions;

namespace Antigravity.Setup
{
    public class ProjectSetupWizard : EditorWindow
    {
        private ProjectSetupConfig _config;
        private Vector2 _scrollPos;

        [MenuItem("Tools/Antigravity/Project Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<ProjectSetupWizard>("Project Setup");
        }

        private void OnEnable()
        {
            // Try to find existing config
            var guids = AssetDatabase.FindAssets("t:ProjectSetupConfig");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _config = AssetDatabase.LoadAssetAtPath<ProjectSetupConfig>(path);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Antigravity Project Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _config = (ProjectSetupConfig)EditorGUILayout.ObjectField("Configuration", _config, typeof(ProjectSetupConfig), false);

            if (_config == null)
            {
                EditorGUILayout.HelpBox("Please create or assign a ProjectSetupConfig to proceed.", MessageType.Warning);
                if (GUILayout.Button("Create New Config"))
                {
                    CreateConfig();
                }
                return;
            }

            EditorGUILayout.Space();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawSection("1. Folder Structure", "Create standard folder hierarchy", () => FolderCreator.Apply(_config));
            DrawSection("2. Packages", "Install Unity Registry & Git packages", () => ProjectPackageInstaller.Apply(_config));
            DrawSection("3. Asset Store", "Import standard assets from cache", () => ProjectAssetImporter.Apply(_config));
            DrawSection("4. Scene Setup", "Create Base Scene & Wire URP", () => SceneSetup.Apply(_config));
            DrawSection("5. Cleanup", "Delete default project junk", () => ProjectCleaner.Apply(_config));
            DrawSection("6. Styling", "Apply VFolders & VHierarchy colors", () => ProjectStyler.Apply(_config));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            if (GUILayout.Button("Run All Steps", GUILayout.Height(40)))
            {
                RunAll();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSection(string title, string description, System.Action action)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Label(description, EditorStyles.miniLabel);
            if (GUILayout.Button("Run"))
            {
                action.Invoke();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void RunAll()
        {
            FolderCreator.Apply(_config);
            ProjectPackageInstaller.Apply(_config);
            ProjectAssetImporter.Apply(_config);
            SceneSetup.Apply(_config);
            ProjectCleaner.Apply(_config);
            ProjectStyler.Apply(_config);
        }

        private void CreateConfig()
        {
            var config = ScriptableObject.CreateInstance<ProjectSetupConfig>();
            AssetDatabase.CreateAsset(config, "Assets/Editor/Setup/ProjectSetupConfig.asset");
            AssetDatabase.SaveAssets();
            _config = config;
            EditorGUILayout.ObjectField("Configuration", _config, typeof(ProjectSetupConfig), false);
        }
    }
}
