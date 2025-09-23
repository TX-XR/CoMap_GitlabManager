using System.Collections;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;
using System.Threading.Tasks;
using MappingAI;
using System.Text;
using System.IO;

public class GitLabUnityManager: MonoBehaviour
{
    private static string _workingDirectory;
    private GitLabManager _gitLabManager;
    string _mainBranchName = "main";
    public static GitLabUnityManager Instance;
    public static bool isInitGitlabSuccess = false;
    private static bool isJoinMuninnRoomSuccess = false; // 标志 GitLab 是否已初始化
    private string jsonFilePath = Path.Combine(Application.streamingAssetsPath, "gitLabData.json");
    private GitLabData gitLabData = null;

    public GitLabUnityManager()
    {
        Instance = this;
    }

    private void Start()
    {
        _gitLabManager = new GitLabManager();
        _workingDirectory = DirectoryManager.Instance.GetWorkingDirectory();
    }

    private void OnEnable()
    {
        EventManagerVR.StartListening(EventManagerVR.OnCreateRoom, InitGitLab);
        EventManagerVR.StartListening(EventManagerVR.OnPlayerEnteredRoom, SendGitLabDataMessage);
    }

    private void OnDisable()
    {
        EventManagerVR.StopListening(EventManagerVR.OnCreateRoom, InitGitLab);
        EventManagerVR.StopListening(EventManagerVR.OnPlayerEnteredRoom, SendGitLabDataMessage);
    }

    public void InitGitLab()
    {
        
        _ = CreateGitLabProject_CreateBranch_InviteAsync(); // Fire-and-forget
        //StartCoroutine(CreateGitLabProject_CreateBranch_InviteCoroutine());

        //if (MuninnEventHandler.IsCollaboratorJoin())
        //{

        //}
        //MuninnRoomView muninnRoomView = MuninnEventHandler.Instance.GetMuninnRoomView();
        //if (muninnRoomView != null && muninnRoomView.Players.Count > 1)
        //{
        //    isJoinMuninnRoomSuccess = true;
        //    StartCoroutine(CreateGitLabProject_CreateBranch_InviteCoroutine());
        //}

    }
    // Method to check for null or uninitialized parameters
    public static bool HasNullOrEmptyFields(GitLabData data)
    {
        if (data == null)
            return true; // The data object itself is null

        if (string.IsNullOrEmpty(data.url) ||
            string.IsNullOrEmpty(data.projectName) ||
            string.IsNullOrEmpty(data.OwnerBranchName) ||
            string.IsNullOrEmpty(data.CollaboratorBranchName) ||
            string.IsNullOrEmpty(data.ownerAccessToken) ||
            string.IsNullOrEmpty(data.collaboratorAccessToken) ||
            string.IsNullOrEmpty(data.ownerUserName) ||
            string.IsNullOrEmpty(data.collaboratorUserName))
        {
            return true;
        }

        return false;
    }
    public async Task CreateGitLabProject_CreateBranch_InviteAsync()
    {
        if (ComponentManager.Instance.ApplicationSettings_Get().IsTest)
        {
            DebugCanvas.Instance.AddMessage($"Is Test Mode!");
            gitLabData = LoadGitLabDataFromFile();
            if (!HasNullOrEmptyFields(gitLabData))
            {
                DebugCanvas.Instance.AddMessage($"Load GitLabData from {jsonFilePath}, url: {gitLabData.url}");
                isJoinMuninnRoomSuccess = true;
                EventManagerVR.TriggerEvent(EventManagerVR.InitGitlabSuccess);
                return;
            }
        }

        try
        {
            // Create the GitLab project
            var (projectUrl, projectId) = await _gitLabManager.CreateGitLabProject(DirectoryManager.Instance.GetProjectName());
            if (string.IsNullOrEmpty(projectUrl) || projectId <= 0)
            {
                Debug.LogError("Project creation failed.");
                return;
            }

            Debug.Log($"GitLabProject created successfully: {projectUrl}");

            string ownerBranchName = ComponentManager.Instance.ApplicationSettings_Get().OwnerBranchName;
            string collaboratorBranchName = ComponentManager.Instance.ApplicationSettings_Get().CollaboratorBranchName;
            // Create branches
            bool isCreateOwnerBranchTaskSuccess = await TryCreateBranch(projectId, ownerBranchName);
            bool isCreateCollaboratorBranchTaskSuccess = await TryCreateBranch(projectId, collaboratorBranchName);

            if (!isCreateOwnerBranchTaskSuccess || !isCreateCollaboratorBranchTaskSuccess)
            {
                Debug.LogError("Failed to create required branches.");
                return;
            }

            // Invite to the project
            bool isInviteSuccess = await GitLabManager.Instance.InviteGitLabProject(projectId);
            if (!isInviteSuccess)
            {
                Debug.LogError("Failed to invite collaborators to the project.");
                return;
            }

            DebugCanvas.Instance.AddMessage($"GitLab Init Successfully.\nUrl: {projectUrl}");

            // Send project details and start cloning
            gitLabData = new GitLabData
            {
                url = projectUrl,
                project_id = projectId,
                projectName = DirectoryManager.Instance.GetProjectName(),
                OwnerBranchName = ownerBranchName,
                CollaboratorBranchName = collaboratorBranchName,
                ownerAccessToken = ComponentManager.Instance.ApplicationSettings_Get().OwnerAccessToken,
                collaboratorAccessToken = ComponentManager.Instance.ApplicationSettings_Get().CollaboratorAccessToken,
                ownerUserName = ComponentManager.Instance.ApplicationSettings_Get().OwnerUserName,
                collaboratorUserName = ComponentManager.Instance.ApplicationSettings_Get().CollaboratorUserName

            };

            SaveGitLabDataToFile(gitLabData);
            isJoinMuninnRoomSuccess = true;
            EventManagerVR.TriggerEvent(EventManagerVR.InitGitlabSuccess);
            //string jsonData = JsonUtility.ToJson(gitLabData);
            //MuninnNetworkController.Instance.sendMessage(jsonData);
            //StartCoroutine(CloneAndSwitchBranch(projectUrl, ApplicationSettings.Instance.OwnerBranchName));
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}");
        }
    }



    public void SendGitLabDataMessage()
    {
        // Include the host
        //if (MuninnEventHandler.Instance.GetMuninnRoomView().Players.Count == 3)
        //{
        //    string jsonData = JsonUtility.ToJson(GetGitLabData());
        //    MuninnNetworkController.Instance.sendMessage(jsonData);
        //}
        string jsonData = JsonUtility.ToJson(GetGitLabData());
        MuninnNetworkController.Instance.sendMessage(jsonData);

    }

    public GitLabData GetGitLabData()
    {
        if (isJoinMuninnRoomSuccess || ComponentManager.Instance.ApplicationSettings_Get().IsTest)
            return gitLabData;
        else
            return null;
    }

    public void SaveGitLabDataToFile(GitLabData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        File.WriteAllText(jsonFilePath, jsonData);
        DebugCanvas.Instance.AddMessage($"GitLabData saved to {jsonFilePath}");
    }

    public GitLabData LoadGitLabDataFromFile()
    {
        if (File.Exists(jsonFilePath))
        {
            string jsonData = File.ReadAllText(jsonFilePath);
            return JsonUtility.FromJson<GitLabData>(jsonData);
        }
        return null;
    }


