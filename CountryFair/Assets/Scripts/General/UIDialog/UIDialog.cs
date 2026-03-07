using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using TMPro;

public class UIDialog : DisplayInPlayerFront
{
    [Header("Dialogue Box")]
    [SerializeField]
    protected GameObject dialogueBoxGameObject;

    [SerializeField]
    protected TextMeshProUGUI dialogueBoxText;

    protected JSONData _data;
    protected string _jsonFileName;

    protected override void Awake()
    {   
       base.Awake();
       SetJSONFileName();

       if (dialogueBoxGameObject == null || dialogueBoxText == null)
       {
          Debug.LogError("Dialogue box or text is not assigned.");
          return;
       }

       StartCoroutine(LoadJSONDataRoutine());
    }

    protected virtual void OnDataLoaded()
    {
       // Debug.Log("Data loaded in base. Waiting for child class logic.");
    }

    protected virtual System.Type GetJSONDataType()
    {
        Debug.LogError("GetJSONDataType must be overridden.");
        return null;
    }

    private IEnumerator LoadJSONDataRoutine()
    {   
        string jsonContent = "";
   
        string relativePath = $"DialogFiles/{_jsonFileName}";

        if (Application.platform == RuntimePlatform.Android)
        {
            string uriPath = Path.Combine(Application.streamingAssetsPath, relativePath).Replace("\\", "/");

            using UnityWebRequest request = UnityWebRequest.Get(uriPath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error reading JSON: {request.error} | URL: {uriPath}");
                yield break;
            }
            jsonContent = request.downloadHandler.text;
        }
        else 
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "DialogFiles", _jsonFileName);
            if (File.Exists(filePath))
            {
                jsonContent = File.ReadAllText(filePath);
            }
            else
            {
                Debug.LogError("File not found on PC: " + filePath);
                yield break;
            }
        }

        // PARSE
        try
        {
            if (!string.IsNullOrWhiteSpace(jsonContent))
            {
                _data = (JSONData)JsonConvert.DeserializeObject(jsonContent, GetJSONDataType());

                OnDataLoaded(); 
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in JSON format: " + e.Message);
        }
    }

    protected virtual void SetJSONFileName()
    {
        Debug.LogError("SetJSONFileName must be overridden in derived classes.");
    }
    public virtual void NextStep()
    {
        Debug.LogError("NextStep method must be overridden in derived classes.");
    }
}