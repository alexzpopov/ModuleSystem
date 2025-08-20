using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputTextReplicator : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputText;
    [SerializeField] private TMP_Text thisText;
    [SerializeField] private string spriteText="<sprite=1>";
    private void Awake()
    {
        if (thisText == null)
        {
            thisText = GetComponent<TMP_Text>();
        }

        inputText.onValueChanged.AddListener(UpdateText);
    }

    private void UpdateText(string arg0)
    {
        thisText.text =spriteText+arg0;
    }
}
