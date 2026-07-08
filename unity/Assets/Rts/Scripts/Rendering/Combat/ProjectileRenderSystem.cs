using System.Collections.Generic;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Performance;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Combat
{
    public sealed class ProjectileRenderSystem : MonoBehaviour
    {
        readonly Dictionary<int, ProjectileViewBehaviour> views = new Dictionary<int, ProjectileViewBehaviour>();
        readonly List<int> removeBuffer = new List<int>();

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        public CombatVisualProfileLibrary profileLibrary;
        public ObjectPoolService objectPoolService;

        Transform projectileRoot;

        public int ProjectileVisualCount { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper, CombatVisualProfileLibrary library, ObjectPoolService poolService = null)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
            profileLibrary = library;
            objectPoolService = poolService;
            if (profileLibrary != null)
                profileLibrary.EnsureInitialized();
            EnsureRoot();
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            if (snapshot == null)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (profileLibrary == null)
                profileLibrary = Object.FindFirstObjectByType<CombatVisualProfileLibrary>();
            if (mapper == null)
                return;

            EnsureRoot();
            var seen = new HashSet<int>();
            for (var i = 0; i < snapshot.Projectiles.Count; i++)
            {
                var projectile = snapshot.Projectiles[i];
                seen.Add(projectile.ProjectileId);
                ProjectileViewBehaviour view;
                if (!views.TryGetValue(projectile.ProjectileId, out view) || view == null)
                {
                    var obj = objectPoolService == null
                        ? new GameObject("Projectile " + projectile.ProjectileId + " " + projectile.WeaponId)
                        : objectPoolService.Acquire("ProjectileView", CreateProjectileObject, projectileRoot);
                    obj.name = "Projectile " + projectile.ProjectileId + " " + projectile.WeaponId;
                    obj.transform.SetParent(projectileRoot, false);
                    view = obj.GetComponent<ProjectileViewBehaviour>();
                    if (view == null)
                        view = obj.AddComponent<ProjectileViewBehaviour>();
                    views[projectile.ProjectileId] = view;
                }

                var profile = profileLibrary == null ? null : profileLibrary.GetProfileForWeapon(projectile.WeaponId);
                view.ApplySnapshot(
                    projectile,
                    mapper.FixedWorldToBoardWorld(projectile.CurrentPositionFixed),
                    mapper.FixedWorldToBoardWorld(projectile.TargetPositionFixed),
                    profile);
            }

            removeBuffer.Clear();
            foreach (var pair in views)
                if (!seen.Contains(pair.Key))
                    removeBuffer.Add(pair.Key);

            for (var i = 0; i < removeBuffer.Count; i++)
            {
                var id = removeBuffer[i];
                var view = views[id];
                views.Remove(id);
                if (view != null && objectPoolService != null)
                    objectPoolService.Release(view.gameObject);
                else if (view != null)
                    CombatObjectUtility.DestroyObject(view.gameObject);
            }

            ProjectileVisualCount = views.Count;
        }

        void EnsureRoot()
        {
            if (projectileRoot != null)
                return;
            var root = new GameObject("Projectile Views");
            root.transform.SetParent(transform, false);
            projectileRoot = root.transform;
        }

        static GameObject CreateProjectileObject()
        {
            var obj = new GameObject("Projectile View");
            obj.AddComponent<ProjectileViewBehaviour>();
            return obj;
        }
    }
}
