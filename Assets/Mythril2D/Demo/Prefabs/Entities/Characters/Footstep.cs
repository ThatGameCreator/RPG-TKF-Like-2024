using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Gyvr.Mythril2D
{
    public class Footstep : MonoBehaviour
    {
        [SerializeField] private AudioClipResolver m_moveSound;
        [SerializeField] private AudioClipResolver m_runSound;
        [SerializeField] private GameObject m_body;

        public void PlayMoveFootstepSound()
        {
            if (m_body)
            {
                GameManager.AudioSystem.PlayAudioOnObject(m_moveSound, m_body);
            }
            else 
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_moveSound);
            }
        }

        public void PlayRunFootstepSound()
        {
            if (m_body)
            {
                GameManager.AudioSystem.PlayAudioOnObject(m_runSound, m_body);
            }
            else
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_runSound);

            }
        }
    }
}
    
