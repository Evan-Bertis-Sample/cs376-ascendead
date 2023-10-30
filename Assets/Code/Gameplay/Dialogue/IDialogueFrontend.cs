using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ascendead.Dialogue
{
    public interface IDialogueFrontend
    {
        Task BeginDialogue(string characterName);
        Task<int> DisplayNode(DialogueNode node); // Updated to return an int
        Task EndDialogue();
    }

    public abstract class DialogueFrontendObject : ScriptableObject, IDialogueFrontend
    {
        public virtual async Task BeginDialogue(string characterName) {}
        public virtual async Task<int> DisplayNode(DialogueNode node) {return -1; } // Must return an int
        public virtual async Task EndDialogue() {}
    }
}
