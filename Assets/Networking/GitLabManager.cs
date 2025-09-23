using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MappingAI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GitLabManager
{
    private readonly string gitLabUrl;
    private readonly string ownerID;
    private readonly string collaboratorID;
    public static GitLabManager Instance;
    public GitLabManager()
    {
        Instance = this;
        this.gitLabUrl = ComponentManager.Instance.ApplicationSettings_Get().GitLabUrl;
        this.ownerID = ComponentManager.Instance.ApplicationSettings_Get().OwnerAccessToken;
        this.collaboratorID = ComponentManager.Instance.ApplicationSettings_Get().CollaboratorAccessToken;
    }

    /// <summary>
    /// Creates a new GitLab project.
    /// </summary>
    /// <param name="projectName">The name of the project to create.</param>
    /// <returns>
    /// Returns a tuple containing the HTTP URL of the created project and the Project ID, 
    /// or null and -1 if the creation failed.
    /// </returns>
    public async Task<(string projectUrl, int projectId)> CreateGitLabProject(string projectName)
    {
        using (var client = new HttpClient())
        {
            // Configure HttpClient
            client.BaseAddress = new Uri(gitLabUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.ownerID}");
            // Create project data
            var projectData = new
            {
                name = projectName,
                visibility = "public", // Private project
                initialize_with_readme = true
            };
            // Serialize request body
            var jsonContent = new StringContent(JsonConvert.SerializeObject(projectData), Encoding.UTF8, "application/json");

            try
            {
                // Send POST request
                var response = await client.PostAsync("/api/v4/projects", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Parse response and return project URL and Project ID
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

                    string projectUrl = jsonResponse["http_url_to_repo"].ToString();
                    int projectId = Convert.ToInt32(jsonResponse["id"]);

                    Debug.Log($"Project Created: {projectUrl}, Project ID: {projectId}");

                    // Add .gitignore file
                    await AddGitIgnoreFileAsync(projectId);

                    // Add .gitattributes file
                    await AddGitAttributesFileAsync(projectId, "main");
                    return (projectUrl, projectId);
                }
                else
                {
                    // Log error
                    Debug.LogError($"Failed to create GitLab project. Status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Error: {errorContent}");
                    return (null, -1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception creating GitLab project: {ex.Message}");
                return (null, -1);
            }
        }
    }
    public async Task AddGitIgnoreFileAsync(int projectId)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(gitLabUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ownerID}");

            var fileContent = new
            {
                branch = "main",
                content = UnityGitIgnoreContent,
                commit_message = "Add .gitignore file for Unity"
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(fileContent), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"/api/v4/projects/{projectId}/repository/files/.gitignore", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log(".gitignore file added successfully.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Failed to add .gitignore file. Status: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception adding .gitignore file: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// use Personal Access Token to fetch the ID
    /// </summary>
    public async Task<int> GetCurrentUserId()
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(gitLabUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {collaboratorID}");

            try
            {
                var response = await client.GetAsync("/api/v4/user"); 
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var userJson = JsonConvert.DeserializeObject<JObject>(responseContent);
                    int userId = userJson["id"].Value<int>(); // extract ID
                    Debug.Log($"Current User ID: {userId}");
                    return userId;
                }
                else
                {
                    Debug.LogError($"Failed to fetch user ID. Status: {response.StatusCode}, Response: {responseContent}");
                    return -1; // fail
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while fetching user ID: {ex.Message}");
                return -1; // fail
            }
        }
    }


    // <summary>
    /// invite client to an existing GitLab project as a collaborator.
    /// </summary>
    /// <param name="projectId">The ID of the GitLab project.</param>
    /// <param name="accessLevel">The access level to assign (default: Developer).</param>
    /// <returns>A Task<bool> indicating the success (true) or failure (false) of the operation.</returns>
    public async Task<bool> InviteGitLabProject(int projectId, int accessLevel = 50)
    {

        int userId = await GetCurrentUserId(); // 动态获取 user_id
        if (userId == -1)
        {
            Debug.LogError("Failed to get current user ID. Aborting invite to project.");
            return false;
        }

        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(gitLabUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ownerID}");

            var membershipData = new
            {
                user_id = userId,
                access_level = accessLevel
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(membershipData), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"/api/v4/projects/{projectId}/members", jsonContent);

                if (response.StatusCode == HttpStatusCode.Created) // 201
                {
                    Debug.Log("Success, Status: Created (201)");
                    return true;
                }

                Debug.LogError($"Failed to join project {projectId}. Status: {response.StatusCode}, Response: {response}");

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"Successfully joined project {projectId}");
                    return true; // Return success
                }
                else
                {
                    Debug.LogError($"Failed to join project {projectId}. Status: {response.StatusCode}");
                    return false; // Return failure
                }
            }

            catch (Exception ex)
            {
                Debug.LogError($"Exception joining project: {ex.Message}");
                return false; // Return failure
            }
        }
    }
    public async Task<bool> CreateBranch(int projectId, string branchName, string refBranch = "main")
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(gitLabUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ownerID}");

            // 请求体数据
            var branchData = new
            {
                branch = branchName, // 新分支的名称
                @ref = refBranch     // 基于哪个分支创建（通常是 main）
            };

            // 序列化为 JSON
            var jsonContent = new StringContent(JsonConvert.SerializeObject(branchData), Encoding.UTF8, "application/json");

            try
            {
                // 调用 GitLab API 创建分支
                var response = await client.PostAsync($"/api/v4/projects/{projectId}/repository/branches", jsonContent);

                // 检查返回的响应状态
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"Successfully created branch '{branchName}' for project {projectId}");
                    // Add .gitignore file to the newly created branch
                    //bool gitIgnoreAdded = await AddGitIgnoreFileToBranchAsync(projectId, branchName);

                    return true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync(); 
                    Debug.LogError($"Failed to create branch '{branchName}' for project {projectId}. Status: {response.StatusCode}, Response: {responseContent}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.LogError($"HTTP Request Exception: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"General Exception creating branch: {ex.Message}");
                return false;
            }
        }
    }
    public async Task<bool> AddGitIgnoreFileToBranchAsync(int projectId, string branchName)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(gitLabUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ownerID}");

            var fileContent = new
            {
                branch = branchName,
                content = UnityGitIgnoreContent,
                commit_message = "Add .gitignore file for Unity"
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(fileContent), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"/api/v4/projects/{projectId}/repository/files/.gitignore", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($".gitignore file added successfully to branch '{branchName}'.");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Failed to add .gitignore file to branch '{branchName}'. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception adding .gitignore file to branch '{branchName}': {ex.Message}");
                return false;
            }
        }
    }
    public async Task<bool> AddGitAttributesFileAsync(int projectId, string branchName)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(gitLabUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ownerID}");

            var fileContent = new
            {
                branch = branchName,
                content = GitAttributesContent,
                commit_message = "Add .gitattributes file"
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(fileContent), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"/api/v4/projects/{projectId}/repository/files/.gitattributes", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($".gitattributes file added successfully to branch '{branchName}'.");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Failed to add .gitattributes file to branch '{branchName}'. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception adding .gitattributes file to branch '{branchName}': {ex.Message}");
                return false;
            }
        }
    }
    private const string GitAttributesContent = @"
# Define line ending handling for specific files
*.json text eol=lf
*.meta text eol=lf

# Treat all other files as binary
* binary
";
    private const string UnityGitIgnoreContent = @"
# This .gitignore file should be placed at the root of your Unity project directory

# Ignore meta files
*.meta

# Ignore build directories
[Bb]uild/
[Bb]uilds/

# Ignore Library directory
[Ll]ibrary/

# Ignore Temp directory
[Tt]emp/

# Ignore Obj directory
[Oo]bj/

# Ignore logs
*.log

# Ignore crash reports
sysinfo.txt

# Ignore auto-generated VS/MD solution and project files
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.opendb
*.VC.db
";
}
