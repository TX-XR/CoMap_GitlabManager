using Agora.Rtc;
using Unity.UOS.Common;
using Unity.UOS.Auth;
using Unity.UOS.Hello;
using Unity.UOS.Hello.Exception;
using Unity.UOS.Hello.Model;
using TokenInfo = Unity.UOS.Hello.Model.TokenInfo;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif
namespace MappingAI.HelloService
{

    public class RTCAudioProxy : GenericSingleton<RTCAudioProxy>
    {
        private IRtcEngine RtcEngine;
        private string accessToken;
        private TokenInfo tokenInfo;
        uint userId;

        private async Task Init()
        {
            if (RtcEngine == null)
            {
                InitHelloSDK();
                await AuthTokenManager.ExternalLogin(userId.ToString());
                await CreateRTCEngine();
            }
        }
        private void Update()
        {
            CheckPermissions();
        }

        #region Process

        private void InitHelloSDK()
        {
            try
            {
                HelloSDK.Initialize();
            }
            catch (HelloClientException e)
            {
                Debug.Log($"failed to initialize sdk, clientEx: {e.Message}");
                throw;
            }
            catch (HelloServerException e)
            {
                Debug.Log($"failed to initialize sdk, serverEx: {e.Message}");
                throw;
            }
        }
        private async Task CreateRTCEngine()
        {
            var handler = new UserEventHandler();
            handler.OnUserJoinedEvent += OnUserJoined;
            handler.OnUserOfflineEvent += OnUserOffline;

            var context = new RtcEngineContext
            {
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING,
                areaCode = AREA_CODE.AREA_CODE_GLOB
            };
            // initalization of the RTC engine
            RtcEngine = await HelloSDK.InitRtcEngine(context, handler);
            Debug.Log($"successfully init agora rtc engine");

            RtcEngine.EnableAudio();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.AdjustPlaybackSignalVolume(300);
            RtcEngine.AdjustRecordingSignalVolume(200);
        }
        private void DestroyRTCEngine()
        {
            if (RtcEngine != null)
            {
                RtcEngine.DisableAudio();
                RtcEngine.Dispose();
                RtcEngine = null;
            }
        }
        private void DestroySpatialAudioEngine()
        {
            if (SpatialAudioEngine != null)
            {
                SpatialAudioEngine.Dispose();
                SpatialAudioEngine = null;
            }
        }
        private void OnDestroy()
        {
            DestroySpatialAudioEngine();
            DestroyRTCEngine();
        }

        #endregion

