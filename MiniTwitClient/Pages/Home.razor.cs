using Microsoft.AspNetCore.Components;

namespace MiniTwitClient.Pages
{
    public partial class Home : ComponentBase
    {
        private string InputString { get; set; }

        private void ButtonClick()
        {
            Console.WriteLine($"You clicked the button: {InputString}");
        }
    }
}
