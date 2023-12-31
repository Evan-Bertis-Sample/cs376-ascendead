using System.Threading;
using System.Threading.Tasks;
using CurlyCore.Input;
using TMPro;
using UnityEngine;

namespace Ascendead.Dialogue
{
    public static class Typewriter
    {
        public static async Task ApplyTo(TextMeshProUGUI textMesh, string text, float charactersPerSecond, CancellationToken token = default, InputManager inputManager = null, string inputPrompt = null)
        {
            textMesh.text = "";
            var characterDelay = 1f / charactersPerSecond;

            for (int i = 0; i < text.Length; i++)
            {
                if (token.IsCancellationRequested || (inputManager != null && inputManager.GetInputDown(inputPrompt)))
                {
                    textMesh.text = text; // Show full text if cancelled or input is detected
                    return;
                }

                textMesh.text += text[i];
                await Task.Delay((int)(characterDelay * 1000), token);
            }
        }
    }
}
