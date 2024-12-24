﻿using Gyvr.Mythril2D;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        [SerializeField] private List<AudioSource> m_audioSourcePool = new List<AudioSource>();
        private Dictionary<AudioSource, Coroutine> m_fadeCoroutines = new Dictionary<AudioSource, Coroutine>();
        private HashSet<AudioSource> activeAudioSources = new HashSet<AudioSource>();

        public AudioClipResolver lastPlayedAudioClipResolver => m_lastPlayedClip;
        private AudioClipResolver m_lastPlayedClip = null;
        private const int kMaxAudioSourcePoolSize = 20;
        private Dictionary<AudioSource, float> m_idleTimers = new Dictionary<AudioSource, float>();

        private void Awake()
        {
            // 初始化一个默认音频源
            CreateNewAudioSource();
        }

        private void Start()
        {
            //StartCoroutine(PeriodicRecycle());
        }

        private void OnEnable()
        {
            //StartCoroutine(PeriodicRecycle());
        }

        private void OnDisable()
        {
            StopAllCoroutines(); // 停止所有协程，避免冲突
        }


        private IEnumerator PeriodicRecycle()
        {
            while (m_audioSourcePool != null)
            {
                UpdateIdleTimers();
                // 设置几秒执行一次回收协程
                yield return new WaitForSeconds(10f);
            }
        }

        private void UpdateIdleTimers()
        {
            //foreach (var source in m_audioSourcePool)
            //{
            //    // 检查是否需要回收
            //    if (!source.isPlaying)
            //    {
            //        source.clip = null; // 手动清空 clip
            //    }
            //}

            //Debug.Log($"UpdateIdleTimers called. Pool size: {m_audioSourcePool.Count}");

            // 初始化或特定情况下，清理所有非活跃音频源
            List<AudioSource> sourcesToRecycle = new List<AudioSource>();

            foreach (var source in m_audioSourcePool)
            {
                // AudioSource.clip 可能在音频播放结束后仍然保留，未被设置为 null
                if (!source.isPlaying && source.clip == null)
                {
                    // 标记需要回收的音频源
                    sourcesToRecycle.Add(source);

                    //Debug.Log($"AudioSource {source.name} marked for recycling.");
                }
            }

            // 回收标记的音频源
            foreach (var source in sourcesToRecycle)
            {
                RecycleUnusedAudioSource(source);
            }

            //Debug.Log($"Idle timers updated. {sourcesToRecycle.Count} sources recycled.");
        }

        private void RecycleUnusedAudioSource(AudioSource source)
        {
            if (source == null) return;

            // 从音频池中移除
            m_audioSourcePool.Remove(source);

            // 销毁 GameObject
            if (source.gameObject != null)
            {
                Destroy(source.gameObject);
            }

            // 确保引用被清理
            source = null;
        }

        public void PlayAudio(AudioSource source = null, AudioClip clip = null, float volume = 1, bool isLoop = false)
        {
            if (source == null || clip == null)
            {
                Debug.LogError("AudioSource or AudioClip is null, cannot play audio!");
                return;
            }

            // 将音频源标记为活跃
            activeAudioSources.Add(source);

            source.clip = clip;
            // 不知道是不是默认的关系，现在设置音效没卵用
            //source.volume = volume;
            source.volume = m_volumeScale;
            source.loop = isLoop; // 如果需要循环播放可以设置为 true
            source.Play();

            // 绑定播放结束的回调事件
            //BindAudioSourceCallback(source);
            // 延迟绑定回调，确保音频播放稳定
            StartCoroutine(DelayedBindCallback(source, 1f));
        }

        private IEnumerator DelayedBindCallback(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (source)
            {
                var notifier = source.gameObject.GetComponent<AudioSourceEndNotifier>();
                if (notifier == null)
                {
                    notifier = source.gameObject.AddComponent<AudioSourceEndNotifier>();
                }

                notifier.OnAudioPlayEnd = () =>
                {
                    activeAudioSources.Remove(source);
                    source.clip = null;
                    RecycleUnusedAudioSource(source);
                };
            }
        }

        private void BindAudioSourceCallback(AudioSource source)
        {
            var notifier = source.gameObject.GetComponent<AudioSourceEndNotifier>();
            if (notifier == null)
            {
                notifier = source.gameObject.AddComponent<AudioSourceEndNotifier>();
            }

            notifier.OnAudioPlayEnd = () =>
            {
                // 确保音频已停止
                if (source.isPlaying) return;

                // 从活跃列表中移除
                activeAudioSources.Remove(source);

                // 将 clip 设置为 null 以满足回收条件
                source.clip = null;

                // 调用回收逻辑
                RecycleUnusedAudioSource(source);
            };
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
            PlayAudio(source, clip, 1, isLoop);
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
                // 多个防空引用试试
                if (source.clip != null && source.clip == clip && source.isPlaying)
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
                    // 如果需要循环播放可以设置为 true
                    PlayAudio(source, clip, 1, false);
                }
            }
        }

        private void StartFadeIn(AudioSource source, AudioClip clip)
        {
            if (m_fadeCoroutines.TryGetValue(source, out Coroutine fadeCoroutine))
            {
                StopCoroutine(fadeCoroutine);
            }

            //PlayAudio(source, clip, 0, false);
            PlayAudio(source, clip, 1, false);

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
                //audioSource.clip = newClip;
                //audioSource.Play();

                //// 如果音频是循环播放的，则设置其循环标志
                //audioSource.loop = true;
                PlayAudio(audioSource, newClip, 1, true);

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