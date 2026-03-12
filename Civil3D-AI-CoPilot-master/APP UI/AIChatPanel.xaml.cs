using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Microsoft.Win32;

namespace Cad_AI_Agent.UI
{
    // --- ახალი მოდელები ჩატის ისტორიისთვის ---
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

    // --- მთავარი ლოგიკა ---
    public partial class AIChatPanel : UserControl
    {
        private DispatcherTimer _thinkingTimer;
        private int _dotCount = 0;
        private TextBlock _currentThinkingText;

        // სესიების მართვა
        private List<ChatSession> _allSessions = new List<ChatSession>();
        private ChatSession _currentSession;

        public AIChatPanel()
        {
            InitializeComponent();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            _thinkingTimer = new DispatcherTimer();
            _thinkingTimer.Interval = TimeSpan.FromMilliseconds(500);
            _thinkingTimer.Tick += ThinkingTimer_Tick;

            StartNewSession(); // პირველი ჩართვისას იწყებს ახალ ჩატს
   
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CadAiAgent"))
            {
                if (key != null)
                {
                    ApiKeyBox.Text = key.GetValue("ApiKey")?.ToString() ?? "";
                }
            }
        }

        // ================= ისტორიის მართვის ლოგიკა =================
        private void StartNewSession()
        {
            _currentSession = new ChatSession();
            _allSessions.Insert(0, _currentSession); // ვამატებთ სიის თავში
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
                // ვქმნით Grid-ს სათაურისთვის და წაშლის ღილაკისთვის
                Grid sessionGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
                sessionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                sessionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // ჩატის ჩამრთველი ღილაკი
                Button loadBtn = new Button
                {
                    Content = session.Title,
                    Background = session == _currentSession ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")) : Brushes.Transparent,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4D4D4")),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(10, 8, 10, 8),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Cursor = Cursors.Hand,
                    ToolTip = session.Title
                };
                
                loadBtn.Click += (s, e) => { LoadSessionToUI(session); RefreshSidebarUI(); };
                Grid.SetColumn(loadBtn, 0);
                loadBtn.Style = (Style)FindResource("SidebarButtonStyle");

                // === დაამატე ეს ახალი ბლოკი RENAME (Right-Click) ფუნქციისთვის ===
                ContextMenu ctxMenu = new ContextMenu();
                MenuItem renameItem = new MenuItem { Header = "✏️ Rename" };
                renameItem.Click += (s, ev) =>
                {
                    // ვქმნით დროებით TextBox-ს ტექსტის ჩასასწორებლად
                    TextBox renameBox = new TextBox
                    {
                        Text = session.Title,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E")),
                        Foreground = Brushes.White,
                        Padding = new Thickness(5),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0696D7"))
                    };

                    // ვინახავთ სახელს Enter-ზე დაჭერისას
                    renameBox.KeyDown += (senderBox, args) => {
                        if (args.Key == Key.Enter) { session.Title = renameBox.Text; RefreshSidebarUI(); }
                        else if (args.Key == Key.Escape) { RefreshSidebarUI(); }
                    };

                    // ვინახავთ სახელს ფოკუსის დაკარგვისას (სხვაგან დაკლიკებისას)
                    renameBox.LostFocus += (senderBox, args) => { session.Title = renameBox.Text; RefreshSidebarUI(); };

                    // ვცვლით ღილაკს ამ TextBox-ით
                    Grid.SetColumn(renameBox, 0);
                    sessionGrid.Children.Remove(loadBtn);
                    sessionGrid.Children.Insert(0, renameBox);

                    // ფოკუსი ავტომატურად გადაგვაქვს ტექსტზე
                    Dispatcher.BeginInvoke(new Action(() => {
                        renameBox.Focus();
                        renameBox.SelectAll();
                    }), DispatcherPriority.Input);
                };
                ctxMenu.Items.Add(renameItem);
                loadBtn.ContextMenu = ctxMenu;
                // ==========================================================

                // წაშლის ღილაკი
                Button delBtn = new Button
                {
                    Content = "🗑️",
                    Background = Brushes.Transparent,
                    Foreground = Brushes.Gray,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(5),
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

        // ================= ჩატის ვიზუალიზაცია =================
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

            Border bubble = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = isUser ? new Thickness(40, 0, 0, 10) : new Thickness(0, 0, 40, 10),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Background = isUser ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0696D7"))
                                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"))
            };

