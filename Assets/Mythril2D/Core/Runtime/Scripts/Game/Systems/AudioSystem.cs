using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        private float m_masterVolume = kDefaultMasterVolume;

        public override void OnSystemStart()
        {
            LoadSettings();

            // 主界面场景没有玩家不用寻找
            if(SceneManager.GetActiveScene().name != "Main Menu")
            {
                // 找到玩家对象并将音频频道挂载到玩家对象
                foreach (var channel in m_audioChannels.Values)
                {
                    channel.FindPlayer();
                }
            }
            

            GameManager.NotificationSystem.audioPlaybackRequested.AddListener(DispatchAudioPlaybackRequest);
            GameManager.NotificationSystem.audioStopPlaybackRequested.AddListener(StopAudioPlaybackRequest);
        }

        public override void OnSystemStop()
        {
            GameManager.NotificationSystem.audioPlaybackRequested.RemoveListener(DispatchAudioPlaybackRequest);
            GameManager.NotificationSystem.audioStopPlaybackRequested.RemoveListener(StopAudioPlaybackRequest);
            SaveSettings();
        }

        public float GetMasterVolume() => m_masterVolume;

        public void SetMasterVolume(float volume)
        {
            m_masterVolume = volume;
            AudioListener.volume = volume;

            foreach (var channel in m_audioChannels.Values)
            {
                channel.UpdateVolumeWithMaster(volume);
            }
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
                channel.Play(audioClipResolver);
            }
        }

        private void StopAudioPlaybackRequest(AudioClipResolver audioClipResolver)
        {
            if (audioClipResolver && m_audioChannels.TryGetValue(audioClipResolver.targetChannel, out AudioChannel channel))
            {
                channel.StopSpecific(audioClipResolver.GetClip());
            }
        }

        public void PlayWithLoop(AudioClipResolver audioClipResolver)
        {
            // 先检查音频通道是否存在
            if (m_audioChannels.TryGetValue(audioClipResolver.targetChannel, out AudioChannel channel))
            {
                // 如果音频通道存在，播放音频
                channel.PlayWithCallback(audioClipResolver, (source) =>
                {
                    // 在音频播放结束时，停止任何循环播放（通过StopAudioSource）并重新开始播放
                    if (source.loop)
                    {
                        // 如果音频设置了循环播放，则在播放结束时重新调用 PlayWithLoop
                        PlayWithLoop(audioClipResolver);
                    }
                });
            }
        }

        public void PlayAudioOnObject(AudioClipResolver audioClipResolver, GameObject targetObject, bool isLoop = false)
        {
            if (audioClipResolver && m_audioChannels.TryGetValue(audioClipResolver.targetChannel, out AudioChannel channel))
            {
                // 调用 AudioChannel 的新方法，将音源挂载到指定对象
                channel.PlayOnObject(audioClipResolver, targetObject, isLoop);
            }
        }

        public void StopAllChannels()
        {
            foreach (var channel in m_audioChannels.Values)
            {
                channel.StopAllAudio();
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

    }
}
