using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Antigravity.Setup.Actions
{
    public static class ProjectPackageInstaller
    {
        private static Queue<string> _queue = new Queue<string>();
        private static AddRequest _currentRequest;
        private static bool _isInProgress;

        public static void Apply(ProjectSetupConfig config)
        {
            if (config == null) return;

            var allPackages = config.UnityRegistryPackages.Concat(config.GitPackages);
            foreach (var pkg in allPackages)
            {
                _queue.Enqueue(pkg);
            }

            if (_isInProgress)
            {
                Debug.Log("<b>Project Setup</b>: Package installation already in progress. Added to queue.");
                return;
            }

            _isInProgress = true;
            EditorApplication.update += UpdateLoop;
            ProcessNext();
        }

        private static void ProcessNext()
        {
            if (_queue.Count == 0)
            {
                _isInProgress = false;
                EditorApplication.update -= UpdateLoop;
                _currentRequest = null;
                Debug.Log("<b>Project Setup</b>: All packages installed.");
                return;
            }

            var packageId = _queue.Dequeue();
            Debug.Log($"<b>Project Setup</b>: Installing {packageId}...");
            _currentRequest = Client.Add(packageId);
        }

        private static void UpdateLoop()
        {
            if (_currentRequest == null) return;

            if (_currentRequest.IsCompleted)
            {
                if (_currentRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"<b>Project Setup</b>: Successfully installed {_currentRequest.Result.packageId}");
                }
                else if (_currentRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"<b>Project Setup</b>: Failed to install package. {_currentRequest.Error?.message}");
                }

                _currentRequest = null;
                // Small delay to let Editor catch up
                ProcessNext();
            }
        }
    }
}
