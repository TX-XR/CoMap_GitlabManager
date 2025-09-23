using MappingAI;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApplicationSettingsUI : MonoBehaviour
{
    public TMP_InputField expID;
    public TMP_InputField condition;
    public TMP_InputField scene;
        
    public TMP_InputField gitLabUrlInputField;
    public TMP_InputField ownerAccessTokenInputField;
    public TMP_InputField collaboratorAccessTokenInputField;
    public TMP_InputField collaboratorUserNameInputField;
    public TMP_InputField ownerUserNameInputField;
    public TMP_InputField experimentDurationInputField;
    //public TMP_InputField ownerBranchNameInputField;
    //public TMP_InputField collaboratorBranchNameInputField;
    public TextMeshProUGUI gitlabStatus;
    public TextMeshProUGUI uosStatus;
    public TextMeshProUGUI time;
    public Image gitlabStatusImage;
    public Image uosStatusImage;
    //public TMP_InputField roomUUIDInputField;
    public ApplicationSettings applicationSettings;
    public int PlayerCount;
    bool isStartCountDown;
    public float countdownTime = 780;
    public float initialCountdownTime = -1;
    float currentTime;
    ES3Spreadsheet ES3Spreadsheet;
    private void OnEnable()
    {
        EventManagerVR.StartListening(EventManagerVR.InitGitlabSuccess, OnInitGitlabSuccess);
        EventManagerVR.StartListening(EventManagerVR.OnPlayerLeftRoom, OnPlayerLeftRoom);
        EventManagerVR.StartListening(EventManagerVR.OnPlayerEnteredRoom, OnPlayerEnterRoom);
    }

    private void OnDisable()
    {
        EventManagerVR.StopListening(EventManagerVR.InitGitlabSuccess, OnInitGitlabSuccess);
        EventManagerVR.StopListening(EventManagerVR.OnPlayerLeftRoom, OnPlayerLeftRoom);
        EventManagerVR.StopListening(EventManagerVR.OnPlayerEnteredRoom, OnPlayerEnterRoom);
    }

    private void Start()
    {
        applicationSettings = ComponentManager.Instance.ApplicationSettings_Get();
        ES3Spreadsheet = new ES3Spreadsheet();
        string path = Path.Combine(Application.streamingAssetsPath, "ExperimentSettings.csv");
        ES3Spreadsheet.Load(path);
        for (int row = 1; row < ES3Spreadsheet.RowCount; row++)
        {
            string ExperimentID = ES3Spreadsheet.GetCell<string>(0, row).Replace("\n", "").Replace("\r", "").Replace(" ", ""); ;
            string Condition = ES3Spreadsheet.GetCell<string>(1, row).Replace("\n", "").Replace("\r", "").Replace(" ", ""); ;
            string Scene = ES3Spreadsheet.GetCell<string>(2, row).Replace("\n", "").Replace("\r", "").Replace(" ", ""); ;
            string Duration = ES3Spreadsheet.GetCell<string>(3, row).Replace("\n", "").Replace("\r", "").Replace(" ", ""); ;
            string isDone = ES3Spreadsheet.GetCell<string>(4, row).Replace("\n", "").Replace("\r", "").Replace(" ", ""); ;
            if (isDone == "0")
            {
                applicationSettings.ExpID = ExperimentID;
                applicationSettings.Condition = Condition;
                applicationSettings.Scene = Scene;
                countdownTime = float.Parse(Duration);
                ES3Spreadsheet.SetCell(4, row, "1");
                ES3Spreadsheet.Save(path);
                break;
            }
        }

        initialCountdownTime = countdownTime;
        //applicationSettings.LoadSettingsFromFile();
        // Set the default values (from ApplicationSettings) as the placeholder text

        //SetPlaceholderText(gitLabUrlInputField, applicationSettings.GitLabUrl);
        //SetPlaceholderText(ownerAccessTokenInputField, applicationSettings.OwnerAccessToken);
        //SetPlaceholderText(ownerUserNameInputField, applicationSettings.OwnerUserName);

        //SetPlaceholderText(collaboratorAccessTokenInputField, applicationSettings.CollaboratorAccessToken);
        //SetPlaceholderText(collaboratorUserNameInputField, applicationSettings.CollaboratorUserName);
        SetPlaceholderText(experimentDurationInputField, countdownTime.ToString());

        SetPlaceholderText(expID, applicationSettings.ExpID);
        SetPlaceholderText(condition, applicationSettings.Condition);
        SetPlaceholderText(scene, applicationSettings.Scene);
        // Listen for changes in the input fields

        expID.onValueChanged.AddListener(OnExpIDChanged);
        condition.onValueChanged.AddListener(OnConditionChanged);
        scene.onValueChanged.AddListener(OnSceneChanged);
        //gitLabUrlInputField.onValueChanged.AddListener(OnGitLabUrlChanged);
        //ownerAccessTokenInputField.onValueChanged.AddListener(OnOwnerAccessTokenChanged);
        //collaboratorAccessTokenInputField.onValueChanged.AddListener(OnCollaboratorAccessTokenChanged);

        //ownerUserNameInputField.onValueChanged.AddListener(OnOnwerUserNameChanged);
        //collaboratorUserNameInputField.onValueChanged.AddListener(OnCollaboratorUserNameChanged);
        experimentDurationInputField.onValueChanged.AddListener(ModifyTheCountdownTime);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
        currentTime = Time.time;
        // Calculate minutes and seconds
        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);

        // Update the TextMeshPro text component with the current time in minutes and seconds
        time.text = $"{minutes:00}:{seconds:00}";
        //if (isStartCountDown)
        //{
        //    countdownTime = countdownTime - Time.deltaTime;
        //    CountDown countDown = new CountDown();
        //    countDown.countdownTime = Mathf.Floor(countdownTime).ToString();
        //    MuninnNetworkController.Instance.sendMessage<CountDown>(countDown, false);
        //}

    }
    ///// <summary>
    ///// Start count down if player number >= 2
    ///// </summary>
    ///// <returns></returns>
    //public IEnumerator StartCountdown()
    //{
    //    while (countdownTime > 0)
    //    {
    //        // Wait for the next frame
    //        yield return null;
    //        // Decrease the current time
    //        countdownTime = countdownTime - Time.deltaTime;
    //        CountDown countDown = new CountDown();
    //        countDown.countdownTime = Mathf.Floor(countdownTime).ToString();
    //        MuninnNetworkController.Instance.sendMessage<CountDown>(countDown, false);
    //    }
    //}
    public void OnInitGitlabSuccess()
    {
        gitlabStatus.text = "OK!";
        gitlabStatusImage.color = Color.green;
    }

    // Method to get a color based on the value (from red to yellow to green)
    Color GetColorForValue(float value, float minValue, float maxValue)
    {
        // Clamp the value between minValue and maxValue
        float clampedValue = Mathf.Clamp(value, minValue, maxValue);

        // Map the value to a range between 0 and 1
        float t = (clampedValue - minValue) / (maxValue - minValue);

        // Return a color based on the value t (from red to yellow to green)
        if (t < 0.5f)
        {
            // From red to yellow
            return Color.Lerp(Color.red, Color.yellow, t * 2);
        }
        else
        {
            // From yellow to green
            return Color.Lerp(Color.yellow, Color.green, (t - 0.5f) * 2);
        }
    }
    public void OnPlayerEnterRoom()
    {
        PlayerCount++;
        uosStatus.text = PlayerCount.ToString();
        uosStatusImage.color = GetColorForValue(PlayerCount, 0, 2);

        ExperimentSettings experimentSettings = new ExperimentSettings();
        experimentSettings.expID = applicationSettings.ExpID;
        experimentSettings.condition = applicationSettings.Condition;
        experimentSettings.scene = applicationSettings.Scene;
        MuninnNetworkController.Instance.sendMessage<ExperimentSettings>(experimentSettings, true);
        
        if (PlayerCount >= 2)
        {
            isStartCountDown = true;
            CountDown countDown = new CountDown();
            countDown.countdownTime = Mathf.Floor(countdownTime).ToString();
            countDown.sceneID = int.Parse(applicationSettings.Scene);
            MuninnNetworkController.Instance.sendMessage<CountDown>(countDown, false);
        }

    }
    public void OnPlayerLeftRoom()
    {
        isStartCountDown = false;
        countdownTime = initialCountdownTime;
        PlayerCount --;
        uosStatus.text = PlayerCount.ToString();
        uosStatusImage.color = GetColorForValue(PlayerCount, 0, 2);
        if (MuninnEventHandler.Instance.curRoomView.Players.Count < 2)
        {
            uosStatus.text = "Disconnect!";
            uosStatusImage.color = Color.red;
        }

        if (MuninnEventHandler.Instance.curRoomView.Players.Count ==0)
        {
            Quit();
        }
    }

    
    // Helper method to set the placeholder text using TMP_Text
    private void SetPlaceholderText(TMP_InputField inputField, string text)
    {
        TMP_Text placeholderText = inputField.placeholder.GetComponent<TMP_Text>();
        if (placeholderText != null)
        {
            placeholderText.text = text;
        }
    }

    private void OnExpIDChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.ExpID = newText;
        }
    }
    private void OnConditionChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            int value = int.Parse(newText);
            applicationSettings.Condition = Mathf.Clamp(value, 0, 1).ToString();
        }
    }
    private void OnSceneChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            int value = int.Parse(newText);
            applicationSettings.Scene = Mathf.Clamp(value, 0,2).ToString();
        }
    }
    private void OnGitLabUrlChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.GitLabUrl= newText;
        }
    }

    private void OnOwnerAccessTokenChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.OwnerAccessToken = newText;
        }
    }

    private void OnOwnerBranchNameChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.OwnerBranchName = newText;
        }
    }
    private void OnCollaboratorAccessTokenChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.CollaboratorAccessToken = newText;
        }
    }
    private void OnCollaboratorUserNameChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.CollaboratorUserName = newText;
        }
    }
    public void ModifyTheCountdownTime(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            countdownTime = int.Parse(newText);
            initialCountdownTime = int.Parse(newText);
        }
    }
    private void OnOnwerUserNameChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.OwnerUserName = newText;
        }
    }
    private void OnCollaboratorBranchNameChanged(string newText)
    {
        if (!string.IsNullOrEmpty(newText))
        {
            applicationSettings.CollaboratorBranchName = newText;
        }
    }

    public void Quit()
    {
        Application.Quit();    
    }
}
