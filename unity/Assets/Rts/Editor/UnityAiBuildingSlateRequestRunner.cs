using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    [InitializeOnLoad]
    public static class UnityAiBuildingSlateRequestRunner
    {
        const string RequestFileName = "unity_ai_building_slate_capture.request";
        const string DoneFileName = "unity_ai_building_slate_capture.done";
        const string FailedFileName = "unity_ai_building_slate_capture.failed";

        static UnityAiBuildingSlateRequestRunner()
        {
            EditorApplication.delayCall += TryRunRequestedCapture;
        }

        static void TryRunRequestedCapture()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryRunRequestedCapture;
                return;
            }

            var requestPath = GetRequestPath(RequestFileName);
            if (!File.Exists(requestPath))
                return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(requestPath));
                File.Delete(GetRequestPath(DoneFileName));
                File.Delete(GetRequestPath(FailedFileName));
                UnityAiBuildingSlateGenerator.BuildAndCaptureBatch();
                File.WriteAllText(GetRequestPath(DoneFileName), DateTime.Now.ToString("O"));
                File.Delete(requestPath);
                Debug.Log("Unity AI building slate request runner completed capture.");
            }
            catch (Exception ex)
            {
                File.WriteAllText(GetRequestPath(FailedFileName), ex.ToString());
                Debug.LogException(ex);
            }
        }

        static string GetRequestPath(string fileName)
        {
            var unityProjectRoot = Directory.GetParent(Application.dataPath).FullName;
            var repoRoot = Directory.GetParent(unityProjectRoot).FullName;
            return Path.Combine(repoRoot, "build", "requests", fileName);
        }
    }
}
