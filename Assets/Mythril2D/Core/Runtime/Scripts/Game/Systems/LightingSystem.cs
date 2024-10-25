using FunkyCode;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class LightingSystem : AGameSystem
    {
        [SerializeField] private LightingManager2D m_mainManager = null;

        public LightingManager2D mainManager => m_mainManager;
    }
}
