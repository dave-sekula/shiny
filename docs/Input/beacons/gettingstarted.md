Title: Getting Started
Order: 1
---

This library deals specifically with iBeacons (Apple's beacon technology).  iBeacons are used to provide contextual location information to a device.  They are built upon BluetoothLE advertising.  They are basically radio'ing out a small piece of data every 700-1400ms that other devices can "hear" if they are listening properly. 

## Terminology
|Term|Description|
|----|-----------|
|UUID|Universally Unique Identification - this is usually a single value that you use across your entire organization or application
|Major|Major is the second part of the addressing system to find specific beacons within your organization UUID
|Minor|Minor is the final piece of the addressing system to find specific beacons within your organization UUID
|Identifier|This is a string of your own choosing.  You can use this to identify the scan set you are using.
|[Ranging](ranging)|Ranging is a foreground only operation.  You can use this to find all beacons within a filter set.  If you provide only the UUID to filter by, all beacons under that UUID will be scanned for.  The major & minor values will also be provided when they are found.
|[Monitoring](monitoring)|Monitoring is a background operation.  Monitoring will not provide you with an identity of the beacon.  For instance, if you scan by UUID+major - it will not provide the minor (to lock down specific beacons) for you.  Think of this like knowing the person is in a building, but not which floor.
|Region (Monitoring) vs Beacon (Ranging)|Region is a filter set of beacons where as the beacon itself is an individual/specific entity