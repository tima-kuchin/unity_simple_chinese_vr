using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;
using System.Diagnostics;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;
using UnityEngine.TextCore;
using UnityEditor.ShaderKeywordFilter;
using UnityEditor.Search;
using System.Threading;
using System.Net.NetworkInformation;
using Unity.VisualScripting.Dependencies.Sqlite;

public class RunProcess : MonoBehaviour
{
    private float timeRemaining;
    private bool isRunning;
    private bool isPaused;
    public TMP_Text timeText;
    public GameObject pauseMenu;
    public GameObject setingsMenu;
    public GameObject gameView;
    public GameObject startRunButton;
    public GameObject resetRunButton;
    public GameObject hieroglyphField;
    public Button resetTimerButton;
    public Button addTimerButton;
    public Button subTimerButton;
    public TMP_Dropdown difficultDropdown;
    public TMP_Text attemptScore;
    public TMP_Text hUni;
    private int score;

    public TMP_Text chineseHierField;

    private string grCode;
    private Dictionary<string, List<string>> database;

    public Button CheckButton;
    List<string> graphemesToCheck = new List<string>();

    public GameObject mainHierField;
    private int HierComplete;

    public GameObject tilePrefab;
    private string imagesFolderPath = "gr_database/Graphemes/";

    private Texture2D[] textures;
    public GameObject GraphPlaces;

    private bool endedWithTimer;
    XRSocketInteractor[] socketInteractors;

    List<GameObject> createdTiles = new List<GameObject>();
    List<GameObject> socketedTiles = new List<GameObject>();

    [System.Serializable]
    public class AttemptData
    {
        public string TimeTaken;
        public int HieroglyphsCompleted;
        public int Score;
    }

    [System.Serializable]
    public class AttemptDataContainer
    {
        public List<AttemptData> attempts;
    }


    //Базовые функции
    void Start()
    {
        InitGame();
    }

    void Update()
    {
        if (isRunning && !isPaused)
        {
            startRunButton.SetActive(false);
            resetRunButton.SetActive(true);

            resetTimerButton.interactable = false;
            addTimerButton.interactable = false;
            subTimerButton.interactable = false;
            difficultDropdown.interactable = false;

            if (timeRemaining >= 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                endedWithTimer = true;
                StopGame();
            }
        }
    }

    //Загрузка базы
    private void LoadDatabase(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        database = new Dictionary<string, List<string>>();

        foreach (string line in lines)
        {
            string[] parts = line.Split(':');
            if (parts.Length == 2)
            {
                string grCode = parts[0];
                string[] grCodes = parts[1].Split(',');
                List<string> grCodesList = new List<string>(grCodes);
                database.Add(grCode, grCodesList);
            }
        }
    }



    //Работа со счетом
    private void UpdateScore()
    {
        attemptScore.text = score.ToString();
    }

    //Работа с иероглифами
    private void LoadHier()
    {
        int randomIndex = UnityEngine.Random.Range(0, database.Count);
        grCode =  database.Keys.ElementAt(randomIndex);
        int unicodeValue = int.Parse(grCode, System.Globalization.NumberStyles.HexNumber);
        char unicodeGrCode = (char)unicodeValue;
        chineseHierField.SetText(unicodeGrCode.ToString());
        hUni.SetText(grCode);
    }

    public void CheckHier()
    {
        if (isRunning && !isPaused)
        {
            List<string> graphemes = GetSocketedGraphemes();
            if (graphemes.Count == database[grCode].Count && graphemes.OrderBy(x => x).SequenceEqual(database[grCode].OrderBy(x => x)))
            {
                score += 100;
                HierComplete += 1;
                UpdateScore();
                LoadHier();
                UpdateTileField(true);
                UpdateTileField(false);

                AudioManager.Instance.PlaySFX("Correct");
            }
            else
            {
                score -= 10;
                UpdateScore();
                AudioManager.Instance.PlaySFX("Wrong");
            }
        }
    }

    //Работа с таймером
    private void PauseTimer()
    {
        isPaused = true;
        PlayerPrefs.SetFloat("SavedTime", timeRemaining);
    }

    private void ResumeTimer()
    {
        isPaused = false;
        timeRemaining = PlayerPrefs.GetFloat("SavedTime");
    }

    public void ResetTimer()
    {
        timeRemaining = Convert.ToSingle(PlayerPrefs.GetInt("globalTimerValue"));
        DisplayTime(timeRemaining);
    }


    //Работа с игровым процессом

