using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Portfolio.Build
{
    public static class WebGLPagesBuild
    {
        private const string OutputDirectoryName = "docs";
        private const string WebGLTemplate = "APPLICATION:PWA";

        [MenuItem("Portfolio/Build WebGL for GitHub Pages")]
        public static void Build()
        {
            ApplyWebGLSettings();

            var outputPath = GetOutputPath();
            PrepareOutputDirectory(outputPath);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            EnsureNoJekyll(outputPath);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"WebGL build failed: {report.summary.result}");
            }

            UnityEngine.Debug.Log($"WebGL GitHub Pages build completed: {outputPath}");
        }

        private static void ApplyWebGLSettings()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            PlayerSettings.WebGL.template = WebGLTemplate;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.nameFilesAsHashes = false;
        }

        private static string GetOutputPath()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new InvalidOperationException("Could not resolve project root from Application.dataPath.");
            }

            return Path.Combine(projectRoot, OutputDirectoryName);
        }

        private static void PrepareOutputDirectory(string outputPath)
        {
            Directory.CreateDirectory(outputPath);

            foreach (var directoryName in new[] { "Build", "TemplateData" })
            {
                var directory = Path.Combine(outputPath, directoryName);
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }

            foreach (var fileName in new[] { "index.html", "ServiceWorker.js", "manifest.webmanifest", ".nojekyll" })
            {
                var file = Path.Combine(outputPath, fileName);
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private static void EnsureNoJekyll(string outputPath)
        {
            File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), string.Empty);
        }
    }
}
