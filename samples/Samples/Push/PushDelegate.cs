﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shiny.Push;
using Samples.Models;
using Samples.Infrastructure;


namespace Samples.Push
{
    public class PushDelegate : IPushDelegate
    {
        readonly CoreDelegateServices services;
        readonly IPushManager pushManager;


        public PushDelegate(CoreDelegateServices services, IPushManager pushManager)
        {
            this.services = services;
            this.pushManager = pushManager;
        }

        public Task OnReceived(IReadOnlyDictionary<string, string> data)
            => this.Insert("PUSH RECEIVED");

        public Task OnTokenRefreshed(string token)
            => this.Insert("PUSH TOKEN CHANGE");

        Task Insert(string info) => this.services.Connection.InsertAsync(new PushEvent
        {
            Payload = info,
            Token = this.pushManager.CurrentRegistrationToken,
            Timestamp = DateTime.UtcNow
        });
    }
}
