using System.Collections;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public enum EAudioChannelMode
    {
        Multiple,
        Exclusive
    }

    public class AudioChannel : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private EAudioChannelMode m_audioChannelMode;
        [SerializeField] private AudioSource m_audioSource = null;
        [SerializeField] private float m_volumeScale = 0.5f;

        [Header("Exclusive Mode Settings")]
        [SerializeField] private float m_fadeOutDuration = 0.5f;
        [SerializeField] private float m_fadeInDuration = 0.25f;

        private Coroutine m_transitionCoroutine = null;
        private AudioClipResolver m_lastPlayedClip = null;

        public AudioClipResolver lastPlayedAudioClipResolver => m_lastPlayedClip;

        public AudioClip CurrentClip
        {
            get
            {
                // 如果音频源正在播放且有一个最近播放的音频剪辑，则返回该音频剪辑
                if (m_audioSource.isPlaying && m_lastPlayedClip != null)
                {
                    return m_lastPlayedClip.GetClip();
                }

                // 如果没有播放任何音频，则返回null
                return null;
            }
        }


        private void Awake()
        {
            m_audioSource.volume = m_volumeScale;
        }

        public void Stop()
        {
            // 如果当前是独占模式，我们需要处理淡出音频并停止播放
            if (m_audioChannelMode == EAudioChannelMode.Exclusive)
            {
                // 如果有进行中的淡入淡出协程，则停止它
                if (m_transitionCoroutine != null)
                {
                    StopCoroutine(m_transitionCoroutine);
                }

                // 启动淡出协程（可选），然后停止播放音频
                m_transitionCoroutine = StartCoroutine(FadeOutAndStop());
            }
            else
            {
                // 如果不是独占模式，直接停止当前播放的音效
                m_audioSource.Stop();
            }
        }
        private IEnumerator FadeOutAndStop()
        {
            float fadeDuration = 1.0f; // 假设淡出时间为1秒
            float startVolume = m_audioSource.volume;

            // 渐渐降低音量至0
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                m_audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            // 确保音量设为0并停止播放
            m_audioSource.volume = 0;
            m_audioSource.Stop();
        }

        public void Play(AudioClipResolver audioClipResolver)
        {
            AudioClip audioClip = audioClipResolver.GetClip();
            m_lastPlayedClip = audioClipResolver;

            if (audioClip != null)
            {
                if (m_audioChannelMode == EAudioChannelMode.Exclusive)
                {
                    if (m_transitionCoroutine != null)
                    {
                        StopCoroutine(m_transitionCoroutine);
                    }

                    m_transitionCoroutine = StartCoroutine(FadeOutAndIn(audioClip));
                }
                else
                {
                    m_audioSource.PlayOneShot(audioClip);
                }
            }
        }


        public void SetVolumeScale(float scale)
        {
            m_volumeScale = scale;
            m_audioSource.volume = m_volumeScale;
        }

        public float GetVolumeScale()
        {
            return m_volumeScale;
        }

        public IEnumerator FadeOutAndIn(AudioClip newClip)
        {
            // Fade out
            while (m_audioSource.volume > 0)
            {
                m_audioSource.volume -= m_volumeScale * Time.unscaledDeltaTime / m_fadeOutDuration;
                yield return null;
            }
            m_audioSource.Stop();
            m_audioSource.clip = newClip;
            m_audioSource.Play();

            // Fade in
            while (m_audioSource.volume < m_volumeScale)
            {
                m_audioSource.volume += m_volumeScale * Time.unscaledDeltaTime / m_fadeInDuration;
                yield return null;
            }
            m_audioSource.volume = m_volumeScale;
        }
    }
}
