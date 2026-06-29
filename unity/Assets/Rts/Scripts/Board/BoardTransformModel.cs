using System;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Board
{
    [Serializable]
    public sealed class BoardTransformModel
    {
        const string PlayerPrefsKey = "ProjectAegisRTS.Stage3.BoardTransform";

        public Vector3 position = Vector3.zero;
        public float yawDegrees;
        public float heightOffset;
        public float boardScale = 1f;
        public float metersPerCell = 1f;
        public int boardWidth = 32;
        public int boardHeight = 32;

        public void ResetDefault()
        {
            position = Vector3.zero;
            yawDegrees = 0f;
            heightOffset = 0f;
            boardScale = 1f;
            metersPerCell = 1f;
            boardWidth = 32;
            boardHeight = 32;
        }

        public void ApplyToTransform(Transform boardRoot)
        {
            if (boardRoot == null)
                return;

            boardRoot.position = new Vector3(position.x, heightOffset, position.z);
            boardRoot.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            boardRoot.localScale = Vector3.one * boardScale;
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                ClampValues();
                return;
            }

            var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                ClampValues();
                return;
            }

            try
            {
                JsonUtility.FromJsonOverwrite(json, this);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Stage 3 board placement settings could not be loaded: " + ex.Message);
            }

            ClampValues();
        }

        public void Save()
        {
            ClampValues();
            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        public void SetHeight(float value)
        {
            heightOffset = Mathf.Clamp(value, -2f, 4f);
        }

        public void AdjustHeight(float delta)
        {
            SetHeight(heightOffset + delta);
        }

        public void SetYaw(float value)
        {
            yawDegrees = Mathf.Repeat(value, 360f);
        }

        public void AdjustYaw(float delta)
        {
            SetYaw(yawDegrees + delta);
        }

        public void SetScale(float value)
        {
            boardScale = Mathf.Clamp(value, 0.25f, 4f);
        }

        public void AdjustScale(float delta)
        {
            SetScale(boardScale + delta);
        }

        public void SetMetersPerCell(float value)
        {
            metersPerCell = Mathf.Clamp(value, 0.25f, 3f);
        }

        public void Recenter(Vector3 newPosition)
        {
            position = new Vector3(newPosition.x, 0f, newPosition.z);
            SetHeight(newPosition.y);
        }

        public void MoveHorizontal(Vector3 delta)
        {
            position += new Vector3(delta.x, 0f, delta.z);
        }

        public BoardTransformModel Clone()
        {
            var clone = new BoardTransformModel();
            clone.CopyFrom(this);
            return clone;
        }

        public void CopyFrom(BoardTransformModel other)
        {
            if (other == null)
                return;

            position = other.position;
            yawDegrees = other.yawDegrees;
            heightOffset = other.heightOffset;
            boardScale = other.boardScale;
            metersPerCell = other.metersPerCell;
            boardWidth = other.boardWidth;
            boardHeight = other.boardHeight;
            ClampValues();
        }

        public string ToSummary()
        {
            return "Position " + position +
                   " Height " + heightOffset.ToString("0.00") +
                   " Yaw " + yawDegrees.ToString("0.0") +
                   " Scale " + boardScale.ToString("0.00") +
                   " Meters/Cell " + metersPerCell.ToString("0.00");
        }

        void ClampValues()
        {
            SetHeight(heightOffset);
            SetYaw(yawDegrees);
            SetScale(boardScale);
            SetMetersPerCell(metersPerCell);
            boardWidth = Mathf.Max(1, boardWidth);
            boardHeight = Mathf.Max(1, boardHeight);
        }
    }
}
