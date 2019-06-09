using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("DiscordCommandHandler", "noname", "0.0.1")]
    [Description("Analyzes and processes messages from Discord")]
    class DiscordCommandHandler : CovalencePlugin
    {
        [PluginReference] Plugin DiscordLinker;

        object OnDiscordCommand(string username, string msg, string[] args)
        {
            if (args[0] == "test")
            {
                DiscordLinker?.Call("AddMsgOnJsonQueue", null, "HelloWorld!!", true, true);
                return true;
            }
            return null;
        }
    }
}