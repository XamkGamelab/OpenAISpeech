using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// Control UI elements and animations for the "eye" and mic gain cauge.
/// </summary>
public class UiController : MonoBehaviour
{
    public Button ButtonRec;
    public Image ImageAIEye;
    public Image MicGainNeedle;

    public float MicXPositionMaxMoveDistance = 160f;
    public float MicNeedleLerpSpeed = 4f;
    public float EyeSinAnimSpeed = 2f;

    private Vector2 initMicGainNeedleAnchoredPos;
    
    private bool animatingEyeAlpha = false;

    #region Unity
    private void Awake()
    {
        initMicGainNeedleAnchoredPos = MicGainNeedle.rectTransform.anchoredPosition;
        
        ApplicationController.Instance.MicPeakVolume.Subscribe(vol => UpdateMicGainNeedlePosition(vol));
        ApplicationController.Instance.WaitingWebResponses.Subscribe(b => WaitingWebResponse(b));
        ApplicationController.Instance.CurrentAudioListenerSpectrumSample.Subscribe(f => HandleAudioSourceSpectrumChange(f));
        
    }
    #endregion

    #region private

    /// <summary>
    /// Animate eye alpha with current AudioSource output spectrum
    /// </summary>
    /// <param name="currentSpec">Current output spectrum</param>
    private void HandleAudioSourceSpectrumChange(float currentSpec)
    {
        if (currentSpec > 0f)
        {
            float alpha = Mathf.Clamp01(currentSpec * 2f + .5f);            
            ImageAIEye.color = new Color(ImageAIEye.color.r, ImageAIEye.color.g, ImageAIEye.color.b, alpha);
        }
        else //Reset alpha to 1 when AudioSource not playing
            ImageAIEye.color = new Color(ImageAIEye.color.r, ImageAIEye.color.g, ImageAIEye.color.b, 1f);
    }

    private void WaitingWebResponse(bool b)
    {
        ButtonRec.interactable = !b;

        if (b)
            StartCoroutine(AnimateEyeAlphaSin());
        else
            animatingEyeAlpha = false;
    }

    private void UpdateMicGainNeedlePosition(float vol)
    {
        Vector2 targetPos = initMicGainNeedleAnchoredPos + vol * new Vector2(MicXPositionMaxMoveDistance, 0);
        MicGainNeedle.rectTransform.anchoredPosition = Vector2.Lerp(MicGainNeedle.rectTransform.anchoredPosition, targetPos, Time.deltaTime * MicNeedleLerpSpeed);
    }

    private IEnumerator AnimateEyeAlphaSin()
    {
        animatingEyeAlpha = true;
        while (animatingEyeAlpha)
        {
            
            float alpha = (Mathf.Sin(Time.time * EyeSinAnimSpeed) + 1) * 0.5f;
            ImageAIEye.color = new Color(ImageAIEye.color.r, ImageAIEye.color.g, ImageAIEye.color.b, alpha);
            yield return null;
        }
    }

    #endregion
}
