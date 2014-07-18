using System;
using System.Windows.Forms;
using Fiddler;
using Launchpad.Properties;

[assembly: RequiredVersion("4.0.0.0")]


namespace Launchpad
{

    public class LaunchpadExtension : IFiddlerExtension
    {
        private readonly TabPage _tab;
        private readonly LaunchpadView _view;


        public LaunchpadExtension()
        {
            _tab = new TabPage("Launchpad");
            
            
            _view = new LaunchpadView
            {
                Dock = DockStyle.Fill
            };
            
            _tab.Controls.Add(_view);

            _view.RestoreBookmarks();
        }

        public void OnBeforeUnload()
        {
        }

        public void OnLoad()
        {
            FiddlerApplication.UI.tabsViews.TabPages.Add(_tab);
            FiddlerApplication.UI.tabsViews.SelectedIndexChanged += OnTabSelected;

            FiddlerApplication.UI.tabsViews.ImageList.Images.Add(
                @"launchpad-icon", Resources.icon);
            _tab.ImageKey = @"launchpad-icon";
        }

        private void OnTabSelected(object sender, EventArgs args)
        {
            if (FiddlerApplication.isClosing || FiddlerApplication.UI.tabsViews.SelectedTab != _tab)
            {
                return;
            }
            EnsureReady();
        }

        private void EnsureReady()
        {
            FiddlerApplication.UI.tabsViews.SelectedIndexChanged -= OnTabSelected;

            // Setup Fiddler things
        }
    }
}
