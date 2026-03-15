using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using CoreApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Cad_AI_Agent.Models;
using Cad_AI_Agent.CADTransactions;
using Cad_AI_Agent.Services;
using Microsoft.Win32;

namespace Cad_AI_Agent.UI
{
    public class AiResponse
    {
        public string Message { get; set; }
        public List<CadCommand> Commands { get; set; }
    }

    public class ChatMessageData
    {
        public string Text { get; set; }
        public bool IsUser { get; set; }
    }

    public class ChatSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "New Drawing";
        public List<ChatMessageData> Messages { get; set; } = new List<ChatMessageData>();
    }

    public partial class AIChatPanel : UserControl
    {
        private DispatcherTimer _thinkingTimer;
        private int _dotCount = 0;
        private TextBlock _currentThinkingText;
        private const string RegistryPath = @"SOFTWARE\CadAiAgent";
        private const string DefaultProviderTag = "gemini:gemini-2.5-flash";
        private string _activeProviderTag = DefaultProviderTag;
        private bool _isLoadingProviderSettings = false;
        private McpClient _mcpClient;
        private bool _mcpAvailable = false;

        private List<ChatSession> _allSessions = new List<ChatSession>();
        private ChatSession _currentSession;

        public AIChatPanel()
        {
            InitializeComponent();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            _thinkingTimer = new DispatcherTimer();
            _thinkingTimer.Interval = TimeSpan.FromMilliseconds(500);
            _thinkingTimer.Tick += ThinkingTimer_Tick;

            StartNewSession();

            LoadProviderSettings();
            InitializeMcpClient();
        }

        private async void InitializeMcpClient()
        {
            _mcpClient = new McpClient("http://localhost:3000");
            try
            {
                _mcpAvailable = await _mcpClient.CheckAvailabilityAsync();
                if (_mcpAvailable)
                {
                    AddMessageToChat("🔗 MCP server connected - AI can now query drawing context", false, false);
                }
            }
            catch
            {
                _mcpAvailable = false;
            }
        }

        private void LoadProviderSettings()
        {
            _isLoadingProviderSettings = true;
            try
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                string selectedProviderTag = key?.GetValue("SelectedProviderTag")?.ToString() ?? DefaultProviderTag;
                SelectProviderTag(selectedProviderTag);
                _activeProviderTag = GetSelectedProviderTag();
                ApiKeyBox.Text = ReadApiKeyForProvider(_activeProviderTag);
            }
            finally
            {
                _isLoadingProviderSettings = false;
            }
        }

        private void StartNewSession()
        {
            _currentSession = new ChatSession();
            _allSessions.Insert(0, _currentSession);
            LoadSessionToUI(_currentSession);
            RefreshSidebarUI();
        }

        private void LoadSessionToUI(ChatSession session)
        {
            ChatHistoryPanel.Children.Clear();
            _currentSession = session;

            if (session.Messages.Count == 0)
            {
                AddMessageToChat("Hello! I'm your local AI Agent. Tell me what to draw.", false, saveToHistory: false);
            }
            else
            {
                foreach (var msg in session.Messages)
                {
                    AddMessageToChat(msg.Text, msg.IsUser, saveToHistory: false);
                }
            }
        }

        private void RefreshSidebarUI()
        {
            HistoryListPanel.Children.Clear();
            foreach (var session in _allSessions)
            {
                Grid sessionGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
                sessionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                sessionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                Button loadBtn = new Button
                {
                    Content = session.Title,

                    Background = session == _currentSession ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#162033")) : Brushes.Transparent,
                    BorderBrush = session == _currentSession ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A3A55")) : Brushes.Transparent,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5EEF9")),
                    BorderThickness = session == _currentSession ? new Thickness(1) : new Thickness(0),
                    Padding = new Thickness(10, 8, 10, 8),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Cursor = Cursors.Hand,
                    ToolTip = session.Title
                };

                loadBtn.Click += (s, e) => { LoadSessionToUI(session); RefreshSidebarUI(); };
                Grid.SetColumn(loadBtn, 0);
                loadBtn.Style = (Style)FindResource("SidebarButtonStyle");

                ContextMenu ctxMenu = new ContextMenu();
                MenuItem renameItem = new MenuItem { Header = "✏️ Rename" };
                renameItem.Click += (s, ev) =>
                {
                    TextBox renameBox = new TextBox
                    {
                        Text = session.Title,

                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F1A2E")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5EEF9")),
                        CaretBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5EEF9")),
                        Padding = new Thickness(10, 8, 10, 8),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"))
                    };

                    renameBox.KeyDown += (senderBox, args) => {
                        if (args.Key == Key.Enter) { session.Title = renameBox.Text; RefreshSidebarUI(); }
                        else if (args.Key == Key.Escape) { RefreshSidebarUI(); }
                    };

                    renameBox.LostFocus += (senderBox, args) => { session.Title = renameBox.Text; RefreshSidebarUI(); };

                    Grid.SetColumn(renameBox, 0);
                    sessionGrid.Children.Remove(loadBtn);
                    sessionGrid.Children.Insert(0, renameBox);

                    Dispatcher.BeginInvoke(new Action(() => {
                        renameBox.Focus();
                        renameBox.SelectAll();
                    }), DispatcherPriority.Input);
                };
                ctxMenu.Items.Add(renameItem);
                loadBtn.ContextMenu = ctxMenu;

                Button delBtn = new Button
                {
                    Content = "✕",

                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                    BorderThickness = new Thickness(0),

                    Cursor = Cursors.Hand,
                    Padding = new Thickness(8, 8, 8, 8),
                    ToolTip = "Delete Chat"
                };
                delBtn.Click += (s, e) =>
                {
                    _allSessions.Remove(session);
                    if (_currentSession == session) StartNewSession();
                    else RefreshSidebarUI();
                };

                Grid.SetColumn(delBtn, 1);
                delBtn.Style = (Style)FindResource("SidebarButtonStyle");

                sessionGrid.Children.Add(loadBtn);
                sessionGrid.Children.Add(delBtn);
                HistoryListPanel.Children.Add(sessionGrid);
            }
        }

        private void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewSession();
        }

        private TextBlock AddMessageToChat(string text, bool isUser, bool saveToHistory = true)
        {
            if (saveToHistory && _currentSession != null)
            {
                _currentSession.Messages.Add(new ChatMessageData { Text = text, IsUser = isUser });
                if (_currentSession.Messages.Count == 1 && isUser)
                {
                    _currentSession.Title = text.Length > 15 ? text.Substring(0, 15) + "..." : text;
                    RefreshSidebarUI();
                }
            }

            StackPanel messageContainer = new StackPanel
            {
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = isUser ? new Thickness(48, 0, 0, 12) : new Thickness(0, 0, 48, 12),
                MaxWidth = 460
            };

            TextBlock messageMeta = new TextBlock
            {
                Text = isUser ? "You" : "AI Agent",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isUser ? "#7DD3FC" : "#94A3B8")),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(4, 0, 4, 6),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            Border bubble = new Border
            {
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(14, 12, 14, 12),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Background = isUser ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0284C7"))
                                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#162033")),
                BorderBrush = isUser ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"))
                                     : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26354D")),
                BorderThickness = new Thickness(1)
            };

            TextBox txtBox = new TextBox
            {
                Text = text,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC")),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13.5,
                SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C93B7")),
                FontFamily = new FontFamily("Segoe UI"),
                MinWidth = 80
            };

            bubble.Child = txtBox;
            messageContainer.Children.Add(messageMeta);
            messageContainer.Children.Add(bubble);
            ChatHistoryPanel.Children.Add(messageContainer);
            ChatScrollViewer.ScrollToEnd();

            TextBlock hiddenTextBlockForThinking = new TextBlock();
            txtBox.Tag = hiddenTextBlockForThinking;
            txtBox.TextChanged += (s, e) => { hiddenTextBlockForThinking.Text = txtBox.Text; };
            hiddenTextBlockForThinking.TargetUpdated += (s, e) => { txtBox.Text = hiddenTextBlockForThinking.Text; };

            TextBlock proxyText = new TextBlock();
            proxyText.DataContext = txtBox;
            proxyText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Text") { Mode = System.Windows.Data.BindingMode.TwoWay });
            return proxyText;
        }

        private void TabChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatTab.Visibility = Visibility.Visible;
            SettingsTab.Visibility = Visibility.Collapsed;

            TabChatButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0696D7"));
            TabSettingsButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
        }

        private void TabSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ChatTab.Visibility = Visibility.Collapsed;
            SettingsTab.Visibility = Visibility.Visible;
            TabSettingsButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0696D7"));
            TabChatButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
        }

        private async void TestConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            string key = ApiKeyBox.Text.Trim();
            string providerTag = GetSelectedProviderTag();
            string providerName = GetProviderName(providerTag);
            string modelName = GetModelName(providerTag);

            if (string.IsNullOrEmpty(key))
            {
                ConnectionStatusText.Text = "❌ Please enter a key";
                ConnectionStatusText.Foreground = Brushes.Red;
                return;
            }
            ConnectionStatusText.Text = "Testing...";

            ConnectionStatusText.Foreground = Brushes.Yellow;
            TestConnectionBtn.IsEnabled = false;

            try
            {
                using var client = new HttpClient();
                HttpResponseMessage response;

                if (providerName.Equals("openai", StringComparison.OrdinalIgnoreCase))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
                    var requestBody = new
                    {
                        model = modelName,
                        input = "Hello"
                    };
                    var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    response = await client.PostAsync("https://api.openai.com/v1/responses", content);
                }
                else
                {
                    string testUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={key}";
                    var content = new StringContent("{\"contents\":[{\"parts\":[{\"text\":\"Hello\"}]}]}", Encoding.UTF8, "application/json");
                    response = await client.PostAsync(testUrl, content);
                }

                if (response.IsSuccessStatusCode)
                {
                    ConnectionStatusText.Text = "✅ Connected!";
                    SaveProviderSettings(providerTag, key);
                    _activeProviderTag = providerTag;
                    ConnectionStatusText.Foreground = Brushes.LightGreen;
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    ConnectionStatusText.Text = "❌ Connection Failed";
                    ConnectionStatusText.Foreground = Brushes.Red;
                    MessageBox.Show($"{providerName} API Error:\n\n{errorDetails}", "API Error Details", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                ConnectionStatusText.Text = "❌ Connection Failed";
                ConnectionStatusText.Foreground = Brushes.Red;
            }
            finally { TestConnectionBtn.IsEnabled = true; }
        }

        private void UserInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                _ = SendMessageAsync();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendMessageAsync();
        }

        private void ThinkingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentThinkingText != null)
            {
                _dotCount = (_dotCount + 1) % 4;
                _currentThinkingText.Text = "Thinking" + new string('.', _dotCount);
            }
        }

        private async Task SendMessageAsync()
        {
            string message = UserInputBox.Text.Trim();
            string apiKey = ApiKeyBox.Text.Trim();
            string providerTag = GetSelectedProviderTag();
            string providerName = GetProviderName(providerTag);
            string modelName = GetModelName(providerTag);

            if (string.IsNullOrEmpty(message)) return;
            if (string.IsNullOrEmpty(apiKey))
            {
                AddMessageToChat("⚠️ Please enter your API Key in the Settings tab first.", false, false);
                return;
            }

            AddMessageToChat(message, true);
            UserInputBox.Clear();
            UserInputBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            SaveProviderSettings(providerTag, apiKey);
            _activeProviderTag = providerTag;

            _currentThinkingText = AddMessageToChat("Thinking", false, false);
            _dotCount = 0;
            _thinkingTimer.Start();

            try
            {
                // Query MCP server for drawing context if available
                string drawingContext = null;
                if (_mcpAvailable)
                {
                    try
                    {
                        var context = await _mcpClient.GetDrawingContextAsync();
                        drawingContext = context.ToSummary();
                    }
                    catch
                    {
                        // MCP query failed, continue without context
                        drawingContext = null;
                    }
                }

                string jsonPayload = providerName.Equals("openai", StringComparison.OrdinalIgnoreCase)
                    ? await GetOpenAiResponse(message, apiKey, modelName, drawingContext)
                    : await GetGeminiResponse(message, apiKey, modelName, drawingContext);
                _thinkingTimer.Stop();

                if (!string.IsNullOrEmpty(jsonPayload))
                {
                    var responseObj = JsonConvert.DeserializeObject<AiResponse>(jsonPayload);

                    _currentThinkingText.Text = responseObj.Message ?? "Drawing initiated...";
                    _currentSession.Messages.Add(new ChatMessageData { Text = _currentThinkingText.Text, IsUser = false });

                    if (responseObj.Commands != null && responseObj.Commands.Count > 0)
                    {
                        await ExecuteCadCommandsLive(responseObj.Commands);
                    }
                }
            }
            catch (Exception ex)
            {
                _thinkingTimer.Stop();
                _currentThinkingText.Text = $"Error: {ex.Message}";
            }
            finally
            {
                UserInputBox.IsEnabled = true;
                SendButton.IsEnabled = true;
                UserInputBox.Focus();
            }
        }

        private async Task<string> GetOpenAiResponse(string prompt, string key, string modelName, string drawingContext = null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

            string systemInstruction = string.IsNullOrEmpty(drawingContext)
                ? Core.AgentPromptManager.GetSystemInstruction()
                : Core.AgentPromptManager.GetContextAwareInstruction(drawingContext);

            var requestBody = new JObject
            {
                ["model"] = modelName,
                ["input"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "system",
                        ["content"] = systemInstruction
                    },
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = prompt
                    }
                },
                ["temperature"] = 0.0,
                ["text"] = new JObject
                {
                    ["format"] = new JObject
                    {
                        ["type"] = "json_schema",
                        ["name"] = "civil3d_ai_response",
                        ["strict"] = true,
                        ["schema"] = new JObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JObject
                            {
                                ["Message"] = new JObject
                                {
                                    ["type"] = "string"
                                },
                                ["Commands"] = new JObject
                                {
                                    ["type"] = "array",
                                    ["items"] = new JObject
                                    {
                                        ["type"] = "object",
                                        ["properties"] = new JObject
                                        {
                                            ["Action"] = new JObject
                                            {
                                                ["type"] = "string"
                                            },
                                            ["Params"] = new JObject
                                            {
                                                ["type"] = "array",
                                                ["items"] = new JObject
                                                {
                                                    ["type"] = "number"
                                                }
                                            },
                                            ["Args"] = new JObject
                                            {
                                                ["type"] = "object",
                                                ["additionalProperties"] = new JObject
                                                {
                                                    ["anyOf"] = new JArray
                                                    {
                                                        new JObject { ["type"] = "string" },
                                                        new JObject { ["type"] = "number" },
                                                        new JObject { ["type"] = "integer" },
                                                        new JObject { ["type"] = "boolean" },
                                                        new JObject { ["type"] = "null" }
                                                    }
                                                }
                                            }
                                        },
                                        ["required"] = new JArray("Action", "Params"),
                                        ["additionalProperties"] = false
                                    }
                                }
                            },
                            ["required"] = new JArray("Message", "Commands"),
                            ["additionalProperties"] = false
                        }
                    }
                }
            };

            var content = new StringContent(requestBody.ToString(Formatting.None), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.openai.com/v1/responses", content);
            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Error ({modelName}): {responseString}");
            }

            JObject jsonResponse = JObject.Parse(responseString);
            string aiText = ExtractOpenAiText(jsonResponse);
            return aiText.Replace("```json", "").Replace("```", "").Trim();
        }

        private async Task<string> GetGeminiResponse(string prompt, string key, string modelName, string drawingContext = null)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={key}";
            using var client = new HttpClient();

            string systemInstruction = string.IsNullOrEmpty(drawingContext)
                ? Core.AgentPromptManager.GetSystemInstruction()
                : Core.AgentPromptManager.GetContextAwareInstruction(drawingContext);

            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.0 }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                JObject jsonResponse = JObject.Parse(responseString);
                string aiText = jsonResponse["candidates"][0]["content"]["parts"][0]["text"].ToString();
                return aiText.Replace("```json", "").Replace("```", "").Trim();
            }
            string errorRaw = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error ({modelName}): {errorRaw}");
        }

        private string ExtractOpenAiText(JObject jsonResponse)
        {
            string directText = jsonResponse["output_text"]?.ToString();
            if (!string.IsNullOrWhiteSpace(directText))
            {
                return directText;
            }

            JArray outputItems = jsonResponse["output"] as JArray;
            if (outputItems != null)
            {
                foreach (JObject outputItem in outputItems.OfType<JObject>())
                {
                    JArray contentItems = outputItem["content"] as JArray;
                    if (contentItems == null) continue;

                    foreach (JObject contentItem in contentItems.OfType<JObject>())
                    {
                        JToken parsedValue = contentItem["parsed"] ?? contentItem["json"] ?? contentItem["value"];
                        if (parsedValue is JObject || parsedValue is JArray)
                        {
                            return parsedValue.ToString(Formatting.None);
                        }

                        string textValue = contentItem["text"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(textValue))
                        {
                            return textValue;
                        }

                        string textObjectValue = contentItem["text"]?["value"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(textObjectValue))
                        {
                            return textObjectValue;
                        }
                    }
                }
            }

            throw new Exception("OpenAI response did not contain any text output.");
        }

        private async Task ExecuteCadCommandsLive(List<CadCommand> commands)
        {
            try
            {
                Document doc = CoreApp.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                // Create execution progress UI
                var executionPanel = CreateExecutionLogPanel(commands.Count);
                AddMessageContainerToChat(executionPanel);

                // Create progress reporter that updates the UI
                var progress = new Progress<ExecutionStep>(step =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        UpdateExecutionLog(executionPanel, step);
                    });
                });

                var reporter = new ExecutionProgressReporter(progress);
                reporter.Initialize(commands.Count);

                int completedCount = 0;
                int errorCount = 0;

                for (int i = 0; i < commands.Count; i++)
                {
                    var command = commands[i];
                    
                    await CoreApp.DocumentManager.ExecuteInCommandContextAsync(async (obj) =>
                    {
                        try
                        {
                            // ახლა პირდაპირ როუტერს გადავცემთ ბრძანებას პროგრესის რეპორტერით!
                            CommandRouter.Execute(doc, command, reporter);
                            completedCount++;
                        }
                        catch (Exception cmdEx)
                        {
                            errorCount++;
                            // Error is already reported via progress reporter
                            doc.Editor.WriteMessage($"\n[AI Agent] Command error: {cmdEx.Message}");
                        }

                        doc.Editor.UpdateScreen();
                    }, null);

                    await Task.Delay(300);
                }

                // Final summary
                Dispatcher.Invoke(() =>
                {
                    AddExecutionSummary(executionPanel, completedCount, errorCount, commands.Count);
                });
            }
            catch (Exception ex)
            {
                AddMessageToChat($"[Execution Error]: {ex.Message}", false);
            }
        }

        private StackPanel CreateExecutionLogPanel(int totalSteps)
        {
            var container = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 8, 0, 12),
                MaxWidth = 520
            };

            // Header
            var headerBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Padding = new Thickness(12, 10, 12, 10)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var statusIcon = new TextBlock
            {
                Text = "▶️",
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(statusIcon, 0);

            var headerText = new TextBlock
            {
                Text = $"Executing {totalSteps} CAD Commands...",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5EEF9")),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(headerText, 1);

            headerGrid.Children.Add(statusIcon);
            headerGrid.Children.Add(headerText);
            headerBorder.Child = headerGrid;
            container.Children.Add(headerBorder);

            // Execution log content
            var logBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F1A2E")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26354D")),
                BorderThickness = new Thickness(1, 0, 1, 1),
                CornerRadius = new CornerRadius(0, 0, 8, 8),
                Padding = new Thickness(8),
                MaxHeight = 400
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            var logPanel = new StackPanel
            {
                Name = "ExecutionLogPanel"
            };
            scrollViewer.Content = logPanel;
            logBorder.Child = scrollViewer;
            container.Children.Add(logBorder);

            // Store reference for updates
            container.Tag = new ExecutionLogState
            {
                LogPanel = logPanel,
                HeaderIcon = statusIcon,
                HeaderText = headerText,
                ScrollViewer = scrollViewer
            };

            return container;
        }

        private void UpdateExecutionLog(StackPanel container, ExecutionStep step)
        {
            if (container.Tag is not ExecutionLogState state) return;

            // Create step entry
            var stepBorder = new Border
            {
                Background = GetStatusBackground(step.Status),
                BorderBrush = GetStatusBorderBrush(step.Status),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(0, 0, 0, 6)
            };

            var stepGrid = new Grid();
            stepGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Step #
            stepGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Message
            stepGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Status icon

            // Step number with timestamp
            var stepNumber = new TextBlock
            {
                Text = $"[{step.StepNumber}/{step.TotalSteps}] {step.GetFormattedTime()}",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B")),
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(stepNumber, 0);

            // Message area
            var messageStack = new StackPanel();

            var commandName = new TextBlock
            {
                Text = step.CommandName,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 2)
            };

            var messageText = new TextBlock
            {
                Text = step.Message,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5EEF9")),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, step.Details != null ? 4 : 0)
            };

            messageStack.Children.Add(commandName);
            messageStack.Children.Add(messageText);

            if (!string.IsNullOrEmpty(step.Details))
            {
                var detailsText = new TextBlock
                {
                    Text = step.Details,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B")),
                    FontSize = 10,
                    FontStyle = FontStyles.Italic,
                    TextWrapping = TextWrapping.Wrap
                };
                messageStack.Children.Add(detailsText);
            }

            Grid.SetColumn(messageStack, 1);

            // Status icon
            var statusIcon = new TextBlock
            {
                Text = GetStatusIcon(step.Status),
                FontSize = 14,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(statusIcon, 2);

            stepGrid.Children.Add(stepNumber);
            stepGrid.Children.Add(messageStack);
            stepGrid.Children.Add(statusIcon);
            stepBorder.Child = stepGrid;

            state.LogPanel.Children.Add(stepBorder);
            state.ScrollViewer.ScrollToEnd();

            // Update header based on status
            if (step.Status == "Error")
            {
                state.HeaderIcon.Text = "⚠️";
                state.HeaderText.Text = $"Executing {step.TotalSteps} CAD Commands... (Error on step {step.StepNumber})";
                state.HeaderText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"));
            }
        }

        private void AddExecutionSummary(StackPanel container, int completed, int errors, int total)
        {
            if (container.Tag is not ExecutionLogState state) return;

            // Update header
            if (errors > 0)
            {
                state.HeaderIcon.Text = "⚠️";
                state.HeaderText.Text = $"Completed with {errors} error(s)";
                state.HeaderText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"));
            }
            else
            {
                state.HeaderIcon.Text = "✅";
                state.HeaderText.Text = $"All {total} commands executed successfully";
                state.HeaderText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4ADE80"));
            }

            // Add summary footer
            var summaryBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 8, 0, 0)
            };

            var summaryText = new TextBlock
            {
                Text = $"✓ {completed} successful  •  ✗ {errors} failed  •  ⏱️ {DateTime.Now:HH:mm:ss}",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                FontSize = 11,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            summaryBorder.Child = summaryText;

            state.LogPanel.Children.Add(summaryBorder);
            state.ScrollViewer.ScrollToEnd();
        }

        private string GetStatusIcon(string status) => status switch
        {
            "Running" => "⏳",
            "Success" => "✓",
            "Error" => "✗",
            "Skipped" => "⊘",
            _ => "•"
        };

        private Brush GetStatusBackground(string status) => status switch
        {
            "Running" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E3A5F")),
            "Success" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#064E3B")),
            "Error" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#450A0A")),
            "Skipped" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#162033"))
        };

        private Brush GetStatusBorderBrush(string status) => status switch
        {
            "Running" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8")),
            "Success" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E")),
            "Error" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
            "Skipped" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26354D"))
        };

        private void AddMessageContainerToChat(StackPanel container)
        {
            ChatHistoryPanel.Children.Add(container);
            ChatScrollViewer.ScrollToEnd();
        }

        private class ExecutionLogState
        {
            public StackPanel LogPanel { get; set; }
            public TextBlock HeaderIcon { get; set; }
            public TextBlock HeaderText { get; set; }
            public ScrollViewer ScrollViewer { get; set; }
        }
    }
}