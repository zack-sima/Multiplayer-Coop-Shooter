using System.Collections;
using System.Collections.Generic;
using Abilities;
using UnityEngine;
using TMPro;

public class DebugUIManager : MonoBehaviour {

    #region References

    public static DebugUIManager instance;

    [Header("Debug Console")]
    [SerializeField] public GameObject debugScreen;
    [SerializeField] public TMP_InputField debugInputField;
    [SerializeField] public TMP_Text debugConsoleText;

    #endregion
    #region Members

    private Dictionary<string, System.Action<string[]>> commands = new Dictionary<string, System.Action<string[]>>();
    private Queue<string> commandHistory = new Queue<string>();
    private List<string> commandHistoryList = new List<string>();
    private int historyIndex = -1;
    private const string CommandHistoryKey = "CommandHistory";
    private const int MaxCommandHistory = 50; // Maximum number of commands to store

    #endregion

    #region //*==| METHODS |==*//

    public void InterpretDebugText() {
        string input = debugInputField.GetComponent<TMP_InputField>().text;
        debugInputField.GetComponent<TMP_InputField>().text = "";
        //debugInputText.text = "";

        if (!string.IsNullOrEmpty(input)) {

            if (commandHistory.Count >= MaxCommandHistory) {
                commandHistory.Dequeue();
            }
            commandHistory.Enqueue(input);
            commandHistoryList = new List<string>(commandHistory);
            SaveCommandHistory();
            historyIndex = commandHistoryList.Count;

            string[] commands = input.Split('&');
            foreach (string commandInput in commands) {
                string trimmedCommand = commandInput.Trim();
                if (!string.IsNullOrEmpty(trimmedCommand)) {
                    ExecuteCommand(trimmedCommand);
                }
            }
        }
    }

    private void ExecuteCommand(string input) {
        string[] args = input.Split(' ');
        string command = args[0];
        if (commands.ContainsKey(command)) {
            commands[command].Invoke(args);
        } else {
            LogOutput($"Unknown command: {command}");
        }
    }

    public void ToggleDebugMenu() {
        debugScreen.SetActive(!debugScreen.activeSelf);
    }

    public void LogOutput(string output) {
        debugConsoleText.text += output + "\n";
    }

    public void LogOutput(string output, float progress) {
        string progressBar = GenerateProgressBar(progress);
        debugConsoleText.text += output + " " + progressBar + "\n";
    }

    private string GenerateProgressBar(float progress, int length = 20) {
        int filledLength = Mathf.RoundToInt(length * progress);
        string bar = new string('#', filledLength) + new string('-', length - filledLength);
        return $"[{bar}] {progress * 100:0.0}%";
    }  


    private void RegisterCommand(string command, System.Action<string[]> action) {
        commands.Add(command, action);
    }

    private void GiveMoneyCommand(string[] args) {
        if (args.Length < 2) {
            LogOutput("Usage: giveMoney <amount>");
            return;
        }
        if (int.TryParse(args[1], out int amount)) {
            // TODO: Here you can add the logic to give money to the player.
            LogOutput($"Gave {amount} money.");
        } else {
            LogOutput("Invalid amount value.");
        }
    }   

    private void ClearConsoleCommand(string[] args) {
        debugConsoleText.text = "";
    }

    private void ForceResetCommand(string[] args) {
        if (PlayerDataHandler.instance.ForceResetInfos(isDebug: true))
            LogOutput("All infos succesfully force reset.");
        else { 
            LogOutput("Failed to force reset.");
            Debug.LogError("Failed to force reset.");
        }
    }

    private void HelpCommand(string[] args) {
        foreach (var cmd in commands.Keys) {
            LogOutput(cmd);
        }
        LogOutput("Available commands:");
    }

    private void EchoCommand(string[] args) {
        string message = string.Join(" ", args, 1, args.Length - 1);
        LogOutput(message);
    }

    private void EquipCommand(string[] args) {
        if (args.Length < 2) {
            LogOutput("Usage: -equip <item>");
            return;
        }
        
        string item = args[1];
        LogOutput($"Equipped item: {item}");
        // Here you can add the logic to equip the item to the player.
        // For example, call a method on the player script to equip the item.
    }

    private void SaveCommandHistory() {
        PlayerPrefs.SetInt(CommandHistoryKey + "Count", commandHistory.Count);
        int index = 0;
        foreach (var command in commandHistory) {
            PlayerPrefs.SetString(CommandHistoryKey + index, command);
            index++;
        }
    }

    private void LoadCommandHistory() {
        int count = PlayerPrefs.GetInt(CommandHistoryKey + "Count", 0);
        for (int i = 0; i < count; i++) {
            string command = PlayerPrefs.GetString(CommandHistoryKey + i, "");
            if (!string.IsNullOrEmpty(command)) {
                commandHistory.Enqueue(command);
            }
        }
        commandHistoryList = new List<string>(commandHistory);
        historyIndex = commandHistoryList.Count;
    }

    private void ShowPreviousCommand() {
        if (historyIndex > 0) {
            historyIndex--;
            debugInputField.GetComponent<TMP_InputField>().text = commandHistoryList[historyIndex];
            debugInputField.GetComponent<TMP_InputField>().MoveTextEnd(false); // Move the caret to the end
        }
    }

    private void ShowNextCommand() {
        if (historyIndex < commandHistoryList.Count - 1) {
            historyIndex++;
            debugInputField.GetComponent<TMP_InputField>().text = commandHistoryList[historyIndex];
            debugInputField.GetComponent<TMP_InputField>().MoveTextEnd(false); // Move the caret to the end
        } else {
            debugInputField.GetComponent<TMP_InputField>().text = "";
            historyIndex = commandHistoryList.Count;
        }
    }

    private void InitDebugMenu() {
        // Register commands
        RegisterCommand("help", HelpCommand);
        RegisterCommand("echo", EchoCommand);
        //RegisterCommand("--equip", EquipCommand);
        RegisterCommand("reset", ForceResetCommand);
        RegisterCommand("clear", ClearConsoleCommand);
        //RegisterCommand("money", GiveMoneyCommand);
        //Force upgrade

        debugConsoleText.text = "";
        LoadCommandHistory();
        debugScreen.SetActive(false);
    }

    #endregion

    #region //*==| AWAKE & UPDATE |==*//

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitDebugMenu();
    }

    private void Update() {
        if ((Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.D)) || Input.GetKeyDown(KeyCode.BackQuote)) {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
        if (debugScreen.activeInHierarchy) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                 InterpretDebugText();
            }
            // Navigate command history with up/down arrows
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                ShowPreviousCommand();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                ShowNextCommand();
            }
        }
    }

    #endregion
}