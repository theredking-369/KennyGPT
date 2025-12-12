using MauiGPT.Interfaces;
using MauiGPT.Models;
using MauiGPT.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiGPT.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly IAzureService _azureService;
        private string _apiKey = string.Empty;
        private string _messageText = string.Empty;
        private string _systemPrompt = string.Empty;
        private string _conversationTitle = "New Conversation";
        private string? _currentConversationId;
        private bool _isSending;
        private bool _isLoadingConversations;

        public ChatViewModel(IAzureService azureService)
        {
            _azureService = azureService;
            Messages = new ObservableCollection<ChatMessageViewModel>();
            Conversations = new ObservableCollection<MConversation>();
            
            SendMessageCommand = new Command(async () => await SendMessageAsync(), () => !IsSending && !string.IsNullOrWhiteSpace(MessageText));
            NewConversationCommand = new Command(StartNewConversation);
            LoadConversationCommand = new Command<MConversation>(async (conv) => await LoadConversationAsync(conv));
            LogoutCommand = new Command(async () => await LogoutAsync());
            RefreshConversationsCommand = new Command(async () => await LoadConversationsAsync());
            DeleteConversationCommand = new Command<MConversation>(async (conv) => await DeleteConversationAsync(conv));
        }

        public ObservableCollection<ChatMessageViewModel> Messages { get; }
        public ObservableCollection<MConversation> Conversations { get; }

        public string MessageText
        {
            get => _messageText;
            set
            {
                if (SetProperty(ref _messageText, value))
                {
                    ((Command)SendMessageCommand).ChangeCanExecute();
                }
            }
        }

        public string SystemPrompt
        {
            get => _systemPrompt;
            set => SetProperty(ref _systemPrompt, value);
        }

        public string ConversationTitle
        {
            get => _conversationTitle;
            set => SetProperty(ref _conversationTitle, value);
        }

        public bool IsSending
        {
            get => _isSending;
            set
            {
                if (SetProperty(ref _isSending, value))
                {
                    ((Command)SendMessageCommand).ChangeCanExecute();
                }
            }
        }

        public bool IsLoadingConversations
        {
            get => _isLoadingConversations;
            set => SetProperty(ref _isLoadingConversations, value);
        }

        public ICommand SendMessageCommand { get; }
        public ICommand NewConversationCommand { get; }
        public ICommand LoadConversationCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RefreshConversationsCommand { get; }
        public ICommand DeleteConversationCommand { get; }

        public async Task InitializeAsync()
        {
            _apiKey = await SecureStorage.GetAsync("api_key") ?? string.Empty;
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            await LoadConversationsAsync();
        }

        private async Task LoadConversationsAsync()
        {
            IsLoadingConversations = true;
            
            try
            {
                var conversations = await _azureService.GetConversationsAsync(_apiKey);
                
                Conversations.Clear();
                foreach (var conv in conversations.OrderByDescending(c => c.CreatedAt))
                {
                    Conversations.Add(conv);
                }

                if (Conversations.Count > 0 && string.IsNullOrEmpty(_currentConversationId))
                {
                    await LoadConversationAsync(Conversations[0]);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load conversations: {ex.Message}", "OK");
            }
            finally
            {
                IsLoadingConversations = false;
            }
        }

        private async Task LoadConversationAsync(MConversation conversation)
        {
            try
            {
                var messages = await _azureService.GetConversationMessagesAsync(conversation.Id, _apiKey);
                
                Messages.Clear();
                foreach (var msg in messages.OrderBy(m => m.Timestamp))
                {
                    Messages.Add(new ChatMessageViewModel
                    {
                        Content = msg.Content,
                        IsUser = msg.Role == "user",
                        Timestamp = msg.Timestamp
                    });
                }

                _currentConversationId = conversation.Id;
                ConversationTitle = conversation.Title;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load conversation: {ex.Message}", "OK");
            }
        }

        private void StartNewConversation()
        {
            _currentConversationId = null;
            ConversationTitle = "New Conversation";
            Messages.Clear();
            MessageText = string.Empty;
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(MessageText))
                return;

            var userMessage = MessageText;
            MessageText = string.Empty;

            Messages.Add(new ChatMessageViewModel
            {
                Content = userMessage,
                IsUser = true,
                Timestamp = DateTime.Now
            });

            var loadingMessage = new ChatMessageViewModel
            {
                Content = "Thinking...",
                IsUser = false,
                Timestamp = DateTime.Now,
                IsLoading = true
            };
            Messages.Add(loadingMessage);

            IsSending = true;

            try
            {
                var request = new MChatRequest
                {
                    Message = userMessage,
                    ConversationId = _currentConversationId,
                    SystemPrompt = string.IsNullOrWhiteSpace(SystemPrompt) ? null : SystemPrompt
                };

                var response = await _azureService.SendMessageAsync(request, _apiKey);

                Messages.Remove(loadingMessage);

                Messages.Add(new ChatMessageViewModel
                {
                    Content = response.Response,
                    IsUser = false,
                    Timestamp = response.Timestamp
                });

                _currentConversationId = response.ConversationId;

                await LoadConversationsAsync();
            }
            catch (Exception ex)
            {
                Messages.Remove(loadingMessage);

                Messages.Add(new ChatMessageViewModel
                {
                    Content = $"❌ Error: {ex.Message}",
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                IsSending = false;
            }
        }

        private async Task LogoutAsync()
        {
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Logout", 
                "Are you sure you want to logout?", 
                "Yes", 
                "No");

            if (confirm)
            {
                SecureStorage.Remove("api_key");
                SecureStorage.Remove("session_id");
                SecureStorage.Remove("user_id");
                
                await Shell.Current.GoToAsync("..");
            }
        }

        private async Task DeleteConversationAsync(MConversation conversation)
        {
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Delete Conversation",
                $"Are you sure you want to delete '{conversation.Title}'?",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            try
            {
                var success = await _azureService.DeleteConversationAsync(conversation.Id, _apiKey);

                if (success)
                {
                    Conversations.Remove(conversation);

                    if (_currentConversationId == conversation.Id)
                    {
                        StartNewConversation();
                    }

                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        "Conversation deleted successfully",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Failed to delete conversation: {ex.Message}",
                    "OK");
            }
        }
    }

    public class ChatMessageViewModel : ViewModelBase
    {
        private string _content = string.Empty;
        private bool _isUser;
        private DateTime _timestamp;
        private bool _isLoading;

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public bool IsUser
        {
            get => _isUser;
            set => SetProperty(ref _isUser, value);
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string FormattedTime => Timestamp.ToString("HH:mm");
    }
}
