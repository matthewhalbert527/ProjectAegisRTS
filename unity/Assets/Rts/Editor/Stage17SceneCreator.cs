using System;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage17SceneCreator
    {
        [MenuItem("ProjectAegisRTS/Stage 17/Configure Player-Facing Polish")]
        public static void ConfigureStage17Menu()
        {
            ConfigureStage17();
        }

        public static void ConfigureStage17Batch()
        {
            try
            {
                ConfigureStage17();
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

        public static void ConfigureStage17()
        {
            Stage16_5BuildFlowConfigurator.ConfigureBuildFlow();
            Debug.Log("Stage 17 player-facing polish configured.");
        }
    }
}
