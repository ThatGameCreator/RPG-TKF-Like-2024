using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [Serializable]
    public struct MonsterSpawn
    {
        public GameObject prefab;
        public int rate;
    }

    public abstract class AMonsterSpawner : MonoBehaviour
    {
        [Header("General Settings")]
        //[SerializeField] private MonsterSpawn[] m_monsters = null;
        [SerializeField] private GameObject monsterPrefab = null;
        [SerializeField] private int rate = 100;
        [SerializeField][Range(Stats.MinLevel, Stats.MaxLevel)] private int m_minLevel = Stats.MinLevel;
        [SerializeField][Range(Stats.MinLevel, Stats.MaxLevel)] private int m_maxLevel = Stats.MaxLevel;

        [Header("Spawn Settings")]
        [SerializeField] private float m_spawnCooldown = 5.0f;
        [SerializeField] private int m_monstersToPrespawn = 4;
        [SerializeField] private int m_maxSimulatenousMonsterCount = 4;

        [Header("Spawn Limitations")]
        [SerializeField] private bool m_limitMonsterCount = false;
        [SerializeField][Min(1)] private int m_maxMonsterCount = 1;

        private HashSet<Monster> m_spawnedMonsters = new HashSet<Monster>();

        // Private Members
        private float m_spawnTimer = 0.0f;
        private bool m_valid = false;

        private int m_totalSpawnedMonsterCount = 0;

        // Used for the first update to prespawn monsters
        private bool m_isFirstUpdate = true;

        protected abstract Vector2 FindSpawnLocation();

        //private bool Validate()
        //{
        //    int rateSum = 0;

        //    foreach (MonsterSpawn monster in m_monsters)
        //    {
        //        rateSum += monster.rate;
        //    }

        //    return m_valid = rateSum == 100;
        //}

        private void Prespawn()
        {
            //if (Validate() == true)
            //{
            //    //Array.Sort(m_monsters, (a, b) => a.rate.CompareTo(b.rate));

                
            //}
            //else
            //{
            //    Debug.LogError("MonsterSpawner validation failed. Make sure the total spawn rate is equal to 100");
            //}

            for (int i = 0; i < m_monstersToPrespawn; ++i)
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

            if (m_valid && m_spawnedMonsters.Count < m_maxSimulatenousMonsterCount)
            {
                m_spawnTimer += Time.deltaTime;

                if (m_spawnTimer > m_spawnCooldown)
                {
                    m_spawnTimer = 0.0f;
                    TrySpawn();
                }
            }
        }

        private GameObject FindMonsterToSpawn()
        {
            int randomNumber = UnityEngine.Random.Range(0, 100);

            if (randomNumber <= rate)
            {
                return monsterPrefab;
            }
            else
            {
                return null;
            }

            //foreach (MonsterSpawn monster in m_monsters)
            //{
            //    if (randomNumber <= monster.rate)
            //    {
            //        return monster.prefab;
            //    }
            //    else
            //    {
            //        //randomNumber -= monster.rate;
            //    }
            //}

        }

        private bool CanSpawn()
        {
            return !m_limitMonsterCount || m_totalSpawnedMonsterCount < m_maxMonsterCount;
        }

        private void TrySpawn()
        {
            if (CanSpawn())
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            Vector2 position = FindSpawnLocation();
            GameObject monster = FindMonsterToSpawn();

            if (monster)
            {
                GameObject instance = Instantiate(monster, position, Quaternion.identity, transform);
                instance.transform.parent = null;

                Monster monsterComponent = instance.GetComponent<Monster>();
                ++m_totalSpawnedMonsterCount;

                if (monsterComponent)
                {
                    monsterComponent.SetLevel(UnityEngine.Random.Range(m_minLevel, m_maxLevel));
                    // 如果这里增加对销毁的监听 为什么不需要考虑上面怪物数量的 -- ？
                    monsterComponent.destroyed.AddListener(() => m_spawnedMonsters.Remove(monsterComponent));
                    m_spawnedMonsters.Add(monsterComponent);
                }
                else
                {
                    Debug.LogError("No Monster component found on the monster prefab");
                }
            }
            else
            {
                Debug.LogError("Couldn't find a monster to spawn, please check your spawn rates and make sure their sum is 100");
            }
        }
    }
}
