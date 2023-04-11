using MudBlazor;
using MudBlazor.Interfaces;

namespace JustABackup.Components.Forms
{
    public class ExtendedMudForm : MudForm
    {
        public HashSet<IFormComponent> Fields { get => base._formControls; }
    }
}
