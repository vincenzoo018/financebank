using FinanceBank.Services.SaaS;

namespace FinanceBank
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Initialize SaaS database on startup (safe initialization)
            try
            {
                Task.Run(async () => 
                {
                    try
                    {
                        await SaaSDbInitializer.InitializeAsync(_serviceProvider);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SaaS initialization error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App startup error: {ex.Message}");
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "FinanceBank" };
        }
    }
}

