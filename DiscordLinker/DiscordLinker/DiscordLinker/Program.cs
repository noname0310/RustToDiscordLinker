using System;
using System.Timers;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordLinker
{
    class Program
    {
        public static DiscordSocketClient Client;
        public static Timer timer = new Timer();

        public static ConfigFileManager fileManager = new ConfigFileManager();
        static IPCManager IPCManager = new IPCManager();

        static void Main(string[] args)
        {
            Console.WriteLine(
                @"  ______  _                            _  _      _         _               " + "\n" +
                @"  |  _  \(_)                          | || |    (_)       | |              " + "\n" +
                @"  | | | | _  ___   ___  ___   _ __  __| || |     _  _ __  | | __ ___  _ __ " + "\n" +
                @"  | | | || |/ __| / __|/ _ \ | '__|/ _` || |    | || '_ \ | |/ // _ \| '__|" + "\n" +
                @"  | |/ / | |\__ \| (__| (_) || |  | (_| || |____| || | | ||   <|  __/| |   " + "\n" +
                @"  |___/  |_||___/ \___|\___/ |_|   \__,_|\_____/|_||_| |_||_|\_\\___||_|   " + "\n" +
                "\n" +
                "  V1.0.0                                                    made by noname" +
                "\n"
                );
            if (fileManager.CheckandCreateConfig() == false) return;

            timer.Interval = 1000; // 1s
            timer.Elapsed += new ElapsedEventHandler(timer_ElapsedAsync);

            try
            {
                new Program().Start().GetAwaiter().GetResult();
            }
            catch(Discord.Net.HttpException)
            {
                Console.WriteLine("api키가 잘못되었습니다");
                Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("ERROR!");
                Console.ReadLine();
            }
        }

        //static bool WaitNextConnection = true;

        static async void timer_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            /*bool NetAvailable = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (NetAvailable == true)
            {
                if (WaitNextConnection == true)
                {
                    await Client.LogoutAsync();
                    await Client.StopAsync();
                    Client.Dispose();
                    Client = new DiscordSocketClient();

                    await Client.LoginAsync(Discord.TokenType.Bot, fileManager.ConfigJson["apikey"].ToString(), true);
                    await Client.StartAsync();

                    Client.Ready += Client_Ready;
                    Client.MessageReceived += HandleCommand;
                }
            }
            else
            {
                WaitNextConnection = true;
            }*/

            string msg = IPCManager.check_printDataFile(fileManager.ConfigJson["DataFileLocate"].ToString());
            if (msg == null)
                return;

            ISocketMessageChannel channel = (Client.GetChannel(Convert.ToUInt64(fileManager.ConfigJson["DisplayRoom"].ToString())) as ISocketMessageChannel);

            Console.WriteLine(msg);
            try
            {
                await channel.SendMessageAsync(msg);
            }
            catch { }
        }

        public async Task Start()
        {
            Client = new DiscordSocketClient();

            await Client.LoginAsync(Discord.TokenType.Bot, fileManager.ConfigJson["apikey"].ToString(), true);
            await Client.StartAsync();

            Client.Ready += Client_Ready;
            Client.MessageReceived += HandleCommand;

            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            Console.WriteLine("The bot is online");
            Console.WriteLine("");

            ISocketMessageChannel channel = (Client.GetChannel(Convert.ToUInt64(fileManager.ConfigJson["DisplayRoom"].ToString())) as ISocketMessageChannel);
            await channel.SendMessageAsync("```디스코드 링커가 켜졌습니다```\n\n");

            timer.Start();
        }

        public Task HandleCommand(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return null;
            if (msg.Author.IsBot) return null;
            if (msg.Channel.Id.ToString() != fileManager.ConfigJson["DisplayRoom"].ToString()) return null;

            Console.WriteLine(msg.Author.Username + ": " + msg);
            IPCManager.AddMsgOnJsonQueue(fileManager.ConfigJson["DataFileLocate"].ToString(), msg.Author.Username, msg.ToString());
            return null;
        }
    }
}
