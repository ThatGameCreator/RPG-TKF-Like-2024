using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode;

namespace Gyvr.Mythril2D
{
    public class Bonfire : OtherEntity
    {
        [SerializeField] private AudioClipResolver m_bonfireSound;

        public Light2D[] Lights; // �󶨵�����ƹ����
        public float positionScrollSpeed = 2f; // ���Ƶƹ�λ�ñ仯�ٶ�
        public float intensityScrollSpeed = 1f; // ���Ƶƹ�ǿ�ȱ仯�ٶ�
        public float intensityBase = 1f; // �ƹ����ǿ��
        public float positionJumpScale = 1f; // ���Ƶƹ�λ�ö����ķ���
        public float intensityJumpScale = 0.1f; // ���Ƶƹ�ǿ�ȶ����ķ���

        private Vector3[] initialPositions; // ��¼ÿ���ƹ�ĳ�ʼλ��

        private void Awake()
        {
            // ��ʼ���ƹ��ʼλ��
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

            // Awakeʱϵͳ��û�г�ʼ��
            GameManager.AudioSystem.PlayAudioOnObject(m_bonfireSound, this.gameObject, true);
        }

        private void Update()
        {
            if (Lights != null && Lights.Length > 0)
            {
                for (int i = 0; i < Lights.Length; i++)
                {
                    // ����λ�ñ仯
                    Lights[i].transform.localPosition = initialPositions[i] + PositionDelta(positionScrollSpeed, positionJumpScale);

                    // ����ǿ�ȱ仯
                    Lights[i].lightStrength = NewIntensity(intensityBase, intensityJumpScale, intensityScrollSpeed);
                }
            }
        }

        // ����ƹ�λ�õı仯
        private Vector3 PositionDelta(float scrollSpeed, float scale)
        {
            float x = Mathf.PerlinNoise(Time.time * scrollSpeed, 1f + Time.time * scrollSpeed) - 0.5f;
            float y = Mathf.PerlinNoise(2f + Time.time * scrollSpeed, 3f + Time.time * scrollSpeed) - 0.5f;
            return new Vector3(x, y, 0f) * scale; // ���� Z �᲻�������� X �� Y �ᶶ��
        }

        // ����ƹ�ǿ�ȵı仯
        private float NewIntensity(float baseIntensity, float jumpScale, float scrollSpeed)
        {
            return baseIntensity + (jumpScale * Mathf.PerlinNoise(Time.time * scrollSpeed, 1f + Time.time * scrollSpeed));
        }
    }

}
//    private void Update()
//    {
//        // ���������д�����update��ֹ����ս������ĸ��´���
//        // �����ɵ�Ѳ�����һ������ͬʱ����������Դ����ô�Ῠ�÷��𣬲�����Ϊʲô
//        // ֱ��û��һ�����ϵ�֡��
//    }
//}
