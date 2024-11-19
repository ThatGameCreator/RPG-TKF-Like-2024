using FunkyCode;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class UIManagerSystem : AGameSystem
    {
        [SerializeField] private UIMenuManager m_UIMenu = null;
        public UIMenuManager UIMenu => m_UIMenu;
    }
}
