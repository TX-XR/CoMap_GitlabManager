using System;
using System.Collections.Generic;
using System.Text;
using Unity.Muninn;
using Unity.Muninn.Model;
using Unity.Muninn.Transport;
using UnityEngine;
using Unity.UOS.Common;
using static MappingAI.SerializationManager;
using Unity.Properties;


namespace MappingAI{
    public class MuninnEventHandler : MuninnBehaviour
    {
        private string userID = Guid.NewGuid().ToString();
        private uint playerSenderId = 0;
        private uint collaboratorSenderId = 0;
        private uint masterClientId = 0;
        static bool isCollaboratorJoin = false;
        MuninnPlayer owner;
        MuninnRoom curRoom;
        public MuninnRoomView curRoomView;
        private MuninnNetworkController networkController;
        [Tooltip("Room Configuration ID")]
        public string RoomProfileUUID;
        [Tooltip("Selection of different connection protocols")]
        public MuninnTransportType TransportType = MuninnTransportType.WebSocketSecure;
        List<LineData> cachelineData = new List<LineData>();
        public static MuninnEventHandler Instance;
        int globalStrokeIndex = 0;
        int globalPointIndex = 0;
        List<PointData> receivedPointDatas = new List<PointData>();
        List<LineData> receivedLineDatas = new List<LineData>();
        public MuninnEventHandler() { Instance = this; }

        private void Start()
        {
            networkController = ComponentManager.Instance.NetworkController_Get();
            MuninnSettings.TransportType = TransportType;
            MuninnSettings.UosAppId = Settings.AppID;
            MuninnSettings.UosAppSecret = Settings.AppSecret;
            MuninnSettings.RoomProfileUUID = RoomProfileUUID;

            // 当前玩家用户信息的配置 
            MuninnNetwork.PlayerInfo = new MuninnPlayerInfo()
            {
                Id = userID,
                Name = "CoMap_Host_Player",
                Properties = new Dictionary<String, String>()
                {
                    ["Role"] = "Host",
                    ["TimeStamp"] = DirectoryManager.Instance.GetTimestamp(),
                }
            };
        }



        #region Handling event callbacks

        public override void OnJoinedRoom(MuninnRoomView muninnRoomView)
        {
            Debug.Log($"Welcome to room {muninnRoomView.Room.Id}");

            //foreach (MuninnCachedEvent e in muninnRoomView.CachedEvents)
            //{
            //    // 每条 cached 单独展示
            //    NetworkController.GetComponent<NetworkController>().AddMessageItem(Encoding.UTF8.GetString(e.Data, 0, e.Data.Length), e.SenderId, EnumSystem.MessageType.otherMessage);
            //}
            this.curRoom = muninnRoomView.Room;
            this.curRoomView = muninnRoomView;
            // Initialise homeowner id and player id
            this.masterClientId = muninnRoomView.MasterClientId;
            // Player's own information
            owner = muninnRoomView.Self();
            playerSenderId = muninnRoomView.SenderId;
            EventManagerVR.TriggerEvent(EventManagerVR.OnCreateRoom);
            HelloService.RTCAudioProxy.Instance.JoinChannel(playerSenderId, false);
            DebugCanvas.Instance.AddMessage("room join successfully, roomID: " + muninnRoomView.Room.Id + " MasterClientId: " + this.masterClientId + " PlayerID: " + playerSenderId + " Player Count: " + GetMuninnRoomView().Players.Count);
        }


        public override void OnJoinRoomFailed(MuninnError error)
        {
            string errorMessage = MuninnCodeLocalize.GetCodeName(error.Code);
            Debug.LogError($"Failed to join room: {errorMessage}");
        }

        public override void OnMasterClientChanged(uint masterClientId)
        {
            base.OnMasterClientChanged(masterClientId);
            this.masterClientId = masterClientId;
        }

        public override void OnEvent(MuninnEvent e)
        {
            var type = e.SenderId == 0 ? EnumSystem.MessageType.pluginMessage : (playerSenderId == e.SenderId ? EnumSystem.MessageType.myMessage : EnumSystem.MessageType.otherMessage);
            string message = Encoding.UTF8.GetString(e.Data);
            //DebugCanvas.Instance.AddMessage($"Event received: Type = {type}, SenderId = {e.SenderId}");
            if (type == EnumSystem.MessageType.otherMessage)
            {
                // Determine the correct class based on the type field
                HandleMessage(type, message, e.SenderId.ToString());
            }
        }

        public static string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            {
                return input;
            }
            return input.Substring(0, maxLength);
        }

        public override void OnServerCall(MuninnEvent e)
        {
            string message = Encoding.UTF8.GetString(e.Data);
            Debug.Log($"Server call received: SenderId = {e.SenderId}, Message = {message}");
        }

