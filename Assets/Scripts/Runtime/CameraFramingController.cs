using UnityEngine;

namespace FOG.EscapeTheLava
{
    public sealed class CameraFramingController : MonoBehaviour
    {
        private Camera targetCamera;
        private GameConfig config;
        private LevelDefinition level;
        private int lastScreenWidth;
        private int lastScreenHeight;

        public void Initialize(Camera cameraToFrame, GameConfig gameConfig, LevelDefinition levelDefinition)
        {
            targetCamera = cameraToFrame;
            config = gameConfig;
            level = levelDefinition;
            ApplyFrame();
        }

        private void Update()
        {
            if (targetCamera == null || config == null || level == null)
            {
                return;
            }

            if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
            {
                return;
            }

            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (targetCamera == null || config == null || level == null)
            {
                return;
            }

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            targetCamera.orthographic = true;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = new Color(0.035f, 0.045f, 0.06f, 1f);
            targetCamera.transform.position = new Vector3(0f, -0.42f, -10f);

            float aspect = Screen.height > 0 ? Screen.width / (float)Screen.height : 16f / 9f;
            float boardWidth = level.Columns * config.CellPitch;
            float boardHeight = level.Rows * config.CellPitch;
            float sizeForWidth = boardWidth / (2f * Mathf.Max(0.1f, aspect)) + 0.35f;
            float sizeForHeight = boardHeight * 0.5f + 0.95f;
            targetCamera.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight);
        }
    }
}
