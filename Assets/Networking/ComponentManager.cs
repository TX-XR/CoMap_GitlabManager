using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MappingAI
{
    public class ComponentManager : MonoBehaviour
    {
        MuninnEventHandler MuninnEventHandler;
        MuninnNetworkController NetworkController;
        ApplicationSettings applicationSettings;
        public static ComponentManager Instance;
        public ComponentManager()
        {
            Instance = this;
        }
        // Start is called before the first frame update
        void Awake()
        {
            applicationSettings = new ApplicationSettings();
            NetworkController = FindAnyObjectByType< MuninnNetworkController>();
            MuninnEventHandler = FindAnyObjectByType<MuninnEventHandler>();
        }
        public ApplicationSettings ApplicationSettings_Get()
        {
            return applicationSettings;
        }
        public MuninnEventHandler MuninnEventHandler_Get()
        {
            return MuninnEventHandler;
        }
        public MuninnNetworkController NetworkController_Get()
        {
            return NetworkController;
        }
    }
}