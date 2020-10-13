// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace PluralsightBot.Bots
{
    /**
     * ActivityHandler contains methods that handles major bot activity type calls. 
     * If these methods where not here, then you would have to write case statements 
     * to call them depending on the activity type.
     * 
     * ActivityHandler inherets from IBot interface that has one single method on it --
     * OnTurnAsync. This tells you that you don't need the ActivityHandler class. It 
     * simply something that Microsoft has created to make our job easier. If we wanted
     * to, we can inherit directly from the IBot interface. But the ActivityHandler makes
     * things a little bit easier to get started.
     * 
     * OnTurnAsync is the core method that is being called whenever a turn is caught
     */
    public class EchoBot : ActivityHandler
    {
        /**
         * This is called whenever our activity type is equal to "Message".
         */
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            /**
             * The ITurnContext holds information like Activity and other methods like
             * SendActivityAsync with help us take action on the payload.
             * 
             * The SendActivityAsync allows us to take the data from the turn context,
             * serialize it and return it back from the user
             * 
             * The MessageFactory is another Microsoft helper that allows to send certain
             * type of messages back to the user easily. We can send back like text,
             * attachments, carousels, etc.
             */
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        /**
         * Called when a member is added.
         */
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                /**
                 * This check makes sure that we don't send the message if the current member
                 * being iterated is the "bot".
                 */
                if (member.Id != turnContext.Activity.Recipient.Id)
                { 
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
