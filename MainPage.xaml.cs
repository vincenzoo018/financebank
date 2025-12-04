namespace FinanceBank
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            // Set fullscreen on load
            Loaded += OnPageLoaded;
        }
        
        private void OnPageLoaded(object? sender, EventArgs e)
        {
            // Maximize window on load (windowed fullscreen with title bar)
#if WINDOWS
            var window = this.GetParentWindow();
            if (window != null)
            {
                var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                    
                    if (appWindow != null)
                    {
                        // Use Overlapped presenter (normal window with title bar) and maximize it
                        var presenter = Microsoft.UI.Windowing.OverlappedPresenter.Create();
                        presenter.Maximize();
                        appWindow.SetPresenter(presenter);
                    }
                }
            }
#endif
        }
    }
}

