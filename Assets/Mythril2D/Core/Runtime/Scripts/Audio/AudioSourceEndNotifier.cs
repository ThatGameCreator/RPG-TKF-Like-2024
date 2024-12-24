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

            // ����Ƿ�Ӳ���״̬��Ϊ�ǲ���״̬
            if (wasPlaying && !audioSource.isPlaying)
            {
                OnAudioPlayEnd?.Invoke();
                OnAudioPlayEnd = null; // ȷ��ֻ����һ��
            }

            wasPlaying = audioSource.isPlaying;
        }
    }

}
