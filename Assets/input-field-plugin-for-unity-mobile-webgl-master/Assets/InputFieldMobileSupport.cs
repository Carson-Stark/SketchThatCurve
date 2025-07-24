using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputFieldMobileSupport : MonoBehaviour, IPointerClickHandler
{
    [DllImport("__Internal")]
    private static extern void Prompt(string name, string message, string defaultValue);

    public string message;

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Prompt(name, message, GetComponent<InputField>().text);
#endif
    }

    public void OnPromptOk(string message)
    {
        GetComponent<InputField>().text = message;
    }

    public void OnPromptCancel()
    {
        GetComponent<InputField>().text = "";
    }
}