            // TextBlock-ის მაგივრად ვიყენებთ TextBox-ს, რათა ტექსტის მონიშვნა შეგეძლოს
            TextBox txtBox = new TextBox
            {
                Text = text,
                Foreground = new SolidColorBrush(Colors.White),
                Background = Brushes.Transparent, // ფონი გამჭვირვალეა, რომ Border-ის ფერი გამოჩნდეს
                BorderThickness = new Thickness(0),
                IsReadOnly = true, // აკრძალულია ჩასწორება, შესაძლებელია მხოლოდ მონიშვნა
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"))
            };

            bubble.Child = txtBox;
            ChatHistoryPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToEnd();

            // ვაბრუნებთ TextBlock-ს ანიმაციისთვის (დროებითი ხრიკი, რადგან დანარჩენი კოდი TextBlock-ს ელოდება)
            TextBlock hiddenTextBlockForThinking = new TextBlock();
            txtBox.Tag = hiddenTextBlockForThinking;
            txtBox.TextChanged += (s, e) => { hiddenTextBlockForThinking.Text = txtBox.Text; };
            hiddenTextBlockForThinking.TargetUpdated += (s, e) => { txtBox.Text = hiddenTextBlockForThinking.Text; };

            // ეს ფუნქცია მხოლოდ ანიმაციისთვის გვიბრუნებს ობიექტს
            TextBlock proxyText = new TextBlock();
            proxyText.DataContext = txtBox;
            proxyText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Text") { Mode = System.Windows.Data.BindingMode.TwoWay });
            return proxyText;
        }

        // ================= ტაბები & API =================
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
                string testUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={key}";
                using var client = new HttpClient();
                var content = new StringContent("{\"contents\":[{\"parts\":[{\"text\":\"Hello\"}]}]}", Encoding.UTF8, "application/json");
                var response = await client.PostAsync(testUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    ConnectionStatusText.Text = "✅ Connected!";
                    using (RegistryKey regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\CadAiAgent"))
                    {
                        regKey.SetValue("ApiKey", key);
                    }
                    ConnectionStatusText.Foreground = Brushes.LightGreen;
                }
                else
                {
                    // === აქ ვიჭერთ Google-ის რეალურ პასუხს და ვაგდებთ ეკრანზე ===
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    ConnectionStatusText.Text = "❌ Connection Failed";
                    ConnectionStatusText.Foreground = Brushes.Red;
                    MessageBox.Show($"Google API Error:\n\n{errorDetails}", "API Error Details", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                ConnectionStatusText.Text = "❌ Connection Failed";
                ConnectionStatusText.Foreground = Brushes.Red;
            }
            finally { TestConnectionBtn.IsEnabled = true; }
        }

        // ================= GEMINI COMMUNICATION =================
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

            if (string.IsNullOrEmpty(message)) return;
            if (string.IsNullOrEmpty(apiKey))
            {
                AddMessageToChat("⚠️ Please enter your API Key in the Settings tab first.", false, false);
                return;
            }

            // 1. აქ ვკითხულობთ, რომელი მოდელი აირჩია მომხმარებელმა Settings ტაბში
            string selectedModel = "gemini-2.5-flash"; // Default მნიშვნელობა
            if (ProviderCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                selectedModel = selectedItem.Tag.ToString();
            }

            AddMessageToChat(message, true);
            UserInputBox.Clear();
            UserInputBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            _currentThinkingText = AddMessageToChat("Thinking", false, false);
            _dotCount = 0;
            _thinkingTimer.Start();

            try
            {
                // 2. აქ ვაწვდით არჩეულ მოდელს GetGeminiResponse ფუნქციას
                string jsonPayload = await GetGeminiResponse(message, apiKey, selectedModel);
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

        private async Task<string> GetGeminiResponse(string prompt, string key, string modelName)
        {
            // 3. URL დინამიურად იწყობა არჩეული მოდელის მიხედვით
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={key}";
            using var client = new HttpClient();

            // მოგვაქვს პრომპტი ცალკე კლასიდან!
            string systemInstruction = Core.AgentPromptManager.GetSystemInstruction();

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
            // თუ ერორია, ვისვრით Google-ის რეალურ ტექსტს
            string errorRaw = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error ({modelName}): {errorRaw}");
        }

        private async Task ExecuteCadCommandsLive(List<CadCommand> commands)
        {
            try
            {
                Document doc = CoreApp.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                foreach (var command in commands)
                {
                    await CoreApp.DocumentManager.ExecuteInCommandContextAsync(async (obj) =>
                    {
                        // ახლა პირდაპირ როუტერს გადავცემთ ბრძანებას!
                        CommandRouter.Execute(doc, command);

                        doc.Editor.UpdateScreen();
                    }, null);

                    await Task.Delay(300);
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"[Draw Error]: {ex.Message}", false);
            }
        }
    }
}