using System.Collections.Generic;
using ProjectAegisRTS.Actors;
using ProjectAegisRTS.Commands;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Simulation;

namespace ProjectAegisRTS.UnityClient.CoreBridge
{
    public sealed class RtsCommandResult
    {
        public bool Success { get; private set; }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public string Details { get; private set; }

        RtsCommandResult(bool success, string code, string message, string details)
        {
            Success = success;
            Code = code;
            Message = message;
            Details = details;
        }

        public static RtsCommandResult FromCore(string action, CommandResult result)
        {
            var code = result.Success ? "OK" : result.ErrorCode;
            var details = result.Details == null ? string.Empty : string.Join("; ", result.Details);
            return new RtsCommandResult(result.Success, code, action + ": " + result.Message, details);
        }

        public static RtsCommandResult Ok(string message)
        {
            return new RtsCommandResult(true, "OK", message, string.Empty);
        }

        public static RtsCommandResult Fail(string code, string message)
        {
            return new RtsCommandResult(false, code, message, string.Empty);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Details))
                return Success ? Message : Code + ": " + Message;

            return (Success ? Message : Code + ": " + Message) + " (" + Details + ")";
        }
    }

    public static class RtsCommandAdapter
    {
        public static RtsCommandResult SelectActors(RtsWorld world, int playerId, IReadOnlyList<int> actorIds)
        {
            return RtsCommandResult.FromCore(
                "Select",
                world.IssueCommand(new SelectActorsCommand(playerId, ToActorIds(actorIds))));
        }

        public static RtsCommandResult IssueMoveOrder(RtsWorld world, int playerId, IReadOnlyList<int> actorIds, Int2 destinationCell)
        {
            return RtsCommandResult.FromCore(
                "Move",
                world.IssueCommand(new IssueMoveOrderCommand(playerId, ToActorIds(actorIds), destinationCell)));
        }

        public static RtsCommandResult IssueAttackOrder(RtsWorld world, int playerId, IReadOnlyList<int> actorIds, int targetActorId)
        {
            return RtsCommandResult.FromCore(
                "Attack",
                world.IssueCommand(new IssueAttackOrderCommand(playerId, ToActorIds(actorIds), new ActorId(targetActorId))));
        }

        public static RtsCommandResult IssueForceAttackCell(RtsWorld world, int playerId, IReadOnlyList<int> actorIds, Int2 targetCell)
        {
            return RtsCommandResult.FromCore(
                "Force attack",
                world.IssueCommand(new IssueForceAttackCellCommand(playerId, ToActorIds(actorIds), targetCell)));
        }

        public static RtsCommandResult BeginProduction(RtsWorld world, int playerId, int producerActorId, string typeId)
        {
            return RtsCommandResult.FromCore(
                "Begin production",
                world.IssueCommand(new BeginProductionCommand(playerId, new ActorId(producerActorId), typeId)));
        }

        public static RtsCommandResult PlaceBuilding(RtsWorld world, int playerId, string typeId, Int2 topLeftCell)
        {
            return RtsCommandResult.FromCore(
                "Place building",
                world.IssueCommand(new PlaceBuildingCommand(playerId, typeId, topLeftCell)));
        }

        public static RtsCommandResult CancelProduction(RtsWorld world, int playerId, int queueItemId)
        {
            return RtsCommandResult.FromCore(
                "Cancel production",
                world.IssueCommand(new CancelProductionCommand(playerId, queueItemId)));
        }

        public static RtsCommandResult StopActors(RtsWorld world, int playerId, IReadOnlyList<int> actorIds)
        {
            return RtsCommandResult.FromCore(
                "Stop",
                world.IssueCommand(new StopCommand(playerId, ToActorIds(actorIds))));
        }

        public static RtsCommandResult TogglePower(RtsWorld world, int playerId, int actorId)
        {
            return RtsCommandResult.FromCore(
                "Toggle power",
                world.IssueCommand(new PowerToggleCommand(playerId, new ActorId(actorId))));
        }

        static IReadOnlyList<ActorId> ToActorIds(IReadOnlyList<int> actorIds)
        {
            var converted = new List<ActorId>(actorIds.Count);
            for (var i = 0; i < actorIds.Count; i++)
                converted.Add(new ActorId(actorIds[i]));

            return converted;
        }
    }
}
