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
            
            // Initialize SaaS database on startup
            Task.Run(async () => await SaaSDbInitializer.InitializeAsync(_serviceProvider));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "FinanceBank" };
        }
    }
}

