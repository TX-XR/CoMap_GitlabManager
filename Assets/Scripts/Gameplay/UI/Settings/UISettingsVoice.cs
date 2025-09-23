using TMPro;
using UnityEngine;

namespace MappingAI
{
    public class UISettingsVoice : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown m_DropdownMicrophone;
        [SerializeField] private TMP_Dropdown m_DropdownReceiver;

        private void Start()
        {
            m_DropdownMicrophone.onValueChanged.AddListener(OnMicrophoneChanged);
            m_DropdownReceiver.onValueChanged.AddListener(OnReceiverChanged);
        }

        private void OnMicrophoneChanged(int index)
        {
            // todo: handle microphone off/on
            HelloService.RTCAudioProxy.Instance.EnableMicrophone(index == 1);
        }
        
        private void OnReceiverChanged(int index)
        {
            // todo: handle receiver off/on
            HelloService.RTCAudioProxy.Instance.EnableRemoteAudio(index == 1);
        }
    }
}
