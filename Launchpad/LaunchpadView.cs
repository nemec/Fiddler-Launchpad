using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Fiddler;

namespace Launchpad
{
    public partial class LaunchpadView : UserControl
    {
        public static readonly object TreeviewNodeSentinel = new object();

        private const string NewNodeLabel = "New group";


        public LaunchpadView()
        {
            InitializeComponent();

            LauncherTreeview.MouseUp += NodeMouseUpHandler;
            LauncherTreeview.NodeMouseDoubleClick += OnDoubleClick;
            LauncherTreeview.KeyUp += NodeKeyUpHandler;
            LauncherTreeview.DragDrop += OnItemDropped;
            LauncherTreeview.DragOver += OnDragOver;
            LauncherTreeview.ItemDrag += OnItemDrag;
            LauncherTreeview.BeforeLabelEdit += OnStartRenameNode;
            LauncherTreeview.AfterLabelEdit += OnRenameNode;
        }

        public void RestoreBookmarks()
        {
            var node = LauncherTreeview.Nodes.Add("GMD - ITG");
            node.Tag = TreeviewNodeSentinel;
            node.Expand();
        }

        private void OnDoubleClick(object sender, MouseEventArgs e)
        {
            var node = LauncherTreeview.SelectedNode;
            if (node.NodeType() == LauncherNodeType.SessionNode)
            {
                SendSessionToComposeTab(node.GetSession());
            }
        }

        // Drag a treeview node (to session window)
        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            LauncherTreeview.DoDragDrop(GetSelectedSessions(),
                DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Scroll);
        }// Drag session over tree node (not leaf)
        private void OnDragOver(object sender, DragEventArgs e)
        {
            var tree = LauncherTreeview;
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
            if (sessionArray == null || sessionArray.Length < 1)
            {
                return;
            }

            var destinationNode = GetNodeAtPoint(e.X, e.Y);

            if (destinationNode == null)
            {
                return;
            }

            Func<Session, bool> nodeDoesNotExist = s =>
                destinationNode.Nodes.OfType<TreeNode>().All(n =>
                    n.NodeType() == LauncherNodeType.InnerNode || ((Session)n.Tag).id != s.id);

            foreach (var session in sessionArray.Where(nodeDoesNotExist))
            {
                destinationNode.Nodes.Add(new TreeNode(session.GetDisplayLabel())
                {
                    Tag = session
                });
                destinationNode.Expand();
            }

        }

        private void OnStartRenameNode(object sender, NodeLabelEditEventArgs e)
        {
            var session = e.Node.GetSession();
            if (session != null)
            {
                e.Node.Text = session.GetSessionAlias();
            }
        }

        private void OnRenameNode(object sender, NodeLabelEditEventArgs e)
        {
            var session = e.Node.GetSession();
            if (session != null)
            {
                session.SetSessionAlias(e.Label);
                LauncherTreeview.BeginInvoke((Action)(
                    () => e.Node.Text = session.GetDisplayLabel()));
            }
        }

