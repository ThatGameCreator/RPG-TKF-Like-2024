using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class SaveResurrectionPostion : ICommand
    {
        [SerializeField] private GameObject m_gameObject = null;
        [SerializeField] private AudioClipResolver m_saveSound = null;

        public void Execute()
        {
            //Debug.Log("SaveResurrectionPostion");

            GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_saveSound);

            GameManager.SaveSystem.SaveToFile(GameManager.SaveSystem.saveFileName);
        }
    }
}
