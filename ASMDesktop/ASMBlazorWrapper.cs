using System.Drawing;
using System.Windows.Forms;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace ASM.GUI.Desktop
{
    public partial class ASMBlazorWrapper : Form
    {
        public ASMBlazorWrapper()
        {
            InitializeComponent();

            Text = $"ASM {ASM.Lib.ASMCore.VERSION} for DCS World";

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddWindowsFormsBlazorWebView();
            serviceCollection.AddBlazoredLocalStorage();
            var blazor = new BlazorWebView()
            {
                Dock = DockStyle.Fill,
                HostPage = "wwwroot/index.html",
                Services = serviceCollection.BuildServiceProvider()
            };
            blazor.RootComponents.Add<App>("#app");
            Controls.Add(blazor);
        }

    }
}
