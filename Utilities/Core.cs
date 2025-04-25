using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

[assembly: MelonInfo(typeof(Utilities.Core), "Utilities Addon", "222.0.0", "dynaslash & TuanAnh2901", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace Utilities
{
    public class Core : MelonMod
    {

        private static DateTime dtStart;
        private static DateTime? dtStartToast;
        private static string toast_txt;
        public static bool isSeedRain = false;
        public static bool isScaredyDream = false;
        public static bool isTest = false;
        public static bool PlantUpgrade { get; set; } = true;

        // Command chat variables
        private bool showCommandChat = false;
        private string commandInput = "";
        private readonly KeyCode commandKey = KeyCode.L;
        private readonly float commandBoxWidth = 300f;
        private readonly float commandBoxHeight = 400f; // Taller vertical box
        private readonly List<string> commandHistory = new();
        private readonly int maxHistoryLines = 15; // More history lines
        private readonly DateTime lastCommandTime = DateTime.MinValue;
        private GUIStyle commandStyle;
        private GUIStyle historyStyle;
        private GUIStyle inputStyle;
        private GUIStyle titleStyle;
        private GUIStyle dragHandleStyle;
        private readonly float inputDelay = 0.1f; // Delay to prevent 'l' character
        private float inputDelayTimer = 0f;

        // Variables for draggable window
        private Vector2 commandBoxPosition; // Will be initialized in OnInitializeMelon
        private bool isDragging = false;
        private Vector2 dragOffset;

        // Variables for command paging and repetition
        private int currentCommandPage = 0;
        private readonly int commandsPerPage = 10;
        private int commandRepeatCount = 1; // Default to running commands once

        public override void OnEarlyInitializeMelon() => dtStart = DateTime.Now;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Utilities Addon is loaded!");

            // Set default position to bottom left
            commandBoxPosition = new Vector2(10f, Screen.height - commandBoxHeight - 10f);

            // Initialize command chat styles
            commandStyle = new GUIStyle
            {
                fontSize = 16,
                normal = { textColor = Color.white }
            };

            historyStyle = new GUIStyle
            {
                fontSize = 14,
                normal = { textColor = new Color(1f, 1f, 1f, 0.8f) },
                alignment = TextAnchor.UpperLeft
            };

            inputStyle = new GUIStyle
            {
                fontSize = 16,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft
            };

            dragHandleStyle = new GUIStyle
            {
                fontSize = 14,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };



            titleStyle = new GUIStyle
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };
        }

        public override void OnLateInitializeMelon() => dtStart = DateTime.Now;

        public override void OnLateUpdate()
        {
            Utility.OnLateUpdate();

            // Toggle command chat with L key
            if (Input.GetKeyDown(commandKey) && !showCommandChat)
            {
                showCommandChat = true;
                commandInput = "";
                inputDelayTimer = Time.time + inputDelay; // Set delay timer

                // Print all commands to console when opening
                MelonLogger.Msg("===== AVAILABLE COMMANDS =====");
                string[] allCommands = {
                    "0 - Generate Trophy",
                    "1 - Generate Fertilizer",
                    "2 - Generate Bucket",
                    "3 - Generate Helmet",
                    "4 - Generate Jack-in-the-Box",
                    "5 - Generate Pickaxe",
                    "6 - Generate Mecha Fragment",
                    "7 - Generate Giga Mecha",
                    "8 - Generate Meteor",
                    "9 - Generate Sprout",
                    "10 - Portal Heart",
                    "11 - Solar Star",
                    "12 - Red Iron Head",
                    "13 - Iron Head",
                    "14 - Charm All Zombies",
                    "15 - Kill All Zombies",
                    "16 - Add 1000 to AbyssMoney",
                    "16=X - Set AbyssMoney to X",
                    "help - Show all commands",
                    "- - Next page of commands"
                };
                foreach (string cmd in allCommands)
                {
                    MelonLogger.Msg(cmd);
                }
                MelonLogger.Msg("==============================");
            }

            // Handle command input
            if (showCommandChat)
            {
                // Close chat with Escape
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    showCommandChat = false;
                    commandInput = "";
                }

                // Execute command with Enter
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    // Process the command but don't automatically close the console
                    // The ProcessCommand method will handle closing the console when appropriate
                    string cmd = commandInput.Trim().ToLower();
                    commandInput = "";
                    ProcessCommand(cmd);
                }

                // Check for page change key
                if (Input.GetKeyDown(KeyCode.Minus))
                {
                    // Cycle through pages
                    string[] spawnCommands = {
                        "0 - Generate Trophy",
                        "1 - Generate Fertilizer",
                        "2 - Generate Bucket",
                        "3 - Generate Helmet",
                        "4 - Generate Jack-in-the-Box",
                        "5 - Generate Pickaxe",
                        "6 - Generate Mecha Fragment",
                        "7 - Generate Giga Mecha",
                        "8 - Generate Meteor",
                        "9 - Generate Sprout",
                        "10 - Portal Heart",
                        "11 - Solar Star",
                        "12 - Red Iron Head",
                        "13 - Iron Head",
                        "14 - Charm All Zombies",
                        "15 - Kill All Zombies",
                        "16 - Add 1000 to AbyssMoney",
                        "16=X - Set AbyssMoney to X",
                        "help - Show all commands",
                        "- - Next page of commands"
                    };
                    int totalPages = (int)Math.Ceiling((double)spawnCommands.Length / commandsPerPage);
                    currentCommandPage = (currentCommandPage + 1) % totalPages;
                    ShowToast($"Showing command page {currentCommandPage + 1}/{totalPages}");

                    // Clear the command input so user doesn't have to delete the '-'
                    commandInput = "";
                }

                // Only process input after delay has passed
                if (Time.time > inputDelayTimer)
                {
                    // Handle text input manually
                    foreach (char c in Input.inputString)
                    {
                        // Backspace
                        if (c == '\b')
                        {
                            if (commandInput.Length > 0)
                            {
                                commandInput = commandInput[..^1]; // Simplified substring
                            }
                        }
                        // Tab - ignore
                        else if (c == '\t')
                        {
                            // Do nothing
                        }
                        // Escape - already handled above
                        else if (c == '\u001b')
                        {
                            // Do nothing
                        }
                        // Enter - already handled above
                        else if (c == '\r' || c == '\n')
                        {
                            // Do nothing
                        }
                        // Regular character input
                        else
                        {
                            commandInput += c;
                        }
                    }
                }
            }
        }

        public override void OnGUI()
        {
            if (Utility.GetActive(Utility.UtilityType.ShowUtilities) || DateTime.Now - dtStart < new TimeSpan(0 , 0, 0, 5))
            {
                string text = Utility.GetUtilities();
                int num = 0;
                int num2 = 20;
                foreach (string text2 in text.Split('\n', StringSplitOptions.None))
                {
                    if (text2.Length > num2)
                    {
                        num2 = text2.Length;
                    }
                    num++;
                }
                GUI.Button(new Rect(10f, 30f, num2 * 10f, num * 16f + 15f), text);
            }

            if (dtStartToast != null)
            {
                // Improved toast display with dark background and yellow text
                GUIStyle toastStyle = new GUIStyle()
                {
                    fontSize = 16, // Increased font size
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true,
                    normal = { textColor = Color.yellow },
                    padding = new RectOffset { left = 10, right = 10, top = 5, bottom = 5 }
                };

                // Calculate size based on text length
                float toastWidth = Math.Min(Screen.width - 40, Math.Max(300, toast_txt.Length * 8));
                float toastHeight = 50f; // Taller for better visibility

                // Draw dark background
                GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                GUI.Box(new Rect(10f, 10f, toastWidth, toastHeight), "");
                GUI.color = Color.white;

                // Draw text
                GUI.Label(new Rect(10f, 10f, toastWidth, toastHeight), toast_txt, toastStyle);
                TimeSpan? timeSpan = DateTime.Now - dtStartToast;
                TimeSpan t = new(0, 0, 0, 3); // Increased display time to 3 seconds
                if (timeSpan > t)
                {
                    dtStartToast = null;
                }
            }

            // Draw command chat when active
            if (showCommandChat)
            {
                // Use the draggable position
                float boxX = commandBoxPosition.x;
                float boxY = commandBoxPosition.y;

                // Semi-transparent background
                GUI.color = new Color(0, 0, 0, 0.8f);
                GUI.Box(new Rect(boxX, boxY, commandBoxWidth, commandBoxHeight), "");
                GUI.color = Color.white;

                // Title with drag handle
                GUI.Label(new Rect(boxX, boxY + 5, commandBoxWidth, 30), "COMMAND CONSOLE - [DRAG HERE]", dragHandleStyle);

                // Handle dragging
                Rect dragRect = new Rect(boxX, boxY, commandBoxWidth, 40);
                if (Event.current.type == EventType.MouseDown && dragRect.Contains(Event.current.mousePosition))
                {
                    isDragging = true;
                    dragOffset = Event.current.mousePosition - new Vector2(boxX, boxY);
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    isDragging = false;
                }

                if (isDragging && Event.current.type == EventType.MouseDrag)
                {
                    commandBoxPosition = Event.current.mousePosition - dragOffset;

                    // Keep window within screen bounds
                    commandBoxPosition.x = Mathf.Clamp(commandBoxPosition.x, 0, Screen.width - commandBoxWidth);
                    commandBoxPosition.y = Mathf.Clamp(commandBoxPosition.y, 0, Screen.height - commandBoxHeight);

                    Event.current.Use();
                }

                // Horizontal line
                GUI.color = new Color(1, 1, 1, 0.5f);
                GUI.Box(new Rect(boxX + 10, boxY + 35, commandBoxWidth - 20, 2), "");
                GUI.color = Color.white;

                // Command list header
                GUI.Label(new Rect(boxX + 10, boxY + 45, commandBoxWidth - 20, 20), "Available Commands:", commandStyle);

                // List of spawn commands
                string[] spawnCommands = {
                    "0 - Generate Trophy",
                    "1 - Generate Fertilizer",
                    "2 - Generate Bucket",
                    "3 - Generate Helmet",
                    "4 - Generate Jack-in-the-Box",
                    "5 - Generate Pickaxe",
                    "6 - Generate Mecha Fragment",
                    "7 - Generate Giga Mecha",
                    "8 - Generate Meteor",
                    "9 - Generate Sprout",
                    "10 - Portal Heart",
                    "11 - Solar Star",
                    "12 - Red Iron Head",
                    "13 - Iron Head",
                    "14 - Charm All Zombies",
                    "15 - Kill All Zombies",
                    "16 - Add 1000 to AbyssMoney",
                    "16=X - Set AbyssMoney to X",
                    "help - Show all commands",
                    "- - Next page of commands",
                    "=X - Set repeat count (e.g., =5)"
                };

                // Display commands with paging
                float cmdY = boxY + 70;
                int startIndex = currentCommandPage * commandsPerPage;
                int endIndex = Math.Min(startIndex + commandsPerPage, spawnCommands.Length);

                // Show page indicator and repeat count
                int totalPages = (int)Math.Ceiling((double)spawnCommands.Length / commandsPerPage);
                GUI.Label(new Rect(boxX + 15, cmdY, commandBoxWidth - 30, 20),
                    $"Page {currentCommandPage + 1}/{totalPages} | Repeat: {commandRepeatCount}x", historyStyle);
                cmdY += 25;

                // Show commands for current page
                for (int i = startIndex; i < endIndex; i++)
                {
                    GUI.Label(new Rect(boxX + 15, cmdY, commandBoxWidth - 30, 20), spawnCommands[i], historyStyle);
                    cmdY += 22;
                }

                // Input area at bottom
                float inputY = boxY + commandBoxHeight - 40;

                // Input background
                GUI.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                GUI.Box(new Rect(boxX + 10, inputY, commandBoxWidth - 20, 30), "");
                GUI.color = Color.white;

                // Command prefix
                GUI.Label(new Rect(boxX + 15, inputY + 5, 20, 20), ">", commandStyle);

                // Display the input text (not using TextField)
                GUI.Label(
                    new Rect(boxX + 30, inputY + 5, commandBoxWidth - 45, 20),
                    commandInput + (Time.time % 1 > 0.5f ? "_" : ""), // Add blinking cursor
                    inputStyle
                );
            }
        }

        public static void ShowToast(string message)
        {
            toast_txt = message;
            dtStartToast = new DateTime?(DateTime.Now);
        }

        private void ProcessCommand(string command)
        {
            // Add command to history
            commandHistory.Insert(0, "> " + command);
            if (commandHistory.Count > maxHistoryLines)
            {
                commandHistory.RemoveAt(commandHistory.Count - 1);
            }

            // Flag to determine if we should close the command chat after processing
            bool closeCommandChat = true;

            // Process commands
            switch (command)
            {
                // Special case for setting repeat count if it's just "="
                case "=":
                    ShowToast("Current repeat count: " + commandRepeatCount + ". Use =X to set (e.g., =5)");
                    closeCommandChat = false;
                    break;
                // Spawn commands using numbers 0-9
                case "0":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Board/Award/TrophyPrefab");
                    }
                    ShowToast($"{commandRepeatCount}x Trophy spawned at cursor position");
                    break;

                case "1":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/fertilize/Ferilize");
                    }
                    ShowToast($"{commandRepeatCount}x Fertilizer spawned at cursor position");
                    break;

                case "2":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/Bucket");
                    }
                    ShowToast($"{commandRepeatCount}x Bucket spawned at cursor position");
                    break;

                case "3":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/Helmet");
                    }
                    ShowToast($"{commandRepeatCount}x Helmet spawned at cursor position");
                    break;

                case "4":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/JackBox");
                    }
                    ShowToast($"{commandRepeatCount}x Jack-in-the-Box spawned at cursor position");
                    break;

                case "5":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/Pickaxe");
                    }
                    ShowToast($"{commandRepeatCount}x Pickaxe spawned at cursor position");
                    break;

                case "6":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/Machine");
                    }
                    ShowToast($"{commandRepeatCount}x Mecha Fragment spawned at cursor position");
                    break;

                case "7":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/SuperMachine");
                    }
                    ShowToast($"{commandRepeatCount}x Giga Mecha Fragment spawned at cursor position");
                    break;

                case "8":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Board.Instance.CreateUltimateMateorite();
                    }
                    ShowToast($"{commandRepeatCount}x Meteor spawned at cursor position");
                    break;

                case "9":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/SproutPotPrize/SproutPotPrize");
                    }
                    ShowToast($"{commandRepeatCount}x Sprout spawned at cursor position");
                    break;

                case "10":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/PortalHeart");
                    }
                    ShowToast($"{commandRepeatCount}x Portal Heart spawned at cursor position");
                    break;

                case "11":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/SolarStar");
                    }
                    ShowToast($"{commandRepeatCount}x Solar Star spawned at cursor position");
                    break;

                case "12":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/RedIronHead");
                    }
                    ShowToast($"{commandRepeatCount}x Red Iron Head spawned at cursor position");
                    break;

                case "13":
                    for (int i = 0; i < commandRepeatCount; i++)
                    {
                        Utility.SpawnItem("Items/IronHead");
                    }
                    ShowToast($"{commandRepeatCount}x Iron Head spawned at cursor position");
                    break;

                case "14":
                    // Charm all zombies (SetMindControl)
                    foreach (Zombie zombie in Board.Instance.zombieArray)
                    {
                        if (zombie != null)
                        {
                            zombie.SetMindControl();
                        }
                    }
                    ShowToast("All zombies have been charmed");
                    break;

                case "15":
                    // Kill all zombies
                    foreach (Zombie zombie in Board.Instance.zombieArray)
                    {
                        if (zombie != null && !zombie.isMindControlled)
                        {
                            zombie.Die(1);
                        }
                    }
                    ShowToast("All non-charmed zombies have been killed");
                    break;
                    // Add default value to abyssMoney if no specific value is provided
                    

                case "help":
                    ShowToast("Available commands: 0-13 (Spawn items), 14 (Charm zombies), 15 (Kill zombies), 16 (AbyssMoney), - (Next page), =X (Set repeats)");
                    MelonLogger.Msg("===== AVAILABLE COMMANDS =====");
                    string[] helpCommands = {
                        "0 - Generate Trophy",
                        "1 - Generate Fertilizer",
                        "2 - Generate Bucket",
                        "3 - Generate Helmet",
                        "4 - Generate Jack-in-the-Box",
                        "5 - Generate Pickaxe",
                        "6 - Generate Mecha Fragment",
                        "7 - Generate Giga Mecha",
                        "8 - Generate Meteor",
                        "9 - Generate Sprout",
                        "10 - Portal Heart",
                        "11 - Solar Star",
                        "12 - Red Iron Head",
                        "13 - Iron Head",
                        "14 - Charm All Zombies",
                        "15 - Kill All Zombies",
                        "- - Next page of commands",
                        "=X - Set repeat count (e.g., =5 to run commands 5 times)"
                    };
                    foreach (string cmd in helpCommands)
                    {
                        MelonLogger.Msg(cmd);
                    }
                    MelonLogger.Msg("==============================");
                    break;

                case "-":
                    // Cycle through pages
                    string[] pageCommands = {
                        "0 - Generate Trophy",
                        "1 - Generate Fertilizer",
                        "2 - Generate Bucket",
                        "3 - Generate Helmet",
                        "4 - Generate Jack-in-the-Box",
                        "5 - Generate Pickaxe",
                        "6 - Generate Mecha Fragment",
                        "7 - Generate Giga Mecha",
                        "8 - Generate Meteor",
                        "9 - Generate Sprout",
                        "10 - Portal Heart",
                        "11 - Solar Star",
                        "12 - Red Iron Head",
                        "13 - Iron Head",
                        "14 - Charm All Zombies",
                        "15 - Kill All Zombies",
                        "help - Show all commands",
                        "- - Next page of commands"
                    };
                    int totalPages = (int)Math.Ceiling((double)pageCommands.Length / commandsPerPage);
                    currentCommandPage = (currentCommandPage + 1) % totalPages;
                    ShowToast($"Showing command page {currentCommandPage + 1}/{totalPages}");

                    // Don't close the command chat when switching pages
                    closeCommandChat = false;
                    break;

                default:
                    //// Check if it's a command to set abyssMoney value (16=X format)
                    //if (command.StartsWith("16=") && command.Length > 3)
                    //{
                    //    string numPart = command.Substring(3);
                    //    if (int.TryParse(numPart, out int abyssValue) && abyssValue >= 0)
                    //    {
                    //        Patches.abyssMoney = abyssValue;
                    //        ShowToast($"AbyssMoney set to {abyssValue}");
                    //    }
                    //    else
                    //    {
                    //        ShowToast("Invalid abyssMoney value. Use 16=X where X is a non-negative number.");
                    //        closeCommandChat = false;
                    //    }
                    //}
                    // Check if it's a command to set repeat count (=X format)
                    if (command.StartsWith("=") && command.Length > 1)
                    {
                        string numPart = command.Substring(1);
                        if (int.TryParse(numPart, out int repeatCount) && repeatCount > 0)
                        {
                            commandRepeatCount = repeatCount;
                            ShowToast($"Repeat count set to {commandRepeatCount}");
                            // Keep command chat open
                            closeCommandChat = false;
                        }
                        else
                        {
                            ShowToast("Invalid repeat count. Use =X where X is a positive number.");
                            closeCommandChat = false;
                        }
                    }
                    else
                    {
                        ShowToast("Unknown command. Type 'help' for assistance.");
                    }
                    break;

            }

            // Close the command chat if needed
            if (closeCommandChat)
            {
                showCommandChat = false;
            }
        }
    }
}