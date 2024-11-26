using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class AEntitySpawner : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private  Entity[] entityPrefabs = null;
        [SerializeField] private int rate = 100;

        [Header("Spawn Settings")]
        [SerializeField] private int m_entitiesToPrespawn = 1;
        [SerializeField] private Vector2 m_offset = Vector2.zero;

        [Header("Spawn Limitations")]
        [SerializeField][Min(1)] private int m_maxEntityCount = 1;

        // Private Members
        private float m_spawnTimer = 0.0f;
        private bool m_valid = false;

        private int m_totalSpawnedEntityCount = 0;

        // Used for the first update to prespawn entities
        private bool m_isFirstUpdate = true;

        private void Prespawn()
        {
            for (int i = 0; i < m_entitiesToPrespawn; ++i)
            {
                TrySpawn();
            }
        }

        private void Update()
        {
            if (m_isFirstUpdate)
            {
                Prespawn();
                m_isFirstUpdate = false;
            }

            if (m_valid)
            {
                TrySpawn();
            }
        }

        private Entity FindEntityToSpawn()
        {
            int randomNumber = UnityEngine.Random.Range(0, 100);

            if (randomNumber <= rate && entityPrefabs.Length > 0)
            {
                // 随机选择一个 entitiy 预制体
                int index = UnityEngine.Random.Range(0, entityPrefabs.Length);
                return entityPrefabs[index];
            }
            else
            {
                return null;
            }
        }

        private bool CanSpawn()
        {
            return m_totalSpawnedEntityCount < m_maxEntityCount;
        }

        private void TrySpawn()
        {
            if (CanSpawn())
            {
                Spawn();
            }
        }

        protected Vector2 FindSpawnLocation()
        {
            return new Vector2(
                transform.position.x,
                transform.position.y
            ) + m_offset;
        }

        private void Spawn()
        {
            Vector2 position = FindSpawnLocation();
            Entity entity = FindEntityToSpawn();

            if (entity != null)
            {
                Entity instance = Instantiate(entity, position, Quaternion.identity, transform);
                instance.transform.parent = null;
                ++m_totalSpawnedEntityCount;
            }
        }
    }
}
