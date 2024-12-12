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
        [SerializeField] private EAudioChannel m_audioChannelType;
        [SerializeField] private float m_volumeScale = 0.5f;

        [Header("Exclusive Mode Settings")]
        [SerializeField] private float m_fadeOutDuration = 0.5f;
        [SerializeField] private float m_fadeInDuration = 0.25f;

        private List<AudioSource> m_audioSourcePool = new List<AudioSource>();
        private Dictionary<AudioSource, Coroutine> m_fadeCoroutines = new Dictionary<AudioSource, Coroutine>();

        public AudioClipResolver lastPlayedAudioClipResolver => m_lastPlayedClip;
        private AudioClipResolver m_lastPlayedClip = null;
        private const int kMaxAudioSourcePoolSize = 20;
        private Dictionary<AudioSource, float> m_idleTimers = new Dictionary<AudioSource, float>();
        private const float kIdleTimeout = 30.0f; // 30秒闲置后销毁

        private void Awake()
        {
            // 初始化一个默认音频源
            CreateNewAudioSource();
        }

        private void Start()
        {
            //StartCoroutine(PeriodicRecycle());
        }

        public void FindPlayer()
        {
            // 找到玩家对象
            if (GameManager.Player != null)
            {
                // 将音频频道挂载到玩家对象上
                this.transform.parent = GameManager.Player.transform;
                this.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("Player object not found. Please ensure the player has the 'Player' tag.");
            }
        }

        // 为指定的对象播放音效，并将音源挂载到发出声音的对象上。
        public void PlayOnObject(AudioClipResolver audioClipResolver, GameObject emitter, bool isLoop = false)
        {
            // 从解析器中获取音频片段
            AudioClip clip = audioClipResolver.GetClip();
            if (clip == null) return;

            // 获取一个空闲的音频源
            AudioSource source = GetAvailableAudioSource();
            if (source == null)
            {
                Debug.LogWarning("No available AudioSource in the pool.");
                return;
            }

            // 将音频源挂载到发出声音的对象上
            source.transform.parent = emitter.transform;
            source.transform.localPosition = Vector3.zero;

            // 保存最后播放的音频片段
            m_lastPlayedClip = audioClipResolver;

            // 播放音效
            source.clip = clip;
            source.loop = isLoop; // 如果需要循环播放可以设置为 true
            source.Play();
        }

        // 创建新的音频源，并提供选择是否挂载到特定对象的能力
        private AudioSource CreateNewAudioSource(GameObject parent = null)
        {
            GameObject audioSourceObject = new GameObject("AudioSource");
            if (parent != null)
            {
                audioSourceObject.transform.parent = parent.transform;
            }
            else
            {
                audioSourceObject.transform.parent = this.transform;
            }

            audioSourceObject.transform.localPosition = Vector3.zero;

            AudioSource source = audioSourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = m_volumeScale;
            // 设置空间
            if(m_audioChannelType == EAudioChannel.GameplaySoundFX)
            {
                source.spatialBlend = 1;
                source.maxDistance = 18;
                //线性衰减
                source.rolloffMode = AudioRolloffMode.Linear;
            }

            m_audioSourcePool.Add(source);
            return source;
        }

        private IEnumerator PeriodicRecycle()
        {
            while (true)
            {
                UpdateIdleTimers();
                yield return new WaitForSeconds(10f); // 每隔10秒检查一次
            }
        }

        private void UpdateIdleTimers()
        {
            List<AudioSource> sourcesToRecycle = new List<AudioSource>();

            foreach (var source in m_audioSourcePool)
            {
                if (!source.isPlaying && source.clip == null)
                {
                    // 标记需要回收的音频源
                    sourcesToRecycle.Add(source);
                }
            }

            // 回收标记的音频源
            foreach (var source in sourcesToRecycle)
            {
                RecycleUnusedAudioSource(source);
            }
        }

        private void RecycleUnusedAudioSource(AudioSource source)
        {
            if (source == null) return; // 避免尝试销毁已经为 null 的对象

            // 从音频源池中移除
            m_audioSourcePool.Remove(source);

            // 销毁音频源的 GameObject
            if (source.gameObject != null)
            {
                Destroy(source.gameObject);
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            // 清理音频池中已经被销毁的引用
            m_audioSourcePool.RemoveAll(source => source == null);

            foreach (var source in m_audioSourcePool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            if (m_audioSourcePool.Count < kMaxAudioSourcePoolSize)
            {
                return CreateNewAudioSource();
            }

            Debug.LogWarning("AudioSource pool limit reached!");
            return null; // 或者返回一个可复用的已使用音频源
        }

        public void StopAllAudio()
        {
            foreach (var source in m_audioSourcePool)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
            }
        }

        public void StopSpecific(AudioClip clip)
        {
            foreach (var source in m_audioSourcePool)
            {
                if (source.clip == clip && source.isPlaying)
                {
                    source.Stop();
                    break;
                }
            }
        }

        public void Play(AudioClipResolver audioClipResolver)
        {
            // 从解析器中获取音频片段
            AudioClip clip = audioClipResolver.GetClip();
            if (clip == null) return;

            // 获取一个空闲的音频源
            AudioSource source = GetAvailableAudioSource();
            if (source == null || source.gameObject == null)
            {
                Debug.LogWarning("No available or valid AudioSource in the pool.");
                return;
            }

            // 保存最后播放的音频片段
            m_lastPlayedClip = audioClipResolver;

            if (m_audioChannelMode == EAudioChannelMode.Exclusive)
            {
                // 如果是独占模式，先停止所有正在播放的音频
                StopAllAudio();
                // 淡入播放新的音频片段
                StartFadeIn(source, clip);
            }
            else
            {
                // 如果是多音轨模式，检查音频源是否已经播放过该片段
                if (source.clip == clip && source.isPlaying)
                {
                    Debug.LogWarning("This audio clip is already playing in the current channel.");
                }
                else
                {
                    // 使用 Play 方法播放音频片段，而不是 PlayOneShot（可以增加对播放控制的灵活性）
                    source.clip = clip;
                    source.loop = false; // 如果需要循环播放可以设置为 true
                    source.Play();
                }
            }
        }

        private void StartFadeIn(AudioSource source, AudioClip clip)
        {
            if (m_fadeCoroutines.TryGetValue(source, out Coroutine fadeCoroutine))
            {
                StopCoroutine(fadeCoroutine);
            }

            source.clip = clip;
            source.volume = 0;
            source.Play();

            m_fadeCoroutines[source] = StartCoroutine(FadeVolume(source, 0, m_volumeScale, m_fadeInDuration));
        }

        private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            source.volume = to;
        }

        public void PlayWithCallback(AudioClipResolver audioClipResolver, System.Action<AudioSource> onComplete)
        {
            AudioClip newClip = audioClipResolver.GetClip();

            if (newClip != null)
            {
                AudioSource audioSource = GetAvailableAudioSource(); // 确保获取到空闲的音频源
                audioSource.clip = newClip;
                audioSource.Play();

                // 如果音频是循环播放的，则设置其循环标志
                audioSource.loop = true;

                // 启动协程来监听音频结束
                StartCoroutine(WaitForAudioEnd(audioSource, (source) =>
                {
                    // 音频播放结束后进行回调
                    onComplete?.Invoke(source);
                    // 在此处清理循环播放的设置
                    audioSource.loop = false; // 取消循环
                }));
            }
        }

        private IEnumerator WaitForAudioEnd(AudioSource audioSource, System.Action<AudioSource> onComplete)
        {
            yield return new WaitUntil(() => !audioSource.isPlaying); // 等待直到音频播放完毕
            onComplete?.Invoke(audioSource); // 回调通知音频结束
        }

        public void SetVolumeScale(float scale)
        {
            m_volumeScale = scale;

            // 为对象池中每个对象也要变换音量
            foreach(var audioSource  in m_audioSourcePool)
            {
                audioSource.volume = scale; 
            }
        }

        public float GetVolumeScale()
        {
            return m_volumeScale;
        }

        public void UpdateVolumeWithMaster(float masterVolume)
        {
            foreach (var source in m_audioSourcePool)
            {
                source.volume = m_volumeScale * masterVolume;
            }
        }
    }
}