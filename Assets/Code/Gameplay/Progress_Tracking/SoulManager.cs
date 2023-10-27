using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascendead.Components;
using CurlyCore;
using Newtonsoft.Json;

namespace Ascendead.Tracking
{
    public class SoulManager : MonoBehaviour
    {
        [System.Serializable]
        public class MetaSoulData
        {
            public int SoulsCollected = 0;
            public int SoulsSpent = 0;
            public List<int> CollectedSouls = new List<int>();

            public MetaSoulData() { }

            [JsonConstructor]
            public MetaSoulData(int soulsCollected, int soulsSpent, List<int> collectedSouls)
            {
                this.SoulsCollected = soulsCollected;
                this.SoulsSpent = soulsSpent;
                this.CollectedSouls = collectedSouls;
            }
        }

        public static SoulManager Instance { get; private set; }
        [field: SerializeField] private MetaSoulData _soulData = new MetaSoulData();

        private const string SOUL_DATA_KEY = "SoulData";

        [GlobalDefault] private ProgressTracker _progressTracker;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DependencyInjector.InjectDependencies(this);
            _soulData = _progressTracker.GetProgress<MetaSoulData>(SOUL_DATA_KEY, null);

            if (_soulData == null)
            {
                _soulData = new MetaSoulData();
                Debug.Log("Failed to load soul data from disc.");
            }
        }

        public static bool HasBeenCollected(Soul soul)
        {
            return Instance._soulData.CollectedSouls.Contains(soul.SoulID);
        }

        public static void CollectSoul(Soul soul)
        {
            Instance._soulData.SoulsCollected = Instance._soulData.SoulsCollected + soul.SoulValue;
            Instance._soulData.CollectedSouls.Add(soul.SoulID);
            SaveSoulData();
        }

        public static int GetSoulCount()
        {
            return Instance._soulData.SoulsCollected - Instance._soulData.SoulsSpent;
        }

        public static bool SpendSouls(int amount)
        {
            if (GetSoulCount() < amount) return false;

            Instance._soulData.SoulsSpent = Instance._soulData.SoulsSpent + amount;
            SaveSoulData();
            return true;
        }

        private static void SaveSoulData()
        {
            Instance._progressTracker.SetProgress(SOUL_DATA_KEY, Instance._soulData);
        }
    }
}
