using Microsoft.AspNetCore.Components;

namespace MiniTwitClient.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        private bool IsDropdownOpen = false;

        private void ToggleDropdown()
        {
            IsDropdownOpen = !IsDropdownOpen;
        }

        private void CloseDropdown()
        {
            IsDropdownOpen = false;
        }
    }
}
