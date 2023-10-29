using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendead.Dialogue
{
    public class DialogueNode
    {
        public string Content { get; private set; }
        public List<object> Parameters {get; private set;}
        public List<DialogueNode> Children { get; private set; }
        public DialogueNode Parent {get; private set;}
        public bool IsMeta { get; private set; }

        public DialogueNode(string content, List<DialogueNode> children, DialogueNode parent = null, bool isEvent = false, List<object> parameters = null)
        {
            Content = content; // if this is a meta node, this is the command
            Children = children;
            Parent = parent;
            IsMeta = isEvent;
            Parameters = parameters;
        }
    }
}
