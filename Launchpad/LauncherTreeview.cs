using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launchpad
{
    public partial class LauncherTreeview : TreeView
    {
        public LauncherTreeview()
        {
            InitializeComponent();
        }

        protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
        {
            var before = e.Node != null ? e.Node.Text : null;

            base.OnBeforeLabelEdit(e);

            if (!e.CancelEdit && e.Node != null)
            {
                var after = e.Node.Text;
                if (before != after)
                {
                    e.CancelEdit = true;
                    BeginInvoke((Action)(() => e.Node.BeginEdit()));
                }
            }
        }
    }
}
