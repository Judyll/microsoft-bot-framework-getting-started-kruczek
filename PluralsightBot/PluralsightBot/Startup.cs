// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PluralsightBot.Bots;
using PluralsightBot.Dialogs;
using PluralsightBot.Services;

namespace PluralsightBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /** 
         * This method gets called by the runtime. Use this method to add services to the container.
         * 
         * The purpose the variable "services" is to facilitate dependency injection
         */
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            /**
             * Our StateService is expecting UserState and ConversationState to be
             * injected in our constructor
             */
            ConfigureState(services);
            /**
             * We need to instantiate the MainDialog since we will be injecting
             * it in the next line.
             */
            services.AddSingleton<MainDialog>();
            /** 
             * Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
             *
             * Anytime we encounter an object with the type of IBot, use an instance of the
             * EchoBot
             */
            services.AddTransient<IBot, DialogBot<MainDialog>>();            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }

        private void ConfigureState(IServiceCollection services)
        {
            /**
             * Create the storage we will be using for the User and Conversation state. 
             * (Memory is great for testing purposes). We need to inject IStorage first
             * since UserState and ConversationState needs IStorage on their constructor.
             */
            services.AddSingleton<IStorage, MemoryStorage>();
            //var storageAccount = "<Connection string from Azure -> Storage -> Settings -> Access Keys>";
            //var storageContainer = "bot-state-data";
            //services.AddSingleton<IStorage>(new AzureBlobStorage(storageAccount, storageContainer));
            // Create the user state
            services.AddSingleton<UserState>();
            // Create the conversation state
            services.AddSingleton<ConversationState>();
            // Create the instance of the state service
            services.AddSingleton<StateService>();
        }
    }
}
