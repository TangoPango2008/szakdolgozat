using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class KonnyuOlvasasScript : MonoBehaviour
{
    public GameObject buttonPrefab;
    public RectTransform panel;
    public Vector2 maxButtonSize = new Vector2(25, 25);
    private StreamReader Reader;
    public Text CurrentObject;

    public GameObject scoreboardPanel;
    public Text scoreText;
    public Text currentscoreText;
    public Button restartButton;
    public Button backButton;
    public Text timeText;

    
    private string[] EasyObjects;
    private List<string> selectedObjects = new List<string>();
    private List<string> spawnedObjectNames = new List<string>();
    private List<GameObject> buttons = new List<GameObject>();
    private List<PlayerScore> bestScores = new List<PlayerScore>();
    private const string BestTimesFile = "Assets/best_times_easy_read.txt";
    private Dictionary<string, List<float>> playerBestTimes = new Dictionary<string, List<float>>();


    private bool TimerActive;
    public static float CurrentTime;
    public Text wrongItem;

    void Start()
    {
        scoreboardPanel.SetActive(false);
        CurrentTime = 0;
        TimerActive = true;

        TextAsset textAsset = Resources.Load<TextAsset>("EasyObjects"); // Load without the .txt extension
        if (textAsset != null)
        {
            string fileContents = textAsset.text; // Get the content of the file
            string[] allObjects = fileContents.Split(';'); // Split by ';' to get individual items

            // Example of handling the split data
            foreach (string obj in allObjects)
            {
                Debug.Log(obj.Trim()); // Use Trim() to remove any extra whitespace
            }
        }
        else
        {
            Debug.LogError("Failed to load EasyObjects.txt from Resources.");
        }

        LoadBestTimes();

        Sprite[] FindableObjects = Resources.LoadAll<Sprite>("OlvasasKonnyuTargyak");

        // Shuffle the array to randomize the objects
        System.Random random = new System.Random();
        FindableObjects = FindableObjects.OrderBy(x => random.Next()).ToArray();

        // Limit the number of objects spawned to 6
        int maxObjects = Mathf.Min(6, FindableObjects.Length);
        for (int i = 0; i < maxObjects; i++)
        {
            Sprite kep = FindableObjects[i];
            spawnedObjectNames.Add(kep.name); // Add the name to the list of spawned objects
            GameObject ujGomb = Instantiate(buttonPrefab, panel);
            buttons.Add(ujGomb);

            Image gombKep = ujGomb.GetComponent<Image>();
            gombKep.sprite = kep;

            RectTransform rectTransform = ujGomb.GetComponent<RectTransform>();
            float aspectRatio = (float)kep.texture.width / kep.texture.height;
            float width = maxButtonSize.x;
            float height = width / aspectRatio;

            if (height > maxButtonSize.y)
            {
                height = maxButtonSize.y;
                width = height * aspectRatio;
            }

            rectTransform.sizeDelta = new Vector2(width, height);

            Vector2 randomPosition = Vector2.zero;
            int maxAttempts = 100; // Maximum attempts to avoid overlap
            int attempt = 0;
            bool positionFound = false;

            while (attempt < maxAttempts && !positionFound)
            {
                float panelWidth = panel.rect.width;
                float panelHeight = panel.rect.height;
                float randomX = UnityEngine.Random.Range(-panelWidth / 2 + width / 2, panelWidth / 2 - width / 2);
                float randomY = UnityEngine.Random.Range(-panelHeight / 2 + height / 2, panelHeight / 2 - height / 2);
                randomPosition = new Vector2(randomX, randomY);

                // Check for collision
                positionFound = true;
                foreach (GameObject button in buttons)
                {
                    if (button == ujGomb) continue;

                    RectTransform otherRectTransform = button.GetComponent<RectTransform>();
                    float distance = Vector2.Distance(randomPosition, otherRectTransform.anchoredPosition);

                    // If the new position is too close to another button, try again
                    if (distance < Mathf.Max(width, height))
                    {
                        positionFound = false;
                        break;
                    }
                }

                attempt++;
            }

            if (positionFound)
            {
                rectTransform.anchoredPosition = randomPosition;
            }
            else
            {
                // Use the last position as fallback
                rectTransform.anchoredPosition = randomPosition;
            }

            ujGomb.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnButtonClick(kep.name, ujGomb);
            });
        }

        SelectRandomObject();
        restartButton.onClick.AddListener(RestartGame);
        backButton.onClick.AddListener(BackToPreviousScene);
    }

    void SelectRandomObject()
    {
        if (selectedObjects.Count < spawnedObjectNames.Count)
        {
            string randomObject;
            do
            {
                int randomIndex = UnityEngine.Random.Range(0, spawnedObjectNames.Count);
                randomObject = spawnedObjectNames[randomIndex];
            } while (selectedObjects.Contains(randomObject));

            selectedObjects.Add(randomObject);
            CurrentObject.text = randomObject;
        }
        else
        {
            TimerActive = false;
            ShowScoreboard();
        }
    }

    void OnButtonClick(string gombNev, GameObject ujGomb)
    {
        if (gombNev == CurrentObject.text)
        {
            Destroy(ujGomb);
            SelectRandomObject();
            wrongItem.gameObject.SetActive(false);
        }
        else
        {
            CurrentTime += 1;
            wrongItem.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (TimerActive)
        {
            CurrentTime += Time.deltaTime;
        }

        timeText.text = CurrentTime.ToString("F3");

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quit");
        }
    }


    void ShowScoreboard()
    {
        string playerName = StaticData.NameValue.ToUpper(); // Convert player name to uppercase

        // Add the new time to the player's list
        if (!playerBestTimes.ContainsKey(playerName))
        {
            playerBestTimes[playerName] = new List<float>();
        }
        playerBestTimes[playerName].Add(CurrentTime);

        // Keep only the best 10 times
        playerBestTimes[playerName].Sort();
        if (playerBestTimes[playerName].Count > 10)
        {
            playerBestTimes[playerName].RemoveRange(10, playerBestTimes[playerName].Count - 10);
        }

        // Save the times to the file
        SaveBestTimes();

        // Display the scoreboard
        scoreText.text = $"{playerName} legjobb 10 ideje:\n";
        int rank = 1;
        foreach (float time in playerBestTimes[playerName])
        {
            scoreText.text += $"{rank}. {time:F3}\n";
            rank++;
        }

        currentscoreText.text = $"Mostani időd: {CurrentTime:F3}\n";

        scoreboardPanel.SetActive(true);
    }

    void LoadBestTimes()
    {
        string persistentFilePath = Path.Combine(Application.persistentDataPath, "best_times_easy_read.txt");

        // Check if the file exists in persistent data path
        if (File.Exists(persistentFilePath))
        {
            // Load from persistentDataPath if it exists
            string[] lines = File.ReadAllLines(persistentFilePath);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2 && float.TryParse(parts[1], out float time))
                {
                    string playerName = parts[0].ToUpper();
                    if (!playerBestTimes.ContainsKey(playerName))
                    {
                        playerBestTimes[playerName] = new List<float>();
                    }
                    playerBestTimes[playerName].Add(time);
                }
            }
        }
        else
        {
            // If no file in persistentDataPath, try loading from Resources as a default
            TextAsset textAsset = Resources.Load<TextAsset>("best_times_easy_read");
            if (textAsset != null)
            {
                string[] lines = textAsset.text.Split('\n');
                foreach (var line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && float.TryParse(parts[1], out float time))
                    {
                        string playerName = parts[0].ToUpper();
                        if (!playerBestTimes.ContainsKey(playerName))
                        {
                            playerBestTimes[playerName] = new List<float>();
                        }
                        playerBestTimes[playerName].Add(time);
                    }
                }
            }
        }

        // Keep only the best 10 times for each player
        foreach (var playerTimes in playerBestTimes.Values)
        {
            playerTimes.Sort();
            if (playerTimes.Count > 10)
            {
                playerTimes.RemoveRange(10, playerTimes.Count - 10);
            }
        }
    }


    void SaveBestTimes()
    {
        string persistentFilePath = Path.Combine(Application.persistentDataPath, "best_times_easy_read.txt");
        var lines = new List<string>();

        foreach (var entry in playerBestTimes)
        {
            string playerName = entry.Key.ToUpper(); // Ensure saved player names are in uppercase
            foreach (float time in entry.Value)
            {
                lines.Add($"{playerName},{time:F3}");
            }
        }

        File.WriteAllLines(persistentFilePath, lines); // Save to persistentDataPath
    }



    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void BackToPreviousScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
}
