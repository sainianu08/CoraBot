﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BotAgentRemi.Middleware
{
    public class TrimIncomingMessageMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Trim the incoming text.
                turnContext.Activity.Text = turnContext.Activity.Text.Trim();
            }

            // Invoke the next middleware.
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
