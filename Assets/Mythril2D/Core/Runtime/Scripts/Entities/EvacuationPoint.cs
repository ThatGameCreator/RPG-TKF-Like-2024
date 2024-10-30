using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class EvacuationPoint : Entity
    {
        [SerializeField] BoxCollider2D m_boxCollider2D = null;

        private void Update()
        {
            if(GameManager.Player.isEvacuating == true)
            {
                m_boxCollider2D.enabled = false;
            }
            else
            {
                m_boxCollider2D.enabled = true;
            }
        }
    }
}

