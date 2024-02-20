using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace ECSDiscord.Services.SlashCommands.Commands;

public class CodeCommand : ISlashCommand
{
    private readonly VerificationService _verificationService;

    public CodeCommand(VerificationService verificationService)
    {
        _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
    }

    public string Name => "verify";

    public SlashCommandProperties Build()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Complete the association of your VUW username with your ECS discord account.")
            .AddOption(
                "code",
                ApplicationCommandOptionType.String,
                "the verification code sent to your VUW student email address.",
                true)
            .Build();
    }

    public async Task ExecuteAsync(ISlashCommandInteraction command)
    {
        var code = (string)command.Data.Options.First().Value;
        var result = await _verificationService.FinishVerificationAsync(code, command.User);
        switch (result)
        {
            case VerificationService.VerificationResult.InvalidToken:
                await command.RespondAsync(
                    ":warning:  Invalid verification code.\n",
                    ephemeral: true);
                break;
            case VerificationService.VerificationResult.TokenExpired:
                await command.RespondAsync(
                    ":clock1:  That token has expired! Please verify again.\n",
                    ephemeral: true);
                break;
            case VerificationService.VerificationResult.Success:
                await command.RespondAsync(
                    ":white_check_mark:  Verification succeed!\nYou can now join course channels",
                    ephemeral: true);
                break;
            case VerificationService.VerificationResult.Failure:
            default:
                await command.RespondAsync(
                    ":fire:  A server error occured. Please ask an admin to check the logs.",
                    ephemeral: true);
                break;
        }
    }
}