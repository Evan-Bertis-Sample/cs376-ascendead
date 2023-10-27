using System;
using System.Collections;
using System.Collections.Generic;
using CurlyCore;
using CurlyCore.CurlyApp;
using CurlyCore.Saving;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ascendead.Tracking
{
    // Easy access to save data, without having to worry about the save manager
    [CreateAssetMenu(fileName = "ProgressTracker", menuName = "Ascendead/Progress Tracker", order = 0)]
    public class ProgressTracker : RuntimeScriptableObject
    {        
        [field: SerializeField] public bool LoadProgressOnBoot { get; private set; } = true;

        [GlobalDefault] private SaveManager _saveManager;
        private const string SAVE_FILE_NAME = "progress";
        private SaveData _saveData;

        public override void OnBoot(App app, UnityEngine.SceneManagement.Scene scene)
        {
            DependencyInjector.InjectDependencies(this);
            LoadProgress();
        }

        public override void OnQuit(App app, Scene scene)
        {
            SaveProgress();
        }

        public void SetProgress<T>(string fact, T value)
        {
            _saveData.Save(fact, value);
        }

        public T GetProgress<T>(string fact, T fallback)
        {
            return _saveData.Load(fact, fallback);
        }

        private void LoadProgress()
        {
#if UNITY_EDITOR
            if (!LoadProgressOnBoot) 
            {
                _saveData = new SaveData();
                Debug.Log("ProgressTracker : Not loading progress on boot.");
                return; // Don't load progress in editor
            }
#endif

            IDataStorage storage = _saveManager.Storage;
            // We have savedata, load it
            if (storage.GetAllKeys().Contains(SAVE_FILE_NAME))
            {
                Debug.Log("Found old progress; Loading...");
                _saveData = _saveManager.LoadUsingDefault(SAVE_FILE_NAME);
            }
            else
            {
                // nuh uh, we don't have savedata, create it
                Debug.Log("Couldn't find old progress; Creating new save data...");
                _saveData = new SaveData();
            }
        }

        private void SaveProgress()
        {

            Debug.Log("ProgressTracker : Saving progress...");

            _saveManager.SaveUsingDefault(_saveData, SAVE_FILE_NAME);
        }
    }
}
