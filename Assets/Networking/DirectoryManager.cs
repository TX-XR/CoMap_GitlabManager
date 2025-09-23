using MappingAI;
using System;
using System.IO;
using UnityEngine;

namespace MappingAI
{


    public class DirectoryManager : MonoBehaviour
    {
        string projectName;
        string Timestamp;
        string WorkingDirectory;
        static public string currentJsonPath;
        static public string currentJsonName;
        public static DirectoryManager Instance;
        public string rootPath;

        public DirectoryManager() { Instance = this; }

        // Start is called before the first frame update
        void Awake()
        {
            Timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
#if UNITY_EDITOR
            rootPath = Path.Combine(Application.dataPath, "GitManagement");
#else
            rootPath = Path.Combine(Application.persistentDataPath, "GitManagement");
#endif
            projectName = $"CoMap_Operation_{Timestamp}";
            WorkingDirectory = Path.Combine(rootPath, projectName);
        }

        public void SetProjectName_WorkingDirectory(string projectName)
        {
            this.projectName = projectName;
            this.WorkingDirectory = Path.Combine(rootPath, projectName);
        }

        public string GetWorkingDirectory()
        {

            return WorkingDirectory;
        }
        public string GetTimestamp()
        {

            return Timestamp;
        }
        public string GetProjectName()
        {
            return projectName;
        }


        public string GetRootPath()
        {
            return rootPath;
        }

        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"Directory created at {directoryPath}");
            }
        }
        public static string GetCurrentJsonPath()
        {
            return currentJsonPath;
        }

        public static string GetCurrentJsonName()
        {
            return currentJsonName;
        }
    }
}