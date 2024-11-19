using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class OtherEntity : Entity
    {
        [SerializeField] protected float m_lootedTime = 2.0f;

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void Update()
        {
            UpdateFieldOfWar();
        }
    }
}
