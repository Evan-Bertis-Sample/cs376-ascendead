using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ascendead.Dialogue
{
    public class DialogueTree
    {
        public DialogueNode Root { get; private set; }

        public DialogueTree(string textFileName)
        {
            BuildDialogueTree(textFileName);
        }

        private void BuildDialogueTree(string textFileName)
        {
            Debug.Log("Attempting to load dialogue tree from " + textFileName + ".");
            TextAsset textAsset = Resources.Load<TextAsset>(textFileName);
            if (textAsset == null)
            {
                Debug.LogError("Dialogue tree file not found at " + textFileName + ".");
                return;
            }

            string instructions = textAsset.text;

            string[] lines = instructions.Split('\n');

            Dictionary<string, DialogueNode> namedBranches = new Dictionary<string, DialogueNode>();
            Stack<DialogueNode> nodeStack = new Stack<DialogueNode>();
            DialogueNode currentNode = null;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed[0] == '#') continue;

                Debug.Log("Parsing line: " + trimmed);

                if (IsMeta(trimmed)) // [] brackets
                {
                    DialogueNode newNode = HandleMetaToken(trimmed);
                    if (currentNode != null)
                    {
                        currentNode.Children.Add(newNode);
                    }
                    else
                    {
                        // This must be the root node
                        Root = newNode;
                    }

                    // If it's a branch or an option, update the currentNode
                    if (newNode.IsMeta && (newNode.Content == "Branch" || newNode.Content == "Option"))
                    {
                        // If the branch is named, add it to the dictionary
                        if (newNode.Parameters.Count > 0 && newNode.Parameters[0] is string branchName)
                        {
                            namedBranches[branchName] = newNode;
                        }

                        if (currentNode != null)
                        {
                            currentNode.Children.Add(newNode);
                        }
                        else
                        {
                            Root = newNode;
                        }

                        nodeStack.Push(currentNode);
                        currentNode = newNode;
                    }
                    else if (newNode.IsMeta && newNode.Content == "Exit")
                    {
                        if (newNode.Parameters.Count > 0 && newNode.Parameters[0] is string && namedBranches.ContainsKey((string)newNode.Parameters[0]))
                        {
                            currentNode = namedBranches[(string)newNode.Parameters[0]];
                            continue;
                        }

                        currentNode = nodeStack.Count > 0 ? nodeStack.Pop() : null;
                        if (currentNode == null)
                        {
                            Debug.LogError("Dialogue tree has an exit node without a branch or option node, quitting.");
                            return;
                        }

                    }
                }
                else // Dialogue
                {
                    // This is a dialogue node
                    DialogueNode dialogueNode = new DialogueNode(trimmed, new List<DialogueNode>(), currentNode);
                    if (currentNode != null)
                    {
                        currentNode.Children.Add(dialogueNode);
                    }
                    else
                    {
                        // this must be our root node
                        Root = dialogueNode;
                        currentNode = dialogueNode;
                    }
                }
            }
        }

        private bool IsMeta(string line)
        {
            return line[0] == '[' && line[line.Length - 1] == ']';
        }

        private DialogueNode HandleMetaToken(string token)
        {
            // tokens should look like this [command(parameters)] or [command]
            // remove this
            // extract the command
            string command = token.Substring(1, token.IndexOf(']') - 1);

            // extract the parameters

            if (command.Contains("(") && !command.Contains(")"))
            {
                Debug.LogError("Dialogue tree has a meta command with mismatched parentheses, quitting.");
                return null;
            }

            if (!command.Contains("(") && command.Contains(")"))
            {
                Debug.LogError("Dialogue tree has a meta command with mismatched parentheses, quitting.");
                return null;
            }

            if (!command.Contains("(") && !command.Contains(")"))
            {
                return new DialogueNode(command, new List<DialogueNode>(), null, true, new List<object>());
            }

            string paramsAll = token.Substring(token.IndexOf('(') + 1, token.IndexOf(')') - token.IndexOf('(') - 1);
            List<string> paremeters = new List<string>(paramsAll.Split(','));

            // trim the parameters
            for (int i = 0; i < paremeters.Count; i++)
            {
                paremeters[i] = paremeters[i].Trim();
            }

            List<object> parameterObject = new List<object>();
            foreach (string parameter in paremeters)
            {
                parameterObject.Add(ParseParameter(parameter));
            }

            // now build the node
            DialogueNode node = new DialogueNode(command, new List<DialogueNode>(), null, true, parameterObject); // TODO: Implement parenting stuff
            return node;
        }

        private object ParseParameter(string parameter)
        {
            // if it's a number
            if (parameter[0] >= '0' && parameter[0] <= '9')
            {
                // if it's an int
                if (parameter.Contains('.'))
                {
                    return float.Parse(parameter);
                }
                else
                {
                    return int.Parse(parameter);
                }
            }
            else if (parameter[0] == '"') // if it's a string
            {
                return parameter.Substring(1, parameter.Length - 2);
            }
            else if (parameter[0] == '[') // if it's a list
            {
                // remove the brackets
                parameter = parameter.Substring(1, parameter.Length - 2);

                // split the list
                string[] list = parameter.Split(',');

                // trim the list
                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = list[i].Trim();
                }

                return list;
            }
            else // if it's a bool
            {
                return bool.Parse(parameter);
            }
        }
    }
}
