using Microsoft.JSInterop;

namespace socmed_front.Services
{
    public class ClientStateManager
    {
        private readonly IJSRuntime _jsRuntime;
        public bool IsConnected { get; private set; } = false;

        public ClientStateManager(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Проверяем, можем ли мы выполнить JS-вызов
                await _jsRuntime.InvokeVoidAsync("eval", "1");
                IsConnected = true;
            }
            catch
            {
                IsConnected = false;
            }
        }

        public bool CanAccessLocalStorage()
        {
            return IsConnected;
        }
    }
}