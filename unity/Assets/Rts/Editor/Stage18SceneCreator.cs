using System;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage18SceneCreator
    {
        [MenuItem("ProjectAegisRTS/Stage 18/Configure Tester Playability Pass")]
        public static void ConfigureStage18Menu()
        {
            ConfigureStage18();
        }

        public static void ConfigureStage18Batch()
        {
            try
            {
                ConfigureStage18();
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        public static void ConfigureStage18()
        {
            Stage16_5BuildFlowConfigurator.ConfigureBuildFlow();
            Debug.Log("Stage 18 tester playability pass configured.");
        }
    }
}
