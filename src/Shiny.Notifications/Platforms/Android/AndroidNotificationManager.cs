﻿using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Shiny.Infrastructure;


namespace Shiny.Notifications
{
    public class AndroidNotificationManager
    {
        protected ShinyCoreServices Services { get; }
        public NotificationManagerCompat NativeManager { get; }


        public AndroidNotificationManager(ShinyCoreServices services)
        {
            this.Services = services;
            this.NativeManager = NotificationManagerCompat.From(this.Services.Android.AppContext);
        }


        public Android.App.Notification CreateNativeNotification(Notification notification, Channel channel)
            => this.CreateNativeBuilder(notification, channel).Build();


        public virtual NotificationCompat.Builder CreateNativeBuilder(Notification notification, Channel channel)
        {
            var builder = new NotificationCompat.Builder(this.Services.Android.AppContext)
                .SetContentTitle(notification.Title)
                .SetSmallIcon(this.GetSmallIconResource(notification.Android.SmallIconResourceName))
                .SetAutoCancel(notification.Android.AutoCancel)
                .SetOngoing(notification.Android.OnGoing);

            this.ApplyLaunchIntent(builder, notification);
            if (!notification.Android.ContentInfo.IsEmpty())
                builder.SetContentInfo(notification.Android.ContentInfo);

            if (!notification.Android.Ticker.IsEmpty())
                builder.SetTicker(notification.Android.Ticker);

            if (notification.Android.UseBigTextStyle)
                builder.SetStyle(new NotificationCompat.BigTextStyle().BigText(notification.Message));
            else
                builder.SetContentText(notification.Message);

            this.TrySetLargeIconResource(notification, builder);

            if (!notification.Android.ColorResourceName.IsEmpty())
            {
                var color = this.GetColor(notification.Android.ColorResourceName);
                builder.SetColor(color);
            }

            if (notification.Android.ShowWhen != null)
                builder.SetShowWhen(notification.Android.ShowWhen.Value);

            if (notification.Android.When != null)
                builder.SetWhen(notification.Android.When.Value.ToUnixTimeMilliseconds());

            this.ApplyChannel(builder, notification, channel);
            return builder;
        }


        public virtual void ApplyLaunchIntent(NotificationCompat.Builder builder, Notification notification)
        {
            var pendingIntent = this.GetLaunchPendingIntent(notification);
            builder.SetContentIntent(pendingIntent);
        }


        public virtual void ApplyChannel(NotificationCompat.Builder builder, Notification notification, Channel channel)
        {
            if (channel != null)
                return;

            builder.SetChannelId(channel.Identifier);
            if (channel.Actions != null)
            {
                foreach (var action in channel.Actions)
                {
                    switch (action.ActionType)
                    {
                        case ChannelActionType.OpenApp:
                            break;

                        case ChannelActionType.TextReply:
                            var textReplyAction = this.CreateTextReply(notification, action);
                            builder.AddAction(textReplyAction);
                            break;

                        case ChannelActionType.None:
                        case ChannelActionType.Destructive:
                            var destAction = this.CreateAction(notification, action);
                            builder.AddAction(destAction);
                            break;

                        default:
                            throw new ArgumentException("Invalid action type");
                    }
                }
            }
        }


        public virtual PendingIntent GetLaunchPendingIntent(Notification notification, string? actionId = null)
        {
            Intent launchIntent;

            if (notification.Android.LaunchActivityType == null)
            {
                launchIntent = this.Services
                    .Android
                    .AppContext
                    .PackageManager
                    .GetLaunchIntentForPackage(this.Services.Android.Package.PackageName)
                    .SetFlags(notification.Android.LaunchActivityFlags.ToNative());
            }
            else
            {
                launchIntent = new Intent(
                    this.Services.Android.AppContext,
                    notification.Android.LaunchActivityType
                );
            }

            this.PopulateIntent(launchIntent, notification);

            PendingIntent pendingIntent;
            if ((notification.Android.LaunchActivityFlags & AndroidActivityFlags.ClearTask) != 0)
            {
                pendingIntent = AndroidX.Core.App.TaskStackBuilder
                    .Create(this.Services.Android.AppContext)
                    .AddNextIntent(launchIntent)
                    .GetPendingIntent(notification.Id, (int)PendingIntentFlags.OneShot);
            }
            else
            {
                pendingIntent = PendingIntent.GetActivity(
                    this.Services.Android.AppContext,
                    notification.Id,
                    launchIntent,
                    PendingIntentFlags.OneShot
                );
            }
            return pendingIntent;
        }



        static int counter = 100;
        protected virtual PendingIntent CreateActionIntent(Notification notification, ChannelAction action)
        {
            var intent = this.Services.Android.CreateIntent<ShinyNotificationBroadcastReceiver>(ShinyNotificationBroadcastReceiver.EntryIntentAction);
            this.PopulateIntent(intent, notification);
            intent.PutExtra(AndroidNotificationProcessor.IntentActionKey, action.Identifier);

            counter++;
            var pendingIntent = PendingIntent.GetBroadcast(
                this.Services.Android.AppContext,
                counter,
                intent,
                PendingIntentFlags.UpdateCurrent
            )!;
            return pendingIntent;
        }


