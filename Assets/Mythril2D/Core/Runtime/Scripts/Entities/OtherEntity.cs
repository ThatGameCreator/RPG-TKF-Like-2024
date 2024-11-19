using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class OtherEntity : Entity
    {
        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            UpdateFieldOfWar();
        }
    }
}
