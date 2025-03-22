using System.Collections.Generic;
using System.Reflection;
using BattleGame.Scripts;
using LLMUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCommands : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public TextMeshProUGUI aiText;

    public Transform slimeEnemy;
    public Transform batEnemy;
    public Transform Player;

    string[] GetFunctionNames<T>()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            functionNames.Add(function.Name);
        return functionNames.ToArray();
    }

    string ConstructCommandPrompt(string message)
    {
        string prompt = "From the input, determine the intended action and return it in the correct format:\n\n";

        prompt += "### Input:\n";
        prompt += message + "\n\n";

        prompt += "### Choices:\n";
        prompt += "- Fire\n- Defend\n\n";

        prompt += "### Targets:\n";
        prompt += "- slime\n- dog\n\n";

        prompt += "### Response Formats:\n";
        prompt += "1. **For attacking a target:** Return in this format → `choice target`\n";
        prompt += "   Example: `Fire slime`\n\n";

        prompt += "2. **For defending (shielding):** Return just the action → `Defend`\n";
        prompt += "   Example: `Defend`\n\n";

        prompt += "3. **For other cases:** Return a response in this format from your personality→ `Response: <message>`\n";
        prompt += "   Example: `Response: I don't see any threats.`\n\n";

        prompt += "**Do NOT return anything outside these formats. Stick to the exact format.**";

        return prompt;
    }

    public async void ProcessCommand(string message)
    {
        try
        {
            string getCommand = await llmCharacter.Chat(ConstructCommandPrompt(message));
            Debug.Log(getCommand);

            if (string.IsNullOrWhiteSpace(getCommand))
            {
                Debug.LogError("Received empty response from Chat()");
                aiText.text = "I didn't understand the command. Please try again.";
                return;
            }

            // Handle different response formats
            if (getCommand.StartsWith("Response:"))
            {
                // AI-generated response, not a command
                aiText.text = getCommand.Substring(9).Trim(); // Remove "Response:" prefix
                return;
            }

            string[] strings = getCommand.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            if (strings.Length == 1 && strings[0] == "Defend")
            {
                // Handle defense action
                PlayerController player = Player.GetComponent<PlayerController>();
                if (player != null)
                {
                    if (player.IsShieldAvailable())
                    {
                        player.ActivateShield();
                        aiText.text = "Shield activated!";
                    }
                    else if (player.IsShieldActive())
                    {
                        aiText.text = "Shield is already active!";
                    }
                    else
                    {
                        aiText.text = "Shield is on cooldown!";
                    }
                }
                return;
            }

            if (strings.Length == 2)
            {
                // Handle attack action: "Fire slime"
                string action = strings[0];
                string target = strings[1];

                PlayerController player = Player.GetComponent<PlayerController>();

                if (player != null)
                {
                    switch (target.ToLower())
                    {
                        case "slime":
                            player.TargetEnemy("slime");
                            break;
                        case "dog":
                            player.TargetEnemy("dog");
                            break;
                        default:
                            Debug.LogError($"Unknown target: {target}");
                            aiText.text = "Unknown target. Try again.";
                            return;
                    }

                    switch (action)
                    {
                        case "Fire":
                            player.Fire();
                            aiText.text = $"Firing at {target}!";
                            return;

                        default:
                            Debug.LogError($"Unknown action: {action}");
                            aiText.text = "Unknown action. Try again.";
                            return;
                    }
                }
                else
                {
                    Debug.LogError("PlayerController not found on Player object!");
                    aiText.text = "Player not found.";
                }
            }

            Debug.LogError("Invalid command format received!");
            aiText.text = "Invalid command format. Please try again.";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing command: {e.Message}");
            aiText.text = "I couldn't process that command. Please try again.";
        }
    }

}
