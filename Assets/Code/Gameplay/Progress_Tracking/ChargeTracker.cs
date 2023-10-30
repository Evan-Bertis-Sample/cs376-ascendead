using System.Collections;
using System.Collections.Generic;
using CurlyCore;
using UnityEngine;

namespace Ascendead.Tracking
{
    public class ChargeTracker : MonoBehaviour
    {
        [System.Serializable]
        public class ChargeData
        {
            public int MaxCharges = 3;
        }

        public static ChargeTracker Instance { get; private set; }
        [field: SerializeField] private ChargeData _chargeData = new ChargeData();

        private const string CHARGE_DATA_KEY = "ChargeData";
        [GlobalDefault] private ProgressTracker _progressTracker;

        public delegate void ChargeEventHandler(int chargeLevel);
        public static event ChargeEventHandler OnMaxLevelChange;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DependencyInjector.InjectDependencies(this);
            _chargeData = _progressTracker.GetProgress<ChargeData>(CHARGE_DATA_KEY, null);

            if (_chargeData == null)
            {
                _chargeData = new ChargeData();
                Debug.Log("Failed to load charge data from disc.");
            }
        }

        public static int GetMaxCharges()
        {
            return Instance._chargeData.MaxCharges;
        }

        public static void SetMaxCharges(int newMax)
        {
            Instance._chargeData.MaxCharges = newMax;
            OnMaxLevelChange?.Invoke(Instance._chargeData.MaxCharges);
            SaveChargeData();
        }

        private static void SaveChargeData()
        {
            Instance._progressTracker.SetProgress(CHARGE_DATA_KEY, Instance._chargeData);
        }
    }
}
