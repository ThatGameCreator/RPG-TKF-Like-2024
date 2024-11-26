using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class OpenUpgradeMenu : ICommand
    {
        public void Execute()
        {
            GameManager.NotificationSystem.statsRequested.Invoke();
        }
    }
}
