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
        public abstract Task BeginDialogue();
        public abstract Task<int> DisplayNode(DialogueNode node, string characterName); // Must return an int
        public abstract Task EndDialogue();
    }
}
