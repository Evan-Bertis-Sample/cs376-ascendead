using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascendead.Tracking;
using CurlyCore.Audio;
using CurlyCore;

namespace Ascendead.Components
{
    public class Soul : MonoBehaviour
    {
        [field: SerializeField] public int SoulID { get; private set; } = 0;
        [field: SerializeField] public int SoulValue { get; private set; } = 1;
        [field: SerializeField, AudioPath] public string CollectionSound {get; private set;}

        [GlobalDefault] private AudioManager _audioManager;

        private void OnEnable()
        {
            SoulID = GetInstanceID();
            if (_audioManager == null) DependencyInjector.InjectDependencies(this);
        }

        private void Start()
        {
            if (SoulManager.HasBeenCollected(this)) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                SoulManager.CollectSoul(this);
                Destroy(gameObject);
                _audioManager.PlayOneShot(CollectionSound, transform.position);
            }
        }
    }
}
