using Gyvr.Mythril2D;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float m_volumeScale = 0.5f;

        [Header("Exclusive Mode Settings")]
        [SerializeField] private float m_fadeOutDuration = 0.5f;
        [SerializeField] private float m_fadeInDuration = 0.25f;

        public List<AudioSource> audioSourcePool => m_audioSourcePool;
        private List<AudioSource> m_audioSourcePool = new List<AudioSource>();
        private Dictionary<AudioSource, Coroutine> m_fadeCoroutines = new Dictionary<AudioSource, Coroutine>();
        private AudioClipResolver m_lastPlayedClip = null;

        public AudioClipResolver lastPlayedAudioClipResolver => m_lastPlayedClip;

        private void Awake()
        {
            // 初始化一个默认音频源
            CreateNewAudioSource();
        }

        private AudioSource CreateNewAudioSource()
        {
            // 创建一个新的 GameObject 作为音频源
            GameObject newAudioSourceObject = new GameObject("AudioSource_" + m_audioSourcePool.Count);
            newAudioSourceObject.transform.parent = this.transform; // 将新音频源作为当前对象的子对象

            // 添加 AudioSource 组件
            AudioSource newAudioSource = newAudioSourceObject.AddComponent<AudioSource>();
            newAudioSource.playOnAwake = false; // 确保音频不会自动播放
            newAudioSource.volume = m_volumeScale; // 应用当前音量比例
            m_audioSourcePool.Add(newAudioSource); // 将新音频源加入池中

            return newAudioSource;
        }

        private AudioSource GetAvailableAudioSource()
        {
            foreach (var source in m_audioSourcePool)
            {
                if (!source.isPlaying)
                {
                    return source; // 返回空闲的音频源
                }
            }
            // 如果没有空闲的音频源，创建一个新的音频源
            return CreateNewAudioSource();
        }

        public void Stop(AudioSource specificSource = null)
        {
            if (specificSource != null)
            {
                StopAudioSource(specificSource);
            }
            else
            {
                // 停止所有音频源
                foreach (var source in m_audioSourcePool)
                {
                    StopAudioSource(source);
                }
            }
        }

        private void StopAudioSource(AudioSource source)
        {
            if (m_fadeCoroutines.ContainsKey(source))
            {
                StopCoroutine(m_fadeCoroutines[source]);
                m_fadeCoroutines.Remove(source);
            }
            source.Stop();
        }

        public void Play(AudioClipResolver audioClipResolver)
        {
            AudioClip newClip = audioClipResolver.GetClip();
            if (newClip == null) return;

            AudioSource availableSource = GetAvailableAudioSource();
            m_lastPlayedClip = audioClipResolver;

            if (m_audioChannelMode == EAudioChannelMode.Exclusive)
            {
                // 停止所有其他音频源
                Stop();

                if (m_fadeCoroutines.ContainsKey(availableSource))
                {
                    StopCoroutine(m_fadeCoroutines[availableSource]);
                }

                m_fadeCoroutines[availableSource] = StartCoroutine(FadeInAndPlay(availableSource, newClip));
            }
            else
            {
                availableSource.PlayOneShot(newClip);
            }
        }

        private IEnumerator FadeInAndPlay(AudioSource audioSource, AudioClip newClip)
        {
            // Fade out if playing
            if (audioSource.isPlaying)
            {
                float startVolume = audioSource.volume;
                for (float t = 0; t < m_fadeOutDuration; t += Time.deltaTime)
                {
                    audioSource.volume = Mathf.Lerp(startVolume, 0, t / m_fadeOutDuration);
                    yield return null;
                }
                audioSource.Stop();
            }

            // Play new clip
            audioSource.clip = newClip;
            audioSource.volume = 0;
            audioSource.Play();

            // Fade in
            for (float t = 0; t < m_fadeInDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(0, m_volumeScale, t / m_fadeInDuration);
                yield return null;
            }
            audioSource.volume = m_volumeScale;
        }

        public void PlayWithCallback(AudioClipResolver audioClipResolver, System.Action<AudioSource> onComplete)
        {
            AudioClip newClip = audioClipResolver.GetClip();

            if (newClip != null)
            {
                AudioSource audioSource = GetAvailableAudioSource(); // 确保获取到空闲的音频源
                audioSource.clip = newClip;
                audioSource.Play();

                StartCoroutine(WaitForAudioEnd(audioSource, onComplete)); // 监听音频结束
            }
        }

        private IEnumerator WaitForAudioEnd(AudioSource audioSource, System.Action<AudioSource> onComplete)
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
            onComplete?.Invoke(audioSource);
        }

        public void SetVolumeScale(float scale)
        {
            m_volumeScale = scale;
            foreach (var source in m_audioSourcePool)
            {
                if (!source.isPlaying)
                {
                    source.volume = m_volumeScale;
                }
            }
        }

        public float GetVolumeScale()
        {
            return m_volumeScale;
        }
    }
}