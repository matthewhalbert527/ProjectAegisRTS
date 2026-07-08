using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class ProjectAegisProductionUnitVisualIntegrationValidator
    {
        sealed class UnitSpec
        {
            public string ActorTypeId;
            public string DefinitionPath;
            public string PrefabPath;
            public Type RequiredRigType;
            public int MinimumVertexCount;
            public float ExpectedVisualScale;
            public int ExpectedWheelCountPerSide;
            public string[] RequiredTransforms;
        }

        static readonly string[] CommonTankTransforms =
        {
            "Root", "VisualRoot", "BodyRoot", "TurretRoot", "BarrelRoot", "MuzzlePrimary",
            "TrackLeft", "TrackRight", "SelectionAnchor", "HealthBarAnchor", "UiAnchor",
            "VfxSmoke", "VfxExplosion"
        };

        static readonly string[] HeavyTankTransforms =
        {
            "Root", "VisualRoot", "BodyRoot", "TurretRoot", "BarrelRoot", "MuzzlePrimary",
            "MuzzleSecondary", "MissileLauncherRoot", "MuzzleMissile01", "MuzzleMissile08",
            "TrackLeft", "TrackRight", "SelectionAnchor", "HealthBarAnchor", "UiAnchor",
            "VfxSmoke", "VfxExplosion"
        };

        static readonly string[] HarvesterTransforms =
        {
            "Root", "VisualRoot", "BodyRoot", "TrackLeft", "TrackRight", "MachineGunRoot",
            "MuzzleDefense", "ResourceIntake", "CutterDrumRoot", "AnimatedCutterDrum",
            "HarvesterDock", "SelectionAnchor", "HealthBarAnchor", "UiAnchor",
            "VfxSmoke", "VfxExplosion"
        };

        static readonly string[] WheeledCombatTransforms =
        {
            "Root", "VisualRoot", "BodyRoot", "TurretRoot", "BarrelRoot", "MuzzlePrimary",
            "WheelLeft", "WheelRight", "SelectionAnchor", "HealthBarAnchor", "UiAnchor",
            "VfxSmoke", "VfxExplosion", "LightRoot", "AimPivot"
        };

        static readonly string[] ApcTransforms =
        {
            "Root", "VisualRoot", "BodyRoot", "TurretRoot", "BarrelRoot", "MuzzlePrimary",
            "WheelLeft", "WheelRight", "RearRamp", "TransportExit", "SelectionAnchor",
            "HealthBarAnchor", "UiAnchor", "VfxSmoke", "VfxExplosion", "LightRoot", "AimPivot"
        };

        static readonly UnitSpec[] Units =
        {
            new UnitSpec
            {
                ActorTypeId = "light_tank",
                DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/light_tank_visual.asset",
                PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/light_tank/light_tank.prefab",
                RequiredRigType = typeof(TankVisualRigController),
                MinimumVertexCount = 400,
                ExpectedVisualScale = 0.34f,
                RequiredTransforms = CommonTankTransforms
            },
            new UnitSpec
            {
                ActorTypeId = "medium_tank",
                DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/medium_tank_visual.asset",
                PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/medium_tank/medium_tank.prefab",
                RequiredRigType = typeof(TankVisualRigController),
                MinimumVertexCount = 400,
                ExpectedVisualScale = 0.36f,
                RequiredTransforms = CommonTankTransforms
            },
            new UnitSpec
            {
                ActorTypeId = "heavy_tank",
                DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/heavy_tank_visual.asset",
                PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/heavy_tank/heavy_tank.prefab",
                RequiredRigType = typeof(TankVisualRigController),
                MinimumVertexCount = 500,
                ExpectedVisualScale = 0.38f,
                RequiredTransforms = HeavyTankTransforms
            },
            new UnitSpec
            {
                ActorTypeId = "harvester",
                DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/harvester_visual.asset",
                PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/harvester/harvester.prefab",
                RequiredRigType = typeof(ProjectAegisHarvesterVisualRig),
                MinimumVertexCount = 500,
                ExpectedVisualScale = 0.34f,
                RequiredTransforms = HarvesterTransforms
            },
            new UnitSpec
            {
                ActorTypeId = "scout_rover",
                DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/scout_rover_visual.asset",
                PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/scout_rover/scout_rover.prefab",
                RequiredRigType = typeof(TankVisualRigController),
                MinimumVertexCount = 400,
                ExpectedVisualScale = 0.34f,
                ExpectedWheelCountPerSide = 2,
                RequiredTransforms = WheeledCombatTransforms
            },
            new UnitSpec
            {
                ActorTypeId = "apc",
                DefinitionPath = "Assets/Rts/ScriptableObjects/Art/ActorVisualDefinitions/apc_visual.asset",
                PrefabPath = "Assets/Rts/Art/Prefabs/Actors/Production/Units/apc/apc.prefab",
                RequiredRigType = typeof(TankVisualRigController),
                MinimumVertexCount = 500,
                ExpectedVisualScale = 0.34f,
                ExpectedWheelCountPerSide = 3,
                RequiredTransforms = ApcTransforms
            }
        };

        [MenuItem("ProjectAegisRTS/Validation/Validate Production Unit Visual Integration")]
        public static void ValidateFromMenu()
        {
            Validate();
        }

        public static void ValidateBatch()
        {
            try
            {
                Validate();
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

        public static void Validate()
        {
            var errors = new List<string>();
            for (var i = 0; i < Units.Length; i++)
                ValidateUnitAssets(Units[i], errors);

            ValidateStage16Scene(errors);

            if (errors.Count > 0)
                throw new InvalidOperationException("Production unit visual integration failed:\n- " + string.Join("\n- ", errors));

            Debug.Log("Production unit visual integration validated for light_tank, medium_tank, heavy_tank, harvester, scout_rover, and apc.");
        }

        static void ValidateUnitAssets(UnitSpec spec, List<string> errors)
        {
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(spec.DefinitionPath);
            if (definition == null)
            {
                errors.Add(spec.ActorTypeId + ": missing visual definition at " + spec.DefinitionPath);
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            if (prefab == null)
            {
                errors.Add(spec.ActorTypeId + ": missing production prefab at " + spec.PrefabPath);
                return;
            }

            if (definition.actorTypeId != spec.ActorTypeId)
                errors.Add(spec.ActorTypeId + ": definition actorTypeId is " + definition.actorTypeId);
            if (definition.preferredPrefabMode != ActorVisualPrefabMode.ProductionPrefab)
                errors.Add(spec.ActorTypeId + ": visual definition is not in ProductionPrefab mode.");
            if (definition.productionPrefab != prefab)
                errors.Add(spec.ActorTypeId + ": visual definition does not reference the expected production prefab.");
            if (definition.GetBestPrefab() != prefab)
                errors.Add(spec.ActorTypeId + ": GetBestPrefab does not resolve to the production prefab.");
            if (Mathf.Abs(definition.visualScale - spec.ExpectedVisualScale) > 0.001f)
                errors.Add(spec.ActorTypeId + ": visualScale is " + definition.visualScale + ", expected " + spec.ExpectedVisualScale + ".");

            var descriptor = prefab.GetComponent<ActorPrefabDescriptor>();
            if (descriptor == null)
            {
                errors.Add(spec.ActorTypeId + ": production prefab is missing ActorPrefabDescriptor.");
            }
            else
            {
                if (descriptor.actorTypeId != spec.ActorTypeId)
                    errors.Add(spec.ActorTypeId + ": descriptor actorTypeId is " + descriptor.actorTypeId);
                if (descriptor.productionStatus != ActorArtProductionStatus.Animated)
                    errors.Add(spec.ActorTypeId + ": descriptor productionStatus is not Animated.");
                if (descriptor.generatedByStage8)
                    errors.Add(spec.ActorTypeId + ": descriptor still marks the prefab as a Stage8 generated blockout.");
                if (!descriptor.requiredSocketsPresent)
                    errors.Add(spec.ActorTypeId + ": descriptor reports missing required sockets.");
            }

            if (prefab.GetComponent<LODGroup>() == null)
                errors.Add(spec.ActorTypeId + ": production prefab is missing LODGroup.");
            var rig = prefab.GetComponentInChildren(spec.RequiredRigType, true);
            if (rig == null)
                errors.Add(spec.ActorTypeId + ": production prefab is missing " + spec.RequiredRigType.Name + ".");
            ValidateWheeledRig(spec, rig as TankVisualRigController, errors);

            ValidateRenderers(spec, prefab, errors);
            ValidateTransforms(spec, prefab.transform, errors);
        }

        static void ValidateWheeledRig(UnitSpec spec, TankVisualRigController rig, List<string> errors)
        {
            if (spec.ExpectedWheelCountPerSide <= 0 || rig == null)
                return;

            ValidateWheelSide(spec, "left", "WheelLeft", rig.wheelLeft, spec.ExpectedWheelCountPerSide, errors);
            ValidateWheelSide(spec, "right", "WheelRight", rig.wheelRight, spec.ExpectedWheelCountPerSide, errors);
        }

        static void ValidateWheelSide(UnitSpec spec, string sideName, string semanticSocketName, Transform[] pivots, int expectedCount, List<string> errors)
        {
            if (pivots == null || pivots.Length < expectedCount)
            {
                errors.Add(spec.ActorTypeId + ": expected at least " + expectedCount + " individual " + sideName + " wheel spin pivots.");
                return;
            }

            for (var i = 0; i < pivots.Length; i++)
            {
                var pivot = pivots[i];
                if (pivot == null)
                {
                    errors.Add(spec.ActorTypeId + ": " + sideName + " wheel pivot " + i + " is null.");
                    continue;
                }

                if (pivot.name == semanticSocketName)
                    errors.Add(spec.ActorTypeId + ": " + semanticSocketName + " is wired as a wheel spin pivot instead of a stable side socket.");
                if (pivot.GetComponentInChildren<MeshRenderer>(true) == null)
                    errors.Add(spec.ActorTypeId + ": " + pivot.name + " has no wheel mesh renderer child.");
            }
        }

        static void ValidateRenderers(UnitSpec spec, GameObject prefab, List<string> errors)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                errors.Add(spec.ActorTypeId + ": production prefab has no renderers.");
                return;
            }

            var materialCount = 0;
            for (var i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                for (var j = 0; j < materials.Length; j++)
                    if (materials[j] != null)
                        materialCount++;
            }
            if (materialCount < 3)
                errors.Add(spec.ActorTypeId + ": production prefab has too few assigned materials.");

            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            var vertexCount = 0;
            var hasNonPrimitiveMesh = false;
            for (var i = 0; i < meshFilters.Length; i++)
            {
                var mesh = meshFilters[i].sharedMesh;
                if (mesh == null)
                    continue;

                vertexCount += mesh.vertexCount;
                if (!IsUnityPrimitiveMeshName(mesh.name))
                    hasNonPrimitiveMesh = true;
            }

            if (vertexCount < spec.MinimumVertexCount)
                errors.Add(spec.ActorTypeId + ": production prefab vertex count is suspiciously low (" + vertexCount + ").");
            if (!hasNonPrimitiveMesh)
                errors.Add(spec.ActorTypeId + ": production prefab appears to be primitive-only.");
        }

        static void ValidateTransforms(UnitSpec spec, Transform root, List<string> errors)
        {
            for (var i = 0; i < spec.RequiredTransforms.Length; i++)
                if (FindChildRecursive(root, spec.RequiredTransforms[i]) == null)
                    errors.Add(spec.ActorTypeId + ": missing required transform " + spec.RequiredTransforms[i] + ".");
        }

        static void ValidateStage16Scene(List<string> errors)
        {
            var fullScenePath = Path.Combine(Directory.GetCurrentDirectory(), Stage16SceneCreator.ScenePath);
            if (!File.Exists(fullScenePath))
            {
                errors.Add("Stage16 playable scene is missing at " + Stage16SceneCreator.ScenePath);
                return;
            }

            EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            var visualLibrary = UnityEngine.Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            var resolver = UnityEngine.Object.FindFirstObjectByType<ActorVisualPrefabResolver>();
            var actorRenderer = UnityEngine.Object.FindFirstObjectByType<ActorRenderSystem>();

            if (visualLibrary == null)
            {
                errors.Add("Stage16 is missing ActorVisualDefinitionLibrary.");
                return;
            }

            visualLibrary.RebuildLookup();

            if (resolver == null)
                errors.Add("Stage16 is missing ActorVisualPrefabResolver.");
            else
            {
                if (resolver.definitionLibrary != visualLibrary)
                    errors.Add("Stage16 prefab resolver is not linked to the ActorVisualDefinitionLibrary.");
                if (resolver.preferFallbackPrefabForBudgetValidation)
                    errors.Add("Stage16 prefab resolver is still forcing fallback prefabs.");
            }

            if (actorRenderer == null)
                errors.Add("Stage16 is missing ActorRenderSystem.");
            else
            {
                if (actorRenderer.actorVisualDefinitionLibrary != visualLibrary)
                    errors.Add("Stage16 ActorRenderSystem is not linked to the ActorVisualDefinitionLibrary.");
                if (actorRenderer.actorVisualPrefabResolver != resolver)
                    errors.Add("Stage16 ActorRenderSystem is not linked to the ActorVisualPrefabResolver.");
            }

            for (var i = 0; i < Units.Length; i++)
                ValidateStage16Resolution(Units[i], visualLibrary, resolver, errors);
        }

        static void ValidateStage16Resolution(UnitSpec spec, ActorVisualDefinitionLibrary visualLibrary, ActorVisualPrefabResolver resolver, List<string> errors)
        {
            var expectedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
            ActorVisualDefinition definition;
            if (!visualLibrary.TryGetDefinition(spec.ActorTypeId, out definition) || definition == null)
            {
                errors.Add("Stage16 visual library does not include " + spec.ActorTypeId + ".");
                return;
            }

            if (definition.productionPrefab != expectedPrefab)
                errors.Add("Stage16 " + spec.ActorTypeId + " definition is not pointing at the expected production prefab.");

            if (resolver == null)
                return;

            ActorVisualDefinition resolvedDefinition;
            GameObject resolvedPrefab;
            if (!resolver.ResolvePrefab(spec.ActorTypeId, out resolvedDefinition, out resolvedPrefab))
                errors.Add("Stage16 resolver cannot resolve " + spec.ActorTypeId + ".");
            else if (resolvedPrefab != expectedPrefab)
                errors.Add("Stage16 resolver resolves " + spec.ActorTypeId + " to " + resolvedPrefab.name + " instead of the production prefab.");
        }

        static Transform FindChildRecursive(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var child = FindChildRecursive(root.GetChild(i), name);
                if (child != null)
                    return child;
            }
            return null;
        }

        static bool IsUnityPrimitiveMeshName(string meshName)
        {
            if (string.IsNullOrEmpty(meshName))
                return false;
            return meshName == "Cube" ||
                meshName == "Sphere" ||
                meshName == "Capsule" ||
                meshName == "Cylinder" ||
                meshName == "Plane" ||
                meshName == "Quad";
        }
    }
}
