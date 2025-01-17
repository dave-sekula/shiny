﻿using System;
using Shiny.Stores;


namespace Shiny.Infrastructure
{
    public class ShinyCoreServices
    {
#if __ANDROID__
        public ShinyCoreServices(IAndroidContext context,
                                 IPlatform platform,
                                 IKeyValueStoreFactory store,
                                 IRepository repository,
                                 IServiceProvider services,
                                 ISerializer serializer,
                                 IMessageBus bus)
            : this(platform, store, repository, services, serializer, bus)
        {
            this.Android = context;
        }

#elif __IOS__
        public ShinyCoreServices(AppleLifecycle lifecycle,
                                 IPlatform platform,
                                 IKeyValueStoreFactory store,
                                 IRepository repository,
                                 IServiceProvider services,
                                 ISerializer serializer,
                                 IMessageBus bus)
            : this(platform, store, repository, services, serializer, bus)
        {
            this.Lifecycle = lifecycle;
        }

#endif
        public ShinyCoreServices(IPlatform platform,
                                 IKeyValueStoreFactory store,
                                 IRepository repository,
                                 IServiceProvider services,
                                 ISerializer serializer,
                                 IMessageBus bus)
        {
            this.Platform = platform;
            this.Settings = store.DefaultStore;
            this.Repository = repository;
            this.Services = services;
            this.Serializer = serializer;
            this.Bus = bus;
        }

#if __ANDROID__
        public IAndroidContext Android { get; }
#elif __IOS__
        public AppleLifecycle Lifecycle { get; }
#endif
        public IPlatform Platform { get; }
        public IKeyValueStore Settings { get; }
        public IRepository Repository { get; }
        public IServiceProvider Services { get; }
        public ISerializer Serializer { get; }
        public IMessageBus Bus { get; }
    }
}
