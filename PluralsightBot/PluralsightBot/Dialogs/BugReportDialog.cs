using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Dialogs
{
    public class BugReportDialog : ComponentDialog
    {
        private readonly StateService _stateService;

        /**
         * The reason why we are injecting a dialogId since with each dialog, we need to
         * assign an Id just like the way we assign Id for state variables. This is a way
         * we can identify and activate dialogs when it is sitting on the stack.
         */
        public BugReportDialog(string dialogId, StateService stateService) 
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
                DescriptionStepAsync,
                CallbackTimeStepAsync,
                PhoneNumberStepAsync,
                BugStepAsync,
                SummaryStepAsync
            };
            /**
             * Add named dialogs. You can think of this like the bot state bag. 
             * We are initializing it and giving it a unique ID.
             * We will be using the WaterfallDialog and Prompt (Text)
             * sub-dialogs that we are adding in our dialog bag.  
             */
            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainFlow", 
                waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime",
                CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.phoneNumber",
                PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));

            // Set the starting dialog
            InitialDialogId = $"{nameof(BugReportDialog)}.mainFlow";
        }
        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.description",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter a description for your report")
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            /**
             * We are saving a value to the object which is stepContext. Step Context is a 
             * bag that is alive for the entirety of your waterfall flow. It's kind of a
             * place to store information for the time period of that flow only.
             * 
             * We could save it on the state but this is not ideal specially in the scenario
             * where the user bails out before the flow ends.  So, it is better to save 
             * the conversation in the Step Context first, and then last in the State.
             * 
             * The below is saving the information that we are getting from the
             * dialog DescriptionStepAsync. stepContext.Result contains the return value
             * of that method.
             */
            stepContext.Values["description"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.callbackTime",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter in a callback time"),
                    /**
                     * This will be activated once the value entered is invalid. Validation
                     * is done on a validator.
                     */
                    RetryPrompt = MessageFactory.Text("The value entered must be between the hours of 9AM and 5PM.")
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            /**
             * We are saving a value to the object which is stepContext. Step Context is a 
             * bag that is alive for the entirety of your waterfall flow. It's kind of a
             * place to store information for the time period of that flow only.
             * 
             * We could save it on the state but this is not ideal specially in the scenario
             * where the user bails out before the flow ends.  So, it is better to save 
             * the conversation in the Step Context first, and then last in the State.
             * 
             * The below is saving the information that we are getting from the
             * dialog CallbackTimeStepAsync. stepContext.Result contains the return value
             * of that method and we are adding some datetime magic to convert the value
             * into a date time value.
             */
            stepContext.Values["callbackTime"] = 
                Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result)
                .FirstOrDefault().Value);

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.phoneNumber",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter in a phone number that we can call you back at"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid phone number")
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            /**
             * We are saving a value to the object which is stepContext. Step Context is a 
             * bag that is alive for the entirety of your waterfall flow. It's kind of a
             * place to store information for the time period of that flow only.
             * 
             * We could save it on the state but this is not ideal specially in the scenario
             * where the user bails out before the flow ends.  So, it is better to save 
             * the conversation in the Step Context first, and then last in the State.
             * 
             * The below is saving the information that we are getting from the
             * dialog PhoneNumberStepAsync. stepContext.Result contains the return value
             * of that method and we are adding some datetime magic to convert the value
             * into a date time value.
             */
            stepContext.Values["phoneNumber"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.bug",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter the type of bug."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Security",
                        "Crash", "Power", "Performance", "Usability", "Serious Bug",
                        "Other"})
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            /**
             * FoundChoice is the value that we get back from the choice prompt
             */
            stepContext.Values["bug"] = ((FoundChoice)stepContext.Result).Value;

            // Get the current profile object from user
            var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context,
                () => new UserProfile(), cancellationToken);

            // Save all of the data inside the user profile
            userProfile.Description = (string)stepContext.Values["description"];
            userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
            userProfile.PhoneNumber = (string)stepContext.Values["phoneNumber"];
            userProfile.Bug = (string)stepContext.Values["bug"];

            // Show the summary to the user
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is a summary of your bug report:"),
                cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Description: {userProfile.Description}"),
                cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Callback Time: {userProfile.CallbackTime}"),
                cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Phone Number: {userProfile.PhoneNumber}"),
                cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Bug: {userProfile.Bug}"),
                cancellationToken);

            // Save data in userstate
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext,
            CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var resolution = promptContext.Recognized.Value.First();
                DateTime selectedDate = Convert.ToDateTime(resolution.Value);
                TimeSpan start = new TimeSpan(9, 0, 0); // 9 o'clock
                TimeSpan end = new TimeSpan(17, 0, 0); // 5 o'clock
                if ((selectedDate.TimeOfDay >= start) && (selectedDate.TimeOfDay <= end))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                return Task.FromResult(Regex.Match(promptContext.Recognized.Value, 
                    @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$")
                    .Success);
            }

            return Task.FromResult(false);
        }
    }
}
