# Skyline.DataMiner.Utils.SNMP

## About

Skyline.DataMiner.Utils.SNMP is a NuGet package in the public [nuget store](https://www.nuget.org/) that contain assemblies that assist in developing connectors with SNMP Communication.

Specifically they can help with such things as calculating the delta time between snmp polling requests or verifying if a device SNMP Agent has restarted, which can be useful for Rate Calculations.

> Documentation: [Rate Calculations](https://community.dataminer.services/the-many-pitfalls-of-rate-calculations-and-how-you-can-avoid-them/?hilite=rate)

## Requirements

The "DataMiner Integration Studio" Visual Studio extension is required for development of connectors and Automation scripts using NuGets.

See [Installing DataMiner Integration Studio](https://aka.dataminer.services/DisInstallation)

> [!IMPORTANT]
> NuGets are mandatory to be installed with PackageReferences. DIS was redesigned to work with PackageReferences and be future-proof.
>
> For more information on how to migrate from packages.config to PackageReferences, see [docs.microsoft.com](https://docs.microsoft.com/en-us/nuget/consume-packages/migrate-packages-config-to-package-reference).

## Getting started

```cs
    SnmpDeltaHelper.UpdateRateDeltaTracking(protocol, groupId: 1000, CalculationMethod.Accurate);
```
