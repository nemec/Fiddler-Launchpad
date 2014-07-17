using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fiddler;

[assembly: RequiredVersion("4.0.0.0")]


namespace Launchpad
{

    public class Launchpad : IFiddlerExtension
    {
        private readonly TabPage _tab;
        private readonly LaunchpadView _view;

        private static readonly object TreeviewNodeSentinel = new object();

        public Launchpad()
        {
            _tab = new TabPage("Launchpad");
            
            _view = new LaunchpadView
            {
                Dock = DockStyle.Fill
            };
            
            _tab.Controls.Add(_view);
            var treeview = _view.BookmarkTreeview;
            treeview.DragDrop += OnItemDropped;
            treeview.DragOver += OnDragOver;
            treeview.ItemDrag += OnItemDrag;

            RestoreBookmarks();
        }

        private void RestoreBookmarks()
        {
            var tree = _view.BookmarkTreeview;

            var node = tree.Nodes.Add("GMD - ITG");
            node.Tag = TreeviewNodeSentinel;
            node.Expand();
        }

        public void OnBeforeUnload()
        {
        }

        public void OnLoad()
        {
            FiddlerApplication.UI.tabsViews.TabPages.Add(_tab);
            FiddlerApplication.UI.tabsViews.SelectedIndexChanged += OnTabSelected;

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

        // Drag a treeview node (to session window)
        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            
        }

        private IEnumerable<Session> GetSelectedSessions()
        {
            var tree = _view.BookmarkTreeview;
            var rootNode = tree.SelectedNode;

            var session = rootNode.Tag as Session;
            return session != null 
                ? new[]{session}
                : GetRecursiveSessionsForNode(rootNode);
        }

        private IEnumerable<Session> GetRecursiveSessionsForNode(TreeNode rootNode)
        {
            var sessions = new List<Session>();
            foreach (TreeNode node in rootNode.Nodes)
            {
                var session = node.Tag as Session;
                if (session != null)
                {
                    sessions.Add(session);
                }
                else
                {
                    sessions.AddRange(GetRecursiveSessionsForNode(rootNode));
                }
            }
            return sessions;
        }

        // Drag session over tree node (not leaf)
        private void OnDragOver(object sender, DragEventArgs e)
        {
            var tree = _view.BookmarkTreeview;
            var dropPt = tree.PointToClient(new Point(e.X, e.Y));
            var destinationNode = tree.GetNodeAt(dropPt);
            if (destinationNode == null)
            {
                return;
            }

            var isInnerNode = ReferenceEquals(destinationNode.Tag, TreeviewNodeSentinel);
            e.Effect = isInnerNode &&
                       (e.Data.GetDataPresent("Fiddler.Session[]") ||
                       e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        // Drop session into tree
        private void OnItemDropped(object sender, DragEventArgs e)
        {
            var sessionArray = (Session[])e.Data.GetData("Fiddler.Session[]");
            var tree = _view.BookmarkTreeview;
            if (sessionArray == null || sessionArray.Length < 1 || 
                tree == null)
            {
                return;
            }

            var dropPt = tree.PointToClient(new Point(e.X, e.Y));
            var destinationNode = tree.GetNodeAt(dropPt);
            if (destinationNode == null)
            {
                return;
            }

            Func<Session, bool> filterExisting = s =>
                destinationNode.Nodes.OfType<TreeNode>().All(n =>
                    ReferenceEquals(n.Tag, TreeviewNodeSentinel) || ((Session) n.Tag).id != s.id);

            foreach (var session in sessionArray.Where(filterExisting))
            {
                var label = session.url;
                destinationNode.Nodes.Add(new TreeNode(label)
                {
                    Tag = session
                });
            }

        }
    }
}
