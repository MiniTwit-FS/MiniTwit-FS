using Microsoft.AspNetCore.Components;

namespace MiniTwitClient.Components
{
    public partial class Tweet : ComponentBase
    {
        [Parameter] public string? Username { get; set; }
    }
}
