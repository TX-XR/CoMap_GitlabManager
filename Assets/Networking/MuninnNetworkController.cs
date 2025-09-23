using System;
using System.Collections.Generic;
using System.Text;
using Unity.Muninn;
using Unity.Muninn.Model;
using Unity.Muninn.MuninnLobby;
using UnityEngine;

namespace MappingAI
{
    public class MuninnNetworkController : MonoBehaviour
    {
        private string SaveDirectory;
        private MuninnEventHandler MuninnEventHandler;
        public static MuninnNetworkController Instance;
        public MuninnNetworkController() { Instance = this; }
        int cacheNumber = 1;
        List<LineData> allLineDatas = new List<LineData>();
        static EnumSystem.TranmissionType isCacheMode = EnumSystem.TranmissionType.realtime;
        int framecount = 0;
        bool isJoinRoom = false;
        bool firstTimeTryToJoinRoom = false;

        bool firstTimeCreateRoom = false;
        string roomID = "";
        //void OnEnable()
        //{
        //    EventManagerVR.StartListening(EventManagerVR.TryJoinRoom, () => { if (roomID.Length > 0) { MuninnNetwork.JoinRoomByUUID(roomID); } });
        //}

        //private void OnDisable()
        //{
        //    EventManagerVR.StopListening(EventManagerVR.TryJoinRoom, () => { if (roomID.Length > 0) { MuninnNetwork.JoinRoomByUUID(roomID); } });
        //}


        private void Start()
        {
            firstTimeCreateRoom = false;
            allLineDatas = new List<LineData>();
            cacheNumber = 1;
            MuninnEventHandler = ComponentManager.Instance.MuninnEventHandler_Get();

        }

        public static EnumSystem.TranmissionType GetCacheMode()
        {
            return isCacheMode;
        }
       
        // 1. 与 Muninn 相关的网络请求
        /// <summary>
        /// 创建并加入房间
        /// </summary>
        public void CreateAndJoin()
        {
            string roomName = "CoMapRoom";
            string roomNamespace = "CoMapNamespace";
            int maxPlayers = 3;
            CreateRoomRequest createRoomRequest = new CreateRoomRequest()
            {
                Name = roomName,
                Namespace = roomNamespace,
                MaxPlayers = maxPlayers,
                Visibility = MuninnRoomVisibility.Public,
                CustomProperties = new Dictionary<string, string>()  
                {
                    {"k1", "v1"},
                    {"k2", "v2"},
                },
            };
            MuninnNetwork.CreateAndJoinRoom(createRoomRequest);
            Debug.Log("CreateAndJoinRoom");
        }


        public void Create()
        {
            string roomName = "CoMapRoom";
            string roomNamespace = "CoMapNamespace";
            int maxPlayers = 10;
            string newUUID = Guid.NewGuid().ToString();
            // 3.创建房间并进入（如果房间已存在则直接进入）
            CreateOrJoinRoomRequest createOrJoinRoomRequest = new CreateOrJoinRoomRequest()
            {
                RoomUUID = newUUID,
                Name = roomName,
                Namespace = roomNamespace,
                MaxPlayers = maxPlayers,
                Visibility = MuninnRoomVisibility.Public,
                // 创建私有房间
                // Visibility = MuninnRoomVisibility.Private,
                // JoinCode = "<joincode>"
                CustomProperties = new Dictionary<string, string>()
                {
                    { "key1", "value1" },
                    { "Timestamp", DirectoryManager.Instance.GetTimestamp() },
                },
                
            };
            HelloService.RTCAudioProxy.Instance.SetChannelName(newUUID);
            MuninnNetwork.CreateOrJoinRoom(createOrJoinRoomRequest);
            DebugCanvas.Instance.AddMessage($"Creating Room {newUUID}");
        }

