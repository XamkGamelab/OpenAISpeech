using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Threading;
using UniRx;

/// <summary>
/// WebRequest as observable to VoiceRSS text-to-speech API
/// See: https://rapidapi.com/voicerss/api/text-to-speech-1 and https://www.voicerss.org/
/// </summary>
public static class TextToSpeech
{
	public static IObservable<AudioClip> GetTextToSpeechAudioClip(string text)
	{
		// convert coroutine to IObservable
		return Observable.FromCoroutine<AudioClip>((observer, cancellationToken) => GetTextToSpeechAudioClipWWW(text, observer, cancellationToken));
	}

	static IEnumerator GetTextToSpeechAudioClipWWW(string text, IObserver<AudioClip> observer, CancellationToken cancellationToken)
	{
		WWWForm form = new WWWForm();
		form.AddField("src", text, System.Text.Encoding.UTF8);
		form.AddField("hl", "en-us", System.Text.Encoding.UTF8); //<-- english: en-us
		form.AddField("r", "0", System.Text.Encoding.UTF8);
		form.AddField("c", "mp3", System.Text.Encoding.UTF8);
		form.AddField("f", "24khz_16bit_stereo", System.Text.Encoding.UTF8);

		DownloadHandlerAudioClip handlerAudioClip = new DownloadHandlerAudioClip("https://voicerss-text-to-speech.p.rapidapi.com/?key=" + APIKeys.VoiceRSS_APIKey, AudioType.MPEG);

		UnityWebRequest www = UnityWebRequest.Post("https://voicerss-text-to-speech.p.rapidapi.com/?key=" + APIKeys.VoiceRSS_APIKey, form);
		www.downloadHandler = handlerAudioClip;
		www.method = "POST";
		www.SetRequestHeader("X-RapidAPI-Key", APIKeys.XRapid_APIKey);
		www.SetRequestHeader("X-RapidAPI-Host", "voicerss-text-to-speech.p.rapidapi.com");

		using (www)
		{
			yield return www.SendWebRequest();

			if (cancellationToken.IsCancellationRequested) yield break;

			if (www.error != null || www.result == UnityWebRequest.Result.ConnectionError)
			{
				observer.OnError(new Exception("Connection error."));
			}
			else
			{
				Debug.Log("www response code: " + www.responseCode + " | bytes downloaded: " + www.downloadedBytes);
				AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
				observer.OnNext(clip);
				observer.OnCompleted();
			}
		}
	}
}
