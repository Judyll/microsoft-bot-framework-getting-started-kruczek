using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly StateService _stateService;

        public MainDialog(StateService stateService) 
            : base(nameof(MainDialog))
        {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));

            InitializeWaterfallDialog();
        }
        private void InitializeWaterfallDialog()
        {
            /**
             * We are going to use a waterfall dialog to frame-up our conversation.
             * The great thing about the waterfall dialog is that it gives us a good
             * back and forth template to utilize for our conversation.
             * 
             * The first thing that we are going to do set-up our waterfall dialog
             * is to establish all the steps by creating an instance of the waterfall 
             * steps below. In the list are the detailed methods that are going to be
             * called and on what order in the waterfall flow. The order is very important
             * here since this will be called in the order based on the list.
             */
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _stateService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _stateService));

            // Set the starting dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            /**
             * Our bot runs in a LOOP. If on the first loop you input "hi", then 
             * the greeting dialog will be activated.  If not, then the bug report
             * dialog will be activated.
             */
            if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hi").Success)
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting",
                    null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport",
                    null, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
