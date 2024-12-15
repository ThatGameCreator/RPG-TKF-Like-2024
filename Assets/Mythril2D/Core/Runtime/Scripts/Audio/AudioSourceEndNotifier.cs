using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gyvr.Mythril2D
{
    public class AudioSourceEndNotifier : MonoBehaviour
    {
        public UnityAction OnAudioPlayEnd;

        private AudioSource audioSource;
        private bool wasPlaying;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (audioSource == null) return;

            // 检测是否从播放状态变为非播放状态
            if (wasPlaying && !audioSource.isPlaying)
            {
                OnAudioPlayEnd?.Invoke();
                OnAudioPlayEnd = null; // 确保只调用一次
            }

            wasPlaying = audioSource.isPlaying;
        }
    }

}
