using System.Text;
using Fiddler;

namespace Launchpad
{
    internal static class SessionExtensions
    {
        public const string SessionAliasKey = "launchpad.alias";

        private const string AliasDelimiter = "-";

        public static void SetSessionAlias(this Session session, string label)
        {
            if (label == null)
            {
                return;
            }
            session.oFlags[SessionAliasKey] = label
                .Replace(AliasDelimiter, "")
                .Trim();
        }

        public static string GetSessionAlias(this Session session)
        {
            return session.oFlags[SessionAliasKey];
        }

        public static string GetDisplayLabel(this Session session)
        {
            var builder = new StringBuilder();
            var alias = session.GetSessionAlias();
            if (alias != null)
            {
                builder.Append(alias);
                builder.Append(" ");
                builder.Append(AliasDelimiter);
                builder.Append(" ");
            }
            builder.Append(session.url);
            return builder.ToString();
        }
    }
}
