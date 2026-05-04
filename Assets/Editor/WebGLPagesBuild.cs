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

            ApplyCacheBustShell(outputPath, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            UnityEngine.Debug.Log($"WebGL GitHub Pages build completed: {outputPath}");
        }

        private static void ApplyWebGLSettings()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            PlayerSettings.WebGL.template = WebGLTemplate;
            PlayerSettings.WebGL.dataCaching = false;
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

        private static void ApplyCacheBustShell(string outputPath, string buildVersion)
        {
            var indexPath = Path.Combine(outputPath, "index.html");
            var index = File.ReadAllText(indexPath);
            index = index.Replace(
                "var buildUrl = \"Build\";\n      var loaderUrl = buildUrl + \"/docs.loader.js\";",
                $"var buildVersion = \"{buildVersion}\";\n      var buildUrl = \"Build\";\n      var loaderUrl = buildUrl + \"/docs.loader.js?v=\" + buildVersion;");
            index = index.Replace(
                "dataUrl: buildUrl + \"/docs.data\",\n        frameworkUrl: buildUrl + \"/docs.framework.js\",\n        codeUrl: buildUrl + \"/docs.wasm\",",
                "dataUrl: buildUrl + \"/docs.data?v=\" + buildVersion,\n        frameworkUrl: buildUrl + \"/docs.framework.js?v=\" + buildVersion,\n        codeUrl: buildUrl + \"/docs.wasm?v=\" + buildVersion,");
            index = index.Replace(
                "productVersion: \"1.0\",",
                "productVersion: buildVersion,");
            index = index.Replace(
                "window.addEventListener(\"load\", function () {\n        if (\"serviceWorker\" in navigator) {\n          navigator.serviceWorker.register(\"ServiceWorker.js\");\n        }\n      });",
                "window.addEventListener(\"load\", function () {\n        if (\"serviceWorker\" in navigator) {\n          navigator.serviceWorker.getRegistrations()\n            .then((registrations) => registrations.forEach((registration) => registration.unregister()));\n        }\n        if (\"caches\" in window) {\n          caches.keys().then((keys) => Promise.all(keys.map((key) => caches.delete(key))));\n        }\n      });");
            File.WriteAllText(indexPath, index);

            var serviceWorkerPath = Path.Combine(outputPath, "ServiceWorker.js");
            File.WriteAllText(
                serviceWorkerPath,
                "self.addEventListener('install', (event) => { self.skipWaiting(); });\n" +
                "self.addEventListener('activate', (event) => {\n" +
                "  event.waitUntil(caches.keys().then((keys) => Promise.all(keys.map((key) => caches.delete(key)))).then(() => self.clients.claim()));\n" +
                "});\n" +
                "self.addEventListener('fetch', (event) => {\n" +
                "  if (event.request.method === 'GET') {\n" +
                "    event.respondWith(fetch(event.request));\n" +
                "  }\n" +
                "});\n");
        }
    }
}
