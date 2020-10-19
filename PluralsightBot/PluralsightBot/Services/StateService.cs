using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Models;
using System;

namespace PluralsightBot.Services
{
    /**
     * This class is our one-stop shop for all the state service we need.
     */
    public class StateService
    {
        public ConversationState ConversationState { get; }
        /**
         * The person name does not change within the conversation that is why we 
         * are using UserState.
         */
        public UserState UserState { get; }
        /**
         * This is for our dialogs
         */
        public DialogState DialogState { get; }
        /**
         * This is our accessor. This will allow us to push, pull, and delete data
         * from our property inside our state bucket.
         */
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
        /**
         * The next thing we need is somehow to identify the user profile data inside
         * the user bucket.
         */
        private static string UserProfileId { get; } = $"{nameof(StateService)}.UserProfile";
        private static string ConversationDataId { get; } = $"{nameof(StateService)}.ConversationData";
        private static string DialogStateId { get; } = $"{nameof(StateService)}.DialogState";
        public StateService(UserState userState, ConversationState conversationState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            InitializeAccessor();
        }
        private void InitializeAccessor()
        {
            /**
             * Initialize the user state
             * 
             * In order to put properties inside our state bucket, we have to tell the 
             * bucket about the variable that we want to store. We are saying to the 
             * user state bucket that we want a property "UserProfile" that we want
             * to store on a state and we are going to identify that variable
             * using the "UserProfileId"
             */
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);

        }
    }
}
