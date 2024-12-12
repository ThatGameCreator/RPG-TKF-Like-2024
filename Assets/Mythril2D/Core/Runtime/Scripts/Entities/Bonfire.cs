using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode;

namespace Gyvr.Mythril2D
{
    public class Bonfire : OtherEntity
    {
        [SerializeField] private AudioClipResolver m_bonfireSound;

        public Light2D[] Lights; // 绑定到多个灯光对象
        public float positionScrollSpeed = 2f; // 控制灯光位置变化速度
        public float intensityScrollSpeed = 1f; // 控制灯光强度变化速度
        public float intensityBase = 1f; // 灯光基础强度
        public float positionJumpScale = 1f; // 控制灯光位置抖动的幅度
        public float intensityJumpScale = 0.1f; // 控制灯光强度抖动的幅度

        private Vector3[] initialPositions; // 记录每个灯光的初始位置

        private void Awake()
        {
            // 初始化灯光初始位置
            if (Lights != null && Lights.Length > 0)
            {
                initialPositions = new Vector3[Lights.Length];
                for (int i = 0; i < Lights.Length; i++)
                {
                    initialPositions[i] = Lights[i].transform.localPosition;
                }
            }

        }

        protected override void Start()
        {
            base.Start();

            // Awake时系统还没有初始化
            GameManager.AudioSystem.PlayAudioOnObject(m_bonfireSound, this.gameObject, true);
        }

        private void Update()
        {
            if (Lights != null && Lights.Length > 0)
            {
                for (int i = 0; i < Lights.Length; i++)
                {
                    // 计算位置变化
                    Lights[i].transform.localPosition = initialPositions[i] + PositionDelta(positionScrollSpeed, positionJumpScale);

                    // 计算强度变化
                    Lights[i].lightStrength = NewIntensity(intensityBase, intensityJumpScale, intensityScrollSpeed);
                }
            }
        }

        // 计算灯光位置的变化
        private Vector3 PositionDelta(float scrollSpeed, float scale)
        {
            float x = Mathf.PerlinNoise(Time.time * scrollSpeed, 1f + Time.time * scrollSpeed) - 0.5f;
            float y = Mathf.PerlinNoise(2f + Time.time * scrollSpeed, 3f + Time.time * scrollSpeed) - 0.5f;
            return new Vector3(x, y, 0f) * scale; // 假设 Z 轴不动，仅在 X 和 Y 轴抖动
        }

        // 计算灯光强度的变化
        private float NewIntensity(float baseIntensity, float jumpScale, float scrollSpeed)
        {
            return baseIntensity + (jumpScale * Mathf.PerlinNoise(Time.time * scrollSpeed, 1f + Time.time * scrollSpeed));
        }
    }

}
//    private void Update()
//    {
//        // 这个篝火重写父类的update防止调用战争迷雾的更新代码
//        // 他这个傻卵插件如果一个物体同时调用两个光源，那么会卡得飞起，不晓得为什么
//        // 直接没有一半以上的帧数
//    }
//}
