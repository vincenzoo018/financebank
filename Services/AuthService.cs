namespace FinanceBank.Services
{
    public class AuthService
    {
        public bool IsAuthenticated { get; private set; }
        public string? CurrentUser { get; private set; }
        public string? CurrentRole { get; private set; }

        public void Login(string username, string role)
        {
            IsAuthenticated = true;
            CurrentUser = username;
            CurrentRole = role;
        }

        public void Logout()
        {
            IsAuthenticated = false;
            CurrentUser = null;
            CurrentRole = null;
        }
    }
}