    public void InitGame()
    {
        socketInteractors = GraphPlaces.GetComponentsInChildren<XRSocketInteractor>();
        StopGame();
        LoadDatabase("Assets/Resources/gr_database/Database.txt");
        LoadTextures();

        foreach (XRSocketInteractor socketInteractor in socketInteractors)
        {
            socketInteractor.onSelectEntered.AddListener(interactable => OnTileAddedToSocket(interactable));
            socketInteractor.onSelectExited.AddListener(interactable => OnTileRemovedFromSocket(interactable));
        }

        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        timeRemaining = Convert.ToSingle(PlayerPrefs.GetInt("globalTimerValue"));
        isRunning = true;
        score = 0;
        HierComplete = 0;
        endedWithTimer = false;
        LoadHier();
        UpdateTileField(true);
        UpdateScore();

        mainHierField.SetActive(true);


        AudioManager.Instance.PlaySFX("StartRun");
    }

    public void PauseGame()
    {
        PauseTimer();
        isPaused = true;

        pauseMenu.SetActive(true);
        gameView.SetActive(false);
        hieroglyphField.SetActive(false);
        GraphPlaces.SetActive(false);

        List<GameObject> createdTilesCopy = new List<GameObject>(createdTiles);
        List<GameObject> socketedTilesCopy = new List<GameObject>(socketedTiles);

        foreach (GameObject tile in createdTilesCopy)
        {
            if (tile != null)
            {
                tile.gameObject.SetActive(false);
            }
        }

        foreach (GameObject tile in socketedTilesCopy)
        {
            if (tile != null)
            {
                tile.gameObject.SetActive(false);
            }
        }

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (isRunning) { ResumeTimer(); }

        if (!isRunning) { ResetTimer(); }

        isPaused = false;

        pauseMenu.SetActive(false);
        gameView.SetActive(true);
        hieroglyphField.SetActive(true);
        GraphPlaces.SetActive(true);

        foreach (GameObject tile in createdTiles)
        {
            if (tile != null)
            {
                tile.gameObject.SetActive(true);
            }
        }

        foreach (GameObject tile in socketedTiles)
        {
            if (tile != null)
            {
                tile.gameObject.SetActive(true);
            }
        }

        Time.timeScale = 1f;
    }

    public void StopGame()
    {
        isRunning = false;
        isPaused = false;

        if (PlayerPrefs.HasKey("SavedTime"))
        {
            PlayerPrefs.DeleteKey("SavedTime");
        }

        ResetTimer();

        resetTimerButton.interactable = true;
        addTimerButton.interactable = true;
        subTimerButton.interactable = true;
        difficultDropdown.interactable = true;

        startRunButton.SetActive(true);
        resetRunButton.SetActive(false);
        mainHierField.SetActive(false);

        DestroyAllTiles();

        if (endedWithTimer)
        {
            RecordAttempt(GetTime(Convert.ToSingle(PlayerPrefs.GetInt("globalTimerValue"))), HierComplete, score);
        }
    }

    void RecordAttempt(string timeTaken, int hieroglyphsCompleted, int score)
    {
        AttemptData attemptData = new AttemptData
        {
            TimeTaken = timeTaken,
            HieroglyphsCompleted = hieroglyphsCompleted,
            Score = score
        };

        string filePath = "attempt.json";
        AttemptDataContainer attemptDataContainer;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            attemptDataContainer = JsonUtility.FromJson<AttemptDataContainer>(json);
        }
        else
        {
            attemptDataContainer = new AttemptDataContainer();
            attemptDataContainer.attempts = new List<AttemptData>();
        }

        attemptDataContainer.attempts.Add(attemptData);

        string updatedJson = JsonUtility.ToJson(attemptDataContainer);

