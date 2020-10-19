using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using PluralsightBot.Helpers;
using PluralsightBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Bots
{
    /**
     * The purpose of this class is to give us a pass-through for dialogs
     */
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        private readonly StateService _stateService;
        private readonly T _dialog;
        private readonly ILogger<DialogBot<T>> _logger;

        public DialogBot(StateService stateService, T dialog, ILogger<DialogBot<T>> logger)
        {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /**
         * This is another hook of the base class of the ActivityHandler.
         * This is called anytime a bot gets any activity at all regardless of the message 
         * type.
         * So, if the bot receives a message, this OnTurnAsync method and OnMessageActivityAsync
         * will be called.
         */        
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn
            await _stateService.UserState.SaveChangesAsync(turnContext);
            await _stateService.ConversationState.SaveChangesAsync(turnContext);
        }
        /**
         * This is the where the real magic happens.
         */
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the dialog with the new message activity
            await _dialog.Run(turnContext, _stateService.DialogStateAccessor,
                cancellationToken);
        }
    }
}
