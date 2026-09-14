using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Headless WebGL build entry point. Invoked via:
//   Unity.exe -quit -batchmode -executeMethod WebBuilder.BuildWeb
public static class WebBuilder
{
    const string OutputPath = @"C:\Users\Huti\Downloads\SteelFang_Web";

    public static void BuildWeb()
    {
        try
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .Distinct()
                .ToArray();

            Debug.Log("[WebBuilder] scenes: " + string.Join(" | ", scenes));

            if (scenes.Length == 0)
            {
                Debug.LogError("[WebBuilder] no enabled scenes in build settings");
                EditorApplication.Exit(2);
                return;
            }

            // Must match the Content-Encoding headers in SteelFang_Web/vercel.json
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None
            };

            Debug.Log("[WebBuilder] starting build -> " + OutputPath);
            var summary = BuildPipeline.BuildPlayer(opts).summary;
            Debug.Log($"[WebBuilder] RESULT={summary.result} errors={summary.totalErrors} warnings={summary.totalWarnings} bytes={summary.totalSize}");

            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
        catch (Exception e)
        {
            Debug.LogError("[WebBuilder] EXCEPTION: " + e);
            EditorApplication.Exit(3);
        }
    }
}
