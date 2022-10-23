using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public Toggle toggle = null;

    [SerializeField]
    private StartButton Startbutton;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(Function_Toggle);
    }

    private void Function_Toggle(bool _bool)
    {
        Startbutton.isMulti = _bool;
    }
}
