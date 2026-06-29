using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Board
{
    public sealed class BoardPlacementController : MonoBehaviour
    {
        public Transform boardRoot;
        public BoardCoordinateMapper coordinateMapper;
        public BoardTransformModel model = new BoardTransformModel();
        public bool loadOnStart = true;
        public float moveSpeedMetersPerSecond = 4f;
        public float heightStep = 0.08f;
        public float yawStepDegrees = 45f;
        public float scaleStep = 0.08f;

        BoardTransformModel placementStartModel;
        bool initialized;

        public bool IsPlacementModeActive { get; private set; }
        public float CurrentHeight { get { return model.heightOffset; } }
        public float CurrentYaw { get { return model.yawDegrees; } }
        public float CurrentScale { get { return model.boardScale; } }
        public float CurrentMetersPerCell { get { return model.metersPerCell; } }
        public string StatusText { get; private set; }

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (initialized)
                return;

            EnsureReferences();
            if (loadOnStart)
                model.Load();

            ApplyModel();
            StatusText = "Board placement ready.";
            initialized = true;
        }

        public void TogglePlacementMode()
        {
            SetPlacementMode(!IsPlacementModeActive);
        }

        public void SetPlacementMode(bool active)
        {
            Initialize();
            if (IsPlacementModeActive == active)
                return;

            IsPlacementModeActive = active;
            if (active)
            {
                placementStartModel = model.Clone();
                StatusText = "Board placement mode active.";
            }
            else
            {
                placementStartModel = null;
                StatusText = "Board placement mode inactive.";
            }
        }

        public void ConfirmPlacement()
        {
            Initialize();
            IsPlacementModeActive = false;
            placementStartModel = null;
            model.Save();
            ApplyModel();
            StatusText = "Board placement confirmed and saved.";
        }

        public void CancelPlacement()
        {
            Initialize();
            if (placementStartModel != null)
                model.CopyFrom(placementStartModel);

            IsPlacementModeActive = false;
            placementStartModel = null;
            ApplyModel();
            StatusText = "Board placement cancelled.";
        }

        public void ResetPlacement()
        {
            Initialize();
            model.ResetDefault();
            ApplyModel();
            StatusText = "Board placement reset to defaults.";
        }

        public void Save()
        {
            Initialize();
            model.Save();
            StatusText = "Board placement saved.";
        }

        public void Load()
        {
            Initialize();
            model.Load();
            ApplyModel();
            StatusText = "Board placement loaded.";
        }

        public void Recenter(Vector3 position)
        {
            Initialize();
            model.Recenter(position);
            ApplyModel();
            StatusText = "Board recentered.";
        }

        public void MoveHorizontal(Vector3 delta)
        {
            Initialize();
            model.MoveHorizontal(delta);
            ApplyModel();
            StatusText = "Board moved.";
        }

        public void SetHeight(float value)
        {
            Initialize();
            model.SetHeight(value);
            ApplyModel();
            StatusText = "Board height adjusted.";
        }

        public void AdjustHeight(float delta)
        {
            SetHeight(model.heightOffset + delta);
        }

        public void SetYaw(float value)
        {
            Initialize();
            model.SetYaw(value);
            ApplyModel();
            StatusText = "Board yaw adjusted.";
        }

        public void AdjustYaw(float delta)
        {
            SetYaw(model.yawDegrees + delta);
        }

        public void SetScale(float value)
        {
            Initialize();
            model.SetScale(value);
            ApplyModel();
            StatusText = "Board scale adjusted.";
        }

        public void AdjustScale(float delta)
        {
            SetScale(model.boardScale + delta);
        }

        public void ApplyModel()
        {
            EnsureReferences();
            model.ApplyToTransform(boardRoot);
            if (coordinateMapper != null)
                coordinateMapper.Configure(model.boardWidth, model.boardHeight, model.metersPerCell, boardRoot);
        }

        void EnsureReferences()
        {
            if (boardRoot == null)
            {
                var existing = GameObject.Find("BoardRoot");
                if (existing != null)
                    boardRoot = existing.transform;
            }

            if (coordinateMapper == null && boardRoot != null)
                coordinateMapper = boardRoot.GetComponent<BoardCoordinateMapper>();
        }
    }
}