        /// <summary>
        /// 通过房间 ID 加入房间
        /// </summary>
        public void JoinRoomById()
        {
            Action<ListRoomResponse> callback = (resp) =>
            {
                if (resp.Code == (uint)MuninnCode.OK)
                {
                    if (resp.Items.Count > 0)
                    {
                        Debug.LogFormat("List Room successfully, count: {0}", resp.TotalCount);
                        string id = resp.Items[0].RoomUuid;
                        if (string.IsNullOrEmpty(id))
                        {
                            Debug.LogError("Room ID is null");
                            return;
                        }

                        if (!firstTimeTryToJoinRoom)
                        {
                            //Debug.Log("firstTimeTryToJoinRoom" + Time.time);
                            firstTimeTryToJoinRoom = true;
                            roomID = id;
                            MuninnNetwork.JoinRoomByUUID(roomID);
                            //EventManagerVR.TriggerEvent(EventManagerVR.TryJoinRoom);
                        }     
                    }
                }
                else
                {
                    Debug.LogError($"List room failed, errorCode: {resp.Code}");
                }
            };

            ListRoomRequest request = new ListRoomRequest { Status = MuninnLobbyRoomStatus.Ready };
            MuninnNetwork.ListRooms(request, callback);
        }
        public void JoinRoomOrCreate()
        {
            Action<ListRoomResponse> callback = (resp) =>
            {
                if (resp.Code == (uint)MuninnCode.OK)
                {
                    if (resp.Items.Count > 0)
                    {
                        Debug.LogFormat("List Room successfully, count: {0}", resp.TotalCount);
                        string id = resp.Items[0].RoomUuid;
                        if (string.IsNullOrEmpty(id))
                        {
                            Debug.LogError("Room ID is null");
                            return;
                        }

                        if (!firstTimeTryToJoinRoom)
                        {
                            //Debug.Log("firstTimeTryToJoinRoom" + Time.time);
                            firstTimeTryToJoinRoom = true;
                            roomID = id;
                            //MuninnNetwork.JoinRoomByUUID(roomID);
                            MuninnNetwork.CloseRoom(roomID);
                            Create();
                            //EventManagerVR.TriggerEvent(EventManagerVR.TryJoinRoom);
                        }
                    }
                    else
                    {
                        Create();
                    }
                }
                else
                {
                    Debug.LogError($"List room failed, errorCode: {resp.Code}");
                }
            };

            ListRoomRequest request = new ListRoomRequest { Status = MuninnLobbyRoomStatus.Ready };
            MuninnNetwork.ListRooms(request, callback);
        }
        /// <summary>
        /// 发送一般事件
        /// </summary>
        public void sendMessage(string msg)
        {
            DebugCanvas.Instance.AddMessage($"send Message:  {msg}");
            //string msg = "接收到一般事件。事件是 Sync 同步信息的载体，分为一般事件（Event）、缓存事件（Cached Event）和记事贴（StickyEvent）。
            //当前为一般事件，用于玩家之间的信息同步与共享，它是实时发送和接收的，不会被房间存储。发送者可以选择接收方范围，只有在接收范围内的玩家才会收到事件。";
            MuninnNetwork.RaiseEvent(Encoding.UTF8.GetBytes(msg), new RaiseEventOptions() { Target = RaiseEventTarget.TO_ALL_BUT_ME });
        }
        public void sendMessage<H>(H wrapper, bool addMessage = true)
        {
            string msg = JsonUtility.ToJson(wrapper);
            if (addMessage)
                DebugCanvas.Instance.AddMessage($"Send Message: {MuninnEventHandler.TruncateString(msg, 50)}");
            MuninnNetwork.RaiseEvent(Encoding.UTF8.GetBytes(msg), new RaiseEventOptions() { Target = RaiseEventTarget.TO_ALL_BUT_ME });
        }
        /// <summary>
        /// 离开房间
        /// </summary>
        public void leaveRoom()
        {
            MuninnRoomView view = MuninnEventHandler.GetComponent<MuninnEventHandler>().GetMuninnRoomView();
            if (view != null && view.Players.Count <= 1)
            {
                // 关闭房间
                MuninnNetwork.CloseRoom(view.Room.Id, CloseRoomCallback);
                Debug.Log("CloseRoom");
            }
            else
            {
                // 离开房间
                MuninnNetwork.Disconnect();
                Debug.Log("Disconnect");
            }
        }


        /// <summary>
        /// 发起 serve call
        /// </summary>
        public void ServerCall()
        {
            string data = "接收到 server call。在实时模式下，玩家客户端可以选择直接调用房主端上的方法（类似 RPC）。届时，房主端会收到 OnServerCall 事件。";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            MuninnNetwork.ServerCall(bytes);
        }

        /// <summary>
        /// 踢走玩家
        /// </summary>
        /// <param name="senderId"></param>
        public void KickPlayer(uint senderId)
        {
            MuninnNetwork.KickPlayer(senderId);
        }


        /// <summary>
        /// 离开房间执行的回调
        /// </summary>
        /// <param name="code"></param>
        public void CloseRoomCallback(CloseRoomResponse code)
        {
            Debug.Log("CloseRoomCallback");
        }

        /// <summary>
        /// 离开房间执行的回调
        /// </summary>
        /// <param name="code"></param>
        public void ListRoomResponseCallback(ListRoomResponse code)
        {
            Debug.Log("ListRoomResponseCallback" + code);
        }


    }
    
}