using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json; 

/// <summary>
/// Singleton that loads and saves the patient's persistent JSON save file (<c>survivorData.json</c>).
/// On Android (Quest) the file lives in <see cref="Application.persistentDataPath"/>; in the Editor it is placed at the project root.
/// </summary>
public class DataFileManager
{
    private const string SAVE_FILE_NAME = "survivorData.json";

    // Instância global (Singleton)
    private static DataFileManager _instance  = null;

    // A variável que guarda os dados em memória
    public DataFileRoot CurrentData { get; private set; }

    private string _filePath;

    private DataFileManager()
    {
        LoadData();
        SetFilePath();
    }

    /// <summary>Returns the singleton instance, creating it on first call (which triggers an immediate file load).</summary>
    public static DataFileManager GetInstance()
    {
        return _instance ??= new DataFileManager();
    }

    private void SetFilePath()
    {
        #if UNITY_EDITOR
         
            string projectRoot = Directory.GetParent(Application.dataPath).ToString();
            _filePath = Path.Combine(projectRoot, SAVE_FILE_NAME);
        #else
            _filePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        #endif
    }

    private void SaveData()
    {
        string path = _filePath;

        string jsonString = JsonConvert.SerializeObject(CurrentData, Formatting.Indented);

        try
        {
            File.WriteAllText(path, jsonString);

            // Debug.Log($"[DataFileManager] Data saved in : {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataFileManager] Error in saving: {e.Message}");
        }
    }

    /// <summary>
    /// Reads and deserializes the save file into <see cref="CurrentData"/>.
    /// Creates a new empty <see cref="DataFileRoot"/> if the file does not exist or cannot be parsed.
    /// </summary>
    public void LoadData()
    {

        if (File.Exists(_filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                CurrentData = JsonConvert.DeserializeObject<DataFileRoot>(jsonString);
                Debug.Log($"[DataFileManager] Dados carregados de: {_filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataFileManager] Erro ao ler ficheiro (criando novo): {e.Message}");
                CurrentData = new DataFileRoot();
            }
        }
        else
        {
            Debug.Log($"[DataFileManager] Ficheiro não encontrado em {_filePath}. A criar novo perfil.");
            CurrentData = new DataFileRoot();
        }
    }

    /// <summary>
    /// Adds or updates a session record in <see cref="CurrentData"/> and immediately writes it to disk.
    /// </summary>
    /// <param name="newSession">Metrics to store for the session.</param>
    /// <param name="sessionID">Timestamp key used to identify the session (e.g., "2024-01-01_10-00-00").</param>
    /// <param name="miniGameName">"frisbee" or "archery" — determines which sub-dictionary receives the record.</param>
    public void SaveSessionData(SessionData newSession, string sessionID, string miniGameName)
    {
        // Pequena proteção para garantir que os objetos internos existem
        if (CurrentData == null) CurrentData = new DataFileRoot();
        if (CurrentData.frisbeeGame == null) CurrentData.frisbeeGame = new ();
        if (CurrentData.archeryGame == null) CurrentData.archeryGame = new ();

        if (miniGameName == "frisbee")
        {
            if (CurrentData.frisbeeGame.SessionsData.ContainsKey(sessionID))
            {
                CurrentData.frisbeeGame.SessionsData[sessionID] = newSession; 
            }
            else
            {
                CurrentData.frisbeeGame.SessionsData.Add(sessionID, newSession);
            }
        }
        else if (miniGameName == "archery")
        {
            if (CurrentData.archeryGame.SessionsData.ContainsKey(sessionID))
            {
                CurrentData.archeryGame.SessionsData[sessionID] = newSession;
            }
            else
            {
                CurrentData.archeryGame.SessionsData.Add(sessionID, newSession);
            }
        }
        else
        {
            Debug.LogError($"Invalid Mini Game Name: {miniGameName}");
            return;
        }

        SaveData(); 
    }


    /// <summary>
    /// Replaces the Frisbee adaptive parameters dictionary and writes the file to disk.
    /// </summary>
    /// <param name="adaptiveParameters">New key-value map of adaptive algorithm parameters.</param>
    public void AddFrisbeeAdaptiveParameters(Dictionary<string, string> adaptiveParameters)
    {
        CurrentData.frisbeeGame.AdadaptiveParameters = adaptiveParameters;

        SaveData();
    }
}