        private void NodeMouseUpHandler(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 1 && e.Button == MouseButtons.Right)
            {
                var destinationNode = LauncherTreeview.GetNodeAt(e.X, e.Y);
                if (destinationNode == null)
                {
                    OnRightClickBackground(e);
                    return;
                }
                switch (destinationNode.NodeType())
                {
                    case LauncherNodeType.InnerNode:
                        OnRightClickInnerNode(destinationNode, e);
                        break;
                    case LauncherNodeType.SessionNode:
                        OnRightClickSession(destinationNode, e);
                        break;
                }
            }
        }

        private void OnRightClickBackground(MouseEventArgs e)
        {
            var menu = new ContextMenu();
            AddMenuItem(menu, "Add node group", CreateNewInnerNode, null);
            menu.Show(LauncherTreeview, e.Location);
        }

        private void OnRightClickInnerNode(TreeNode node, MouseEventArgs e)
        {
            var menu = new ContextMenu();
            AddMenuItem(menu, "Add node group", CreateNewInnerNode, node);
            AddMenuItem(menu, "Delete", RemoveNodeAndWarn, node);
            menu.Show(LauncherTreeview, e.Location);
        }

        private void OnRightClickSession(TreeNode node, MouseEventArgs e)
        {
            var session = node.GetSession();

            var menu = new ContextMenu();
            AddMenuItem(menu,
                "Open in Compose", 
                (sender, args) => SendSessionToComposeTab((Session) ((MenuItem) sender).Tag),
                session);
            var openInBrowserItem = AddMenuItem(menu, 
                "Open in Browser", 
                (sender, args) => OpenSessionInBrowser((Session) ((MenuItem) sender).Tag),
                session);

            if (!CanOpenSessionInBrowser(session))
            {
                openInBrowserItem.Enabled = false;
                openInBrowserItem.Text += @" - GET requests only";
            }

            menu.Show(LauncherTreeview, e.Location);
        }

        private void NodeKeyUpHandler(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Delete:
                    RemoveNodeAndWarn(new MenuItem
                    {
                        Tag = LauncherTreeview.SelectedNode
                    }, null);
                    break;
            }
        }

        private TreeNode GetNodeAtPoint(int x, int y)
        {
            var dropPt = LauncherTreeview.PointToClient(new Point(x, y));
            return LauncherTreeview.GetNodeAt(dropPt);
        }

        private void CreateNewInnerNode(object sender, EventArgs e)
        {
            var menu = sender as MenuItem;
            if (menu == null)
            {
                return;
            }
            TreeNode newNode;

            var groupNode = menu.Tag as TreeNode;
            if (groupNode != null)
            {
                newNode = groupNode.Nodes.Add(NewNodeLabel);
            }
            else
            {
                newNode = LauncherTreeview.Nodes.Add(NewNodeLabel);
            }
            newNode.Tag = TreeviewNodeSentinel;
            newNode.BeginEdit();
        }

        private static void RemoveNodeAndWarn(object sender, EventArgs e)
        {
            var menu = sender as MenuItem;
            if (menu == null)
            {
                return;
            }

            var node = menu.Tag as TreeNode;
            if (node == null)
            {
                return;
            }

            var message = "Are you sure?";
            switch (node.NodeType())
            {
                case LauncherNodeType.InnerNode:
                    message = "Are you sure? This will delete all groups and nodes nested within.";
                    break;
                case LauncherNodeType.SessionNode:
                    message = "Are you sure you want to delete this session?";
                    break;
            }

            var confirm = MessageBox.Show(
                message, @"Confirm delete", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.Yes)
            {
                node.Remove();
            }
        }

        private static MenuItem AddMenuItem(
            Menu menu, string text, EventHandler handler, object context,
            Shortcut shortcut = Shortcut.None)
        {
            var item = new MenuItem(text, handler)
            {
                Tag = context,
                Shortcut = shortcut,
                ShowShortcut = shortcut != Shortcut.None
            };
            menu.MenuItems.Add(item);
            return item;
        }

        private static void SendSessionToComposeTab(Session session)
        {
            FiddlerApplication.DoComposeByCloning(session);
        }

        private static bool CanOpenSessionInBrowser(Session session)
        {
            return session.RequestMethod.ToLowerInvariant() == "get";
        }

        private static void OpenSessionInBrowser(Session session)
        {
            if (!CanOpenSessionInBrowser(session))
            {
                throw new ArgumentException(String.Format(
                    "Only GET requests may be opened in the browser. " +
                    "Failed opening {0} request to {1}.",
                    session.RequestMethod,
                    session.url), "session");
            }
            Process.Start(session.fullUrl);
        }

        private Session[] GetSelectedSessions()
        {
            var rootNode = LauncherTreeview.SelectedNode;

            var session = rootNode.Tag as Session;
            return session != null
                ? new[] { session }
                : GetRecursiveSessionsForNode(rootNode).ToArray();
        }

        private static IEnumerable<Session> GetRecursiveSessionsForNode(TreeNode rootNode)
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
    }
}
