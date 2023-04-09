using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// Easier way to handle button pointer ups and downs as a ReactiveProperty.
/// </summary>
public class UiButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType { Rec, LangFI, LangEN}

    public Button ButtonComponent => GetComponent<Button>();

    public static ReactiveProperty<UiButton> ButtonDownUp = new ReactiveProperty<UiButton>();

    public ButtonType Type;
    public bool PointerDown { get; set; }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        PointerDown = true;
        ButtonDownUp.SetValueAndForceNotify(this);
        
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        PointerDown = false;
        ButtonDownUp.SetValueAndForceNotify(this);
    }
}
