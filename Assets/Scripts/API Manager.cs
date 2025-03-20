using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;

public class APIManager : MonoBehaviour
{
    public async Task<string> SendAudioTo_SpeechToText(byte[] bytes)
    {
        try
        {
            //byte[] bytesBuffer = await File.ReadAllBytesAsync(_recordingAudioScript.audioFilePath);

            using UnityWebRequest request = new UnityWebRequest(Constants.STT_API_URL, "POST");
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {Constants.TOKEN}");
            request.SetRequestHeader("Content-Type", "application/octet-stream");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error" + request.error);
                return null;
            }

            string jsonResponse = request.downloadHandler.text;
            HuggingFaceAPI huggingFaceResponse = JsonConvert.DeserializeObject<HuggingFaceAPI>(jsonResponse);

            return huggingFaceResponse.text;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching text: {e.Message}");
            return null;
        }
    }


    public async Task<string> SendAudioTo_TextGen(string inputText)
    {
        try
        {
            string jsonPayload = $"{{\"inputs\": \"{inputText}\"}}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

            using UnityWebRequest request = new UnityWebRequest(Constants.TEXTGEN_API_URL, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {Constants.TOKEN}");
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error" + request.error);
                return null;
            }

            string jsonResponse = request.downloadHandler.text;
            List<TextGenResponse> responses = JsonConvert.DeserializeObject<List<TextGenResponse>>(jsonResponse);

            return responses[0].generated_text;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching text: {e.Message}");
            return null;
        }
    }

}
