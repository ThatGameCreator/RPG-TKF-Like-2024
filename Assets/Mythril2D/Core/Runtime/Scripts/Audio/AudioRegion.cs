using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class AudioRegion : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected AudioClipResolver m_audioClipResolver = null;
        [SerializeField] private bool m_loopPlayback = false; // 新增循环播放选项

        private AudioClipResolver m_previousAudio = null;

        public bool IsPlayer(Collider2D collision)
        {
            if (GameManager.Player && GameManager.Player.gameObject)
            {
                return collision.gameObject == GameManager.Player.gameObject;
            }

            return false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsPlayer(collision))
            {
                m_previousAudio = GameManager.AudioSystem.GetLastPlayedAudioClipResolver(m_audioClipResolver.targetChannel);

                if (m_loopPlayback)
                {
                    GameManager.AudioSystem.PlayWithLoop(m_audioClipResolver); // 使用支持循环的播放方法
                }
                else
                {
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_audioClipResolver);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (IsPlayer(collision))
            {
                GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_previousAudio);
            }
        }
    }
}