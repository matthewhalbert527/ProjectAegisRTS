using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class CombatDebugHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public ProjectileRenderSystem projectileRenderSystem;
        public CombatEventRenderSystem combatEventRenderSystem;
        public bool visible = true;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible)
                return;
            if (driver == null)
                driver = Object.FindFirstObjectByType<RtsSimulationDriver>();

            GUILayout.BeginArea(new Rect(12f, 420f, 360f, 260f), "Stage 9 Combat", GUI.skin.window);
            if (driver == null || driver.LatestSnapshot == null)
            {
                GUILayout.Label("No combat snapshot.");
                GUILayout.EndArea();
                return;
            }

            var snapshot = driver.LatestSnapshot;
            GUILayout.Label("Tick: " + snapshot.Tick + "  Projectiles: " + snapshot.Projectiles.Count + "  Events: " + snapshot.CombatEvents.Count);
            GUILayout.Label("Selected: " + driver.SelectedActorIdsText());
            ActorSnapshot selected = null;
            if (driver.SelectedActorIds.Count > 0)
                driver.TryGetActorSnapshot(driver.SelectedActorIds[0], out selected);
            if (selected != null)
            {
                GUILayout.Label("Actor: " + selected.ActorId + " " + selected.TypeId + " HP " + selected.Health + "/" + selected.MaxHealth);
                GUILayout.Label("Weapon: " + selected.ActiveWeaponId + "  Cooldown: " + selected.WeaponCooldownRemaining);
                GUILayout.Label("Target: " + selected.AttackTargetActorId + "  Destroyed: " + selected.IsDestroyed);
            }

            if (GUILayout.Button("Reset Combat Demo"))
                driver.TryCreateCombatDemoWorld();
            if (GUILayout.Button("Select Attacker"))
                SelectFirstOwnedArmed(snapshot);
            if (GUILayout.Button("Issue Attack"))
                IssueFirstEnemyAttack(snapshot);
            if (GUILayout.Button("Stop"))
                driver.TryStopSelectedCombat();
            GUILayout.EndArea();
        }

        void SelectFirstOwnedArmed(WorldSnapshot snapshot)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId == driver.PlayerId && !actor.IsDestroyed)
                {
                    driver.SetSelectedActorIds(new[] { actor.ActorId });
                    return;
                }
            }
        }

        void IssueFirstEnemyAttack(WorldSnapshot snapshot)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != driver.PlayerId && !actor.IsDestroyed)
                {
                    driver.TryIssueAttackSelectedToActor(actor.ActorId);
                    return;
                }
            }
        }
    }
}
