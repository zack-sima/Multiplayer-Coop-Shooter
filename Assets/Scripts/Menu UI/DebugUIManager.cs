
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CSV.Parsers;
using System;

public class DebugUIManager : MonoBehaviour {

    #region SerializeFields

    public static DebugUIManager instance;

    [Header("Debug Console")]
    [SerializeField] public GameObject debugScreen;
    [SerializeField] public TMP_InputField debugInputField;
    [SerializeField] public TMP_Text debugConsoleText;

    #endregion
    #region Members
#if UNITY_EDITOR
    const bool allowDebugMenu = true; // enables/disables the usage of the debug menu.
#else
    const bool allowDebugMenu = true;
#endif


    private FileSystemNode root, currentDirectory;

    private Dictionary<string, System.Action<string[]>> commands = new Dictionary<string, System.Action<string[]>>();
    private Queue<string> commandHistory = new Queue<string>();
    private List<string> commandHistoryList = new List<string>();

    private int historyIndex = -1;
    private const string CommandHistoryKey = "CommandHistory";
    private const int MaxCommandHistory = 50; // Maximum number of commands to store

    private bool isWelcomeScreen = true;
    bool firstInit = false;

    private string GetWelcomeScreen() {
        return @"
>>===============| TERMINAL |==============<< 
                
    Type 'help' for available commands.   

>>===============| TERMINAL |==============<< 
    



";

    }

    #endregion

    #region //*==| METHODS |==*//

