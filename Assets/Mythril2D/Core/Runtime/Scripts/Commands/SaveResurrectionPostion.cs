using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public class SaveResurrectionPostion : ICommand
    {
        [SerializeField] private GameObject m_gameObject = null;

        public void Execute()
        {
            GameManager.MapLoadingSystem.SetSaveResurrectionPostion(m_gameObject.transform.position);
        }
    }
}