        #region Permission

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        private string[] permissionList = new string[]
        {
            //Permission.Camera,
            Permission.Microphone
        };
#endif
        private void CheckPermissions()
        {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            foreach (string permission in permissionList)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Permission.RequestUserPermission(permission);
                }
            }
#endif
        }

        #endregion

        #region Interface

        private string channelName;
        private bool isInChannel;
        public async void JoinChannel(uint uid, bool enableSpatialAudio)
        {
            userId = uid;
            if (RtcEngine == null)
            {
                await Init();
            }
            if (!isInChannel && RtcEngine != null)
            {
                await AuthTokenManager.ExternalLogin(uid.ToString());
                await GenerateAccessToken(channelName, new HelloOptions { role = Role.Publisher });
                int ret = RtcEngine.JoinChannel(accessToken, channelName, "", uint.Parse(tokenInfo.UserId));
                isInChannel = true;
                Debug.Log($"JoinChannel ## ret:{ret}, userId: {tokenInfo.UserId}");
            }
            if (enableSpatialAudio)
            {
                Instance.EnableSpatialAudio();
            }
            else
            {
                Instance.DisableSpatialAudio();
            }
        }
        public void LeaveChannel()
        {
            if (isInChannel && RtcEngine != null)
            {
                int ret = RtcEngine.LeaveChannel();
                Debug.Log($"LeaveChannel ## ret:{ret}");
                DisableSpatialAudio();
                remoteUidList.Clear();
                remotePositionInfoDict.Clear();
                isInChannel = false;
            }
        }
        public void SetChannelName(string name)
        {
            this.channelName = name;
        }
        private async Task GenerateAccessToken(string channel, HelloOptions options = null)
        {
            try
            {
                tokenInfo = await HelloSDK.Instance.GenerateAccessToken(channel, options);
                accessToken = tokenInfo.AccessToken;
            }
            catch (HelloClientException e)
            {
                Debug.LogErrorFormat("failed to generate token, clientEx: {0}", e.Message);
                throw;
            }
            catch (HelloServerException e)
            {
                Debug.LogErrorFormat("failed to generate token, serverEx: {0}", e.Message);
                throw;
            }
        }

        #endregion

        #region Spatial Audio

        private ILocalSpatialAudioEngine SpatialAudioEngine;

        private List<uint> remoteUidList = new List<uint>();
        private Dictionary<uint, RemoteVoicePositionInfo> remotePositionInfoDict = new Dictionary<uint, RemoteVoicePositionInfo>();
        private bool isOpenSpatialAudio;
        public void EnableSpatialAudio()
        {
            if (RtcEngine != null)
            {
                RtcEngine.MuteLocalAudioStream(true);
                RtcEngine.MuteAllRemoteAudioStreams(true);
                if (SpatialAudioEngine == null)
                {
                    SpatialAudioEngine = RtcEngine.GetLocalSpatialAudioEngine();
                    var ret = SpatialAudioEngine.Initialize();
                    Debug.Log("_spatialAudioEngine: Initialize " + ret);

                    //设置音频属性和场景 ####
                    RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_DEFAULT);
                    RtcEngine.SetAudioScenario(AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
                }
                else
                {
                    int ret = RtcEngine.EnableSpatialAudio(true);
                    Debug.Log($"EnableSpatialAudio ## ret:{ret}");
                }
                isOpenSpatialAudio = true;
                SpatialAudioEngine.SetMaxAudioRecvCount(10);
                //SetSpatialAudioRecvRange(ClientPrefs.GetRangeAudioDistance());
                //SetSpatialAudioBlur(ClientPrefs.GetVoiceBlur() > 0);
                //SetSpatialAudioAttenuation(ClientPrefs.GetAttenuation());

                //SpatialAudioEngine.MuteLocalAudioStream(ClientPrefs.GetMicrophoneState() != 1);
                //SpatialAudioEngine.MuteAllRemoteAudioStreams(ClientPrefs.GetReceiverState() != 1);
            }
        }
        public void DisableSpatialAudio()
        {
            isOpenSpatialAudio = false;
            if (RtcEngine != null)
            {
                if (SpatialAudioEngine != null)
                {
                    SpatialAudioEngine.ClearRemotePositions();
                    SpatialAudioEngine.MuteLocalAudioStream(true);
                    SpatialAudioEngine.MuteAllRemoteAudioStreams(true);
                }
                int ret = RtcEngine.EnableSpatialAudio(false);
                Debug.Log($"DisableSpatialAudio ## ret:{ret}");

                //RtcEngine.MuteLocalAudioStream(ClientPrefs.GetMicrophoneState() != 1);
                //RtcEngine.MuteAllRemoteAudioStreams(ClientPrefs.GetReceiverState() != 1);
            }
        }
        public void SetSpatialAudioRecvRange(float range)
        {
            if (SpatialAudioEngine != null)
            {
                int ret = SpatialAudioEngine.SetAudioRecvRange(range);
                Debug.Log($"SetSpatialAudioRecvRange ## ret:{ret}");
            }
        }
        public void SetSpatialAudioAttenuation(double value)
        {
            if (RtcEngine != null && SpatialAudioEngine != null)
            {
                foreach (var uid in remoteUidList)
                {
                    int ret = SpatialAudioEngine.SetRemoteAudioAttenuation(uid, value, false);
                    Debug.Log($"SetSpatialAudioAttenuation ##  uid:{uid} ## ret:{ret}");
                }
            }
        }
        private void SetSpatialAudioAttenuationByUid(uint uid, double value)
        {
            if (SpatialAudioEngine != null)
            {
                SpatialAudioEngine.SetRemoteAudioAttenuation(uid, value, false);
            }
        }
        public void SetSpatialAudioBlur(bool enable)
        {
            if (RtcEngine != null)
            {
                SpatialAudioParams audioParams = new SpatialAudioParams();
                audioParams.enable_blur.SetValue(enable);
                foreach (var uid in remoteUidList)
                {
                    int ret = RtcEngine.SetRemoteUserSpatialAudioParams(uid, audioParams);
                    Debug.Log($"SetSpatialAudioBlur ##  uid:{uid} ## ret:{ret}");
                }
            }
        }
        private void SetSpatialAudioBlurByUid(uint uid, bool enable)
        {
            if (SpatialAudioEngine != null)
            {
                SpatialAudioParams audioParams = new SpatialAudioParams();
                audioParams.enable_blur.SetValue(enable);
                RtcEngine.SetRemoteUserSpatialAudioParams(uid, audioParams);
            }
        }

        private void OnUserJoined(uint uid)
        {
            remoteUidList.Add(uid);
            RemoteVoicePositionInfo positionInfo = new RemoteVoicePositionInfo() { position = new float[3], forward = new float[3] };
            remotePositionInfoDict.Add(uid, positionInfo);
            //SetSpatialAudioBlurByUid(uid, ClientPrefs.GetVoiceBlur() > 0);
            //SetSpatialAudioAttenuationByUid(uid, ClientPrefs.GetAttenuation());
            if (SpatialAudioEngine != null)
            {
                SpatialAudioEngine.UpdateRemotePosition(uid, positionInfo);
            }
        }
        private void OnUserOffline(uint uid)
        {
            remoteUidList.Remove(uid);
            remotePositionInfoDict.Remove(uid);
            if (SpatialAudioEngine != null)
            {
                SpatialAudioEngine.RemoveRemotePosition(uid);
            }
        }

        float[] _localUserPosition = new float[3];
        float[] _forward = new float[3];
        float[] _right = new float[3];
        float[] _up = new float[3];
        public void UpdateLocalPosAndRot(Vector3 pos, Vector3 forwordV3, Vector3 rightV3, Vector3 upV3)
        {
            if (isOpenSpatialAudio)
            {
                SetFloatArray(_localUserPosition, pos);
                SetFloatArray(_forward, forwordV3);
                SetFloatArray(_right, rightV3);
                SetFloatArray(_up, upV3);
                var ret = SpatialAudioEngine.UpdateSelfPosition(_localUserPosition, _forward, _right, _up);
            }
        }
        public void UpdateRemotePosAndRot(uint remoteUid, Vector3 pos, Vector3 forwordV3)
        {
            if (isOpenSpatialAudio)
            {
                if (remotePositionInfoDict.TryGetValue(remoteUid, out var posInfo))
                {
                    SetFloatArray(posInfo.position, pos);
                    SetFloatArray(posInfo.forward, forwordV3);
                    SpatialAudioEngine.UpdateRemotePosition(remoteUid, posInfo);
                }
            }
        }

        private void LateUpdate()
        {
        }

        private void SetFloatArray(float[] array, Vector3 v)
        {
            array[0] = v.x; array[1] = v.y; array[2] = v.z;
        }

        #endregion

        #region Settings

        public void EnableMicrophone(bool enable)
        {
            if (isOpenSpatialAudio)
            {
                //RtcEngine.MuteLocalAudioStream(true);
                int r = SpatialAudioEngine.MuteLocalAudioStream(!enable);
                Debug.Log($"SpatialAudioEngine EnableMicrophone:{enable} ## ret:{r}");
            }
            else if (RtcEngine != null)
            {
                int ret = RtcEngine.MuteLocalAudioStream(!enable);
                Debug.Log($"EnableMicrophone:{enable} ## ret:{ret}");
            }
        }
        public void EnableRemoteAudio(bool enable)
        {
            if (isOpenSpatialAudio)
            {
                //RtcEngine.MuteAllRemoteAudioStreams(true);
                int r = SpatialAudioEngine.MuteAllRemoteAudioStreams(!enable);
                Debug.Log($"SpatialAudioEngine EnableMicrophone:{enable} ## ret:{r}");
            }
            else if (RtcEngine != null)
            {
                int ret = RtcEngine.MuteAllRemoteAudioStreams(!enable);
                Debug.Log($"EnableRemoteAudio:{enable} ## ret:{ret}");
            }
        }

        #endregion
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        public event System.Action<uint> OnJoinChannelSuccessEvent;
        public event System.Action<uint> OnUserJoinedEvent;
        public event System.Action<uint> OnUserOfflineEvent;

        public UserEventHandler()
        {
        }
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Join Channel Success " + connection.channelId + "  " + connection.localUid);
            OnJoinChannelSuccessEvent?.Invoke(connection.localUid);
        }
        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            Debug.Log("Leave Channel Success" + connection.channelId + "  " + connection.localUid);
        }
        public override void OnUserJoined(RtcConnection connection, uint remoteUid, int elapsed)
        {
            Debug.Log("OnUserJoined " + connection.channelId + "  " + remoteUid);
            OnUserJoinedEvent?.Invoke(remoteUid);
            base.OnUserJoined(connection, remoteUid, elapsed);
        }
        public override void OnUserOffline(RtcConnection connection, uint remoteUid, USER_OFFLINE_REASON_TYPE reason)
        {
            Debug.Log("OnUserOffline " + connection.channelId + "  " + remoteUid);
            OnUserOfflineEvent?.Invoke(remoteUid);
            base.OnUserOffline(connection, remoteUid, reason);
        }
        public override void OnError(int err, string msg)
        {
            Debug.LogWarning("######### OnError ###########" + err + "  " + msg);
        }
    }
}