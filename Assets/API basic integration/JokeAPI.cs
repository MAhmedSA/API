using System;
using System.Threading.Tasks; //used 3shan el async await
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking; //some features like UnityWebRequest
using Newtonsoft.Json.Linq; //import json.net parse JSON response using JArray and Jobj


/// <summary>
/// This class represents the structure of the JSON response we get from the Chuck Norris API.
/// Each property matches the JSON fields in the API response.
/// </summary>
[Serializable]
public class ChuckNorrisJoke   //class to store api response data
{
    public string icon_url;  // URL of the joke's icon
    public string id;        // Unique identifier for the joke
    public string url;       // Web URL where the joke can be found
    public string value;     // The actual joke text
}

/// <summary>
/// This class handles fetching random jokes from the Chuck Norris API and displaying them in the UI.
/// It demonstrates basic API integration in Unity using UnityWebRequest.
/// </summary>
public class JokeAPI : MonoBehaviour
{
    // UI elements that we'll connect in the Unity Inspector
    [SerializeField] private Text _jokeText;        // Text component to display the joke
    [SerializeField] private Button _fetchJokeButton; // Button to trigger new joke fetch

    // API endpoint we'll be calling to get random jokes
    //private const string API_URL = "https://api.chucknorris.io/jokes/random";
    private const string API_URL = "https://api-inference.huggingface.co/models/google/gemma-2-2b-it";
    private string API_TOKEN = "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";
    /// <summary>
    /// Called when the script instance is being loaded.
    /// Sets up the button click listener.
    /// </summary>
    private void Awake()
    {
        _fetchJokeButton.onClick.AddListener(OnButtonClick_FetchNewJoke);
    }

    /// <summary>
    /// Called when the script instance is being destroyed.
    /// Cleans up the button click listener to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        _fetchJokeButton.onClick.RemoveListener(OnButtonClick_FetchNewJoke);
    }

    /// <summary>
    /// Asynchronously fetches a random joke from the API.
    /// Shows how to:
    /// 1. Make an HTTP GET request
    /// 2. Handle the response
    /// 3. Parse JSON data
    /// 4. Update UI elements
    /// 5. Handle errors
    /// </summary>




    
    public async Task GenerateText()
    {

        await SendTextGenerationRequest("how to make a pasta");

    }


    private async Task SendTextGenerationRequest(string prompt)
    {


        try
        {
            // Create the JSON request body
            string requestBody = "{\"inputs\": \"" + prompt + "\"}";

            using UnityWebRequest request = new UnityWebRequest(API_URL, "POST");
            // convert  the json string to raw byte data for transmission
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            //send json data
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            //recieve response
            request.downloadHandler = new DownloadHandlerBuffer();

            //headers needed  
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + API_TOKEN);

            // Send request
            var operation = request.SendWebRequest();

            // Await request completion
            while (!operation.isDone)
                await Task.Yield();

            // Handle response
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                _jokeText.text = "Error fetching response!";
                return;
            }

            // Parse JSON response
            //convert the response to string
            string jsonResponse = request.downloadHandler.text;
            JArray responseArray = JArray.Parse(jsonResponse);
            string generatedText = responseArray[0]["generated_text"].ToString();

            // Display the generated text in UI
            _jokeText.text = generatedText;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching text: {e.Message}");
            _jokeText.text = "Failed to fetch response";
        }





    }

    public void OnButtonClick_FetchNewJoke()
    {
        _ = SendTextGenerationRequest("tell me how to make pasta");
    }
}
