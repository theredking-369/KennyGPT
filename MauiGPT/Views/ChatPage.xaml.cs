using MauiGPT.ViewModels;
using MauiGPT.Services;

namespace MauiGPT.Views
{
    public partial class ChatPage : ContentPage
    {
        private readonly ChatViewModel _viewModel;
        private bool _isSidebarOpen = false;

        public ChatPage(ChatViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            if (_isSidebarOpen)
            {
                await CloseSidebar();
            }
            else
            {
                await OpenSidebar();
            }
        }

        private async void OnOverlayTapped(object sender, EventArgs e)
        {
            await CloseSidebar();
        }

        private void OnThemeToggled(object sender, EventArgs e)
        {
            ThemeService.Instance.ToggleTheme();
        }

        private async Task OpenSidebar()
        {
            _isSidebarOpen = true;
            SidebarOverlay.IsVisible = true;
            await Sidebar.TranslateTo(0, 0, 250, Easing.CubicOut);
        }

        private async Task CloseSidebar()
        {
            await Sidebar.TranslateTo(-300, 0, 250, Easing.CubicIn);
            SidebarOverlay.IsVisible = false;
            _isSidebarOpen = false;
        }
    }
}
