using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net;
using System.Linq;

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
            var dic = (File.ReadAllLines(Path.GetFullPath(@"..\..") + "\\credentials\\token.conf")).Select(l => l.Split(new[] { '=' })).ToDictionary(s => s[0].Trim(), s => s[1].Trim()); //create dictionary from config file
            string token = dic["token"]; //get token from dictionary
            Console.WriteLine("Logging in as: " + token);

            client = new DiscordSocketClient();
            commands = new CommandService();

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
            var username = "[" + msg.Author.Username + "]";

            if (!(Directory.Exists(imagesPath)))
            {
                Console.WriteLine("Wrong Directory: " + imagesPath);
            }

            if (!(Directory.Exists(cemserPath)))
            {
                Console.WriteLine("Wrong Directory: " + cemserPath);
            }

            if (msg.HasCharPrefix('!', ref argPos) /*|| message.HasMentionPrefix(client.CurrentUser, ref argPos))*/)
            {
                var parts = msg.Content.Split(' ');
                switch (parts[0])
                {
                    case "!meme":
                        if (parts.Length == 1)
                        {
                            String[] imgs1 = Directory.GetFiles(imagesPath);
                            Random rnd1 = new Random();
                            await context.Channel.SendFileAsync(imgs1[rnd1.Next(0, imgs1.Length)]);
                            break;
                        }
                        switch (parts[1])
                        {
                            case "add": //add a meme
                                var webClient = new WebClient();
                                if (parts.Length == 4)
                                {
                                    Console.WriteLine("Downloading " + parts[2] + " to " + imagesPath + parts[3] + ".png" + "...");
                                    webClient.DownloadFile(parts[2], imagesPath + parts[3] + ".png");
                                    await context.Channel.SendMessageAsync("`" + parts[3    ] + ".png" + "` added!");
                                }
                                else
                                {
                                    //var url = msg.Content.Substring(8);
                                    //var parts = url.Split('/');

                                    var urlParts = parts[2].Split('/');
                                    String imgName = urlParts[urlParts.Length - 1];
                                    String pngName = imgName.Remove(imgName.Length - 4) + ".png";
                                    Console.WriteLine("Downloading " + parts[2] + " to " + imagesPath + pngName + "...");
                                    webClient.DownloadFile(parts[2], imagesPath + pngName);
                                    await context.Channel.SendMessageAsync("`" + pngName + "` added!");
                                }

                                break;
                            case "remove": //remove a meme
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

                            case "list": //list all memes
                                String[] imgNames = Directory.GetFiles(imagesPath);
                                String listString = "";
                                for (int i = 0; i < imgNames.Length; i++)
                                {
                                    String imgName = imgNames[i].Substring(imagesPath.Length);
                                    imgName = imgName.Remove(imgName.Length - 4);
                                    listString += imgName + "\n";
                                }
                                await context.Channel.SendMessageAsync(listString);
                                break;

                            default: //send meme with name
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
                        Console.WriteLine(username + " requested cemser");
                        String[] imgs2 = Directory.GetFiles(cemserPath);
                        Random rnd2 = new Random();
                        await context.Channel.SendFileAsync(imgs2[rnd2.Next(0, imgs2.Length)]);
                        break;
                    case "!clear":
                        if(parts.Length == 2){
                            var messages = await context.Channel.GetMessagesAsync(int.Parse(parts[1]) + 1).Flatten();
                            await context.Channel.TriggerTypingAsync();
                            await context.Channel.DeleteMessagesAsync(messages);
                            var response = await context.Channel.SendMessageAsync(int.Parse(parts[1]) + " messages deleted.");
                            Console.WriteLine(username + " deleted " + int.Parse(parts[1]) + " messages.");
                            Thread.Sleep(1000);
                            await response.DeleteAsync();
                        } else if(parts[1] == "bot")
                        {
                            for(int i = 0; i < int.Parse(parts[2]); i++)
                            {
                                var message = await msg.Channel.GetMessageAsync(context.Channel.Id);
                                if (message.Author.IsBot)
                                {
                                    await message.DeleteAsync();
                                }
                            }
                        }

                        break;
                    case "!brätz":
                        Console.WriteLine(username + " brätzed " + parts[1]);
                        //await context.User.GetOrCreateDMChannelAsync();
                        await context.Channel.SendMessageAsync(parts[1] + ", get brätzt by " + msg.Author.Username);
                        //await context.Channel.GetUserAsync().SendMessageAsync(msg.Author.Username + " whispered to you: " + parts[1]);
                        break;
                    case "!sendNudes":
                        Console.WriteLine(username + " requested nudes! o.o");
                        await msg.Author.SendMessageAsync("Here you go, " + msg.Author.Username + ": (.)(.)");
                        break;
                    case "!watch":
                        var output = "";
                        int num;
                        output += parts[2] + " watch " + parts[1];
                        if (parts.Length == 4) { num = int.Parse(parts[3]); }
                        else { num = 1; }
                        Console.WriteLine(username + " told " + parts[2] + " to watch " + parts[1] + " " + num + " time(s)");
                        for (int i = 1; i <= num; i++) { await context.Channel.SendMessageAsync(output); }
                        break;

                    case "!help":
                        String helpString = "";
                        foreach (String line in File.ReadAllLines(Path.GetFullPath(@"..\..") + "\\help.txt")){
                            helpString += line + "\n";
                        }

                        Console.WriteLine(msg.Author.Username + " requested help");

                        Console.WriteLine(username + " requested help");

=======
                        Console.WriteLine(username + " requested help");
>>>>>>> 43e8331276d85ae4c9856b06274f9f93760f48a8
                        await context.Channel.SendMessageAsync("```" + helpString + "```");
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
                Thread.Sleep(2000);
                await msg.DeleteAsync();
            }

        }
    }
}