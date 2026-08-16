using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FOG.EscapeTheLava
{
    public sealed class BoardInput : MonoBehaviour
    {
        private Camera _targetCamera;
        private BoardController _board;

        public void Initialize(Camera cameraToUse, BoardController boardController)
        {
            _targetCamera = cameraToUse;
            _board = boardController;
        }

        private void Update()
        {
            if (_targetCamera == null || _board == null || !_board.AcceptsInput)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                var pointerPosition = Pointer.current.position.ReadValue();
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
                for (int touchIndex = 0; touchIndex < Input.touchCount; touchIndex++)
                {
                    Touch touch = Input.GetTouch(touchIndex);
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
            var worldPosition = _targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_targetCamera.transform.position.z));
            worldPosition.z = 0f;

            var tile = _board.GetTileAtWorldPosition(worldPosition);
            if (tile != null)
            {
                _board.NotifyTileClicked(tile, worldPosition, screenPosition);
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
