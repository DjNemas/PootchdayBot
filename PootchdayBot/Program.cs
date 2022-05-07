using PootchdayBot;
using PootchdayBot.FolderManagment;

FolderFile.InitAll();

DiscordInit main = new DiscordInit();
await main.MainAsync();