    public void InterpretDebugText() {
        if (isWelcomeScreen) {
            ClearConsoleCommand(null);
            debugConsoleText.alignment = TextAlignmentOptions.Left;
            isWelcomeScreen = false;
        }
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

    public void LogWarning(string error, string debugId) {
        LogOutput(debugId + " : " + error);
        Debug.LogWarning(debugId + " : " + error);
    }

    public void LogError(string error, string debugId) {
        LogOutput(debugId + " : " + error);
        Debug.LogError(debugId + " : " + error);
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
       if (allowDebugMenu) debugScreen.SetActive(!debugScreen.activeSelf);
    }

    public void LogOutput(string output) {
        debugConsoleText.text += output + "\n";
    }

    public void LogOutput(string output, float progress) {
        string progressBar = GenerateProgressBar(progress);
        LogOutput(output + " " + progressBar + "\n");
    }

    //TODO: Make progress bar stay in one spot.
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

    private void NullDirectory() {
        LogOutput("Null directory.\n");
    }

    private void CdCommand(string[] args) {
        if (root == null) { NullDirectory(); return; }
        if (args.Length < 2) {
            LogOutput("Usage: cd <directory>");
            return;
        }
        string result = ChangeDirectory(args[1]);
        LogOutput(result);
    }

    private void LsCommand(string[] args) {
        if (root == null) { NullDirectory(); return; }
        string result = ListContents();
        LogOutput(result);
    }

    private void CatCommand(string[] args) {
        if (root == null) { NullDirectory(); return; }
        if (args.Length < 2) {
            LogOutput("Usage: cat <file>");
            return;
        }

        string pattern = args[1];

        // Check if pattern contains a wildcard
        if (pattern.Contains('*')) {
            List<string> matchedFiles = new List<string>();

            // Match all files in the current directory against the pattern
            foreach (var child in currentDirectory.Children.Values) {
                if (!child.IsDirectory && WildcardMatcher.IsMatch(child.Name, pattern)) {
                    matchedFiles.Add(child.Name);
                }
            }

            if (matchedFiles.Count == 0) {
                LogOutput($"No files matching pattern: {pattern}");
            } else {
                foreach (string fileName in matchedFiles) {
                    string result = ViewFileContent(fileName);
                    LogOutput($"Content of {fileName}:\n{result}");
                }
            }
        } else {
            // If no wildcard, just view the file content
            string result = ViewFileContent(pattern);
            LogOutput(result);
        }
        // string result = ViewFileContent(args[1]);
        // LogOutput(result);
    }

    private void ResetCommand(string[] args) {
        if (args.Length < 2) {
            LogOutput("Usage: reset <type>\n Types: playerData, consoleFiles, *");
            return;
        }
        switch (args[1]) {
            case "playerData":
                ForcePlayerDataResetCommand(args);
                break;
            case "consoleFiles":
                ForceResetConsoleFilesCommand(args);
                break;
            case "equipped":
                ForceResetEquippedCommand(args);
                break;
            case "*":
                LogOutput("Resetting all data...");
                ForceResetEquippedCommand(args);
                ForcePlayerDataResetCommand(args);
                ForceResetConsoleFilesCommand(args);
                break;
            default:
                LogOutput("Invalid reset type.");
                break;
        }
    }

    private void ForceResetEquippedCommand(string[] args) {
        //TODO: Reset equipped items.
        //LogOutput("Equipped items succesfully reset.");
    }

    private void ForceResetConsoleFilesCommand(string[] args) {
        InitFileSystem();
        LogOutput("Console files succesfully reset.");
    }

    private void ForcePlayerDataResetCommand(string[] args) {
        if (PlayerDataHandler.instance.ForceResetInfos(isDebug: true))
            LogOutput("All infos succesfully force reset.");
        else { 
            LogOutput("Failed to force reset.");
            Debug.LogError("Failed to force reset.");
        }
    }

    private void HelpCommand(string[] args) {
        LogOutput("Available commands:");
        foreach (var cmd in commands.Keys) {
            LogOutput(cmd);
        }
    }

    private static class WildcardMatcher {
        public static bool IsMatch(string text, string pattern) {
            // Escape regex special characters in the pattern, replace * with .*
            string regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(text, regexPattern);
        }
    }

    #region Command Save and Loading

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

    #endregion

    #region File System

    private class FileSystemNode {
        public string Name { get; private set; }
        public bool IsDirectory { get; private set; }
        public string Content { get; set; }
        public Dictionary<string, FileSystemNode> Children { get; private set; }

        public FileSystemNode(string name, bool isDirectory) {
            Name = name;
            IsDirectory = isDirectory;
            Children = new Dictionary<string, FileSystemNode>();
        }

        public void AddChild(FileSystemNode node) {
            if (IsDirectory) {
                Children[node.Name] = node;
            }
        }

        public FileSystemNode GetChild(string name) {
            return Children.ContainsKey(name) ? Children[name] : null;
        }
    }

    private string ChangeDirectory(string dirName) {
        if (dirName == "..") {
            if (currentDirectory == root) {
                return "Already at root directory.";
            }
            var parent = FindParent(root, currentDirectory);
            if (parent != null) {
                currentDirectory = parent;
                return $"Changed to directory: {currentDirectory.Name}";
            }
        } else if (dirName == "/" || dirName.ToLower() == "root") {
            currentDirectory = root;
            return "Changed to directory: root";
        } else {
            var dir = currentDirectory.GetChild(dirName);
            if (dir != null && dir.IsDirectory) {
                currentDirectory = dir;
                return $"Changed to directory: {currentDirectory.Name}";
            }
            return $"Directory not found: {dirName}";
        }
        return "Error changing directory.";
    }

    private string ListContents() {
        string contents = $"Listing contents of directory: {currentDirectory.Name}\n";
        foreach (var child in currentDirectory.Children.Values) {
            contents += $"{(child.IsDirectory ? "DIR" : "FILE")} - {child.Name}\n";
        }
        return contents;
    }

    private string ViewFileContent(string fileName) {
        var file = currentDirectory.GetChild(fileName);
        if (file != null && !file.IsDirectory) {
            return file.Content;
        }
        return $"File not found: {fileName}";
    }

    private FileSystemNode FindParent(FileSystemNode node, FileSystemNode target) {
        foreach (var child in node.Children.Values) {
            if (child == target) {
                return node;
            }

            if (child.IsDirectory) {
                var result = FindParent(child, target);
                if (result != null) {
                    return result;
                }
            }
        }
        return null;
    }


    private void InitFileSystem() {
        root = new FileSystemNode("root", true);
        currentDirectory = root;

        var actives = new FileSystemNode("actives", true);
        var gadgets = new FileSystemNode("gadgets", true);
        var hulls = new FileSystemNode("hulls", true);
        var turrets = new FileSystemNode("turrets", true);

        var equip = new FileSystemNode("equipped", true);

        root.AddChild(actives);
        root.AddChild(hulls);
        root.AddChild(gadgets);
        root.AddChild(turrets);
        root.AddChild(equip);

        string enums = "CSV Modifiers:\n";
        foreach(string s in Enum.GetNames(typeof(CSVMd))) {
            enums += s + "\n";
        }
        enums += "\nCSV Ids:\n";
        foreach(string s in Enum.GetNames(typeof(CSVId))) {
            enums += s + "\n";
        }
        
        var csvEnums = new FileSystemNode("csvenums.txt", false) { Content = enums };
        root.AddChild(csvEnums);

        var verison = new FileSystemNode("version.txt", false) { Content = "0.0.1" };
        root.AddChild(verison);

        Dictionary<string, InventoryInfo> abilities1 = new(), gadgets1 = new(), hulls1 = new(), turrets1 = new();

        if (abilities1.TryParse(PlayerDataHandler.instance.GetActiveRawCSV(), debugId: "Abilities"))  {
            foreach(InventoryInfo i in abilities1.Values) {
                var ability = new FileSystemNode($"{i.id}.txt", false) { Content = i.ToString() };
                actives.AddChild(ability);
            }
        }

        if (gadgets1.TryParse(PlayerDataHandler.instance.GetGadgetRawCSV())) {
            foreach(InventoryInfo i in gadgets1.Values) {
                var gadget = new FileSystemNode($"{i.id}.txt", false) { Content = i.ToString() };
                gadgets.AddChild(gadget);
            }
        }

        if (hulls1.TryParse(PlayerDataHandler.instance.GetHullRawCSV())) {
            foreach(InventoryInfo i in hulls1.Values) {
                var hull = new FileSystemNode($"{i.id}.txt", false) { Content = i.ToString() };
                hulls.AddChild(hull);
            }
        }

        if (turrets1.TryParse(PlayerDataHandler.instance.GetTurretRawCSV(), debugId: "Turrets")) {
            foreach(InventoryInfo i in turrets1.Values) {
                var turret = new FileSystemNode($"{i.id}.txt", false) { Content = i.ToString() };
                turrets.AddChild(turret);
            }
        }

        Dictionary<string, Dictionary<string, int>> equipped = PlayerDataHandler.instance?.GetEquippedInfos();
        if (equipped != null) {

            if (equipped.ContainsKey(nameof(CSVType.ACTIVES))) {
                var Eactives = new FileSystemNode("Eactives", true);
                foreach(KeyValuePair<string, int> e in equipped[nameof(CSVType.ACTIVES)]) {
                    if (abilities1.ContainsKey(e.Key)) {
                        var ability = new FileSystemNode($"{e.Key}.txt", false) { Content = abilities1[e.Key].ToString() };
                        Eactives.AddChild(ability);
                    }
                }
                equip.AddChild(Eactives);
            }

            if (equipped.ContainsKey(nameof(CSVType.GADGETS))) {
                var Egadgets = new FileSystemNode("Egadgets", true);
                foreach(KeyValuePair<string, int> e in equipped[nameof(CSVType.GADGETS)]) {
                    if (gadgets1.ContainsKey(e.Key)) {
                        var gadget = new FileSystemNode($"{e.Key}.txt", false) { Content = gadgets1[e.Key].ToString() };
                        Egadgets.AddChild(gadget);
                    }
                }
                equip.AddChild(Egadgets);
            }

            if (equipped.ContainsKey(nameof(CSVType.HULLS))) {
                var Ehulls = new FileSystemNode("Ehulls", true);
                foreach(KeyValuePair<string, int> e in equipped[nameof(CSVType.HULLS)]) {
                    if (hulls1.ContainsKey(e.Key)) {
                        var hull = new FileSystemNode($"{e.Key}.txt", false) { Content = hulls1[e.Key].ToString() };
                        Ehulls.AddChild(hull);
                    }
                }
                equip.AddChild(Ehulls);
            }

            if (equipped.ContainsKey(nameof(CSVType.TURRETS))) {
                var Eturrets = new FileSystemNode("Eturrets", true);
                foreach(KeyValuePair<string, int> e in equipped[nameof(CSVType.TURRETS)]) {
                    if (turrets1.ContainsKey(e.Key)) {
                        var turret = new FileSystemNode($"{e.Key}.txt", false) { Content = turrets1[e.Key].ToString() };
                        Eturrets.AddChild(turret);
                    }
                }
                equip.AddChild(Eturrets);
            }
        
        }

    }

    #endregion

    private void InitRegisterCommands() {
        // Register commands
        RegisterCommand("help", HelpCommand);
        //RegisterCommand("--equip", EquipCommand);
        
        RegisterCommand("clear", ClearConsoleCommand);
        RegisterCommand("cd", CdCommand);
        RegisterCommand("ls", LsCommand);
        RegisterCommand("cat", CatCommand);

        RegisterCommand("reset", ResetCommand);
        //RegisterCommand("money", GiveMoneyCommand);
        //Force upgrade

        debugConsoleText.text = "";
        LoadCommandHistory();
        debugScreen.SetActive(false);
    }

    #endregion

    #region //*==| AWAKE & UPDATE |==*//

    private void Awake() {
        if (!allowDebugMenu) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        InitRegisterCommands();
    }

    private void Start() {
        
    }

    private void Update() {
        if (((Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.D)) || Input.GetKeyDown(KeyCode.BackQuote)) && allowDebugMenu) {
            if (!debugScreen.activeInHierarchy) {
                if (!firstInit) {
                    firstInit = true;
                    debugConsoleText.alignment = TextAlignmentOptions.Center;
                    LogOutput(GetWelcomeScreen());
                    InitFileSystem();
                }
            }
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
        if (debugScreen.activeInHierarchy) {
            debugInputField.ActivateInputField();
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