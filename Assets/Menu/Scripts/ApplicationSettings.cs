using Google.Protobuf.WellKnownTypes;
using System.IO;
using UnityEngine;


//[CreateAssetMenu(fileName = "ApplicationSettings", menuName = "CoMap/ApplicationSettings", order = 1)]
[System.Serializable]
public class ApplicationSettings
{
    [Header("Settings")]
    // GitLab API base URL and Personal Access Token

    [SerializeField] private string _GitLabUrl; // GitLab API base URL
    [SerializeField] private string _OwnerAccessToken; // Personal Access Token (PAT)
    [SerializeField] private string _CollaboratorAccessToken; // Personal Access Token (PAT)
    [SerializeField] private string _OwnerBranchName = "OwnerBranch";
    [SerializeField] private string _CollaboratorBranchName = "CollaboratorBranch";
    [SerializeField] private string _RoomUUID;
    [SerializeField] private string _OwnerUserName;
    [SerializeField] private string _CollaboratorUserName;
    [SerializeField] private bool _isTest = true;
    [SerializeField] private string expID = "0";
    [SerializeField] private string condition = "0";
    [SerializeField] private string scene = "0";

    public ApplicationSettings()
    {
        LoadSettingsFromStreamingAssets();
    }
    //private static ApplicationSettings _instance;

    //public static ApplicationSettings Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {        // Load the settings from the file

    //            _instance = Resources.Load<ApplicationSettings>("ApplicationSettings");
    //            _instance.LoadSettingsFromFile();
    //        }

    //        return _instance;
    //    }

    //    set { _instance = value; }
    //}
    // Method to load settings from a txt file

    public void LoadSettingsFromStreamingAssets()
    {
        // Path to the file in StreamingAssets
        string filePath = Path.Combine(Application.streamingAssetsPath, "Settings.txt");

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] keyValue = line.Split('=');

                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    // Map the key-value pairs to the class fields
                    switch (key)
                    {
                        case "GitLabUrl":
                            _GitLabUrl = value;
                            break;
                        case "OwnerAccessToken":
                            _OwnerAccessToken = value;
                            break;
                        case "CollaboratorAccessToken":
                            _CollaboratorAccessToken = value;
                            break;
                        case "OwnerBranchName":
                            _OwnerBranchName = value;
                            break;
                        case "CollaboratorBranchName":
                            _CollaboratorBranchName = value;
                            break;
                        case "OwnerUserName":
                            _OwnerUserName = value;
                            break;
                        case "CollaboratorUserName":
                            _CollaboratorUserName = value;
                            break;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Settings file not found at path: " + filePath);
        }
    }
    public string Scene
    {
        get { return scene; }
        set { scene = value; }
    }

    public string Condition
    {
        get { return condition; }
        set { condition = value; }
    }

    public string ExpID
    {
        get { return expID; }
        set { expID = value; }
    }

    // Additional methods for getting default values for placeholders
    public string GitLabUrl
    {
        get { return _GitLabUrl; }
        set { _GitLabUrl = value; }
    }    
    
    public bool IsTest
    {
        get { return _isTest; }
        set { _isTest = value; }
    }
    public string OwnerAccessToken
    {
        get { return _OwnerAccessToken; }
        set { _OwnerAccessToken = value; }
    }
    public string CollaboratorAccessToken
    {
        get { return _CollaboratorAccessToken; }
        set { _CollaboratorAccessToken = value; }
    }
    public string OwnerBranchName
    {
        get { return _OwnerBranchName; }
        set { _OwnerBranchName = value; }
    }
    public string CollaboratorBranchName
    { 
        get { return _CollaboratorBranchName; }
        set { _CollaboratorBranchName = value;}
    }
    public string OwnerUserName
    {
        get { return _OwnerUserName; }
        set { _OwnerUserName = value; }
    }
    public string CollaboratorUserName
    {
        get { return _CollaboratorUserName; }
        set { _CollaboratorUserName = value; }
    }

}