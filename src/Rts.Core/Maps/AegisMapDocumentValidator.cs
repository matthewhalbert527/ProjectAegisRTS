using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps
{
    public sealed class AegisMapDocumentValidator
    {
        public MapValidationResult Validate(AegisMapDocument document)
        {
            return Validate(document, null);
        }

        public MapValidationResult Validate(AegisMapDocument document, RtsRules rules)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (document == null)
            {
                errors.Add("MapDocumentMissing:The map document is null.");
                return new MapValidationResult(errors, warnings);
            }

            ValidateSize(document, errors);
            ValidateTerrainId(document.DefaultTerrainId, "DefaultTerrainId", errors);
            ValidateTerrainLayer(document.Width, document.Height, document.TerrainBase, AegisMapDocument.TerrainBaseLayerName, errors);
            ValidateTerrainLayer(document.Width, document.Height, document.TerrainOverlay, AegisMapDocument.TerrainOverlayLayerName, errors);
            ValidateBlockers(document.Width, document.Height, document.Blockers, errors);
            ValidateResources(document.Width, document.Height, document.Resources, errors);
            ValidatePlayerStarts(document.Width, document.Height, document.PlayerStarts, errors, warnings);
            ValidateActorPlacements(document.Width, document.Height, document.ActorPlacements, rules, errors);
            ValidateRegions(document.Width, document.Height, document.Regions, errors);
            ValidateNavOverrides(document.Width, document.Height, document.NavOverrides, errors);

            if (document.Resources == null || document.Resources.Count == 0)
                warnings.Add("NoResourcePlacements:The map has no authored resource placements.");

            return new MapValidationResult(errors, warnings);
        }

        static void ValidateSize(AegisMapDocument document, List<string> errors)
        {
            if (document.Width < AegisMapDocument.MinWidth)
                errors.Add("MapWidthTooSmall:Width must be at least " + AegisMapDocument.MinWidth + ".");
            if (document.Height < AegisMapDocument.MinHeight)
                errors.Add("MapHeightTooSmall:Height must be at least " + AegisMapDocument.MinHeight + ".");
            if (document.Width > AegisMapDocument.MaxWidth)
                errors.Add("MapWidthTooLarge:Width must be at most " + AegisMapDocument.MaxWidth + ".");
            if (document.Height > AegisMapDocument.MaxHeight)
                errors.Add("MapHeightTooLarge:Height must be at most " + AegisMapDocument.MaxHeight + ".");
        }

        static void ValidateTerrainLayer(int width, int height, List<AegisTerrainCell> cells, string layerName, List<string> errors)
        {
            if (cells == null)
                return;

            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell == null)
                {
                    errors.Add(layerName + ":NullCell:" + i);
                    continue;
                }

                ValidateCell(width, height, cell.X, cell.Y, layerName + ":" + i, errors);
                ValidateTerrainId(cell.TerrainId, layerName + ":" + i, errors);
            }
        }

        static void ValidateBlockers(int width, int height, List<AegisBlockerCell> blockers, List<string> errors)
        {
            if (blockers == null)
                return;

            for (var i = 0; i < blockers.Count; i++)
            {
                var blocker = blockers[i];
                if (blocker == null)
                {
                    errors.Add("BlockerNull:" + i);
                    continue;
                }

                ValidateCell(width, height, blocker.X, blocker.Y, "Blocker:" + i, errors);
            }
        }

        static void ValidateResources(int width, int height, List<AegisResourcePlacement> resources, List<string> errors)
        {
            if (resources == null)
                return;

            for (var i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                if (resource == null)
                {
                    errors.Add("ResourceNull:" + i);
                    continue;
                }

                ValidateCell(width, height, resource.X, resource.Y, "Resource:" + i, errors);
                if (string.IsNullOrEmpty(resource.ResourceKind))
                    errors.Add("ResourceKindMissing:" + i);
                if (resource.Amount <= 0)
                    errors.Add("ResourceAmountInvalid:" + i + ":Amount must be greater than zero.");
                if (resource.MaxAmount < resource.Amount)
                    errors.Add("ResourceMaxAmountInvalid:" + i + ":MaxAmount must be greater than or equal to Amount.");
                if (resource.RegenerationRatePerTick < 0)
                    errors.Add("ResourceRegenerationRateInvalid:" + i + ":RegenerationRatePerTick must not be negative.");
                if (resource.RegenerationDelayTicks < 0)
                    errors.Add("ResourceRegenerationDelayInvalid:" + i + ":RegenerationDelayTicks must not be negative.");
            }
        }

        static void ValidatePlayerStarts(int width, int height, List<AegisPlayerStart> starts, List<string> errors, List<string> warnings)
        {
            if (starts == null || starts.Count == 0)
            {
                warnings.Add("NoPlayerStarts:The map has no player start positions.");
                return;
            }

            var seen = new HashSet<int>();
            for (var i = 0; i < starts.Count; i++)
            {
                var start = starts[i];
                if (start == null)
                {
                    errors.Add("PlayerStartNull:" + i);
                    continue;
                }

                if (start.PlayerId <= 0)
                    errors.Add("PlayerStartInvalidPlayer:" + i + ":PlayerId must be greater than zero.");
                if (!seen.Add(start.PlayerId))
                    errors.Add("PlayerStartDuplicate:" + start.PlayerId);
                ValidateCell(width, height, start.X, start.Y, "PlayerStart:" + i, errors);
            }
        }

        static void ValidateActorPlacements(int width, int height, List<AegisActorPlacement> actors, RtsRules rules, List<string> errors)
        {
            if (actors == null)
                return;

            for (var i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                if (actor == null)
                {
                    errors.Add("ActorPlacementNull:" + i);
                    continue;
                }

                if (string.IsNullOrEmpty(actor.TypeId))
                    errors.Add("ActorTypeMissing:" + i);
                if (actor.OwnerPlayerId <= 0)
                    errors.Add("ActorOwnerInvalid:" + i + ":OwnerPlayerId must be greater than zero.");

                ValidateCell(width, height, actor.X, actor.Y, "ActorPlacement:" + i, errors);

                ActorDefinition definition;
                if (rules != null && !string.IsNullOrEmpty(actor.TypeId) && !rules.TryGetDefinition(actor.TypeId, out definition))
                    errors.Add("ActorTypeUnknown:" + i + ":" + actor.TypeId);
                else if (rules != null && !string.IsNullOrEmpty(actor.TypeId) && rules.TryGetDefinition(actor.TypeId, out definition))
                {
                    var building = definition as BuildingDefinition;
                    if (building != null)
                    {
                        var footprint = building.FootprintCells;
                        if (actor.X + footprint.X > width || actor.Y + footprint.Y > height)
                            errors.Add("ActorFootprintOutsideMap:" + i + ":" + actor.TypeId);
                    }
                }
            }
        }

        static void ValidateRegions(int width, int height, List<AegisRegion> regions, List<string> errors)
        {
            if (regions == null)
                return;

            for (var i = 0; i < regions.Count; i++)
            {
                var region = regions[i];
                if (region == null)
                {
                    errors.Add("RegionNull:" + i);
                    continue;
                }

                if (string.IsNullOrEmpty(region.RegionId))
                    errors.Add("RegionIdMissing:" + i);
                if (region.Width <= 0 || region.Height <= 0)
                    errors.Add("RegionSizeInvalid:" + i);
                ValidateCell(width, height, region.X, region.Y, "Region:" + i, errors);
                ValidateCell(width, height, region.X + region.Width - 1, region.Y + region.Height - 1, "RegionEnd:" + i, errors);
            }
        }

        static void ValidateNavOverrides(int width, int height, List<AegisNavOverride> navOverrides, List<string> errors)
        {
            if (navOverrides == null)
                return;

            for (var i = 0; i < navOverrides.Count; i++)
            {
                var navOverride = navOverrides[i];
                if (navOverride == null)
                {
                    errors.Add("NavOverrideNull:" + i);
                    continue;
                }

                ValidateCell(width, height, navOverride.X, navOverride.Y, "NavOverride:" + i, errors);
                if (navOverride.Cost < 0)
                    errors.Add("NavOverrideCostInvalid:" + i);
            }
        }

        static void ValidateCell(int width, int height, int x, int y, string subject, List<string> errors)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                errors.Add(subject + ":CellOutsideMap:" + new Int2(x, y));
        }

        static void ValidateTerrainId(string terrainId, string subject, List<string> errors)
        {
            TerrainKind ignored;
            if (!AegisMapTerrainIds.TryParseTerrainKind(terrainId, out ignored))
                errors.Add(subject + ":UnknownTerrain:" + terrainId);
        }
    }
}
