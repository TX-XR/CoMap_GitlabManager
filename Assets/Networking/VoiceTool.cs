using System.Collections;
using System.Collections.Generic;
using Unity.UOS.Hello.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace MappingAI
{


    public class VoiceTool : MonoBehaviour
    {
        public bool isReceiver = true;
        public bool isOn = true;
        public Sprite ReceiverOn;
        public Sprite ReceiverOff;
        public Sprite MicOn;
        public Sprite MicOff;
        public Image icon;
        // Start is called before the first frame update
        void Start()
        {
            updateValue();
        }

        private void updateValue()
        {
            if (isReceiver)
            {
                if (isOn)
                {
                    icon.sprite = ReceiverOn;
                    HelloService.RTCAudioProxy.Instance.EnableRemoteAudio(true);
                }
                else
                {
                    icon.sprite = ReceiverOff;
                    HelloService.RTCAudioProxy.Instance.EnableRemoteAudio(false);
                }
            }
            else
            {
                if (isOn)
                {
                    icon.sprite = MicOn;
                    HelloService.RTCAudioProxy.Instance.EnableMicrophone(true);
                }
                else
                {
                    icon.sprite = MicOff;
                    HelloService.RTCAudioProxy.Instance.EnableMicrophone(false);
                }
            }
        }
        public void onValueChanged()
        {
            isOn = !isOn;
            updateValue();
        }

    }
}