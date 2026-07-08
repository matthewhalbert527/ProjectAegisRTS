using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor.Visuals
{
    public sealed class AegisMapVisualCompileContext
    {
        public const float CellSize = 1f;

        public string MapId;
        public string SourceAssetPath;
        public int Width;
        public int Height;
        public int Seed;
        public bool PersistAssets;
        public AegisMapVisualTheme Theme;
        public AegisMapVisualCompileSettings Settings = AegisMapVisualCompileSettings.ProductionDefault();
        public GameObject Root;
        public readonly List<AegisVisualStartModel> Starts = new List<AegisVisualStartModel>();
        public readonly List<AegisVisualResourceFieldModel> ResourceFields = new List<AegisVisualResourceFieldModel>();
        public readonly List<AegisVisualPathSegment> RoadSegments = new List<AegisVisualPathSegment>();
        public readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

        readonly string[] terrainRoles;
        readonly bool[] blockers;
        readonly bool[] cliffs;
        readonly bool[] resources;
        readonly bool[] startProtected;
        readonly string[] resourceKinds;
        readonly string[] resourceFieldIds;
        readonly int[] resourceAmounts;

        public AegisMapVisualCompileContext(int width, int height)
        {
            Width = width;
            Height = height;
            var cells = Math.Max(1, width * height);
            terrainRoles = new string[cells];
            blockers = new bool[cells];
            cliffs = new bool[cells];
            resources = new bool[cells];
            startProtected = new bool[cells];
            resourceKinds = new string[cells];
            resourceFieldIds = new string[cells];
            resourceAmounts = new int[cells];
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public AegisMapVisualRenderMode RenderMode
        {
            get { return Settings == null ? AegisMapVisualRenderMode.ProductionPreview : Settings.RenderMode; }
        }

        public bool IsProductionPreview
        {
            get { return RenderMode == AegisMapVisualRenderMode.ProductionPreview; }
        }

        public bool IsDebugOverlay
        {
            get { return RenderMode == AegisMapVisualRenderMode.DebugOverlay; }
        }

        public bool IsHybrid
        {
            get { return RenderMode == AegisMapVisualRenderMode.Hybrid; }
        }

        public bool ShowOverlay(bool enabled)
        {
            return IsDebugOverlay || (IsHybrid && enabled);
        }

        public bool ShowTerrainOverlay
        {
            get { return ShowOverlay(Settings != null && Settings.Overlays != null && Settings.Overlays.Terrain); }
        }

        public bool ShowBlockerOverlay
        {
            get { return ShowOverlay(Settings != null && Settings.Overlays != null && Settings.Overlays.Blockers); }
        }

        public bool ShowResourceOverlay
        {
            get { return ShowOverlay(Settings != null && Settings.Overlays != null && Settings.Overlays.Resources); }
        }

        public bool ShowBuildPadOverlay
        {
            get { return ShowOverlay(Settings != null && Settings.Overlays != null && Settings.Overlays.BuildPads); }
        }

        public bool ShowCliffOverlay
        {
            get { return ShowOverlay(Settings != null && Settings.Overlays != null && Settings.Overlays.Cliffs); }
        }

        public bool ShowPathabilityOverlay
        {
            get { return ShowOverlay(Settings != null && Settings.Overlays != null && Settings.Overlays.Pathability); }
        }

        public int Index(int x, int y)
        {
            return y * Width + x;
        }

        public void SetTerrainRole(int x, int y, string role)
        {
            if (!InBounds(x, y))
                return;

            terrainRoles[Index(x, y)] = string.IsNullOrEmpty(role) ? "terrain.grass" : role;
        }

        public string TerrainRoleAt(int x, int y)
        {
            if (!InBounds(x, y))
                return "terrain.grass";

            var value = terrainRoles[Index(x, y)];
            return string.IsNullOrEmpty(value) ? "terrain.grass" : value;
        }

        public void SetBlocker(int x, int y, bool cliffLike)
        {
            if (!InBounds(x, y))
                return;

            var index = Index(x, y);
            blockers[index] = true;
            cliffs[index] = cliffLike;
        }

        public bool IsBlocked(int x, int y)
        {
            return InBounds(x, y) && blockers[Index(x, y)];
        }

        public bool IsCliffLike(int x, int y)
        {
            if (!InBounds(x, y))
                return false;

            var index = Index(x, y);
            return cliffs[index] || TerrainRoleAt(x, y) == "terrain.cliff_ground";
        }

        public bool IsWater(int x, int y)
        {
            if (!InBounds(x, y))
                return false;

            var role = TerrainRoleAt(x, y);
            return role == "terrain.shallow_water" || role == "terrain.deep_water";
        }

        public void SetResource(int x, int y, string fieldId, string kind, int amount)
        {
            if (!InBounds(x, y))
                return;

            var index = Index(x, y);
            resources[index] = true;
            resourceFieldIds[index] = string.IsNullOrEmpty(fieldId) ? "resource_" + x + "_" + y : fieldId;
            resourceKinds[index] = string.IsNullOrEmpty(kind) ? "ore" : kind;
            resourceAmounts[index] = Math.Max(0, amount);
        }

        public bool HasResource(int x, int y)
        {
            return InBounds(x, y) && resources[Index(x, y)];
        }

        public string ResourceKindAt(int x, int y)
        {
            if (!HasResource(x, y))
                return null;

            return resourceKinds[Index(x, y)];
        }

        public string ResourceFieldIdAt(int x, int y)
        {
            if (!HasResource(x, y))
                return null;

            return resourceFieldIds[Index(x, y)];
        }

        public int ResourceAmountAt(int x, int y)
        {
            if (!HasResource(x, y))
                return 0;

            return resourceAmounts[Index(x, y)];
        }

        public void MarkStartProtected(int centerX, int centerY, int radius)
        {
            for (var y = centerY - radius; y <= centerY + radius; y++)
                for (var x = centerX - radius; x <= centerX + radius; x++)
                    if (InBounds(x, y))
                    {
                        var dx = x - centerX;
                        var dy = y - centerY;
                        if (dx * dx + dy * dy <= radius * radius)
                            startProtected[Index(x, y)] = true;
                    }
        }

        public bool IsStartProtected(int x, int y)
        {
            return InBounds(x, y) && startProtected[Index(x, y)];
        }

        public Vector3 CellCenter(int x, int y, float elevation)
        {
            return new Vector3((x + 0.5f) * CellSize, elevation, (y + 0.5f) * CellSize);
        }

        public float Hash01(int x, int y, int salt)
        {
            unchecked
            {
                var h = (uint)Seed;
                h ^= (uint)(x * 374761393);
                h = (h << 13) | (h >> 19);
                h ^= (uint)(y * 668265263);
                h ^= (uint)salt * 2246822519u;
                h *= 3266489917u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777215f;
            }
        }

        public int HashRange(int x, int y, int salt, int maxExclusive)
        {
            if (maxExclusive <= 1)
                return 0;

            return Mathf.FloorToInt(Hash01(x, y, salt) * maxExclusive) % maxExclusive;
        }
    }

    public sealed class AegisVisualStartModel
    {
        public int PlayerId;
        public int X;
        public int Y;
        public string Name;
        public int ProtectedRadius = 9;
    }

    public sealed class AegisVisualResourceCellModel
    {
        public int X;
        public int Y;
        public int Amount;
    }

    public sealed class AegisVisualResourceFieldModel
    {
        public string FieldId;
        public string ResourceKind;
        public int CurrentAmount;
        public int MaxAmount;
        public bool Regenerating;
        public readonly List<AegisVisualResourceCellModel> Cells = new List<AegisVisualResourceCellModel>();

        public Vector2 Center
        {
            get
            {
                if (Cells.Count == 0)
                    return Vector2.zero;

                var x = 0f;
                var y = 0f;
                for (var i = 0; i < Cells.Count; i++)
                {
                    x += Cells[i].X + 0.5f;
                    y += Cells[i].Y + 0.5f;
                }

                return new Vector2(x / Cells.Count, y / Cells.Count);
            }
        }

        public float FillRatio
        {
            get { return MaxAmount <= 0 ? 0f : Mathf.Clamp01(CurrentAmount / (float)MaxAmount); }
        }
    }

    public sealed class AegisVisualPathSegment
    {
        public Vector2 A;
        public Vector2 B;
        public float Width;

        public AegisVisualPathSegment(Vector2 a, Vector2 b, float width)
        {
            A = a;
            B = b;
            Width = width;
        }
    }
}
