﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;
using Shared.ApiInterface;
using System.Threading;
using System.Threading.Tasks;
using Shared;
using Greyshirt.Dialogs.NewUser;
using Greyshirt.State;

namespace Greyshirt.Dialogs
{
    public class MasterDialog : DialogBase
    {
        public static string Name = typeof(MasterDialog).FullName;

        public MasterDialog(StateAccessors state, DialogSet dialogs, IApiInterface api, IConfiguration configuration)
            : base(state, dialogs, api, configuration) { }

        public override Task<WaterfallDialog> GetWaterfallDialog(ITurnContext turnContext, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                return new WaterfallDialog(Name, new WaterfallStep[]
                {
                    async (dialogContext, cancellationToken) =>
                    {
                        // Clear the user context when a new converation begins.
                        await this.state.ClearUserContext(dialogContext.Context, cancellationToken);

                        // Handle any keywords.
                        if (Phrases.Keywords.IsKeyword(dialogContext.Context.Activity.Text))
                        {
                            var greyshirt = await api.GetGreyshirtFromContext(dialogContext.Context);
                            if (greyshirt.IsConsentGiven && greyshirt.IsRegistered())
                            {
                                return await BeginDialogAsync(dialogContext, KeywordDialog.Name, null, cancellationToken);
                            }
                        }

                        return await dialogContext.NextAsync(null, cancellationToken);
                    },
                    async (dialogContext, cancellationToken) =>
                    {
                        // The keyword flow can result in ending the conversation.
                        if (dialogContext.Result is bool continueConversation && !continueConversation)
                        {
                            await Messages.SendAsync(Shared.Phrases.Greeting.Goodbye, turnContext, cancellationToken);
                            return await dialogContext.EndDialogAsync(null, cancellationToken);
                        }

                        return await dialogContext.NextAsync(null, cancellationToken);
                    },
                    async (dialogContext, cancellationToken) =>
                    {
                        // Register the user if they are new.
                        var greyshirt = await api.GetGreyshirtFromContext(dialogContext.Context);
                        if (!greyshirt.IsConsentGiven)
                        {
                            return await BeginDialogAsync(dialogContext, NewUserDialog.Name, null, cancellationToken);
                        }

                        return await dialogContext.NextAsync(null, cancellationToken);
                    },
                    async (dialogContext, cancellationToken) =>
                    {
                        // The new user flow can result in no consent. If so, end the conversation.
                        if (dialogContext.Result is bool didConsent && !didConsent)
                        {
                            await Messages.SendAsync(Shared.Phrases.NewUser.NoConsent, dialogContext.Context, cancellationToken);
                            return await dialogContext.EndDialogAsync(null, cancellationToken);
                        }

                        // Start the main menu flow.
                        return await BeginDialogAsync(dialogContext, MenuDialog.Name, null, cancellationToken);
                    },
                    async (dialogContext, cancellationToken) =>
                    {
                        await Messages.SendAsync(Shared.Phrases.Greeting.Goodbye, turnContext, cancellationToken);
                        return await dialogContext.EndDialogAsync(null, cancellationToken);
                    }
                });
            });
        }
    }
}