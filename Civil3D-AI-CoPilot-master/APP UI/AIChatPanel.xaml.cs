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
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using CoreApp = Autodesk.AutoCAD.ApplicationServices.Application;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;
using WpfVisibility = System.Windows.Visibility;
using Cad_AI_Agent.Models;
using Cad_AI_Agent.CADTransactions;
using Cad_AI_Agent.Services;
using Microsoft.Win32;

namespace Cad_AI_Agent.UI
{
    public class AiResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<CadCommand> Commands { get; set; } = new List<CadCommand>();
    }

    public class ChatMessageData
    {
        public string Text { get; set; } = string.Empty;
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
        private TextBlock? _currentThinkingText;
        private const string RegistryPath = @"SOFTWARE\CadAiAgent";
        private const string DefaultProviderTag = "gemini:gemini-2.5-flash";
        private const string DefaultMcpUrl = "http://localhost:3000";
        private string _activeProviderTag = DefaultProviderTag;
        private bool _isLoadingProviderSettings = false;
        private McpClient _mcpClient = new McpClient(DefaultMcpUrl);
        private bool _mcpAvailable = false;

        private List<ChatSession> _allSessions = new List<ChatSession>();
        private ChatSession _currentSession = new ChatSession();

        public AIChatPanel()
        {
            try
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
            catch (Exception ex)
            {
                Exception baseException = ex.GetBaseException();
                throw new InvalidOperationException($"AIChatPanel initialization failed. ExceptionType={ex.GetType().FullName}; BaseExceptionType={baseException.GetType().FullName}; Message={ex.Message}; BaseMessage={baseException.Message}", ex);
            }
        }

        private async void InitializeMcpClient()
        {
            string mcpUrl = DefaultMcpUrl;
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                mcpUrl = key?.GetValue("McpServerUrl") as string ?? DefaultMcpUrl;
            }
            catch { /* Registry read failure is non-critical */ }

            // Populate Settings URL field
            if (McpServerUrlBox != null)
                McpServerUrlBox.Text = mcpUrl;

            await ConnectMcpAsync(mcpUrl);
        }

        private async Task ConnectMcpAsync(string mcpUrl)
        {
            if (McpStatusText != null)
            {
                McpStatusText.Text = "Connecting...";
                McpStatusText.Foreground = Brushes.Yellow;
            }

            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await httpClient.GetAsync($"{mcpUrl}/health");

                _mcpClient = new McpClient(mcpUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Civil 3D is open and connected
                    _mcpAvailable = true;
                    AddMessageToChat("✅ MCP server connected — AI can query drawing context", false, false);
                    if (McpStatusText != null)
                    {
                        McpStatusText.Text = "✅ Civil 3D connected";
                        McpStatusText.Foreground = Brushes.LightGreen;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    // Server is running but Civil 3D isn't open yet — queries will still be attempted
                    _mcpAvailable = true;
                    if (McpStatusText != null)
                    {
                        McpStatusText.Text = "⚠️ Server up — open Civil 3D";
                        McpStatusText.Foreground = Brushes.Orange;
                    }
                }
                else
                {
                    _mcpAvailable = false;
                    if (McpStatusText != null)
                    {
                        McpStatusText.Text = $"❌ HTTP {(int)response.StatusCode}";
                        McpStatusText.Foreground = Brushes.Red;
                    }
                }
            }
            catch
            {
                _mcpAvailable = false;
                if (McpStatusText != null)
                {
                    McpStatusText.Text = "❌ Server not running";
                    McpStatusText.Foreground = Brushes.Red;
                }
            }
        }

        private async void ReconnectMcpBtn_Click(object sender, RoutedEventArgs e)
        {
            string mcpUrl = McpServerUrlBox?.Text?.Trim() ?? DefaultMcpUrl;
            if (string.IsNullOrWhiteSpace(mcpUrl))
                mcpUrl = DefaultMcpUrl;

            // Save to registry
            try
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                key?.SetValue("McpServerUrl", mcpUrl);
            }
            catch { /* non-critical */ }

            ReconnectMcpBtn.IsEnabled = false;
            try
            {
                await ConnectMcpAsync(mcpUrl);
            }
            finally
            {
                ReconnectMcpBtn.IsEnabled = true;
            }
        }

        private void LoadProviderSettings()
        {
            _isLoadingProviderSettings = true;
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                string selectedProviderTag = key?.GetValue("SelectedProviderTag")?.ToString() ?? DefaultProviderTag;
                SelectProviderTag(selectedProviderTag);
                _activeProviderTag = GetSelectedProviderTag();
                ApiKeyBox.Text = ReadApiKeyForProvider(_activeProviderTag);
                VectorStoreIdBox.Text = key?.GetValue("OpenAIVectorStoreId")?.ToString() ?? string.Empty;
            }
            finally
            {
                _isLoadingProviderSettings = false;
            }
        }

        private void ProviderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingProviderSettings || ApiKeyBox == null || ConnectionStatusText == null)
            {
                return;
            }

            string providerTag = GetSelectedProviderTag();
            _activeProviderTag = providerTag;
            ApiKeyBox.Text = ReadApiKeyForProvider(providerTag);
            ConnectionStatusText.Text = string.Empty;
        }

        private void SelectProviderTag(string providerTag)
        {
            string resolvedProviderTag = string.IsNullOrWhiteSpace(providerTag) ? DefaultProviderTag : providerTag;

            foreach (var item in ProviderCombo.Items)
            {
                if (item is ComboBoxItem comboBoxItem && string.Equals(comboBoxItem.Tag?.ToString(), resolvedProviderTag, StringComparison.OrdinalIgnoreCase))
                {
                    ProviderCombo.SelectedItem = comboBoxItem;
                    return;
                }
            }

            if (ProviderCombo.Items.Count > 0)
            {
                ProviderCombo.SelectedIndex = 0;
            }
        }

        private string GetSelectedProviderTag()
        {
            if (ProviderCombo.SelectedItem is ComboBoxItem comboBoxItem)
            {
                return comboBoxItem.Tag?.ToString() ?? DefaultProviderTag;
            }

            return DefaultProviderTag;
        }

        private string ReadApiKeyForProvider(string providerTag)
        {
            string providerName = GetProviderName(providerTag);

            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            if (providerName.Equals("openai", StringComparison.OrdinalIgnoreCase))
                return key?.GetValue("OpenAIApiKey")?.ToString() ?? string.Empty;
            if (providerName.Equals("anthropic", StringComparison.OrdinalIgnoreCase))
                return key?.GetValue("AnthropicApiKey")?.ToString() ?? string.Empty;
            return key?.GetValue("GeminiApiKey")?.ToString() ?? string.Empty;
        }

        private void SaveProviderSettings(string providerTag, string apiKey)
        {
            string providerName = GetProviderName(providerTag);

            using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key?.SetValue("SelectedProviderTag", providerTag);

            if (providerName.Equals("openai", StringComparison.OrdinalIgnoreCase))
                key?.SetValue("OpenAIApiKey", apiKey ?? string.Empty);
            else if (providerName.Equals("anthropic", StringComparison.OrdinalIgnoreCase))
                key?.SetValue("AnthropicApiKey", apiKey ?? string.Empty);
            else
                key?.SetValue("GeminiApiKey", apiKey ?? string.Empty);

            key?.SetValue("OpenAIVectorStoreId", VectorStoreIdBox?.Text?.Trim() ?? string.Empty);
        }

        private string GetProviderName(string providerTag)
        {
            if (string.IsNullOrWhiteSpace(providerTag))
            {
                return "gemini";
            }

            string[] parts = providerTag.Split(':');
            return parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : "gemini";
        }

        private string GetModelName(string providerTag)
        {
            if (string.IsNullOrWhiteSpace(providerTag))
            {
                return "gemini-2.5-flash";
            }

            string[] parts = providerTag.Split(':');
            return parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "gemini-2.5-flash";
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
            ChatTab.Visibility = WpfVisibility.Visible;
            SettingsTab.Visibility = WpfVisibility.Collapsed;

            TabChatButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0696D7"));
            TabSettingsButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
        }

        private void TabSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ChatTab.Visibility = WpfVisibility.Collapsed;
            SettingsTab.Visibility = WpfVisibility.Visible;
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
                else if (providerName.Equals("anthropic", StringComparison.OrdinalIgnoreCase))
                {
                    client.DefaultRequestHeaders.Add("x-api-key", key);
                    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                    var requestBody = new
                    {
                        model = modelName,
                        max_tokens = 16,
                        messages = new[] { new { role = "user", content = "Hello" } }
                    };
                    var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);
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

        private void ThinkingTimer_Tick(object? sender, EventArgs e)
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
                // Query drawing context — direct C# first (always works inside Civil 3D),
                // fall back to MCP server if direct query returns nothing.
                string? drawingContext = GetDirectDrawingContext();
                if (drawingContext == null && _mcpAvailable)
                {
                    try
                    {
                        var context = await _mcpClient.GetDrawingContextAsync();
                        drawingContext = context.ToSummary();
                    }
                    catch
                    {
                        drawingContext = null;
                    }
                }

                string jsonPayload;
                if (providerName.Equals("openai", StringComparison.OrdinalIgnoreCase))
                {
                    string vectorStoreId = VectorStoreIdBox?.Text?.Trim() ?? string.Empty;
                    jsonPayload = await GetOpenAiResponse(message, apiKey, modelName, drawingContext, vectorStoreId);
                }
                else if (providerName.Equals("anthropic", StringComparison.OrdinalIgnoreCase))
                    jsonPayload = await GetAnthropicResponse(message, apiKey, modelName, drawingContext);
                else
                    jsonPayload = await GetGeminiResponse(message, apiKey, modelName, drawingContext);
                _thinkingTimer.Stop();

                if (!string.IsNullOrEmpty(jsonPayload))
                {
                    var responseObj = JsonConvert.DeserializeObject<AiResponse>(jsonPayload);
                    if (responseObj == null)
                    {
                        throw new InvalidOperationException("AI response could not be deserialized.");
                    }

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

        private string? GetDirectDrawingContext()
        {
            try
            {
                var doc = CoreApp.DocumentManager.MdiActiveDocument;
                if (doc == null) return null;

                var civilDoc = CivilDocument.GetCivilDocument(doc.Database);
                var sb = new StringBuilder();
                sb.AppendLine("Drawing Context:");

                using var trans = doc.Database.TransactionManager.StartTransaction();

                try
                {
                    var alignmentIds = civilDoc.GetAlignmentIds();
                    sb.AppendLine($"- Alignments: {alignmentIds.Count}");
                    foreach (ObjectId id in alignmentIds)
                    {
                        if (trans.GetObject(id, OpenMode.ForRead) is Alignment a)
                            sb.AppendLine($"  • {a.Name} (Length: {a.Length:F2}, {a.StartingStation:F2}–{a.EndingStation:F2})");
                    }
                }
                catch { sb.AppendLine("- Alignments: none"); }

                try
                {
                    var surfaceIds = civilDoc.GetSurfaceIds();
                    sb.AppendLine($"- Surfaces: {surfaceIds.Count}");
                    foreach (ObjectId id in surfaceIds)
                    {
                        if (trans.GetObject(id, OpenMode.ForRead) is CivilSurface s)
                            sb.AppendLine($"  • {s.Name} ({s.GetType().Name})");
                    }
                }
                catch { sb.AppendLine("- Surfaces: none"); }

                try
                {
                    var assemblyIds = civilDoc.AssemblyCollection;
                    sb.AppendLine($"- Assemblies: {assemblyIds.Count}");
                    foreach (ObjectId id in assemblyIds)
                    {
                        if (trans.GetObject(id, OpenMode.ForRead) is Autodesk.Civil.DatabaseServices.Assembly asm)
                            sb.AppendLine($"  • {asm.Name}");
                    }
                }
                catch { sb.AppendLine("- Assemblies: none"); }

                try
                {
                    var corridorIds = civilDoc.CorridorCollection;
                    sb.AppendLine($"- Corridors: {corridorIds.Count}");
                    foreach (ObjectId id in corridorIds)
                    {
                        if (trans.GetObject(id, OpenMode.ForRead) is Corridor c)
                            sb.AppendLine($"  • {c.Name}");
                    }
                }
                catch { sb.AppendLine("- Corridors: none"); }

                trans.Commit();
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> GetOpenAiResponse(string prompt, string key, string modelName, string? drawingContext = null, string? vectorStoreId = null)
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

            if (!string.IsNullOrEmpty(vectorStoreId))
            {
                requestBody["tools"] = new JArray
                {
                    new JObject
                    {
                        ["type"] = "file_search",
                        ["vector_store_ids"] = new JArray(vectorStoreId)
                    }
                };
            }

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
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

        private async Task<string> GetGeminiResponse(string prompt, string key, string modelName, string? drawingContext = null)
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
                string? aiText = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                if (string.IsNullOrWhiteSpace(aiText))
                {
                    throw new Exception("Gemini response did not contain any text output.");
                }
                return aiText.Replace("```json", "").Replace("```", "").Trim();
            }
            string errorRaw = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error ({modelName}): {errorRaw}");
        }

        private async Task<string> GetAnthropicResponse(string prompt, string key, string modelName, string? drawingContext = null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", key);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            string systemInstruction = string.IsNullOrEmpty(drawingContext)
                ? Core.AgentPromptManager.GetSystemInstruction()
                : Core.AgentPromptManager.GetContextAwareInstruction(drawingContext);

            // Append JSON format instruction since Anthropic uses system prompt for output control
            systemInstruction += "\n\nYou MUST respond with ONLY valid JSON matching this exact schema — no markdown, no code fences, no extra text:\n{\"Message\": \"<string>\", \"Commands\": [{\"Action\": \"<string>\", \"Params\": [<numbers>], \"Args\": {}}]}";

            var requestBody = new JObject
            {
                ["model"] = modelName,
                ["max_tokens"] = 4096,
                ["system"] = systemInstruction,
                ["messages"] = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["content"] = prompt
                    }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);
            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Error ({modelName}): {responseString}");
            }

            JObject jsonResponse = JObject.Parse(responseString);
            string? aiText = jsonResponse["content"]?[0]?["text"]?.ToString();
            if (string.IsNullOrWhiteSpace(aiText))
            {
                throw new Exception("Anthropic response did not contain any text output.");
            }

            return aiText.Replace("```json", "").Replace("```", "").Trim();
        }

        private string ExtractOpenAiText(JObject jsonResponse)
        {
            string? directText = jsonResponse["output_text"]?.ToString();
            if (!string.IsNullOrWhiteSpace(directText))
            {
                return directText;
            }

            JArray? outputItems = jsonResponse["output"] as JArray;
            if (outputItems != null)
            {
                foreach (JObject outputItem in outputItems.OfType<JObject>())
                {
                    JArray? contentItems = outputItem["content"] as JArray;
                    if (contentItems == null) continue;

                    foreach (JObject contentItem in contentItems.OfType<JObject>())
                    {
                        JToken? parsedValue = contentItem["parsed"] ?? contentItem["json"] ?? contentItem["value"];
                        if (parsedValue is JObject || parsedValue is JArray)
                        {
                            return JsonConvert.SerializeObject(parsedValue);
                        }

                        string? textValue = contentItem["text"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(textValue))
                        {
                            return textValue;
                        }

                        string? textObjectValue = contentItem["text"]?["value"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(textObjectValue))
                        {
                            return textObjectValue;
                        }
                    }
                }
            }

            throw new Exception("OpenAI response did not contain any text output.");
        }

        private static bool IsHydrologyCommand(string action) =>
            action == "HydrologyTraceFlowPath"
            || action == "HydrologyFindLowPoint"
            || action == "HydrologyDelineateWatershed"
            || action == "HydrologyCalculateCatchment"
            || action == "HydrologyEstimateRunoff";

        // Any action using the raw MCP tool name (civil3d_* prefix) is routed directly
        // to the MCP server as an analytical/data command with no CAD drawing required.
        private static bool IsMcpDirectCommand(string action) =>
            action.StartsWith("civil3d_") || action == "create_cogo_point";

        // Returns updated lastOutlet (or null if this command does not produce one)
        private async Task<(double X, double Y, double Elev)?> HandleHydrologyCommandAsync(
            Document doc,
            CadCommand command,
            ExecutionProgressReporter reporter,
            (double X, double Y, double Elev)? lastOutlet)
        {
            if (!_mcpAvailable)
            {
                reporter.ReportSkipped(command.Action, "MCP server not connected", "Connect the MCP server to use hydrology analysis tools.");
                return null;
            }

            switch (command.Action)
            {
                case "HydrologyTraceFlowPath":
                {
                    string surfaceName = command.Args?["surfaceName"]?.ToString() ?? string.Empty;
                    double x = command.Args?["x"]?.Value<double>() ?? 0;
                    double y = command.Args?["y"]?.Value<double>() ?? 0;
                    double? stepDist = command.Args?["stepDistance"]?.Value<double>();
                    int? maxSteps = command.Args?["maxSteps"]?.Value<int>();

                    reporter.ReportRunning(command.Action, $"Tracing flow path on '{surfaceName}'...",
                        $"Start: ({x:F2}, {y:F2})");

                    var result = await _mcpClient.TraceFlowPathAsync(surfaceName, x, y, stepDist, maxSteps);

                    await CoreApp.DocumentManager.ExecuteInCommandContextAsync((_) =>
                    {
                        CADTransactions.HydrologyDrawer.DrawFlowPath(doc, result.Points,
                            $"Drop: {result.DropElevation:F2} | Dist: {result.TotalDistance:F2}");
                        doc.Editor.UpdateScreen();
                        return Task.CompletedTask;
                    }, null);

                    reporter.ReportSuccess(command.Action,
                        $"Flow path drawn — {result.StepCount} steps, {result.TotalDistance:F1} units, drop {result.DropElevation:F2}",
                        $"Status: {result.Status}");
                    return lastOutlet; // flow path doesn't change the outlet
                }

                case "HydrologyFindLowPoint":
                {
                    string surfaceName = command.Args?["surfaceName"]?.ToString() ?? string.Empty;
                    double? spacing = command.Args?["sampleSpacing"]?.Value<double>();

                    reporter.ReportRunning(command.Action, $"Finding low point on '{surfaceName}'...", "Sampling surface grid");

                    var result = await _mcpClient.FindLowPointAsync(surfaceName, spacing);

                    await CoreApp.DocumentManager.ExecuteInCommandContextAsync((_) =>
                    {
                        CADTransactions.HydrologyDrawer.MarkOutletPoint(doc, result.X, result.Y, result.Elevation, "HYDRO_LOW");
                        doc.Editor.UpdateScreen();
                        return Task.CompletedTask;
                    }, null);

                    reporter.ReportSuccess(command.Action,
                        $"Low point found at ({result.X:F2}, {result.Y:F2}), elev {result.Elevation:F3}",
                        $"Sampled {result.SampledPointCount} points");
                    return (result.X, result.Y, result.Elevation); // save for chained commands
                }

                case "HydrologyDelineateWatershed":
                {
                    string surfaceName = command.Args?["surfaceName"]?.ToString() ?? string.Empty;
                    double outletX = command.Args?["outletX"]?.Value<double>() ?? lastOutlet?.X ?? 0;
                    double outletY = command.Args?["outletY"]?.Value<double>() ?? lastOutlet?.Y ?? 0;
                    double? gridSpacing = command.Args?["gridSpacing"]?.Value<double>();
                    double? searchRadius = command.Args?["searchRadius"]?.Value<double>();

                    reporter.ReportRunning(command.Action, $"Delineating watershed on '{surfaceName}'...",
                        $"Outlet: ({outletX:F2}, {outletY:F2})");

                    var result = await _mcpClient.DelineateWatershedAsync(surfaceName, outletX, outletY, gridSpacing, searchRadius);

                    await CoreApp.DocumentManager.ExecuteInCommandContextAsync((_) =>
                    {
                        CADTransactions.HydrologyDrawer.DrawWatershedBoundary(doc, result.BoundaryPoints, result.ApproximateArea);
                        CADTransactions.HydrologyDrawer.MarkOutletPoint(doc, result.OutletX, result.OutletY, result.OutletElevation, "WATERSHED_OUTLET");
                        doc.Editor.UpdateScreen();
                        return Task.CompletedTask;
                    }, null);

                    reporter.ReportSuccess(command.Action,
                        $"Watershed drawn — {result.ContributingPointCount} contributing points, area ≈ {result.ApproximateArea:F0} sq units",
                        $"{result.BoundaryPoints.Count} boundary vertices");
                    return (result.OutletX, result.OutletY, result.OutletElevation);
                }

                case "HydrologyCalculateCatchment":
                {
                    string surfaceName = command.Args?["surfaceName"]?.ToString() ?? string.Empty;
                    double outletX = command.Args?["outletX"]?.Value<double>() ?? lastOutlet?.X ?? 0;
                    double outletY = command.Args?["outletY"]?.Value<double>() ?? lastOutlet?.Y ?? 0;
                    double? sampleSpacing = command.Args?["sampleSpacing"]?.Value<double>();
                    double? maxDist = command.Args?["maxDistance"]?.Value<double>();

                    reporter.ReportRunning(command.Action, $"Calculating catchment area on '{surfaceName}'...",
                        $"Outlet: ({outletX:F2}, {outletY:F2})");

                    var result = await _mcpClient.CalculateCatchmentAreaAsync(surfaceName, outletX, outletY, sampleSpacing, maxDist);

                    reporter.ReportSuccess(command.Action,
                        $"Catchment area = {result.CatchmentArea:F0} sq units ({result.ContributingCellCount} cells)",
                        $"Elev range: {result.ElevMin:F2} – {result.ElevMax:F2}, relief {result.Relief:F2}");

                    // Surface a chat message with the area for easy reading
                    Dispatcher.Invoke(() =>
                    {
                        AddMessageToChat(
                            $"Catchment area: {result.CatchmentArea:F0} sq units\n" +
                            $"Elevation: min {result.ElevMin:F2} / max {result.ElevMax:F2} / avg {result.ElevAverage:F2}\n" +
                            $"Relief: {result.Relief:F2} units",
                            false);
                    });
                    break;
                }

                case "HydrologyEstimateRunoff":
                {
                    double area = command.Args?["drainageArea"]?.Value<double>() ?? 0;
                    double coeff = command.Args?["runoffCoefficient"]?.Value<double>() ?? 0;
                    double intensity = command.Args?["rainfallIntensity"]?.Value<double>() ?? 0;
                    string areaUnits = command.Args?["areaUnits"]?.ToString() ?? "acres";
                    string intensityUnits = command.Args?["intensityUnits"]?.ToString() ?? "in_per_hr";

                    reporter.ReportRunning(command.Action, "Estimating peak runoff (Rational Method)...",
                        $"A={area} {areaUnits}, C={coeff}, i={intensity} {intensityUnits}");

                    var result = await _mcpClient.EstimateRunoffAsync(area, coeff, intensity, areaUnits, intensityUnits);

                    reporter.ReportSuccess(command.Action,
                        $"Q = {result.RunoffCfs:F2} CFS  ({result.RunoffCubicMetersPerSecond:F4} m³/s)",
                        $"Rational Method: Q = C × i × A");

                    Dispatcher.Invoke(() =>
                    {
                        AddMessageToChat(
                            $"Peak Runoff (Rational Method Q = CiA):\n" +
                            $"  Area: {result.DrainageArea:F2} {result.AreaUnits}\n" +
                            $"  C: {result.RunoffCoefficient:F2}\n" +
                            $"  Intensity: {result.RainfallIntensity:F2} {result.IntensityUnits}\n" +
                            $"  Q = {result.RunoffCfs:F2} CFS  ({result.RunoffCubicMetersPerSecond:F4} m³/s)",
                            false);
                    });
                    return lastOutlet; // runoff doesn't produce outlet coords
                }
            }

            return lastOutlet;
        }

        /// <summary>
        /// Route a civil3d_* MCP tool command: call MCP server, display result in chat.
        /// No CAD drawing occurs here — this is purely for analytical/data tools.
        /// </summary>
        private async Task HandleMcpDirectCommandAsync(
            Document doc,
            CadCommand command,
            ExecutionProgressReporter reporter)
        {
            if (!_mcpAvailable)
            {
                reporter.ReportSkipped(command.Action, "MCP server not connected", "Connect the MCP server to use this tool.");
                return;
            }

            reporter.ReportRunning(command.Action, $"Calling MCP tool '{command.Action}'...", "Sending request to Civil 3D MCP server");

            var result = await _mcpClient.ExecuteMcpToolAsync(command.Action, command.Args);

            // Format result for display — extract a summary string if available, else use JSON
            string summary = FormatMcpResult(result);

            reporter.ReportSuccess(command.Action, $"Tool '{command.Action}' completed", summary.Length > 120 ? summary.Substring(0, 120) + "…" : summary);

            Dispatcher.Invoke(() =>
            {
                AddMessageToChat(
                    $"**{command.Action}**\n{summary}",
                    false);
            });
        }

        /// <summary>
        /// Produce a readable one-or-two-line summary from a MCP tool JObject result.
        /// </summary>
        private static string FormatMcpResult(JObject result)
        {
            if (result == null) return "(no result)";

            // Common patterns across Civil 3D MCP tools
            if (result["message"] is JToken msg && msg.Type == JTokenType.String)
                return msg.ToString();
            if (result["summary"] is JToken summ && summ.Type == JTokenType.String)
                return summ.ToString();
            if (result["result"] is JToken res && res.Type == JTokenType.String)
                return res.ToString();
            if (result["error"] is JToken err && err.Type == JTokenType.String)
                return $"Error: {err}";

            // For array results, count items
            var firstArray = result.Properties()
                .FirstOrDefault(p => p.Value.Type == JTokenType.Array);
            if (firstArray != null)
                return $"{firstArray.Name}: {((JArray)firstArray.Value).Count} items — {result.ToString(Newtonsoft.Json.Formatting.None).Substring(0, Math.Min(200, result.ToString().Length))}";

            // Fallback: compact JSON
            string json = result.ToString(Newtonsoft.Json.Formatting.None);
            return json.Length > 400 ? json.Substring(0, 400) + "…" : json;
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

                // Track last known low-point outlet for chained hydrology commands
                (double X, double Y, double Elev)? lastHydroOutlet = null;

                for (int i = 0; i < commands.Count; i++)
                {
                    var command = commands[i];

                    if (IsHydrologyCommand(command.Action))
                    {
                        try
                        {
                            lastHydroOutlet = await HandleHydrologyCommandAsync(doc, command, reporter, lastHydroOutlet);
                            completedCount++;
                        }
                        catch (Exception hydEx)
                        {
                            errorCount++;
                            reporter.ReportError(command.Action, $"Hydrology error: {hydEx.Message}", hydEx.Message);
                        }
                    }
                    else if (IsMcpDirectCommand(command.Action))
                    {
                        // Route civil3d_* and create_cogo_point directly to MCP server (async, no CAD context lock)
                        try
                        {
                            await HandleMcpDirectCommandAsync(doc, command, reporter);
                            completedCount++;
                        }
                        catch (Exception mcpEx)
                        {
                            errorCount++;
                            reporter.ReportError(command.Action, $"MCP error: {mcpEx.Message}", mcpEx.Message);
                        }
                    }
                    else
                    {
                        await CoreApp.DocumentManager.ExecuteInCommandContextAsync((obj) =>
                        {
                            try
                            {
                                // Pass the command to the router with live progress reporting
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
                            return Task.CompletedTask;
                        }, null);
                    }

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
            public required StackPanel LogPanel { get; set; }
            public required TextBlock HeaderIcon { get; set; }
            public required TextBlock HeaderText { get; set; }
            public required ScrollViewer ScrollViewer { get; set; }
        }
    }
}