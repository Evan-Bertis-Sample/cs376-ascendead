using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.Audio
{
    [CreateAssetMenu(fileName = "AudioOverride", menuName = "Curly/Audio/Audio Override"), System.Serializable]
    public class AudioOverride : ScriptableObject, IAudioOverride
    {
        [field: SerializeField] public bool IdentifyByFileName {get; private set;} // If this flag is true, then we consider each sound file in the directory as a different sfx -- they are identified by name, not directory

        public void ApplyOverride(AudioSource source)
        {
            // throw new System.NotImplementedException();
        }
    }
}