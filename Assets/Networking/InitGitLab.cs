using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MappingAI
{
    public class InitGitLab : MonoBehaviour
    {

        bool isInit = false;
        public Image Image;
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(InitButton);
        }

        public void InitButton()
        {
            if (!isInit)
            {
                Image.color = Color.green;
                isInit = true;
                MuninnNetworkController NetworkController = ComponentManager.Instance.NetworkController_Get();
                //NetworkController.CreateAndJoin();
                NetworkController.JoinRoomOrCreate();
            }

        }
    }
}