using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage21ArtistModelImportScanner
    {
        public const string SourceModelFolder = "Assets/Rts/Art/Models/Source/MVP";
        public const string ImportedModelFolder = "Assets/Rts/Art/Models/Imported/MVP";
        public const string ProductionPrefabFolder = "Assets/Rts/Art/Prefabs/Actors/Production/MVP";
        public const string ManifestAssetPath = "Assets/Rts/ScriptableObjects/Art/ProductionSpecs/stage21_artist_model_import_manifest.asset";
        public const string ImportStatusMarkdownPath = "docs/STAGE21_ARTIST_MODEL_IMPORT_STATUS.md";

        static readonly string[] SupportedExtensions = { ".fbx", ".glb", ".gltf", ".obj" };

        [MenuItem("ProjectAegisRTS/Stage 21/Scan MVP Artist Models")]
        public static void ScanMvpArtistModelsMenu()
        {
            ScanMvpArtistModels();
        }

        public static void ScanMvpArtistModelsBatch()
        {
            try
            {
                ScanMvpArtistModels();
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

        public static Stage21ArtistModelImportSummary ScanMvpArtistModels()
        {
            EnsureImportFolders();
            var manifest = LoadOrCreateManifest();
            manifest.entries.Clear();

            var summary = new Stage21ArtistModelImportSummary
            {
                entries = new List<ArtistModelImportEntry>(),
                errors = new List<string>(),
                warnings = new List<string>()
            };

            var specs = Stage8ActorCatalog.LoadSpecs();
            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                if (!Stage20MvpVisualActorSet.Contains(spec.ActorTypeId))
                    continue;

                var entry = BuildEntryForSpec(spec, summary);
                manifest.entries.Add(entry);
                summary.entries.Add(entry);
            }

            manifest.notes = summary.CandidateCount == 0
                ? "No artist-authored models found; Stage 20/21 proxies remain active."
                : "Artist-authored model candidates found. Draft prefabs are not active until validation and manual definition assignment.";

            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteImportStatus(summary);

            if (summary.errors.Count > 0)
                throw new InvalidOperationException("Stage 21 artist model import scan failed: " + string.Join(" | ", summary.errors.ToArray()));

            Debug.Log("Stage 21 artist model import scan completed. Candidates: " + summary.CandidateCount + ", draft prefabs: " + summary.DraftPrefabCount);
            return summary;
        }

        public static ArtistModelImportManifest LoadOrCreateManifest()
        {
            var manifest = AssetDatabase.LoadAssetAtPath<ArtistModelImportManifest>(ManifestAssetPath);
            if (manifest != null)
                return manifest;

            EnsureImportFolders();
            manifest = ScriptableObject.CreateInstance<ArtistModelImportManifest>();
            AssetDatabase.CreateAsset(manifest, ManifestAssetPath);
            return manifest;
        }

        public static void EnsureImportFolders()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            EnsureFolder("Assets/Rts/Art/Models/Source", "MVP");
            EnsureFolder("Assets/Rts/Art/Models/Imported", "MVP");
            EnsureFolder("Assets/Rts/Art/Prefabs/Actors/Production", "MVP");
            EnsurePlaceholder(SourceModelFolder, "Place artist-authored MVP FBX/GLB/OBJ source files here. Filenames should include the safe actor id.");
            EnsurePlaceholder(ImportedModelFolder, "Unity-imported MVP model candidates may live here. Filenames should include the safe actor id.");
            EnsurePlaceholder(ProductionPrefabFolder, "Stage 21 draft artist model prefabs are generated here when safe model candidates are present.");
        }

        static ArtistModelImportEntry BuildEntryForSpec(Stage8ActorSpec spec, Stage21ArtistModelImportSummary summary)
        {
            var candidatePath = FindCandidateAssetPath(spec.ActorTypeId);
            if (string.IsNullOrEmpty(candidatePath))
            {
                return new ArtistModelImportEntry
                {
                    actorTypeId = spec.ActorTypeId,
                    status = ArtistModelImportStatus.NoCandidateFound,
                    notes = "No artist-authored models found; proxies remain active."
                };
            }

            summary.CandidateCount++;
            var entry = new ArtistModelImportEntry
            {
                actorTypeId = spec.ActorTypeId,
                status = ArtistModelImportStatus.CandidateSourceFound,
                sourceAssetPath = candidatePath,
                importedAssetPath = candidatePath,
                notes = "Candidate model found. Proxy remains active until draft prefab validation and manual assignment."
            };

            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePath);
            if (modelPrefab == null)
            {
                entry.status = ArtistModelImportStatus.NeedsValidation;
                entry.notes = "Candidate file exists but Unity did not expose a loadable GameObject. Keep proxy active.";
                summary.warnings.Add(spec.ActorTypeId + ": candidate model could not be loaded as a GameObject: " + candidatePath);
                return entry;
            }

            var draftPath = CreateDraftProductionPrefab(spec, modelPrefab);
            entry.status = ArtistModelImportStatus.CandidateImported;
            entry.draftPrefabPath = draftPath;
            entry.notes = "Draft prefab scaffold generated. Validate sockets, pivot, scale, materials, and LOD before replacing the active proxy.";
            summary.DraftPrefabCount++;
            return entry;
        }

        static string FindCandidateAssetPath(string actorTypeId)
        {
            var candidates = new List<string>();
            AddCandidatesFromFolder(SourceModelFolder, actorTypeId, candidates);
            AddCandidatesFromFolder(ImportedModelFolder, actorTypeId, candidates);
            candidates.Sort(StringComparer.OrdinalIgnoreCase);
            return candidates.Count == 0 ? string.Empty : candidates[0];
        }

        static void AddCandidatesFromFolder(string assetFolder, string actorTypeId, List<string> candidates)
        {
            var absoluteFolder = AssetFolderToAbsolutePath(assetFolder);
            if (!Directory.Exists(absoluteFolder))
                return;

            var files = Directory.GetFiles(absoluteFolder, "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var extension = Path.GetExtension(files[i]);
                if (!IsSupportedExtension(extension))
                    continue;

                var fileName = Path.GetFileNameWithoutExtension(files[i]).ToLowerInvariant();
                if (!fileName.Contains(actorTypeId.ToLowerInvariant()))
                    continue;

                candidates.Add(ToAssetPath(files[i]));
            }
        }

        static string CreateDraftProductionPrefab(Stage8ActorSpec spec, GameObject modelPrefab)
        {
            var root = new GameObject(spec.ActorTypeId + "_artist_model_candidate");
            var model = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
            if (model != null)
            {
                model.name = "Artist Model " + spec.ActorTypeId;
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
            }

            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(Stage8ActorCatalog.DefinitionAssetPath(spec));
            var descriptor = root.AddComponent<ActorPrefabDescriptor>();
            descriptor.actorTypeId = spec.ActorTypeId;
            descriptor.category = spec.Category;
            descriptor.sourceDefinition = definition;
            descriptor.productionStatus = ActorArtProductionStatus.FirstPassModel;
            descriptor.generatedByStage8 = false;
            descriptor.declaredRequiredSockets = RequiredSocketsForStage21(spec);
            descriptor.notes = "Stage 21 draft artist model prefab. Proxy remains the active production prefab until this candidate passes replacement validation.";

            var tag = root.AddComponent<ProductionVisualValidationTag>();
            tag.actorTypeId = spec.ActorTypeId;
            tag.visualTier = ProductionVisualTier.FirstPassProxy;
            tag.hasGridAccurateBase = false;
            tag.hasSocketScaffold = true;
            tag.hasArtistReplacementMetadata = true;
            tag.replacementNotes = "Draft import scaffold; artist geometry must be aligned to this root and sockets before activation.";
            tag.notes = "Generated by Stage 21 artist model import scanner.";

            CreateSocketScaffold(root.transform, spec, descriptor.declaredRequiredSockets);
            descriptor.requiredSocketsPresent = descriptor.ValidateRequiredSockets(descriptor.declaredRequiredSockets).Count == 0;
            AddLodGroup(root);

            var draftPath = ProductionPrefabFolder + "/" + spec.ActorTypeId + "_artist_model_candidate.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, draftPath);
            UnityEngine.Object.DestroyImmediate(root);
            return draftPath;
        }

        static List<ActorPrefabSocketKind> RequiredSocketsForStage21(Stage8ActorSpec spec)
        {
            var sockets = Stage8ActorCatalog.RequiredSocketsFor(spec);
            if (spec.ActorTypeId == "refinery" && !sockets.Contains(ActorPrefabSocketKind.DockPumpRoot))
                sockets.Add(ActorPrefabSocketKind.DockPumpRoot);
            sockets.Sort();
            return sockets;
        }

        static void CreateSocketScaffold(Transform root, Stage8ActorSpec spec, List<ActorPrefabSocketKind> requiredSockets)
        {
            for (var i = 0; i < requiredSockets.Count; i++)
            {
                var kind = requiredSockets[i];
                var socketRoot = kind == ActorPrefabSocketKind.Root ? root : new GameObject("Socket_" + kind).transform;
                if (kind != ActorPrefabSocketKind.Root)
                {
                    socketRoot.SetParent(root, false);
                    socketRoot.localPosition = SocketPosition(kind, spec);
                }

                var socket = socketRoot.gameObject.GetComponent<ActorPrefabSocket>();
                if (socket == null)
                    socket = socketRoot.gameObject.AddComponent<ActorPrefabSocket>();
                socket.socketKind = kind;
                socket.socketName = kind.ToString();
                socket.actorTypeId = spec.ActorTypeId;
                socket.notes = "Stage 21 draft artist import socket scaffold";
            }
        }

        static Vector3 SocketPosition(ActorPrefabSocketKind kind, Stage8ActorSpec spec)
        {
            var footprintX = Mathf.Max(1f, spec.FootprintWidth);
            var footprintZ = Mathf.Max(1f, spec.FootprintHeight);
            switch (kind)
            {
                case ActorPrefabSocketKind.BodyRoot: return new Vector3(0f, 0.25f, 0f);
                case ActorPrefabSocketKind.VisualRoot: return Vector3.zero;
                case ActorPrefabSocketKind.SelectionAnchor: return new Vector3(0f, 0.04f, 0f);
                case ActorPrefabSocketKind.HealthBarAnchor: return new Vector3(0f, 1.18f, -0.35f);
                case ActorPrefabSocketKind.UiAnchor: return new Vector3(0f, 1.34f, 0f);
                case ActorPrefabSocketKind.TurretRoot: return new Vector3(0f, 0.58f, 0.02f);
                case ActorPrefabSocketKind.BarrelRoot: return new Vector3(0f, 0.58f, 0.34f);
                case ActorPrefabSocketKind.MuzzlePrimary: return new Vector3(0f, 0.58f, 0.76f);
                case ActorPrefabSocketKind.TrackLeft: return new Vector3(-0.52f, 0.18f, 0f);
                case ActorPrefabSocketKind.TrackRight: return new Vector3(0.52f, 0.18f, 0f);
                case ActorPrefabSocketKind.DoorRoot: return new Vector3(0f, 0.32f, footprintZ * 0.45f);
                case ActorPrefabSocketKind.ProductionExit: return new Vector3(0f, 0.05f, footprintZ * 0.7f);
                case ActorPrefabSocketKind.RallyExit: return new Vector3(0.6f, 0.05f, footprintZ * 0.7f);
                case ActorPrefabSocketKind.HarvesterDock: return spec.ActorTypeId == "harvester" ? new Vector3(0f, 0.55f, -0.5f) : new Vector3(-footprintX * 0.45f, 0.25f, 0f);
                case ActorPrefabSocketKind.DockPumpRoot: return new Vector3(-footprintX * 0.42f, 0.62f, 0.1f);
                case ActorPrefabSocketKind.CraneRoot: return new Vector3(-footprintX * 0.25f, 1.05f, 0f);
                case ActorPrefabSocketKind.TurbineRoot: return new Vector3(0f, 0.95f, 0f);
                case ActorPrefabSocketKind.LightRoot: return new Vector3(-footprintX * 0.25f, 0.95f, footprintZ * 0.2f);
                case ActorPrefabSocketKind.VfxSmoke: return new Vector3(-footprintX * 0.2f, 1.12f, -footprintZ * 0.2f);
                case ActorPrefabSocketKind.VfxExplosion: return new Vector3(0f, 0.6f, 0f);
                case ActorPrefabSocketKind.VfxProduction: return new Vector3(0f, 0.45f, footprintZ * 0.45f);
                case ActorPrefabSocketKind.Head: return new Vector3(0f, 0.88f, 0f);
                case ActorPrefabSocketKind.WeaponSocket: return new Vector3(0.22f, 0.55f, 0.28f);
                case ActorPrefabSocketKind.AimPivot: return new Vector3(0f, 0.62f, 0.42f);
                default: return Vector3.zero;
            }
        }

        static void AddLodGroup(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;

            var lod = root.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(0.01f, renderers) });
            lod.RecalculateBounds();
        }

        static void WriteImportStatus(Stage21ArtistModelImportSummary summary)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, ImportStatusMarkdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, BuildMarkdown(summary), Encoding.UTF8);
        }

        static string BuildMarkdown(Stage21ArtistModelImportSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 21 Artist Model Import Status");
            builder.AppendLine();
            if (summary.CandidateCount == 0)
                builder.AppendLine("No artist-authored models found; proxies remain active.");
            else
                builder.AppendLine("Artist-authored model candidates were found. Draft prefabs are not active until validated and manually assigned.");
            builder.AppendLine();
            builder.AppendLine("- MVP actors checked: " + summary.entries.Count);
            builder.AppendLine("- Candidate model files: " + summary.CandidateCount);
            builder.AppendLine("- Draft candidate prefabs: " + summary.DraftPrefabCount);
            builder.AppendLine("- Errors: " + summary.errors.Count);
            builder.AppendLine("- Warnings: " + summary.warnings.Count);
            builder.AppendLine();
            builder.AppendLine("## Per Actor");
            for (var i = 0; i < summary.entries.Count; i++)
            {
                var entry = summary.entries[i];
                builder.AppendLine("- `" + entry.actorTypeId + "`: " + entry.status + " - " + entry.notes);
                if (!string.IsNullOrEmpty(entry.sourceAssetPath))
                    builder.AppendLine("  Source: `" + entry.sourceAssetPath + "`");
                if (!string.IsNullOrEmpty(entry.draftPrefabPath))
                    builder.AppendLine("  Draft prefab: `" + entry.draftPrefabPath + "`");
            }
            builder.AppendLine();
            builder.AppendLine("## Replacement Rule");
            builder.AppendLine("Do not replace an active proxy automatically. A real model should become active only after it preserves the proxy footprint, pivot, sockets, LODGroup, materials budget, and fallback path.");
            return builder.ToString();
        }

        static bool IsSupportedExtension(string extension)
        {
            for (var i = 0; i < SupportedExtensions.Length; i++)
                if (string.Equals(extension, SupportedExtensions[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        static string ToAssetPath(string absolutePath)
        {
            var normalized = absolutePath.Replace("\\", "/");
            var index = normalized.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
            return index >= 0 ? normalized.Substring(index + 1) : normalized;
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        static void EnsurePlaceholder(string assetFolder, string text)
        {
            var absoluteFolder = AssetFolderToAbsolutePath(assetFolder);
            Directory.CreateDirectory(absoluteFolder);
            var placeholderPath = Path.Combine(absoluteFolder, "README.md");
            if (!File.Exists(placeholderPath))
                File.WriteAllText(placeholderPath, "# " + Path.GetFileName(absoluteFolder) + Environment.NewLine + Environment.NewLine + text + Environment.NewLine, Encoding.UTF8);
        }

        static string AssetFolderToAbsolutePath(string assetFolder)
        {
            return Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", assetFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
    }

    public sealed class Stage21ArtistModelImportSummary
    {
        public List<ArtistModelImportEntry> entries;
        public List<string> errors;
        public List<string> warnings;
        public int CandidateCount;
        public int DraftPrefabCount;
    }
}
