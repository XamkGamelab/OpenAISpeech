using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Instantiate and initialize necessary objects.
/// </summary>
public class ApplicationController : SingletonMono<ApplicationController>
{
    public ReactiveProperty<float> MicPeakVolume = new ReactiveProperty<float>(0);
    public ReactiveProperty<bool> WaitingWebResponses = new ReactiveProperty<bool>(false);
    public ReactiveProperty<float> CurrentAudioListenerSpectrumSample = new ReactiveProperty<float>(0f);

    public AudioSource audioSource;

    private AudioMixerGroup audioMixer;
    private AudioClip audioClip;
    private OpenAICalls openAICalls;
    
    private const int micWaveSamples = 128;
    
    private float[] micWaveData = new float[micWaveSamples];
    private float[] spectrum = new float[64];
    
    [RuntimeInitializeOnLoadMethod]
    static void OnInit()
    {
        Instance.Init();
    }

    #region public
    public void Init()
    {
        audioMixer = Resources.Load<AudioMixerGroup>("AudioMixerMaster");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = audioMixer;
        audioSource.clip = audioClip;

        openAICalls = new OpenAICalls();
        openAICalls.SpeechResponseAudioClip.Subscribe(clip => HandleSpeechResponse(clip));

        UiButton.ButtonDownUp.Subscribe(button => HandleUiButtonUpDownStates(button));
    }
    #endregion

    #region private

    private void HandleSpeechResponse(AudioClip speechAudioClip)
    {
        if (speechAudioClip != null)
        {
            audioSource.clip = speechAudioClip;
            audioSource.Play();
            StartCoroutine(GetAudioSpectrum());
            WaitingWebResponses.Value = false;
        }
    }

    private void HandleUiButtonUpDownStates(UiButton button)
    {
        if (button?.ButtonComponent != null && !button.ButtonComponent.interactable)
            return;

        if (button?.Type == UiButton.ButtonType.Rec)
        {
            if (button.PointerDown)
            {
                Debug.Log("*** Start recording ***");
                audioSource.Stop();
                audioSource.clip = Microphone.Start(null, true, 20, 44100);
            }
            else
            {
                Debug.Log("*** Stop recording ***");
                Microphone.End(null);
                audioSource.Play();
                WaitingWebResponses.Value = true;
                
                openAICalls.TranscribeAudioClip(audioSource.clip);
            }
        }
    }

    /// <summary>
    /// Microphone avg. input volume from set of samples. Tweaked from:
    /// https://forum.unity.com/threads/check-current-microphone-input-volume.133501/
    /// </summary>
    /// <returns></returns>
    private float GetMicWavePeak()
    {
        int micPosition = Microphone.GetPosition(null) - (micWaveSamples + 1); // null means the first microphone
        audioSource.clip.GetData(micWaveData, micPosition);

        //Getting a peak on the last 128 samples
        float levelMax = 0;
        for (int i = 0; i < micWaveSamples; i++)
        {
            float wavePeak = micWaveData[i] * micWaveData[i];
            if (levelMax < wavePeak)
                levelMax = wavePeak;
        }
        //LevelMax equals to the highest normalized value power 2, a small number because < 1
        //use it like:        
        return Mathf.Sqrt(levelMax);
    }

    private IEnumerator GetAudioSpectrum()
    {
        while (audioSource.isPlaying)
        {
            //Get spectrum samples (arbitrary, but min 64) to spectrum array
            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            //Lerp only 20% towards current to dampen the effect
            CurrentAudioListenerSpectrumSample.Value = Mathf.Lerp(CurrentAudioListenerSpectrumSample.Value, spectrum[0], .2f); //<-- dampen towards present
            
            yield return null;
        }
        //Reset value to 0 when audio stops playing
        CurrentAudioListenerSpectrumSample.Value = 0;
    }
    #endregion

    #region Unity
    private void Update()
    {
        //Update mic peak reactive val if recording
        if (Microphone.IsRecording(null) && Microphone.GetPosition(null) > 0)
            MicPeakVolume.Value = GetMicWavePeak();
    }

    #endregion
}