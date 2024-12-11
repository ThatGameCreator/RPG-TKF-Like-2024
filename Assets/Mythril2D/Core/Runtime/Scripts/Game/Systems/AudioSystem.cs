using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public enum EAudioChannel
    {
        BackgroundMusic,
        BackgroundSound,
        InterfaceSoundFX,
        GameplaySoundFX,
        Miscellaneous
    }

    public class AudioSystem : AGameSystem
    {
        [SerializeField] private SerializableDictionary<EAudioChannel, AudioChannel> m_audioChannels;

        const string kVolumePlayerPrefsKey = "M2D_AudioSystem_Volume_";
        const string kChannelVolumePlayerPrefsKey = kVolumePlayerPrefsKey + "Channel_";
        const string kMasterVolumePlayerPrefsKey = kVolumePlayerPrefsKey + "Master";
        const float kDefaultMasterVolume = 0.5f;

        private AudioClipResolver currentAudioClipResolver = null;
        private float m_masterVolume = kDefaultMasterVolume;

        public override void OnSystemStart()
        {
            LoadSettings();
            GameManager.NotificationSystem.audioPlaybackRequested.AddListener(DispatchAudioPlaybackRequest);
            GameManager.NotificationSystem.audioStopPlaybackRequested.AddListener(StopAudioPlaybackRequest);
        }

        public override void OnSystemStop()
        {
            GameManager.NotificationSystem.audioPlaybackRequested.RemoveListener(DispatchAudioPlaybackRequest);
            GameManager.NotificationSystem.audioStopPlaybackRequested.RemoveListener(StopAudioPlaybackRequest);
            SaveSettings();
        }

        public void SetMasterVolume(float volume)
        {
            m_masterVolume = volume;
            AudioListener.volume = volume;

            // 更新所有通道的音量以适应主音量更改
            foreach (var channel in m_audioChannels.Values)
            {
                channel.SetVolumeScale(channel.GetVolumeScale() * m_masterVolume);
            }
        }

        public float GetMasterVolume() => m_masterVolume;

        private void LoadSettings()
        {
            SetMasterVolume(PlayerPrefs.GetFloat(kMasterVolumePlayerPrefsKey, m_masterVolume));

            foreach (KeyValuePair<EAudioChannel, AudioChannel> channel in m_audioChannels)
            {
                float savedVolume = PlayerPrefs.GetFloat($"{kChannelVolumePlayerPrefsKey}{channel.Key}", channel.Value.GetVolumeScale());
                channel.Value.SetVolumeScale(savedVolume / m_masterVolume);
            }
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(kMasterVolumePlayerPrefsKey, m_masterVolume);

            foreach (KeyValuePair<EAudioChannel, AudioChannel> channel in m_audioChannels)
            {
                PlayerPrefs.SetFloat($"{kChannelVolumePlayerPrefsKey}{channel.Key}", channel.Value.GetVolumeScale());
            }

            PlayerPrefs.Save();
        }

        private void DispatchAudioPlaybackRequest(AudioClipResolver audioClipResolver)
        {
            if (audioClipResolver && m_audioChannels.TryGetValue(audioClipResolver.targetChannel, out AudioChannel channel))
            {
                currentAudioClipResolver = audioClipResolver;
                channel.Play(currentAudioClipResolver);
            }
        }

        private void StopAudioPlaybackRequest(AudioClipResolver audioClipResolver)
        {
            if (audioClipResolver && m_audioChannels.TryGetValue(audioClipResolver.targetChannel, out AudioChannel channel))
            {
                // 检查当前正在播放的音效是否与请求的音效相同
                foreach (var source in channel.audioSourcePool)
                {
                    if (source.clip == audioClipResolver.GetClip() && source.isPlaying)
                    {
                        source.Stop();
                        break;
                    }
                }
            }
        }

        public void PlayWithLoop(AudioClipResolver audioClipResolver)
        {
            if (m_audioChannels.TryGetValue(audioClipResolver.targetChannel, out AudioChannel channel))
            {
                channel.PlayWithCallback(audioClipResolver, (source) =>
                {
                    // 在音频结束后重新播放
                    PlayWithLoop(audioClipResolver);
                });
            }
        }

        public AudioClipResolver GetLastPlayedAudioClipResolver(EAudioChannel channel)
        {
            if (m_audioChannels.TryGetValue(channel, out AudioChannel channelInstance))
            {
                return channelInstance.lastPlayedAudioClipResolver;
            }

            return null;
        }

        public void SetChannelVolumeScale(EAudioChannel channel, float volume)
        {
            if (m_audioChannels.TryGetValue(channel, out AudioChannel channelInstance))
            {
                channelInstance.SetVolumeScale(volume);
            }
        }

        public float GetChannelVolumeScale(EAudioChannel channel)
        {
            if (m_audioChannels.TryGetValue(channel, out AudioChannel channelInstance))
            {
                return channelInstance.GetVolumeScale();
            }

            return 0.0f;
        }
    }
}
