using System.Collections.Generic;
using UnityEngine;

namespace FunkyCode
{
	public class LightColliderLayer<T>
	{
		public List<T>[] layerList;

		public LightColliderLayer()
		{
			layerList = new List<T>[10];

			for(int i = 0; i < 10; i++)
			{
				layerList[i] = new List<T>();
			}
		}

        public int Update(int targetLayer, int newLayer, T obj)
        {
            if (newLayer >= layerList.Length)
            {
                // 动态扩展layerList
                var newSize = newLayer + 1;  // 确保有足够的空间
                var newLayerList = new List<T>[newSize];

                for (int i = 0; i < layerList.Length; i++)
                {
                    newLayerList[i] = layerList[i];
                }

                // 新的层未初始化，初始化新的层
                for (int i = layerList.Length; i < newSize; i++)
                {
                    newLayerList[i] = new List<T>();
                }

                layerList = newLayerList;
            }

            if (targetLayer != newLayer)
            {
                if (targetLayer > -1 && targetLayer < layerList.Length)
                {
                    layerList[targetLayer].Remove(obj);
                }

                targetLayer = newLayer;

                if (targetLayer >= 0 && targetLayer < layerList.Length)
                {
                    layerList[targetLayer].Add(obj);
                }
            }

            return targetLayer;
        }


        public void Remove(int targetLayer, T obj)
		{
			if (targetLayer > -1)
			{
				layerList[targetLayer].Remove(obj);
			}
		}
	}
}