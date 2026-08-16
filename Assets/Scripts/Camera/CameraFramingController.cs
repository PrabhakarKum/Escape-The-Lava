using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class CameraFramingController : MonoBehaviour
    {
        private Camera _targetCamera;
        private GameConfig _config;
        private LevelDefinition _level;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        public void Initialize(Camera cameraToFrame, GameConfig gameConfig, LevelDefinition levelDefinition)
        {
            _targetCamera = cameraToFrame;
            _config = gameConfig;
            _level = levelDefinition;
            ApplyFrame();
        }

        private void Update()
        {
            if (_targetCamera == null || _config == null || _level == null)
            {
                return;
            }

            if (Screen.width == _lastScreenWidth && Screen.height == _lastScreenHeight)
            {
                return;
            }

            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (_targetCamera == null || _config == null || _level == null)
            {
                return;
            }

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            _targetCamera.orthographic = true;
            //_targetCamera.clearFlags = CameraClearFlags.SolidColor;
            //_targetCamera.backgroundColor = new Color(0.035f, 0.045f, 0.06f, 1f);
            _targetCamera.transform.position = new Vector3(0f, -0.42f, -10f);

            var aspect = Screen.height > 0 ? Screen.width / (float)Screen.height : 16f / 9f;
            var boardWidth = _level.Columns * _config.CellPitch;
            var boardHeight = _level.Rows * _config.CellPitch;
            var sizeForWidth = boardWidth / (2f * Mathf.Max(0.1f, aspect)) + 0.35f;
            var sizeForHeight = boardHeight * 0.5f + 0.95f;
            _targetCamera.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight);
        }
    }
}
