using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Gyvr.Mythril2D
{
    public class Footstep : MonoBehaviour
    {
        [SerializeField] private AudioClipResolver m_moveSound;
        [SerializeField] private AudioClipResolver m_runSound;

        public void PlayMoveFootstepSound()
        {
            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_moveSound);
        }

        public void PlayRunFootstepSound()
        {
            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_runSound);
        }
    }
}
    
