﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Shiny.BluetoothLE;


namespace Shiny.Beacons
{
    public static class BleManagerExtensions
    {
        public static IObservable<Beacon> ScanForBeacons(this IBleManager manager, BeaconMonitorConfig? config)
        {
            var scanType = config == null
                ? BleScanType.LowLatency
                : BleScanType.LowPowered;

            var cfg = new ScanConfig { ScanType = scanType };
            if (config?.ScanServiceUuids?.Any() ?? false)
                cfg.ServiceUuids = config.ScanServiceUuids;

            return manager
                .Scan(cfg)
                .Where(x => x.IsBeacon())
                .Select(x => x.AdvertisementData.ManufacturerData.Data.Parse(x.Rssi));
        }


        public static bool IsBeacon(this ScanResult result)
        {
            var md = result.AdvertisementData?.ManufacturerData;

            if (md?.Data == null)
                return false;

            return md.Data.IsBeaconPacket();
        }
    }
}
