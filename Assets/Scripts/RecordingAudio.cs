using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLMUnity;
using BattleGame.Scripts;

public class RecordingAudio : MonoBehaviour
{
    [SerializeField] private Button startRecording;
    [SerializeField] private Button stopRecording;
    [SerializeField] private PlayerCommands playerCommands;

    private AudioClip _clip;
    private bool _isRecording;
    private APIManager _apiManager;

    [HideInInspector] public byte[] bytes;
    [HideInInspector] public string RecordedText;


    //[HideInInspector] public string audioFilePath = Application.dataPath + "/Audio/RecordedAudio.wav";

    private void Awake()
    {
        _apiManager = GetComponent<APIManager>();
    }

    private void Start()
    {
        startRecording.onClick.AddListener(StartRecording);
        stopRecording.onClick.AddListener(StopRecording);

        stopRecording.interactable = false;
    }

    private void OnDestroy()
    {
        startRecording.onClick.RemoveListener(StartRecording);
        stopRecording.onClick.RemoveListener(StopRecording);
    }

    private void Update()
    {
        if (_isRecording && Microphone.GetPosition(null) >= _clip.samples)
        {
            StopRecording();
        }
    }

    #region Recording Audio
    public void StartRecording()
    {
        _clip = Microphone.Start(null, false, 10, 44100);
        _isRecording = true;
        stopRecording.interactable = true;
        startRecording.interactable = false;
    }

    public async void StopRecording()
    {
        // returns the samples with audio, avoiding empty samples
        if (_isRecording)
        {
            stopRecording.interactable = false;
            var position = Microphone.GetPosition(null);
            Microphone.End(null);

            if (position <= 0)
            {
                Debug.LogError("No audio recorded!");
            }

            // calculates the total number of audio samples recorded
            // _clip.channels is either 1 for mono or 2 for stereo
            var samples = new float[position * _clip.channels];

            // copies the recorded audio samples from clip into samples
            _clip.GetData(samples, 0);

            // Encode to WAV
            bytes = EncodeAsWAV(samples, _clip.frequency, _clip.channels);

            string speechToText_Response = (await _apiManager.SendAudioTo_SpeechToText(bytes)).Trim();
            playerCommands.ProcessCommand(speechToText_Response);
            Debug.Log(speechToText_Response);
            startRecording.interactable = true;
        }
    }
    #endregion

    #region Convert to WAV and saving file
    // converts the samples to WAV format and prepare them to be sent to the API or further use
    private byte[] EncodeAsWAV(float[] samples, int sampleRate, int channels)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                int sampleCount = samples.Length;
                int byteRate = sampleRate * channels * 2;
                int fileSize = 44 + (sampleCount * 2);

                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(fileSize - 8);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)(channels * 2));
                writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(sampleCount * 2);

                for (int i = 0; i < sampleCount; i++)
                {
                    short intSample = (short)(samples[i] * short.MaxValue);
                    writer.Write(intSample);
                }
            }
            return memoryStream.ToArray();
        }
    }

    private async Task SaveWavFile(string filePath, byte[] wavData)
    {
        await File.WriteAllBytesAsync(filePath, wavData);
        Debug.Log("WAV file saved to: " + filePath);
    }
    #endregion
}
