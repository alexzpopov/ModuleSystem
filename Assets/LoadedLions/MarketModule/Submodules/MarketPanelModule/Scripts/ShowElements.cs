using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowElements : MonoBehaviour
{
    [SerializeField]
    private GameObject[] infos;

    private Toggle _toggle;

    // Start is called before the first frame update
    void Start()
    {
    _toggle = GetComponent<Toggle>();
    _toggle.onValueChanged.AddListener(Show);
    }

    private void Show(bool state)
    {
        foreach (var item in infos)
        {
            item.SetActive(!state);
        }
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(Show);
    }
}
