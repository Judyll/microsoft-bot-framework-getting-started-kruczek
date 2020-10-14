using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Bots
{
    /**
     * Our goal for this class is to let the bot greet the user by name
     * and remember its name that will be used for further conversation
     */
    public class GreetingBot : ActivityHandler
    {
        private readonly StateService _stateService;

        public GreetingBot(StateService stateService)
        {
            _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetName(turnContext, cancellationToken);
                }
            }
        }
        private async Task GetName(ITurnContext turnContext, 
            CancellationToken cancellationToken)
        {
            /**
             * We are calling the "GetAsync" that is going to do one of two things: if UserProfile
             * is not instanciated yet, it is going to do so using the function with the "new"
             * keyword. If it is already an instance, it will retrieve the value inside the 
             * state bucket. We are injecting the "turnContext" which is our payload for everything
             * going inside and outside the turn include the information about the user id
             * that is something we are going to need to get the correct user data from the
             * user object.
             */
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(turnContext,
                () => new UserProfile());
            ConversationData conversationData = await _stateService.ConversationDataAccessor.GetAsync(turnContext,
                () => new ConversationData());

            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                await turnContext.SendActivityAsync(
                    MessageFactory.Text($"Hi {userProfile.Name}. How can I help you today ?"),
                    cancellationToken);
            }
            else
            {
                if (conversationData.PromptedUserForName)
                {
                    // Set the name to what the user provided
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    // Acknowledge that we got their name
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text($"Thanks {userProfile.Name}. How can I help you today ?"), 
                        cancellationToken);

                    // Reset the flag to allow the bot to go to the cycle again
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    // Prompt the user for their name
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text("Hello! What is your name ?"),
                        cancellationToken);

                    // Set the flag to true, so we don't prompt in the next turn
                    conversationData.PromptedUserForName = true;
                }

                // Save any state changes that might have occured during the turn
                await _stateService.UserProfileAccessor.SetAsync(turnContext, userProfile);
                await _stateService.ConversationDataAccessor.SetAsync(turnContext, conversationData);

                await _stateService.UserState.SaveChangesAsync(turnContext);
                await _stateService.ConversationState.SaveChangesAsync(turnContext);
            }
        }
    }
}
