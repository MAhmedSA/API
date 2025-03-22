using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LLMUnity;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class MoveObjectsByCommandExercise : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public InputField playerText;
    public RectTransform blueSquare;
    public RectTransform redSquare;
    public TextMeshProUGUI textResponse;
    bool isWaitingForResponse;

    void Start()
    {
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
    }

    string[] GetFunctionNames<T>()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            functionNames.Add(function.Name);
        return functionNames.ToArray();
    }

    string ConstructDirectionPrompt(string message)
    {
        string directionFunctions = string.Join(", ", GetFunctionNames<DirectionFunctions>());
        string playerFunctions = string.Join(", ", GetFunctionNames<PlayerControler>());

        return $@"Analyze this command: ""{message}""
        Extract the direction and action mentioned.
        
        Available directions: {directionFunctions}
        
        Available actions: Attack, Move
        
        Respond in this exact format:
        Direction: [DirectionFunction]
        Action: [ActionType]
        
        If no direction mentioned, use NoDirectionsMentioned
        If no action mentioned, use NoActionMentioned";
    }

    async void onInputFieldSubmit(string message)
    {
        /* Example prompts and test cases for students:
         * 
         * Test inputs:
         * - "move the blue square up"
         * - "move red square to the right"
         * - "make the blue square go down"
         * - "move the red square left"
         * 
         * Expected AI responses examples:
         * - Direction: "MoveUp", "MoveRight", "MoveDown", "MoveLeft", "NoDirectionsMentioned"
         * - Color: "BlueColor", "RedColor", "NoColorMentioned"
         */

        // 1. Disable the input field
        playerText.interactable = false;

        try
        {
            // 2. Get direction and color from AI
            string prompt = ConstructDirectionPrompt(message);
            string response = await llmCharacter.Chat(prompt);

            // Parse the AI response
            string[] lines = response.Split('\n');
            string directionMethod = lines[0].Replace("Direction: ", "").Trim();
            string actionType = lines[1].Replace("Action: ", "").Trim();

            if (PlayerControler.controler == null)
            {
                Debug.LogError("PlayerControler instance not found!");
                return;
            }

            // Handle different actions
            if (actionType.Equals("Attack", StringComparison.OrdinalIgnoreCase))
            {
                // Call Attack function
                GameObject bullet = PlayerControler.controler.Attack();
            }
            else if (actionType.Equals("Move", StringComparison.OrdinalIgnoreCase) && directionMethod != "NoDirectionsMentioned")
            {
                // Handle movement
                int direction = (int)typeof(DirectionFunctions).GetMethod(directionMethod).Invoke(null, null);
                PlayerControler.controler.MovePlayer(direction);
            }
            else 
            {
                await SendQuery();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing command: {e.Message}");
        }
        finally
        {
            // 5. Re-enable the input field
            playerText.interactable = true;
            playerText.text = "";
            playerText.Select();
        }
    }
    private async Task SendQuery()
    {
        if (isWaitingForResponse) return;

        string inputText = playerText.text;

        try
        {
            isWaitingForResponse = true;

            // Sanitize the input text to prevent JSON formatting issues
            inputText = inputText.Replace("\"", "'").Replace("\n", " ");
            
            // Create the JSON payload
            string jsonPayload = "{\"inputs\": \"" + inputText + "\"}";

            // Create and send the web request
            using UnityWebRequest request = UnityWebRequest.Post(
                "https://api-inference.huggingface.co/models/google/gemma-2-2b-it", 
                jsonPayload, 
                "application/json");
                
            request.SetRequestHeader("Authorization", "Bearer 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");
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            // Wait for the request to complete
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                textResponse.text = "Error: Failed to get response";
                return;
            }

            // Parse the response
            string jsonResponse = request.downloadHandler.text;
            
            // Remove the brackets from the response if they exist
            if (jsonResponse.StartsWith("[") && jsonResponse.EndsWith("]"))
            {
                jsonResponse = jsonResponse.Substring(1, jsonResponse.Length - 2);
            }

            try
            {
                JObject json = JObject.Parse(jsonResponse);
                string generatedText = json["generated_text"]?.ToString() ?? "No response generated";
                textResponse.text = generatedText;
            }
            catch (Exception parseEx)
            {
                Debug.LogError($"Error parsing JSON: {parseEx.Message}\nResponse: {jsonResponse}");
                textResponse.text = "Error: Could not parse response";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching response: {e.Message}");
            textResponse.text = $"Error: {e.Message}";
        }
        finally
        {
            isWaitingForResponse = false;
        }
    }
    private RectTransform GetObjectByColor(Color color)
    {
        if (color == Color.blue)
            return blueSquare;
        else if (color == Color.red)
            return redSquare;

        return null;
    }

    public void CancelRequests()
    {
        llmCharacter.CancelRequests();
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    bool onValidateWarning = true;
    void OnValidate()
    {
        if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
        {
            Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
            onValidateWarning = false;
        }
    }
}