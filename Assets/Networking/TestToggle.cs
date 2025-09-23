using MappingAI;
using UnityEngine;
using UnityEngine.UI;

public class TestToggle : MonoBehaviour
{
    public ApplicationSettings applicationSettings;
    Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        applicationSettings = ComponentManager.Instance.ApplicationSettings_Get();
        toggle = GetComponent<Toggle>();
        toggle.isOn = applicationSettings.IsTest;

    }

    public void OnValueChanged()
    {
        applicationSettings.IsTest = !applicationSettings.IsTest;
        toggle.isOn = applicationSettings.IsTest;
    }
}
