using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net;

namespace testBot
{
    class Program
    {

        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();
            string token = "***REMOVED***";

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            await InstallCommandsAsync();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage; // Nachricht
            if (msg == null) return; //Keine Systemnachrichten

            if (msg.Author.Id == client.CurrentUser.Id || msg.Author.IsBot) return; // keine Antwort auf Bots

            int argPos = 0;

            var context = new SocketCommandContext(client, msg);
            var result = await commands.ExecuteAsync(context, argPos, services);
            var imagesPath = Path.GetFullPath(@"..\..") + "\\images\\";
            var cemserPath = Path.GetFullPath(@"..\..") + "\\cemser\\";

            if (!(Directory.Exists(imagesPath)))
            {
                Console.WriteLine(imagesPath);
                Console.WriteLine("Wrong Directory");
            }

            if (!(Directory.Exists(cemserPath)))
            {
                Console.WriteLine(cemserPath);
                Console.WriteLine("Wrong Directory");
            }

            if (msg.HasCharPrefix('!', ref argPos) /*|| message.HasMentionPrefix(client.CurrentUser, ref argPos))*/)
            {
                var parts = msg.Content.Split(' ');
                switch (parts[0])
                {
                    case "!sieg":
                        await context.Channel.SendMessageAsync("HEIL!");
                        break;
                    case "!meme":
                        if (parts.Length == 1)
                        {
                            String[] imgs1 = Directory.GetFiles(imagesPath);
                            Random rnd1 = new Random();
                            await context.Channel.SendFileAsync(imgs1[rnd1.Next(0, imgs1.Length)] + ".png");
                            break;
                        }
                        switch (parts[1])
                        {
                            case "add":
                                var webClient = new WebClient();
                                if (parts.Length == 4)
                                {
                                    Console.WriteLine("Downloading " + parts[2] + " to " + imagesPath + parts[3] + ".png" + "...");
                                    webClient.DownloadFile(parts[2], imagesPath + parts[3] + ".png");
                                    await context.Channel.SendMessageAsync("`" + parts[3] + ".png" + "` uploaded!");
                                }
                                else
                                {
                                    //var url = msg.Content.Substring(8);
                                    //var parts = url.Split('/');

                                    var urlParts = parts[2].Split('/');
                                    Console.WriteLine("Downloading " + parts[2] + " to " + imagesPath + urlParts[urlParts.Length - 1] + "...");
                                    webClient.DownloadFile(parts[2], imagesPath + urlParts[urlParts.Length - 1]);
                                }

                                break;
                            case "remove":
                                try
                                {
                                    File.Delete(imagesPath + parts[2] + ".png");
                                    await context.Channel.SendMessageAsync("`" + parts[2] + ".png" + "` removed!");
                                    Console.WriteLine("`" + parts[2] + ".png" + "` removed!");
                                }
                                catch
                                {
                                    await context.Channel.SendMessageAsync("There is no such file");
                                }
                                break;

                            case "list":
                                String[] imgNames = Directory.GetFiles(imagesPath);
                                for (int i = 0; i < imgNames.Length; i++)
                                {
                                    String imgName = imgNames[i].Substring(imagesPath.Length);
                                    await context.Channel.SendMessageAsync(imgName);
                                }
                                break;

                            default:
                                try
                                {
                                    await context.Channel.SendFileAsync(imagesPath + parts[1] + ".png");
                                }
                                catch
                                {
                                    await context.Channel.SendMessageAsync("Meme not available");
                                }
                                break;
                        }
                        break;

                    case "!cemser":
                        String[] imgs2 = Directory.GetFiles(cemserPath);
                        Random rnd2 = new Random();
                        await context.Channel.SendFileAsync(imgs2[rnd2.Next(0, imgs2.Length)]);
                        break;
                    case "!help":
                        await context.Channel.SendMessageAsync("!sieg" + "\n" + "!addmeme URL name.ending" + "\n" + "!meme name (if left empty: random meme)" + "\n" + "!listmemes" + "\n" + "!cemser" + "\n");
                        break;

                }

                //if (result.IsSuccess)
                //{
                //    Console.WriteLine(true);
                //} else
                //{
                //    await context.Channel.SendMessageAsync(result.ErrorReason);
                //    Console.WriteLine(false);
                //}

                await msg.DeleteAsync();
            }

        }
    }
}