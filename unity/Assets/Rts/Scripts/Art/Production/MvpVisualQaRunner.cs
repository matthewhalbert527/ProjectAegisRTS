using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Art.Production
{
    public sealed class MvpVisualQaRunner : MonoBehaviour
    {
        public ActorVisualDefinitionLibrary definitionLibrary;
        public ProductionVisualStandardLibrary standardLibrary;
        public ArtistModelImportManifest artistModelImportManifest;
        public List<MvpVisualQaReport> latestReports = new List<MvpVisualQaReport>();

        public int PassCount { get; private set; }
        public int WarningCount { get; private set; }
        public int FailCount { get; private set; }

        public List<MvpVisualQaReport> RunAll()
        {
            EnsureReferences();
            latestReports.Clear();
            PassCount = 0;
            WarningCount = 0;
            FailCount = 0;

            for (var i = 0; i < Stage20MvpVisualActorSet.ActorTypeIds.Length; i++)
            {
                var actorTypeId = Stage20MvpVisualActorSet.ActorTypeIds[i];
                var definition = definitionLibrary == null ? null : definitionLibrary.GetDefinition(actorTypeId);
                var standard = standardLibrary == null ? null : standardLibrary.GetStandard(actorTypeId);
                var report = RunForDefinition(actorTypeId, definition, standard, artistModelImportManifest);
                latestReports.Add(report);

                if (report.overallStatus == MvpVisualQaStatus.Fail)
                    FailCount++;
                else if (report.overallStatus == MvpVisualQaStatus.Warning)
                    WarningCount++;
                else if (report.overallStatus == MvpVisualQaStatus.Pass)
                    PassCount++;
            }

            return latestReports;
        }

        public MvpVisualQaReport RunForDefinition(string actorTypeId, ActorVisualDefinition definition, ProductionVisualStandard standard, ArtistModelImportManifest importManifest)
        {
            var report = new MvpVisualQaReport
            {
                actorTypeId = actorTypeId,
                displayName = definition == null ? Stage8DisplayName(actorTypeId) : definition.displayName
            };

            var manifestEntry = importManifest == null ? null : importManifest.FindEntry(actorTypeId);
            report.artistImportStatus = manifestEntry == null ? ArtistModelImportStatus.NoCandidateFound.ToString() : manifestEntry.status.ToString();
            report.artistModelPath = manifestEntry == null ? string.Empty : manifestEntry.sourceAssetPath;

            if (definition == null)
            {
                report.AddRule("definition", MvpVisualQaStatus.Fail, "ActorVisualDefinition is missing.", actorTypeId);
                return report;
            }

            var prefab = definition.productionPrefab != null ? definition.productionPrefab : definition.GetBestPrefab();
            if (prefab == null)
            {
                report.AddRule("fallback safety", MvpVisualQaStatus.Fail, "No production or fallback prefab is available.", actorTypeId);
                return report;
            }

            var descriptor = prefab.GetComponentInChildren<ActorPrefabDescriptor>(true);
            var tag = prefab.GetComponentInChildren<ProductionVisualValidationTag>(true);
            report.visualTier = tag == null ? ProductionVisualTier.FirstPassProxy : tag.visualTier;

            var bounds = CalculateLocalBounds(prefab, out var rendererCount, out var materialCount);
            report.meshObjectCount = rendererCount;
            report.materialCount = materialCount;
            report.localBoundsCenter = bounds.center;
            report.localBoundsSize = bounds.size;
            report.socketCount = descriptor == null ? 0 : descriptor.GetSockets().Length;
            report.requiredSocketCount = definition.requiredSockets == null ? 0 : definition.requiredSockets.Count;

            ValidateFallback(definition, prefab, report);
            ValidateFootprintScale(definition, tag, bounds, report);
            ValidatePivot(bounds, report);
            ValidateViewCoverage(tag, report);
            ValidateSockets(definition, descriptor, report);
            ValidateAnimationHooks(definition, prefab, descriptor, report);
            ValidatePerformanceBudget(standard, prefab, rendererCount, materialCount, report);
            ValidatePlainBlockout(prefab, tag, rendererCount, materialCount, report);
            ValidatePlayerFacingVisibility(bounds, rendererCount, report);
            ValidateArtistReplacementMetadata(tag, descriptor, manifestEntry, report);

            report.RecalculateStatus();
            return report;
        }

        void EnsureReferences()
        {
            if (definitionLibrary == null)
                definitionLibrary = Object.FindFirstObjectByType<ActorVisualDefinitionLibrary>();
            if (standardLibrary == null)
                standardLibrary = Object.FindFirstObjectByType<ProductionVisualStandardLibrary>();
            if (definitionLibrary != null)
                definitionLibrary.EnsureInitialized();
            if (standardLibrary != null)
                standardLibrary.EnsureInitialized();
        }

        static void ValidateFallback(ActorVisualDefinition definition, GameObject prefab, MvpVisualQaReport report)
        {
            if (definition.productionPrefab != prefab || definition.preferredPrefabMode != ActorVisualPrefabMode.ProductionPrefab)
                report.AddRule("fallback safety", MvpVisualQaStatus.Fail, "MVP definition does not prefer its production proxy.", definition.actorTypeId);
            else if (definition.fallbackPrefab == null && definition.generatedBlockoutPrefab == null)
                report.AddRule("fallback safety", MvpVisualQaStatus.Fail, "No generated blockout or fallback prefab remains assigned.", definition.actorTypeId);
            else
                report.AddRule("fallback safety", MvpVisualQaStatus.Pass, "Production proxy is preferred and fallback remains assigned.", definition.actorTypeId);
        }

        static void ValidateFootprintScale(ActorVisualDefinition definition, ProductionVisualValidationTag tag, Bounds bounds, MvpVisualQaReport report)
        {
            var footprintWidth = Mathf.Max(1f, definition.footprintWidth);
            var footprintDepth = Mathf.Max(1f, definition.footprintHeight);
            var widthAllowance = footprintWidth + 1.25f;
            var depthAllowance = footprintDepth + (definition.category == ActorArtCategory.Building || definition.category == ActorArtCategory.Defense ? 1.75f : 0.65f);

            if (tag == null || !tag.hasGridAccurateBase)
                report.AddRule("footprint scale", MvpVisualQaStatus.Fail, "Grid-accurate base metadata is missing.", definition.actorTypeId);
            else if (bounds.size.x > widthAllowance || bounds.size.z > depthAllowance)
                report.AddRule("footprint scale", MvpVisualQaStatus.Fail, "Visual bounds extend too far beyond gameplay footprint.", BoundsDetail(bounds, definition));
            else if (bounds.size.x > footprintWidth + 0.35f || bounds.size.z > footprintDepth + 0.6f)
                report.AddRule("footprint scale", MvpVisualQaStatus.Warning, "Visual has allowed ramp/socket overhang beyond the fine-grid base.", BoundsDetail(bounds, definition));
            else
                report.AddRule("footprint scale", MvpVisualQaStatus.Pass, "Visual scale stays within fine-grid footprint tolerances.", BoundsDetail(bounds, definition));
        }

        static void ValidatePivot(Bounds bounds, MvpVisualQaReport report)
        {
            if (Mathf.Abs(bounds.center.x) > 0.45f || Mathf.Abs(bounds.center.z) > 0.55f)
                report.AddRule("pivot/origin", MvpVisualQaStatus.Warning, "Visual mass is noticeably off center from the root pivot.", BoundsDetail(bounds, null));
            else if (bounds.min.y < -0.12f || bounds.min.y > 0.22f)
                report.AddRule("pivot/origin", MvpVisualQaStatus.Fail, "Prefab base is not near root Y=0.", BoundsDetail(bounds, null));
            else
                report.AddRule("pivot/origin", MvpVisualQaStatus.Pass, "Root pivot is usable as footprint center/base.", BoundsDetail(bounds, null));
        }

        static void ValidateViewCoverage(ProductionVisualValidationTag tag, MvpVisualQaReport report)
        {
            if (tag == null)
            {
                report.AddRule("360 readability", MvpVisualQaStatus.Fail, "ProductionVisualValidationTag is missing.", string.Empty);
                return;
            }

            var missing = new List<string>();
            if (!tag.hasTopDetail) missing.Add("top");
            if (!tag.hasFrontDetail) missing.Add("front");
            if (!tag.hasBackDetail) missing.Add("rear");
            if (!tag.hasLeftDetail) missing.Add("left");
            if (!tag.hasRightDetail) missing.Add("right");
            if (!tag.hasRoofDetail) missing.Add("roof");
            if (!tag.hasBeveledOrTieredForm) missing.Add("tiered/beveled");

            if (missing.Count > 0)
                report.AddRule("360 readability", MvpVisualQaStatus.Fail, "Missing 360-degree readability markers.", string.Join(", ", missing.ToArray()));
            else
                report.AddRule("360 readability", MvpVisualQaStatus.Pass, "Top, side, rear, roof, and tiering markers are present.", tag.ViewCoverage.ToString());
        }

        static void ValidateSockets(ActorVisualDefinition definition, ActorPrefabDescriptor descriptor, MvpVisualQaReport report)
        {
            if (descriptor == null)
            {
                report.AddRule("socket completeness", MvpVisualQaStatus.Fail, "ActorPrefabDescriptor is missing.", definition.actorTypeId);
                return;
            }

            var required = RequiredSocketsForQa(definition);
            var missing = new List<string>();
            for (var i = 0; i < required.Count; i++)
            {
                Transform socket;
                if (!descriptor.TryGetSocket(required[i], out socket))
                    missing.Add(required[i].ToString());
            }

            if (missing.Count > 0)
                report.AddRule("socket completeness", MvpVisualQaStatus.Fail, "Required sockets are missing.", string.Join(", ", missing.ToArray()));
            else
                report.AddRule("socket completeness", MvpVisualQaStatus.Pass, "Required sockets are present.", required.Count + " sockets checked");
        }

        static void ValidateAnimationHooks(ActorVisualDefinition definition, GameObject prefab, ActorPrefabDescriptor descriptor, MvpVisualQaReport report)
        {
            var missing = new List<string>();
            if (definition.useBuildingVisualController && prefab.GetComponentInChildren<BuildingVisualStateController>(true) == null)
                missing.Add("BuildingVisualStateController");
            if (definition.useVehicleMotionController && prefab.GetComponentInChildren<VehicleVisualMotionController>(true) == null)
                missing.Add("VehicleVisualMotionController");
            if (definition.useInfantryMotionController && prefab.GetComponentInChildren<InfantryVisualMotionController>(true) == null)
                missing.Add("InfantryVisualMotionController");
            if (definition.useTurretVisualController)
            {
                if (prefab.GetComponentInChildren<TurretVisualAimController>(true) == null)
                    missing.Add("TurretVisualAimController");
                if (descriptor != null)
                {
                    RequireSocket(descriptor, ActorPrefabSocketKind.TurretRoot, missing);
                    RequireSocket(descriptor, ActorPrefabSocketKind.BarrelRoot, missing);
                    RequireSocket(descriptor, ActorPrefabSocketKind.MuzzlePrimary, missing);
                }
            }

            if (missing.Count > 0)
                report.AddRule("animation hook readiness", MvpVisualQaStatus.Fail, "Animation/combat hooks are incomplete.", string.Join(", ", missing.ToArray()));
            else
                report.AddRule("animation hook readiness", MvpVisualQaStatus.Pass, "Motion, combat, and building-state hooks are ready for replacement art.", definition.actorTypeId);
        }

        static void ValidatePerformanceBudget(ProductionVisualStandard standard, GameObject prefab, int rendererCount, int materialCount, MvpVisualQaReport report)
        {
            if (prefab.GetComponentInChildren<LODGroup>(true) == null)
            {
                report.AddRule("LOD/performance readiness", MvpVisualQaStatus.Fail, "LODGroup is missing.", prefab.name);
                return;
            }

            var maxMeshes = standard == null ? 48 : Mathf.Max(1, standard.maxRecommendedMeshObjects);
            var maxMaterials = standard == null ? 8 : Mathf.Max(1, standard.maxRecommendedMaterials);
            if (rendererCount > maxMeshes || materialCount > maxMaterials)
                report.AddRule("LOD/performance readiness", MvpVisualQaStatus.Warning, "Proxy is over the recommended Quest budget.", "mesh objects " + rendererCount + "/" + maxMeshes + ", materials " + materialCount + "/" + maxMaterials);
            else
                report.AddRule("LOD/performance readiness", MvpVisualQaStatus.Pass, "LODGroup and Quest-safe proxy budgets are present.", "mesh objects " + rendererCount + "/" + maxMeshes + ", materials " + materialCount + "/" + maxMaterials);
        }

        static void ValidatePlainBlockout(GameObject prefab, ProductionVisualValidationTag tag, int rendererCount, int materialCount, MvpVisualQaReport report)
        {
            if (rendererCount <= 3 || materialCount <= 2 || tag == null || !tag.hasStage21ReadabilityPass)
                report.AddRule("top-down readability", MvpVisualQaStatus.Fail, "Prefab still reads like a plain blockout or lacks Stage 21 readability metadata.", "renderers " + rendererCount + ", materials " + materialCount);
            else
                report.AddRule("top-down readability", MvpVisualQaStatus.Pass, "Prefab has modular identity geometry beyond a plain blockout.", "renderers " + rendererCount + ", materials " + materialCount);
        }

        static void ValidatePlayerFacingVisibility(Bounds bounds, int rendererCount, MvpVisualQaReport report)
        {
            if (rendererCount == 0 || bounds.size.y < 0.08f || bounds.size.x < 0.05f || bounds.size.z < 0.05f)
                report.AddRule("player-facing visibility", MvpVisualQaStatus.Fail, "Prefab has no readable rendered volume.", BoundsDetail(bounds, null));
            else
                report.AddRule("player-facing visibility", MvpVisualQaStatus.Pass, "Prefab has readable rendered volume for live player-facing scenes.", BoundsDetail(bounds, null));
        }

        static void ValidateArtistReplacementMetadata(ProductionVisualValidationTag tag, ActorPrefabDescriptor descriptor, ArtistModelImportEntry manifestEntry, MvpVisualQaReport report)
        {
            var detail = manifestEntry == null ? ArtistModelImportStatus.NoCandidateFound.ToString() : manifestEntry.status + " " + manifestEntry.sourceAssetPath;
            if (tag == null || !tag.hasArtistReplacementMetadata)
                report.AddRule("artist replacement metadata", MvpVisualQaStatus.Fail, "Prefab lacks Stage 21 replacement metadata.", detail);
            else if (descriptor == null || string.IsNullOrEmpty(descriptor.notes) || !descriptor.notes.Contains("Stage 21"))
                report.AddRule("artist replacement metadata", MvpVisualQaStatus.Warning, "Descriptor notes do not include Stage 21 replacement guidance.", detail);
            else
                report.AddRule("artist replacement metadata", MvpVisualQaStatus.Pass, "Replacement metadata is present; proxy remains active until real model validation passes.", detail);
        }

        public static Bounds CalculateLocalBounds(GameObject root, out int rendererCount, out int materialCount)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            rendererCount = renderers.Length;
            var materials = new HashSet<Material>();
            var hasBounds = false;
            var bounds = new Bounds(Vector3.zero, Vector3.zero);

            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                var sharedMaterials = renderer.sharedMaterials;
                for (var m = 0; m < sharedMaterials.Length; m++)
                    if (sharedMaterials[m] != null)
                        materials.Add(sharedMaterials[m]);

                var local = GetRendererLocalBounds(renderer);
                for (var c = 0; c < 8; c++)
                {
                    var corner = new Vector3(
                        (c & 1) == 0 ? local.min.x : local.max.x,
                        (c & 2) == 0 ? local.min.y : local.max.y,
                        (c & 4) == 0 ? local.min.z : local.max.z);
                    var rootLocal = root.transform.InverseTransformPoint(renderer.transform.TransformPoint(corner));
                    if (!hasBounds)
                    {
                        bounds = new Bounds(rootLocal, Vector3.zero);
                        hasBounds = true;
                    }
                    else
                        bounds.Encapsulate(rootLocal);
                }
            }

            materialCount = materials.Count;
            return hasBounds ? bounds : new Bounds(Vector3.zero, Vector3.zero);
        }

        static Bounds GetRendererLocalBounds(Renderer renderer)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
                return meshFilter.sharedMesh.bounds;

            var skinned = renderer as SkinnedMeshRenderer;
            if (skinned != null)
                return skinned.localBounds;

            return new Bounds(Vector3.zero, Vector3.one);
        }

        static List<ActorPrefabSocketKind> RequiredSocketsForQa(ActorVisualDefinition definition)
        {
            var required = new List<ActorPrefabSocketKind>();
            if (definition.requiredSockets != null)
                required.AddRange(definition.requiredSockets);

            AddUnique(required, ActorPrefabSocketKind.SelectionAnchor);
            AddUnique(required, ActorPrefabSocketKind.HealthBarAnchor);

            if (definition.actorTypeId == "refinery")
            {
                AddUnique(required, ActorPrefabSocketKind.HarvesterDock);
                AddUnique(required, ActorPrefabSocketKind.DockPumpRoot);
            }
            if (definition.actorTypeId == "power_plant")
                AddUnique(required, ActorPrefabSocketKind.TurbineRoot);
            if (definition.actorTypeId == "fabrication_hub" || definition.actorTypeId == "barracks" || definition.actorTypeId == "war_factory")
            {
                AddUnique(required, ActorPrefabSocketKind.ProductionExit);
                AddUnique(required, ActorPrefabSocketKind.RallyExit);
            }
            if (definition.actorTypeId == "gun_tower" || definition.actorTypeId == "light_tank")
            {
                AddUnique(required, ActorPrefabSocketKind.TurretRoot);
                AddUnique(required, ActorPrefabSocketKind.BarrelRoot);
                AddUnique(required, ActorPrefabSocketKind.MuzzlePrimary);
            }
            if (definition.actorTypeId == "rifle_infantry")
            {
                AddUnique(required, ActorPrefabSocketKind.Head);
                AddUnique(required, ActorPrefabSocketKind.WeaponSocket);
                AddUnique(required, ActorPrefabSocketKind.AimPivot);
            }

            return required;
        }

        static void AddUnique(List<ActorPrefabSocketKind> sockets, ActorPrefabSocketKind kind)
        {
            if (!sockets.Contains(kind))
                sockets.Add(kind);
        }

        static void RequireSocket(ActorPrefabDescriptor descriptor, ActorPrefabSocketKind kind, List<string> missing)
        {
            Transform socket;
            if (!descriptor.TryGetSocket(kind, out socket))
                missing.Add(kind.ToString());
        }

        static string BoundsDetail(Bounds bounds, ActorVisualDefinition definition)
        {
            var detail = "center " + bounds.center.ToString("F2") + ", size " + bounds.size.ToString("F2");
            if (definition != null)
                detail += ", footprint " + definition.footprintWidth + "x" + definition.footprintHeight;
            return detail;
        }

        static string Stage8DisplayName(string actorTypeId)
        {
            if (string.IsNullOrEmpty(actorTypeId))
                return string.Empty;

            var parts = actorTypeId.Split('_');
            for (var i = 0; i < parts.Length; i++)
                if (parts[i].Length > 0)
                    parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            return string.Join(" ", parts);
        }
    }
}