private async Task<bool> TryCreateBranch(int projectId, string branchName)
    {
        try
        {
            bool isSuccess = await GitLabManager.Instance.CreateBranch(projectId, branchName);
            Debug.Log($"Successfully created branch: {branchName}");
            return isSuccess;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create branch {branchName}: {ex.Message}");
            return false;
        }
    }



    public IEnumerator CreateGitLabProject_CreateBranch_InviteCoroutine()
    {
        
        //Task<string> createTask = _gitLabManager.CreateGitLabProject(projectName);
        Task<(string projectUrl, int projectId)> createTask = _gitLabManager.CreateGitLabProject(DirectoryManager.Instance.GetProjectName());

        while (!createTask.IsCompleted)
        {
            yield return null; // Wait until the task is completed
        }

        if (createTask.IsFaulted)
        {
            Debug.Log($"Still creating project: {createTask.Exception}");
        }
        else
        {
            Debug.Log($"Still creating project: {createTask.Exception}");
            (string projectUrl, int projectId) result = createTask.Result;
            if (!string.IsNullOrEmpty(result.projectUrl) && result.projectId > 0)
            {
                Debug.Log($"GitLabProject created successfully: {result.projectUrl}");
                //Push("--set-upstream origin", mainBranchName);
                // send the project URL to the other clients

                bool isCreateOwnerBranchTaskSuccess = false;
                bool isCreateCollibratorBranchTaskSuccess = false;

                Task<bool> createOwnerBranchTask =  GitLabManager.Instance.CreateBranch(result.projectId, ComponentManager.Instance.ApplicationSettings_Get().OwnerBranchName);
                Task<bool> createCollibratorBranchTask = GitLabManager.Instance.CreateBranch(result.projectId, ComponentManager.Instance.ApplicationSettings_Get().CollaboratorBranchName);
                while (!createOwnerBranchTask.IsCompleted)
                {
                    yield return null; // Wait until the task is completed
                }
                if (createOwnerBranchTask.IsFaulted)
                {
                    Debug.Log($"waiting to creating project: {createOwnerBranchTask.Exception}");
                }
                else
                {
                    Debug.Log($"Create Owner Branch Task Success");
                    isCreateOwnerBranchTaskSuccess = true;
                }

                while (!createCollibratorBranchTask.IsCompleted)
                {
                    yield return null; // Wait until the task is completed
                }
                if (createCollibratorBranchTask.IsFaulted)
                {
                    Debug.Log($"waiting to creating project: {createCollibratorBranchTask.Exception}");
                }
                else
                {
                    Debug.Log($"Create Collibrator Branch Task Success");
                    isCreateCollibratorBranchTaskSuccess = true;
                }

                if (isCreateOwnerBranchTaskSuccess && isCreateCollibratorBranchTaskSuccess)
                {
                    Task<bool> joinTask = GitLabManager.Instance.InviteGitLabProject(result.projectId);
                    while (!joinTask.IsCompleted)
                    {
                        yield return null; // Wait until the task is completed
                    }

                    if (joinTask.IsFaulted)
                    {
                        Debug.Log($"waiting to creating project: {joinTask.Exception}");
                    }
                    else
                    {
                        Debug.Log($"start join");
                        var GitLabData = new GitLabData
                        {
                            projectName = DirectoryManager.Instance.GetProjectName(),
                            url = result.projectUrl,
                            project_id = result.projectId,
                        };
                        string jsonData = JsonUtility.ToJson(GitLabData);
                        MuninnNetworkController.Instance.sendMessage(jsonData);
                        //StartCoroutine(CloneAndSwitchBranch(result.projectUrl, ApplicationSettings.Instance.OwnerBranchName));
                    }
                }
            }
            else
            {
                Debug.LogError("Project creation failed.");
            }
        }
    }


    // Other Git methods (Init, Add, Commit, Push, etc.) remain the same...
    /// <summary>
    /// Executes a Git command in the specified working directory.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the Git command.</param>
    /// <returns>The output from the Git command execution.</returns>
    private static string ExecuteGitCommand(string arguments, string workingDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git", // Ensure Git is installed and added to the system's PATH.
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Git Error: {error}");
            }

            return output;
        }
    }


    private static string ExecuteGitCommand(string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git", // Ensure Git is installed and added to the system's PATH.
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _workingDirectory
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Git Error: {error}");
            }

            return output;
        }
    }
    /// <summary>
    /// Executes a Git command asynchronously in the specified working directory.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the Git command.</param>
    /// <returns>A Task that resolves to the output from the Git command execution.</returns>
    public void ExecuteGitCommandAsync(string arguments, string workingDirectory, Action<string> onOutputReceived, Action<string> onErrorReceived, Action onComplete)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git", // 确保 Git 安装并添加到系统 PATH
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            // 监听标准输出
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    onOutputReceived?.Invoke(args.Data);
                }
            };

            // 监听标准错误
            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    onErrorReceived?.Invoke(args.Data);
                }
            };
            Debug.Log($"Project created successfully");
            process.Start();

            // 开始异步读取输出和错误
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 等待进程退出
            process.WaitForExit();

            // 执行完成回调
            onComplete?.Invoke();
        }
    }


    /// <summary>
    /// Executes a Git command asynchronously in the specified working directory.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the Git command.</param>
    /// <returns>A Task that resolves to the output from the Git command execution.</returns>
    public void ExecuteGitCommandAsync(string arguments, Action<string> onOutputReceived, Action<string> onErrorReceived, Action onComplete)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git", // 确保 Git 安装并添加到系统 PATH
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _workingDirectory
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            // 监听标准输出
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    onOutputReceived?.Invoke(args.Data);
                }
            };

            // 监听标准错误
            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    onErrorReceived?.Invoke(args.Data);
                }
            };
            Debug.Log($"Project created successfully");
            process.Start();

            // 开始异步读取输出和错误
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 等待进程退出
            process.WaitForExit();

            // 执行完成回调
            onComplete?.Invoke();
        }
    }

    public Task<string> ExecuteGitCommandAsync(string arguments, string workingDirectory)
    {
        var tcs = new TaskCompletionSource<string>();

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git", // Ensure Git is installed and added to the system's PATH
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    outputBuilder.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    errorBuilder.AppendLine(args.Data);
                }
            };

            process.Exited += (sender, args) =>
            {
                if (process.ExitCode == 0)
                {
                    tcs.SetResult(outputBuilder.ToString());
                }
                else
                {
                    tcs.SetException(new Exception(errorBuilder.ToString()));
                }
                process.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        return tcs.Task;
    }

    /// <summary>
    /// Initializes a new Git repository in the working directory.
    /// </summary>
    public static void Init(string message)
    {
        Debug.Log("Initializing Git repository...");
        ExecuteGitCommand(message, _workingDirectory);
    }
    /// <summary>
    /// Initializes a new Git repository in the working directory.
    /// </summary>
    public static void RemoteAddOrigin(string message)
    {
        ExecuteGitCommand(message, _workingDirectory);
    }
    /// <summary>
    /// Retrieves the current Git status.
    /// </summary>
    /// <returns>The status output from Git.</returns>
    public string GetStatus()
    {
        Debug.Log("Getting Git status...");
        return ExecuteGitCommand("status", _workingDirectory);
    }

    /// <summary>
    /// Pulls the latest changes from the specified remote and branch.
    /// </summary>
    /// <param name="remote">The name of the remote repository (default: origin).</param>
    /// <param name="branch">The branch name to pull from (default: main).</param>
    public static void Pull(string remote = "origin", string branch = "main")
    {
        Debug.Log($"Pulling from {remote}/{branch}...");
        ExecuteGitCommand($"pull {remote} {branch}", _workingDirectory);
    }

    /// <summary>
    /// Adds files to the staging area.
    /// </summary>
    /// <param name="message">The file path or directory to add (default: all files).</param>
    public static void Add(string message = ".")
    {
        Debug.Log($"Adding files to staging area: {message}...");
        ExecuteGitCommand($"add {message}");
    }

    /// <summary>
    /// Commits staged changes to the repository.
    /// </summary>
    /// <param name="message">The commit message.</param>
    public static void Commit(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogError("Commit message cannot be empty.");
            return;
        }

        Debug.Log($"Committing changes: {message}...");
        ExecuteGitCommand($"commit -m \"{message}\"", _workingDirectory);
    }

    /// <summary>
    /// Pushes local changes to the specified remote and branch.
    /// </summary>
    /// <param name="remote">The name of the remote repository (default: origin).</param>
    /// <param name="branch">The branch name to push to (default: main).</param>
    public static void Push(string branch = "main")
    {
        ExecuteGitCommand($"push origin {branch}");
    }
    //public async Task<string> PushAsync(string branch = "main")
    //{
    //    try
    //    {
    //        string output = await ExecuteGitCommandAsync($"push origin {branch}", _workingDirectory);
    //        Debug.Log($"Git push completed: {output}");
    //        return output;
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Git push failed: {ex.Message}");
    //        return null;
    //    }
    //}

    public async void PushAsync(string branch = "main")
    {
        await Task.Run(() =>
        {
            ExecuteGitCommandAsync(
                $"push origin {branch}",
                output =>
                {
                    Debug.Log($"Git Output: {output}");
                },
                error =>
                {
                    Debug.Log($"Git Error: {error}");
                },
                () =>
                {
                    Debug.Log("Git push completed.");
                }
            );
        });
    }
    public IEnumerator CloneAndSwitchBranch(string repositoryUrl, string branchName)
    {
        Debug.Log("Start cloning repository and switching branch...");

        ExecuteGitCommandAsync($"clone {repositoryUrl}", DirectoryManager.Instance.GetRootPath(), output =>
        {
            if (!string.IsNullOrWhiteSpace(output))
            {
                Debug.Log($"Clone successful: {output}");
            }
            else
            {
                Debug.Log("No output from Git.");
            }
        },
        error => { },
        () =>
        {
            ExecuteGitCommandAsync($"checkout {branchName}", output =>
            {
                Debug.Log($"Switched to branch {branchName}: {output}");
            },
            error => Console.WriteLine($"Error: {error}"),
            () =>
            {
                EventManagerVR.TriggerEvent(EventManagerVR.CalibrationTypeChanged);
                isInitGitlabSuccess = true;
                Debug.Log($"Switch branch successful: {branchName}");
            });
        });

        yield break;
    }

    public static void ResolveMergeConflict(string remote, string branch)
    {
        Debug.Log($"Attempting to resolve merge conflicts for {remote}/{branch}...");
        ExecuteGitCommand("merge --abort"); // 放弃当前冲突的合并
        ExecuteGitCommand($"pull {remote} {branch} --rebase"); // 重新拉取并应用变更
    }


    /// <summary>
    /// Clones a remote Git repository to the specified directory.
    /// </summary>
    /// <param name="repositoryUrl">The URL of the remote repository.</param>
    /// <param name="directory">The target directory for the cloned repository.</param>
    public void Clone(string repositoryUrl, string directory = "")
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            Debug.LogError("Repository URL cannot be empty.");
            return;
        }

        Debug.Log($"Cloning repository: {repositoryUrl}...");
        ExecuteGitCommand($"clone {repositoryUrl} {directory}");
    }
}
