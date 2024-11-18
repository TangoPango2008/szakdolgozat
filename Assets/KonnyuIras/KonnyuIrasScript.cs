using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class KonnyuIrasScript : MonoBehaviour
{
    public GameObject buttonPrefab;
    public RectTransform panel;
    public Vector2 maxButtonSize = new Vector2(100, 100);

    public GameObject centralPlanet;

    public GameObject scoreboardPanel;
    public Text scoreText;
    public Text currentScoreText;
    public Button restartButton;
    public Button backButton;
    public Text timeText;
    public TMP_InputField inputField;
    public Text wrongItemText;
    public static int missedCounter = 0;
    public static float currentTime;
    public Vector2 targetCoordinate = new Vector2(0, 0); // Set your target position here
    public Text statusText;

    public Image Heart1;
    public Image Heart2;
    public Image Heart3;


 
    private List<string> spawnedObjectNames = new List<string>();
    private List<GameObject> buttons = new List<GameObject>();
    private float spawnInterval = 10f;
    private float timeSinceLastSpawn;
    private int currentObjectIndex = 0;
    private int currentScore;
    private bool timerActive;
    private Dictionary<GameObject, float> buttonSpawnTimes = new Dictionary<GameObject, float>();
    private bool missionFailed = false;
    private float moveSpeed = 30f;



    // Scoreboard variables
    private Dictionary<string, List<float>> playerBestTimes = new Dictionary<string, List<float>>();
    private const string BestTimesFile = "Assets/best_times_easy_write.txt";

    void Start()
    {
        missedCounter = 0;
        missionFailed = false;
        scoreboardPanel.SetActive(false);
        wrongItemText.gameObject.SetActive(false);
        currentScore = 0;
        currentTime = 0;
        timerActive = true;
        statusText.text = "Megmentetted az Írás bolygóját!";



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

        // Load sprites from resources and shuffle them
        Sprite[] findableObjects = Resources.LoadAll<Sprite>("OlvasasKonnyuTargyak");
        List<Sprite> shuffledObjects = findableObjects.OrderBy(x => Random.value).Take(6).ToList();
        spawnedObjectNames = shuffledObjects.Select(sprite => sprite.name).ToList();

        inputField.onEndEdit.AddListener(CheckInput);

        restartButton.onClick.AddListener(RestartGame);
        backButton.onClick.AddListener(BackToPreviousScene);

        LoadBestTimes(); // Load best times at the start

        SpawnNextObject(); // Spawn the first item immediately

        inputField.Select();
        inputField.ActivateInputField();
    }

    void Update()
    {
        if (timerActive)
        {
            currentTime += Time.deltaTime;
            timeText.text = $"{currentTime:F3}";

            // Check if enough time has passed to spawn the next item
            timeSinceLastSpawn += Time.deltaTime;
            if (timeSinceLastSpawn >= spawnInterval || buttons.Count == 0)
            {
                if (currentObjectIndex < spawnedObjectNames.Count)
                {
                    SpawnNextObject();
                    timeSinceLastSpawn = 0;
                }
                else if (buttons.Count == 0) // All items spawned and cleared
                {
                    ShowScoreboard(); // Display the scoreboard
                    timerActive = false; // Stop the timer
                }
            }
        }

        // Move each button towards the target coordinate
        foreach (var button in buttons)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            Vector2 currentPosition = rectTransform.anchoredPosition;

            // Calculate moveSpeed based on initial distance to target and 15 seconds travel time
            float distanceToTarget = Vector2.Distance(currentPosition, targetCoordinate);
            

            // Move asteroid towards the target
            rectTransform.anchoredPosition = Vector2.MoveTowards(currentPosition, targetCoordinate, moveSpeed * Time.deltaTime);
        }

        // Handle object destruction if it reaches the target after 15 seconds
        foreach (var button in new List<GameObject>(buttons))
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            RectTransform planetRect = centralPlanet.GetComponent<RectTransform>();

            // Ha a távolság elég kicsi, eltávolítjuk a gombot
            if (RectTransformUtility.RectangleContainsScreenPoint(planetRect, buttonRect.position))
            {
                Destroy(button);
                buttons.Remove(button);
                missedCounter++;


                if (missedCounter == 1)
                {
                    Heart1.color = Color.gray;
                    if (currentObjectIndex >= spawnedObjectNames.Count && buttons.Count == 0)
                    {
                        ShowScoreboard();
                        timerActive = false;
                        return;

                    }
                }
                else if (missedCounter == 2)
                {
                    Heart2.color = Color.gray;
                    if (currentObjectIndex >= spawnedObjectNames.Count && buttons.Count == 0)
                    {
                        ShowScoreboard();
                        timerActive = false;
                        return;

                    }
                }

                if (missedCounter >= 3)
                {
                    Heart3.color = Color.gray;
                    missionFailed = true;
                    ShowScoreboard();
                    timerActive = false;
                    statusText.text = $"Sajnos nem sikerült megmentened az Írás bolygóját!";
                    
                }
            }
        }
    }


    void SpawnNextObject()
    {
        if (currentObjectIndex >= spawnedObjectNames.Count)
        {
            Debug.LogWarning("No more objects to spawn.");
            return;
        }

        string objectName = spawnedObjectNames[currentObjectIndex];
        Sprite sprite = Resources.Load<Sprite>($"OlvasasKonnyuTargyak/{objectName}");

        if (sprite == null)
        {
            Debug.LogWarning($"Sprite for '{objectName}' not found in Resources.");
            return;
        }

        GameObject newButton = Instantiate(buttonPrefab, panel);
        buttons.Add(newButton);

        buttonSpawnTimes[newButton] = Time.time;

        Image buttonImage = newButton.GetComponent<Image>();
        buttonImage.sprite = sprite;

        // Maintain aspect ratio within maxButtonSize
        RectTransform rectTransform = newButton.GetComponent<RectTransform>();
        float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
        float width = maxButtonSize.x;
        float height = width / aspectRatio;

        if (height > maxButtonSize.y)
        {
            height = maxButtonSize.y;
            width = height * aspectRatio;
        }

        rectTransform.sizeDelta = new Vector2(width, height);

        // Position the button outside the panel in a random direction
        float panelWidth = panel.rect.width;
        float panelHeight = panel.rect.height;
        Vector2 randomPosition;
        int spawnEdge = Random.Range(0, 2); // 0=left, 1=right, 2=top, 3=bottom

        switch (spawnEdge)
        {
            case 0: // Left
                randomPosition = new Vector2(-panelWidth / 2 - width, Random.Range(-panelHeight / 2, panelHeight / 2));
                break;
            case 1: // Right
                randomPosition = new Vector2(panelWidth / 2 + width, Random.Range(-panelHeight / 2, panelHeight / 2));
                break;
            
            default:
                randomPosition = Vector2.zero;
                break;
        }

        rectTransform.anchoredPosition = randomPosition;
        currentObjectIndex++;
    }

    void CheckInput(string inputText)
    {
        // Find any button on the screen that matches the input text
        GameObject matchingButton = buttons.FirstOrDefault(button =>
            button.GetComponent<Image>().sprite.name.Equals(inputText, System.StringComparison.OrdinalIgnoreCase));

        if (matchingButton != null)
        {
            // Remove the matched button and update the score
            Destroy(matchingButton);
            buttons.Remove(matchingButton);
            currentScore++;
            wrongItemText.gameObject.SetActive(false);

            // Check if all items have been cleared
            if (currentObjectIndex >= spawnedObjectNames.Count && buttons.Count == 0)
            {
                ShowScoreboard();
                timerActive = false; // Stop the timer
            }
        }
        else
        {
            wrongItemText.gameObject.SetActive(true);
        }

        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();
    }

    void ShowScoreboard()
    {
        string playerName = StaticData.NameValue.ToUpper(); // Convert player name to uppercase

        if (missionFailed)
        {
            currentScoreText.text = "Sikertelen küldetés!";
            currentScoreText.color = Color.red;
        }
        else
        {
            if (!playerBestTimes.ContainsKey(playerName))
            {
                playerBestTimes[playerName] = new List<float>();
            }
            playerBestTimes[playerName].Add(currentTime);

            playerBestTimes[playerName].Sort();
            if (playerBestTimes[playerName].Count > 10)
            {
                playerBestTimes[playerName].RemoveRange(10, playerBestTimes[playerName].Count - 10);
            }

            SaveBestTimes();

            currentScoreText.text = $"Mostani időd: {currentTime:F3}\n";
        }

        scoreText.text = $"{playerName} legjobb 10 ideje:\n";
        int rank = 1;
        foreach (float time in playerBestTimes[playerName])
        {
            scoreText.text += $"{rank}. {time:F3}\n";
            rank++;
        }

        scoreboardPanel.SetActive(true);
    }

    void LoadBestTimes()
    {
        string persistentFilePath = Path.Combine(Application.persistentDataPath, "best_times_easy_write.txt");

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
            TextAsset textAsset = Resources.Load<TextAsset>("best_times_easy_write");
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
        string persistentFilePath = Path.Combine(Application.persistentDataPath, "best_times_easy_write.txt");
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }
}
