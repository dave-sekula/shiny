Title: v2.3.0 (Preview)
Order: 1000000
---

### Core
* [BREAKING] Jobs has been moved from Shiny.Core to its own module Shiny.Jobs
* [BREAKING][iOS] Default store is now secure storage (KeyChain) to deal with potential auto-start issues before device has been unlocked
* [Enhancement] Object binding failures now log specifics on what failed
* [Fix] Startup registration was failing on optional delegates
* [Fix] Update job registration to Shiny.Jobs

### BluetoothLE
* [Fix] Managed Scan Result now returns as IAdvertisementData for consistency
* [Fix] ManagedScan.Start is now async to expose permission issues before starting the scan [GH-698](https://github.com/shinyorg/shiny/issues/698)
* [Fix] ManagedScan.Toggle has been moved to an extension method

### Beacons
* [Enhancement] Managed beacon ranging scan now has Start as async so permissions can be checked
* [Fix] ManagedBeacon result now includes the region identifier

### Locations
* [Enhancement] GpsManager.GetCurrentPosition will start/stop the GPS tracking only if it wasn't started before
* [Enhancement] IGpsManager.StartAndReceive starts GPS when subscribe and turns it off when unsubscribed OR the app goes to the background
* [Fix] GPS setting state is only written to settings if background is used
* [Fix][iOS] Will only attempt to auto-start the GPS tracking in background if authorization status has been fully granted (Always Allow)

### Jobs
* [Enhancement] UsesJobs now allows you to manually register the job manager - RegisterJob will call this automatically
* [Enhancement] Foregrounding is now always available if you set your job as "RunInForeground", there is no need to call UseForegroundJobs
* [Fix][iOS] BGTasks - Correct filters applied to all modes

### Notifications
* [Enhancement] Foreground service notifications will use channel that does not show application badge notification [GH-734](https://github.com/shinyorg/shiny/issues/734)
* [Enhancement] Custom sounds work for critical notifications [GH-757](https://github.com/shinyorg/shiny/pull/757)

### Push
* [BREAKING] IPushDelegate.OnTokenChanged is now called IPushDelegate.OnTokenRefreshed
* [BREAKING] Channels are no longer registered within Push.  Use INotificationManager to do this.  If you need more customization over notification, send data messages and use INotificationManager
* [Fix][Android] OnEntry now functions under all modules
* [Fix][iOS][Native] IPushDelegate.OnTokenChanged is no longer called by RequestAccess and the proper token is returned

### Push - Azure Notification Hubs
* [Fix] ANH can often fail with "Microsoft.Azure.NotificationHubs.Messaging.MessagingEntityNotFoundException: Installation not found.TrackingId:XXXX" for rapid calls (ie. RequestAccess immediately followed by SetTags)
* [Enhancement] You can now configure a timespan for when azure installations should expire if not used. 