using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class Bonfire : OtherEntity
    {
        private void Update()
        {
            // 这个篝火重写父类的update防止调用战争迷雾的更新代码
            // 他这个傻卵插件如果一个物体同时调用两个光源，那么会卡得飞起，不晓得为什么
            // 直接没有一半以上的帧数
        }
    }
}