        public override void OnPlayerEnteredRoom(MuninnPlayer player)
        {
            EventManagerVR.TriggerEvent(EventManagerVR.OnPlayerEnteredRoom);
            player.Properties.TryGetValue(key: "Role", out string value);
            DebugCanvas.Instance.AddMessage($"Player {player.SenderId}, Role {value}, entered room!");
            SendGlobalStrokeIndex();
            SendGlobalPointIndex();
        }

        public override void OnPlayerLeftRoom(MuninnPlayer player)
        {
            EventManagerVR.TriggerEvent(EventManagerVR.OnPlayerLeftRoom);
            player.Properties.TryGetValue(key: "Role", out string value);
            DebugCanvas.Instance.AddMessage($"Player {player.SenderId}, Role {value}, left room!");
        }
        public static bool IsCollaboratorJoin()
        {
            return isCollaboratorJoin;
        }

        public override void OnKickedPlayer(MuninnKickPlayerResponse rsp)
        {
            Debug.Log("kicked success");
        }

        public override void OnKickPlayerFailed(MuninnError error)
        {
            Debug.Log("Kick player failed.");
            string errorMessage = MuninnCodeLocalize.GetCodeName(error.Code);
            Debug.Log($"Error Details: {errorMessage}");
        }

        /// <summary>
        /// 根据 senderId 查询玩家
        /// </summary>
        /// <param name="senderId"></param>
        /// <returns></returns>
        public MuninnPlayer GetPlayerBySenderId(uint senderId)
        {
            MuninnRoomView view = GetMuninnRoomView();
            return view.GetPlayerBySenderId(senderId);
        }
        #endregion

        #region Self_written_functions

        private void HandleMessage(EnumSystem.MessageType type, string message, string senderID)
        { 
            string messageJsonType = DeserializeJson(message);
            DebugCanvas.Instance.AddMessage($"Message Type = {messageJsonType}, SenderID = {senderID}");
            switch (messageJsonType)
            {
                case "LineData":
                    LineData lineData = JsonUtility.FromJson<LineData>(message);
                    if (!isLineDataExist(lineData, receivedLineDatas).Item1)
                    {
                        receivedLineDatas.Add(lineData);
                        globalStrokeIndex++;
                        SendGlobalStrokeIndex();
                    }
                    else
                    {
                        DebugCanvas.Instance.AddMessage($"modify existing line, id: {lineData.StrokeIndex}");
                    }
                    break;
                case "PointDataGenerate":
                    globalPointIndex++;
                    SendGlobalPointIndex();
                    break;
                case "PointData":
                    PointData pointData = JsonUtility.FromJson<PointData>(message);
                    if (!isPointDataExist(pointData))
                    {
                        receivedPointDatas.Add(pointData);
                    }
                    else
                    {
                        DebugCanvas.Instance.AddMessage($"modify existing point, id: {pointData.PointIndex}");
                    }
                    break;
                default:
                    Debug.Log("Unknown type: " + type);
                    break;

            }
        }
        public static Tuple<bool, int> isLineDataExist(LineData lineData, List<LineData> receivedLineDatas)
        {
            bool isDataExist = false;
            int index = -1;
            for (int i = 0; i < receivedLineDatas.Count; i++)
            {
                LineData Data1 = receivedLineDatas[i];
                if (Data1.timestamp == lineData.timestamp)
                {
                    index = i;
                    isDataExist = true;
                    //LineRendererProperty p = GlobalIndex.Instance.GlobalReceivedLineRenderers[i].GetComponent<LineRendererProperty>();
                    //p.setState(lineData.LineState);
                    break;
                }
            }
            return Tuple.Create(isDataExist, index);
        }
        private bool isPointDataExist(PointData pointData)
        {
            bool isPointDataExist = false;
            for (int i = 0; i < receivedPointDatas.Count; i++)
            {
                PointData pointData1 = receivedPointDatas[i];
                if (pointData1.timestamp == pointData.timestamp)
                {
                    isPointDataExist = true;
                    receivedPointDatas[i] = pointData;
                    break;
                }
            }
            return isPointDataExist;
        }
        private void SendGlobalStrokeIndex()
        {
            
            GlobalStrokeIndexJson globalStrokeIndexJson = new GlobalStrokeIndexJson();
            globalStrokeIndexJson.index = globalStrokeIndex;
            MuninnNetworkController.Instance.sendMessage<GlobalStrokeIndexJson>(globalStrokeIndexJson);
            DebugCanvas.Instance.AddMessage($"send globalStrokeIndexJson,: {globalStrokeIndex}");
        }

        private void SendGlobalPointIndex()
        {
            
            GlobalPointIndexJson globalPointIndexJson = new GlobalPointIndexJson();
            globalPointIndexJson.index = globalPointIndex;
            MuninnNetworkController.Instance.sendMessage<GlobalPointIndexJson>(globalPointIndexJson);
            DebugCanvas.Instance.AddMessage($"send globalPointIndexJson,: {globalPointIndex}");
        }



        #endregion

    }
}