        File.WriteAllText(filePath, updatedJson);
        UnityEngine.Debug.Log("Прошла запись в json");
    }

    string GetTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    //Отображение времени
    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    //Работа с тайлами
    void LoadTextures()
    {
        string folderPath = Path.Combine("Assets/Resources", imagesFolderPath);
        if (Directory.Exists(folderPath))
        {
            string[] imagePaths = Directory.GetFiles(folderPath, "*.png");
            textures = new Texture2D[imagePaths.Length];
            for (int i = 0; i < imagePaths.Length; i++)
            {
                byte[] fileData = File.ReadAllBytes(imagePaths[i]);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                string fileName = Path.GetFileNameWithoutExtension(imagePaths[i]);
                texture.name = fileName;
                textures[i] = texture;
            }
        }
    }

    private List<string> GetSocketedGraphemes()
    {
        return socketInteractors
            .Where(socketInteractor => socketInteractor.selectTarget != null)
            .Select(socketInteractor => socketInteractor.selectTarget.gameObject.name)
            .ToList();
    }

    public List<string> GetRelatedTiles(List<string> targetTiles)
    {
        List<string> relatedTiles = new List<string>();

        if (targetTiles != null && targetTiles.Count != 0)
        {
            foreach (var item in database.Values)
            {
                List<string> tmpItem = new List<string>(item);
                List<string> tmpTarget = new List<string>(targetTiles);

                for (int i = tmpItem.Count - 1; i >= 0; i--)
                {
                    for (int j = tmpTarget.Count - 1; j >= 0; j--)
                    {
                        if (tmpItem[i] == tmpTarget[j])
                        {
                            tmpItem.RemoveAt(i);
                            tmpTarget.RemoveAt(j);
                            break;
                        }
                    }
                }
                if (tmpTarget.Count == 0)
                {
                    relatedTiles.AddRange(tmpItem);
                }
            }
            relatedTiles = relatedTiles.Distinct().ToList();
            relatedTiles.Sort(new NumericStringComparer());
            return relatedTiles;
        }
        else
        {
            for (int i = 1; i <= 444; i++)
            {
                relatedTiles.Add(i.ToString("000"));
            }
        }
        return relatedTiles;
    }

    private void CreateTile(List<string> tileArray)
    {
        float startX = -1.50f;
        float endX = 1.70f;
        float startY = 2.05f;
        float endY = 0.05f;
        int numTilesX = 30;
        int numTilesY = 15;
        float tileSizeX = (endX - startX) / numTilesX;
        float tileSizeY = (startY - endY) / numTilesY;

        int index = 0;

        for (int i = 0; i < numTilesY; i++)
        {
            for (int j = 0; j < numTilesX; j++)
            {
                if (index < tileArray.Count)
                {
                    float x = startX + j * tileSizeX;
                    float y = startY - i * tileSizeY;

                    GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 4.3f), Quaternion.identity);
                    createdTiles.Add(tile);
                    tile.AddComponent<ReturnToStartPosition>();
                    RawImage rawImage = tile.GetComponentInChildren<RawImage>();

                    int textureIndex = int.Parse(tileArray[index]) - 1;
                    rawImage.texture = textures[textureIndex];
                    tile.name = tileArray[index];
                    index++;
                }
                else
                {
                    break;
                }
            }
        }
    }

    private void DestroyFieldTiles()
    {
        List<GameObject> createdTilesCopy = new List<GameObject>(createdTiles);

        foreach (GameObject tile in createdTilesCopy)
        {
            Destroy(tile);
        }
        createdTiles.Clear();
    }

    private void DestroyAllTiles()
    {
        List<GameObject> createdTilesCopy = new List<GameObject>(createdTiles);
        List<GameObject> socketedTilesCopy = new List<GameObject>(socketedTiles);

        foreach (GameObject tile in createdTilesCopy)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
        createdTiles.Clear();

        foreach (GameObject tile in socketedTilesCopy)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
        socketedTiles.Clear();
    }

    private void UpdateTileField(bool isNewHier)
    {
        if (isNewHier)
        {
            DestroyAllTiles();
        }
        else 
        {
            DestroyFieldTiles();
        }
        CreateTile(GetRelatedTiles(GetSocketedGraphemes()));
    }


    private void OnTileAddedToSocket(XRBaseInteractable interactable)
    {
        GameObject addedTile = interactable.gameObject;
        socketedTiles.Add(addedTile);
        createdTiles.Remove(addedTile);
        UpdateTileField(false);
    }

    private void OnTileRemovedFromSocket(XRBaseInteractable interactable)
    {
        GameObject removedTile = interactable.gameObject;
        if (removedTile != null && socketedTiles.Contains(removedTile))
        {
            removedTile.name += "_toRm";

            // Поиск объектов в сцене по названию и удаление их
            GameObject[] tilesToRemove = GameObject.FindGameObjectsWithTag("Tile");
            foreach (GameObject tile in tilesToRemove)
            {
                if (tile.name.Contains("_toRm"))
                {
                    Destroy(tile);
                }
            }
            UpdateTileField(false);
        }
    }
    //Остальное
    public void ReturnTimeScale()
    {
        Time.timeScale = 1f;
    }

    public class NumericStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int numX = int.Parse(x);
            int numY = int.Parse(y);
            return numX.CompareTo(numY);
        }
    }
}