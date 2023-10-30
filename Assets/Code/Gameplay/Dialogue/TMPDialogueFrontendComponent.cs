using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ascendead.Dialogue
{
    public class TMPDialogueFrontendComponent : MonoBehaviour
    {
        [field: SerializeField] public TextMeshProUGUI DialogueText {get; private set;}
        [field: SerializeField] public TextMeshProUGUI CharacterNameText {get; private set;}
        [field: SerializeField] public VerticalLayoutGroup ChoicesLayoutGroup {get; private set;}

    }
}
