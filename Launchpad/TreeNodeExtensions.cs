using System.Windows.Forms;
using Fiddler;

namespace Launchpad
{
    internal static class TreeNodeExtensions
    {
        public static LauncherNodeType NodeType(this TreeNode node)
        {
            if (node == null)
            {
                return LauncherNodeType.None;
            }
            if (ReferenceEquals(node.Tag, LaunchpadView.TreeviewNodeSentinel))
            {
                return LauncherNodeType.InnerNode;
            }
            if (node.Tag is Session)
            {
                return LauncherNodeType.SessionNode;
            }
            return LauncherNodeType.None;
        }

        public static Session GetSession(this TreeNode node)
        {
            if (node == null || node.NodeType() != LauncherNodeType.SessionNode)
            {
                return null;
            }
            return (Session) node.Tag;
        }
    }
}
