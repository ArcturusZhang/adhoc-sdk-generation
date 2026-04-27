using System;
using System.Threading.Tasks;
using Azure.Identity;
using Playground.ComputeAdhoc;

namespace Playground.AvailabilitySetDemo;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Required env vars (or pass on the command line as positional args):
        //   AZURE_SUBSCRIPTION_ID, AZURE_RESOURCE_GROUP, AZURE_LOCATION
        //   (optional) AVSET_NAME — defaults to "demo-avset"
        string subscriptionId = Get(args, 0, "AZURE_SUBSCRIPTION_ID");
        string resourceGroup  = Get(args, 1, "AZURE_RESOURCE_GROUP");
        string location       = Get(args, 2, "AZURE_LOCATION");
        string avsetName      = args.Length > 3 ? args[3] : (Environment.GetEnvironmentVariable("AVSET_NAME") ?? "demo-avset");

        if (string.IsNullOrWhiteSpace(subscriptionId) || string.IsNullOrWhiteSpace(resourceGroup) || string.IsNullOrWhiteSpace(location))
        {
            Console.Error.WriteLine("Usage: dotnet run -- <subscriptionId> <resourceGroup> <location> [availabilitySetName]");
            Console.Error.WriteLine("   or set AZURE_SUBSCRIPTION_ID, AZURE_RESOURCE_GROUP, AZURE_LOCATION env vars.");
            return 1;
        }

        Console.WriteLine($"Creating availability set '{avsetName}' in '{resourceGroup}' ({location})...");

        var credential = new DefaultAzureCredential();
        var client = new AvailabilitySetsClient(credential);

        var body = new AvailabilitySet
        {
            Location = location,
            Sku = new Sku { Name = "Aligned" },
            Properties = new AvailabilitySetProperties
            {
                PlatformUpdateDomainCount = 5,
                PlatformFaultDomainCount = 2,
            },
        };

        var result = await client.CreateOrUpdateAsync(subscriptionId, resourceGroup, avsetName, body);
        var avset = result.Value;

        Console.WriteLine($"Created: {avset.Id}");
        Console.WriteLine($"  name     : {avset.Name}");
        Console.WriteLine($"  type     : {avset.Type}");
        Console.WriteLine($"  location : {avset.Location}");
        Console.WriteLine($"  sku.name : {avset.Sku?.Name}");
        Console.WriteLine($"  PUD/PFD  : {avset.Properties?.PlatformUpdateDomainCount}/{avset.Properties?.PlatformFaultDomainCount}");
        return 0;
    }

    private static string Get(string[] args, int idx, string envVar)
        => args.Length > idx ? args[idx] : Environment.GetEnvironmentVariable(envVar);
}
