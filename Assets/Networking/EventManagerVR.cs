using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MappingAI
{
    /// <summary>Event Manager</summary>
    public class EventManagerVR : MonoBehaviour
    {
        public static string StylusCalibrationCalibrate = "StylusCalibration";
        public static string StylusCalibrationReset = "StylusCalibrationReset";
        public static string StylusHandChanged = "StylusHandChanged";

        public static string SurfaceCalibrationCalibrate = "SurfaceCalibration";
        public static string SurfaceCalibrationInProgress = "SurfaceCalibrationInProgress";
        public static string SurfaceCalibrationCompleted = "SurfaceCalibrationCompleted";
        public static string SurfaceCalibrationReset = "SurfaceCalibrationReset";
        public static string CalibrationTypeChanged = "CalibrationTypeChanged";

        public static string PassthroughStart = "PassthroughStart";
        public static string PassthroughEnd = "PassthroughEnd";

        public static string CacheModeStart = "CacheModeStart";
        public static string CacheModeEnd = "CacheModeEnd";

        public static string OnCreateRoom = "OnCreateRoom";
        public static string OnRoomFail = "OnRoomFail";
        public static string OnPlayerEnteredRoom = "OnPlayerEnteredRoom";
        public static string OnPlayerLeftRoom = "OnPlayerLeftRoom";
        public static string InitGitlabSuccess = "InitGitlabSuccess";

        public static string TryJoinRoom = "TryJoinRoom";

        public static string SpatialAnchorloaded = "SpatialAnchorloaded";

        public static string ResetRenderTexture = "ResetRenderTexture";

        public static string UpdateVideoRecordingCamera = "UpdateVideoRecordingCamera";
        public static string ToggleVideoRecordingCamera = "ToggleVideoRecordingCamera";

        public static string _2DSketchBegin = "_2DSketchBegin";
        public static string _3DSketchBegin = "_3DSketchBegin";
        public static string _3DSketchMaliangBegin = "_3DSketchMaliangBegin";
        public static string SketchEnd = "SketchEnd";
        public static string SketchMaliangEnd = "SketchMaliangEnd";


        public static string InSketchCollider = "InSketchCollider";
        public static string OutSketchCollider = "OutSketchCollider";

        public static string HeightUpdateBegin = "HeightUpdateBegin";
        public static string HeightUpdateEnd = "HeightUpdateEnd";

        public static string Undo = "Undo";
        public static string Redo = "Redo";

        public static string _MidAirSketchBegin = "_MidAirSketchBegin";
        public static string _MidAirSketchEnd = "_MidAirSketchEnd";

        public static string EraserBegin = "EraserBegin";
        public static string EraserEnd = "EraserEnd";
        public static string SnapBegin = "SnapBegin";
        public static string SnapEnd = "SnapEnd";



        /// <summary>Available events</summary>
        private Dictionary<string, UnityEvent> _eventDictionary = new Dictionary<string, UnityEvent>();

        /// <summary>Event Manager Instance Internal</summary>
        private static EventManagerVR _instance;

        /// <summary>Event Manager Instance</summary>
        public static EventManagerVR Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EventManagerVR>();
                    _instance.Init();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        /// <summary>Initialize Event Manager</summary>
        void Init()
        {
            if (_eventDictionary == null)
            {
                _eventDictionary = new Dictionary<string, UnityEvent>();
            }
        }


        /// <summary>Adds a new event</summary>
        public static void StartListening(string eventName, UnityAction listener)
        {
            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                Instance._eventDictionary.Add(eventName, thisEvent);
            }
        }

        /// <summary>Removes an event</summary>
        public static void StopListening(string eventName, UnityAction listener)
        {
            if (_instance == null) return;
            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        /// <summary>Trigger an event</summary>
        public static void TriggerEvent(string eventName)
        {
            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.Invoke();
            }
        }
    }
}