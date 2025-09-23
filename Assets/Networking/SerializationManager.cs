using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MappingAI
{
    [System.Serializable]
    public class ExperimentSettings
    {
        public string type = "ExperimentSettings"; // Type identifier
        public string expID; // Type identifier
        public string condition; // Type identifier
        public string scene;
    }

    [System.Serializable]
    public class CountDown
    {
        public string type = "CountDown"; // Type identifier
        public string countdownTime;
        public int sceneID;
    }
    [System.Serializable]
    public class Delete_Edit_Action
    {
        public string type = "Delete_Edit_Action"; // Type identifier
    }
    [System.Serializable]
    public class BaseType
    {
        public string type;
    }
    [Serializable]
    public class PointDataGenerate
    {
        public string type = "PointDataGenerate"; // Type identifier
    }
    [System.Serializable]
    public class GitLabData
    {
        public string type = "GitLabData"; // Type identifier
        public string url;
        public int project_id;
        public string projectName;
        public string OwnerBranchName;
        public string CollaboratorBranchName;
        public string ownerAccessToken;
        public string collaboratorAccessToken;
        public string ownerUserName;
        public string collaboratorUserName;

    }
    [System.Serializable]
    public class GlobalStrokeIndexJson
    {
        public string type = "GlobalStrokeIndexJson";
        public int index;
    }
    [System.Serializable]
    public class GlobalPointIndexJson
    {
        public string type = "GlobalPointIndexJson";
        public int index;
    }
    [System.Serializable]
    public class Cache_lineData
    {
        public string type = "Cache_lineData"; // Type identifier
        public List<LineData> lineDatas = new List<LineData>();
    }

    [Serializable]
    public class PointData
    {
        public string type = "PointData"; // Type identifier
        // User data
        public string Name; // Name for creator
        // Stroke data
        public int LayerIndex;
        public int PointIndex;
        public Vector3 origin;
        public string timestamp;
        public string jsonFilePath;
    }

    [Serializable]
    public class LineData
    {
        public string type = "LineData"; // Type identifier
        // User data
        public string Name; // Name for creator
        public EnumSystem.LineType lineType;
        public EnumSystem.AreaSketchType AreaSketchType;
        public EnumSystem.Action Action;
        // Stroke data
        public int LayerIndex;
        public int StrokeIndex;
        public EnumSystem.LineState LineState; // Active or Hidden
        public Vector3[] Positions;
        public LineAlignment generateLightingData;
        public bool loop;
        public Material material;
        public int positionCount;
        public ShadowCastingMode shadowCastingMode;
        public Color startColor;
        public Color endColor;
        public float startWidth;
        public float endWidth;
        public bool useWorldSpace;
        public string timestamp;
        public string jsonFilePath;
    }

    public class SerializationManager
    {
        public static string DeserializeJson(string message)
        {
            // First, parse the JSON into the base class to check the type field
            BaseType baseType = JsonUtility.FromJson<BaseType>(message);
            if (baseType.type == null || string.IsNullOrEmpty(baseType.type))
            {
                Debug.LogError("Invalid JSON or missing type field.");
                return null;
            }
            else
            {
                Debug.Log($"Event received: Type = {baseType.type}");
            }
            return baseType.type;

        }
    }
}