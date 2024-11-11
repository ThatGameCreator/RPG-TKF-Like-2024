using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class OpenUpgradeMenu : ICommand
    {
        public void Execute()
        {
            Debug.Log("OpenUpgradeMenu");

            GameManager.NotificationSystem.statsRequested.Invoke();
        }
    }
}
