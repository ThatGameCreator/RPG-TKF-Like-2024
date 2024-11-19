using UnityEngine;

namespace Gyvr.Mythril2D
{
    public enum EVerticalDirection { None, Up, Down }
    public enum EHorizontalDirection { None, Left, Right }

    public class Teleporter : TeleportLoadingSystem
    {
        [Header("Destination Settings")]
        [SerializeField] private string m_destinationMap = string.Empty;
        [SerializeField] private string m_destinationGameObjectName = string.Empty;

        [Header("Activation Settings")]
        [SerializeField] private EVerticalDirection m_requiredVerticalMovement = EVerticalDirection.None;
        [SerializeField] private EHorizontalDirection m_requiredHorizontalMovement = EHorizontalDirection.None;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_activationAudio;

        // Used to prevent a teleporter from triggering multiple teleportations before the previous one is fully completed
        private static bool _teleportationInProgress = false;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!_teleportationInProgress && collision != null && collision.gameObject == GameManager.Player.gameObject)
            {
                if (m_requiredVerticalMovement == EVerticalDirection.Up && !GameManager.Player.IsMovingUp()) return;
                if (m_requiredVerticalMovement == EVerticalDirection.Down && !GameManager.Player.IsMovingDown()) return;
                if (m_requiredHorizontalMovement == EHorizontalDirection.Left && !GameManager.Player.IsMovingLeft()) return;
                if (m_requiredHorizontalMovement == EHorizontalDirection.Right && !GameManager.Player.IsMovingRight()) return;

                GameManager.Player.InterruptPush();

                _teleportationInProgress = true;

                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_activationAudio);

                // Debug.Log(m_destinationMap);

                // same map teleport
                if (m_destinationMap == GameManager.TeleportLoadingSystem.GetCurrentMapName())
                {
                    _teleportationInProgress = TeloportPlayerPosition(m_destinationGameObjectName);
                }
                else
                {
                    GameManager.TeleportLoadingSystem.RequestTransition(m_destinationMap, null, () =>
                    {
                        _teleportationInProgress = TeloportPlayerPosition(m_destinationGameObjectName);
                    });
                }
            }
        }

    }
}
