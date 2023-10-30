using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ascendead.Dialogue
{
    public interface IDialogueFrontend
    {
        Task BeginDialogue();
        Task<int> DisplayNode(DialogueNode node, string characterName); // Updated to return an int
        Task EndDialogue();
    }

    public abstract class DialogueFrontendObject : ScriptableObject, IDialogueFrontend
    {
        public virtual async Task BeginDialogue() {}
        public virtual async Task<int> DisplayNode(DialogueNode node, string characterName) {return -1; } // Must return an int
        public virtual async Task EndDialogue() {}
    }
}
