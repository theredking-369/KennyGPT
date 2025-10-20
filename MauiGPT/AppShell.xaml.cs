namespace MauiGPT
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register ChatPage as a route (not a global route)
            Routing.RegisterRoute("ChatPage", typeof(Views.ChatPage));
        }
    }
}
