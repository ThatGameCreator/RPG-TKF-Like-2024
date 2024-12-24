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
                // 好像并没有执行

                // 找到玩家对象并将音频频道挂载到玩家对象
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

        // 加载音频设置方法
        private void LoadSettings()
        {
            // 先加载主音量，如果未存储，则设置为1（最大值）
            m_masterVolume = PlayerPrefs.GetFloat(kMasterVolumePlayerPrefsKey, 1.0f);
            SetMasterVolume(m_masterVolume);


            // 遍历所有音频通道
            foreach (KeyValuePair<EAudioChannel, AudioChannel> channel in m_audioChannels)
            {
                // 从 PlayerPrefs 获取当前通道的音量比例，若无保存值则使用默认值
                float savedVolume = PlayerPrefs.GetFloat(
                    $"{kChannelVolumePlayerPrefsKey}{channel.Key}",
                    1.0f // 默认值为1
                );

                // 根据主音量对音量比例进行调整并应用到音频通道
                channel.Value.SetVolumeScale(savedVolume / m_masterVolume);
            }
        }

        // 保存音频设置方法
        private void SaveSettings()
        {
            // 将主音量保存到 PlayerPrefs 中
            PlayerPrefs.SetFloat(kMasterVolumePlayerPrefsKey, m_masterVolume);

            // 遍历所有音频通道
            foreach (KeyValuePair<EAudioChannel, AudioChannel> channel in m_audioChannels)
            {
                // 当前音量比例
                float volumeScale = channel.Value.GetVolumeScale();

                if (volumeScale <= 0) // 确保不为0
                    volumeScale = 1.0f;

                // 保存当前通道的音量比例到 PlayerPrefs 中
                // 键名：频道音量的唯一标识
                PlayerPrefs.SetFloat($"{kChannelVolumePlayerPrefsKey}{channel.Key}", volumeScale);

            }

            // 调用 Save() 方法确保设置持久化到存储中
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
