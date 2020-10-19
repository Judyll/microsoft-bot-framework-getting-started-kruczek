using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Dialogs
{
    /**
     * We are inheriting from the ComponentDialog type which is the dialog that we use
     * to create reusable pieces of conversation. Most of the classes we create for our
     * dialog will inherit from this type. It make sense because it is the container
     * type that is meant to build a modular and reusable component which is what we 
     * want all our dialogs to be.
     */
    public class GreetingDialog : ComponentDialog
    {
        private readonly StateService _stateService;

        /**
         * The reason why we are injecting a dialogId since with each dialog, we need to
         * assign an Id just like the way we assign Id for state variables. This is a way
         * we can identify and activate dialogs when it is sitting on the stack.
         */
        public GreetingDialog(string dialogId, StateService stateService)
            : base(dialogId)
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
                InitializeStepAsync,
                FinalStepAsync
            };

            /**
             * Add named dialogs. You can think of this like the bot state bag. 
             * We are initializing it and giving it a unique ID.
             * We will be using the WaterfallDialog and Prompt (Text)
             * sub-dialogs that we are adding in our dialog bag.  
             */
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow",
                waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            // Set the starting dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }
        private async Task<DialogTurnResult> InitializeStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Check if we already have the user's name
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context,
                () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                /**
                 * If we don't have the name, then we will kick start the text prompt dialog
                 * that we have just added in the code with the ID:
                 * 
                 * AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));
                 * 
                 */
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name ?")
                    }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            /**
             * Check if we already have the user's name. Why do we need to do this ?
             * 
             * If in our previous step we did not have the name, and then we call TextPrompt,
             * this still funnels us to this method (FinalStepAsync). We still need to get
             * the name and save it into our user's profile field.
             */
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context,
                () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // Set the name
                userProfile.Name = (string)stepContext.Result;
                // Save any state changes that might have occured during the turn
                await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hi {userProfile.Name}. How can I help you today ?"),
                cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
