using System;
using System.IO;
using System.Threading.Tasks;
using HuggingFace.API;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows;

public class SpeechRecognitionTest : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI responseText;

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;
    private string errorColorHex;

    private bool isWaitingForResponse;

    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = false;
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        text.color = Color.white;
        text.text = "Recording...";
        startButton.interactable = false;
        stopButton.interactable = true;
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
    }

    private void StopRecording()
    {
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();
    }

    private void SendRecording()
    {
        text.color = Color.yellow;
        text.text = "Sending...";
        stopButton.interactable = false;
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
            text.color = Color.white;
            text.text = response;
            _ = SendQuery();
            
        }, error => {
            text.color = Color.red;
            text.text = error;
            startButton.interactable = true;
        });
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    private async Task SendQuery()
    {
        startButton.interactable = true;
        if (isWaitingForResponse) return;

        string inputText = text.text;

        try
        {
            isWaitingForResponse = true;
            startButton.interactable = false;

            // Create and send the web request
            using UnityWebRequest request = UnityWebRequest.Post("https://api-inference.huggingface.co/models/google/gemma-2-2b-it",$"{{\"inputs\": \"{inputText}\"}}" , "application/json");
            request.SetRequestHeader("Authorization", "Bearer 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");
            var operation = request.SendWebRequest();

            // Wait for the request to complete
            while (!operation.isDone)
                await Task.Yield();

            // Check if the request was successful
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                return;
            }

            // Parse the JSON response into our ChuckNorrisJoke class
            string jsonResponse = request.downloadHandler.text;
            JObject json = JObject.Parse(jsonResponse.Substring(1, jsonResponse.Length - 2));

            // Display the joke text in the UI
            responseText.text = json["generated_text"].ToString();
        }
        catch (Exception e)
        {
            // Handle any errors that occurred during the process
            Debug.LogError($"Error fetching response: {e.Message}");
            responseText.text = $"\n<color=#{errorColorHex}>Error: {e.Message}</color>\n\n";
        }
        finally
        {
            startButton.interactable = true;
            isWaitingForResponse = false;
        }

    }
}