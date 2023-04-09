using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using OpenAI.Audio;
using UniRx;

/// <summary>
/// OpenAI API auth and TranscribeAudio
/// </summary>
public class OpenAICalls
{
    public ReactiveProperty<AudioClip> SpeechResponseAudioClip = new ReactiveProperty<AudioClip>(null);
    
    private OpenAIClient openAIClient = new OpenAIClient(APIKeys.OpenAI_APIKey);

    #region public
    /// <summary>
    /// Transcribes AudioClip, requests OpenAI completion for transcription and requests VoiceRSS text-to-speech audio. 
    /// Response as ReactiveProperty.
    /// </summary>
    /// <param name="clip">AudioClip to transcribe</param>
    public async void TranscribeAudioClip(AudioClip clip)
    {
        var request = new AudioTranscriptionRequest(clip, language: "en");
        string resultTranscription = await openAIClient.AudioEndpoint.CreateTranscriptionAsync(request);
        Debug.Log(resultTranscription);

        var chatPrompts = new List<ChatPrompt> {  new ChatPrompt("user", resultTranscription) };

        var chatRequest = new ChatRequest(chatPrompts, Model.GPT3_5_Turbo);

        var result = await openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
        Debug.Log(result.FirstChoice);

        TextToSpeech.GetTextToSpeechAudioClip(result.FirstChoice).Subscribe(speechAudioClip => SpeechResponseAudioClip.Value = speechAudioClip);
    }

    #endregion
}
