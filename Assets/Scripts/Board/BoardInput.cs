using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FOG.EscapeTheLava
{
    public sealed class BoardInput : MonoBehaviour
    {
        private Camera targetCamera;
        private BoardController board;

        public void Initialize(Camera cameraToUse, BoardController boardController)
        {
            targetCamera = cameraToUse;
            board = boardController;
        }

        private void Update()
        {
            if (targetCamera == null || board == null || !board.AcceptsInput)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                Vector2 pointerPosition = Pointer.current.position.ReadValue();
                if (!IsPointerOverUi(-1))
                {
                    TrySelect(pointerPosition);
                }

                return;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == UnityEngine.TouchPhase.Began && !IsPointerOverUi(touch.fingerId))
                    {
                        TrySelect(touch.position);
                    }
                }

                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi(-1))
            {
                TrySelect(Input.mousePosition);
            }
#endif
        }

        private void TrySelect(Vector2 screenPosition)
        {
            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -targetCamera.transform.position.z));
            worldPosition.z = 0f;

            TileView tile = board.GetTileAtWorldPosition(worldPosition);
            if (tile != null)
            {
                board.NotifyTileClicked(tile, worldPosition, screenPosition);
            }
        }

        private static bool IsPointerOverUi(int pointerId)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return pointerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(pointerId)
                : EventSystem.current.IsPointerOverGameObject();
        }
    }
}