        protected virtual void PopulateIntent(Intent intent, Notification notification)
        {
            var content = this.Services.Serializer.Serialize(notification);
            intent.PutExtra(AndroidNotificationProcessor.IntentNotificationKey, content);
        }


        protected virtual NotificationCompat.Action CreateAction(Notification notification, ChannelAction action)
        {
            var pendingIntent = this.CreateActionIntent(notification, action);
            var iconId = this.Services.Android.GetResourceIdByName(action.Identifier);
            var nativeAction = new NotificationCompat.Action.Builder(iconId, action.Title, pendingIntent).Build();

            return nativeAction;
        }


        protected virtual NotificationCompat.Action CreateTextReply(Notification notification, ChannelAction action)
        {
            var pendingIntent = this.CreateActionIntent(notification, action);
            var input = new AndroidX.Core.App.RemoteInput.Builder(AndroidNotificationProcessor.RemoteInputResultKey)
                .SetLabel(action.Title)
                .Build();

            var iconId = this.Services.Android.GetResourceIdByName(action.Identifier);
            var nativeAction = new NotificationCompat.Action.Builder(iconId, action.Title, pendingIntent)
                .SetAllowGeneratedReplies(true)
                .AddRemoteInput(input)
                .Build();

            return nativeAction;
        }


        public virtual void SendNative(int id, Android.App.Notification notification)
            => this.NativeManager.Notify(id, notification);


        // Construct a raw resource path of the form
        // "android.resource://<PKG_NAME>/raw/<RES_NAME>", e.g.
        // "android.resource://com.shiny.sample/raw/notification"
        public virtual Android.Net.Uri GetSoundResourceUri(string soundResourceName)
        {
            // Strip file extension and leading slash from resource name to allow users
            // to specify custom sounds like "notification.mp3" or "/raw/notification.mp3"
            if (File.Exists(soundResourceName))
                return Android.Net.Uri.Parse("file://" + soundResourceName)!;

            soundResourceName = soundResourceName.TrimStart('/').Split('.').First();
            var resourceId = this.Services.Android.GetRawResourceIdByName(soundResourceName);
            var resources = this.Services.Android.AppContext.Resources;
            return new Android.Net.Uri.Builder()
                .Scheme(ContentResolver.SchemeAndroidResource)!
                .Authority(resources.GetResourcePackageName(resourceId))!
                .AppendPath(resources.GetResourceTypeName(resourceId))!
                .AppendPath(resources.GetResourceEntryName(resourceId))!
                .Build()!;
        }





        protected virtual int GetColor(string colorResourceName)
        {
            var colorResourceId = this.Services.Android.GetColorByName(colorResourceName);
            if (colorResourceId <= 0)
                throw new ArgumentException($"Color ResourceId for {colorResourceName} not found");

            return ContextCompat.GetColor(this.Services.Android.AppContext, colorResourceId);
        }


        public virtual int GetSmallIconResource(string resourceName)
        {
            if (resourceName.IsEmpty())
            {
                var id = this.Services.Android.GetResourceIdByName("notification");
                if (id > 0)
                    return id;

                return this.Services.Android.AppContext.ApplicationInfo.Icon;
            }
            var smallIconResourceId = this.Services.Android.GetResourceIdByName(resourceName);
            if (smallIconResourceId <= 0)
                throw new ArgumentException($"Icon ResourceId for {resourceName} not found");

            return smallIconResourceId;
        }


        protected virtual void TrySetLargeIconResource(Notification notification, NotificationCompat.Builder builder)
        {
            if (notification.Android.LargeIconResourceName.IsEmpty())
                return;

            var iconId = this.Services.Android.GetResourceIdByName(notification.Android.LargeIconResourceName);
            if (iconId > 0)
                builder.SetLargeIcon(BitmapFactory.DecodeResource(this.Services.Android.AppContext.Resources, iconId));
        }


        //protected virtual void SetAlarm(Notification notification)
        //{
        //    var date = notification.ScheduleDate.Value.LocalDateTime;
        //    var intent = this.services.Android.CreateIntent<ShinyNotificationBroadcastReceiver>(ShinyNotificationBroadcastReceiver.AlarmIntentAction);
        //    intent.PutExtra("NotificationId", notification.Id);

        //    var calendar = Calendar.GetInstance((Android.Icu.Util.TimeZone)null)!;
        //    calendar.Set(date.Year, date.Month, date.Day, date.Hour, date.Minute);

        //    var pendingIntent = PendingIntent.GetBroadcast(this.services.Android.AppContext, 0, intent, PendingIntentFlags.OneShot);
        //    AlarmManagerCompat.SetExactAndAllowWhileIdle(
        //        this.services.Android.GetSystemService<AlarmManager>(Context.AlarmService),
        //        (int)AlarmType.RtcWakeup,
        //        calendar.TimeInMillis,
        //        pendingIntent
        //    );
        //}
    }
}
