using Microsoft.AspNetCore.Components;
using MiniTwitClient.Models;

namespace MiniTwitClient.Components
{
    public partial class Tweet : ComponentBase
    {
        [Parameter] public Message Message { get; set; }
    }
}
