using System;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Playground.ComputeAdhoc;

namespace Playground.AvailabilitySetDemo;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var subscriptionId = Get(args, 0, "AZURE_SUBSCRIPTION_ID");
        var resourceGroup = Get(args, 1, "AZURE_RESOURCE_GROUP");
        var name = Get(args, 2, "AVAILABILITY_SET_NAME");
        var location = Get(args, 3, "AZURE_LOCATION") ?? "eastus";

        if (string.IsNullOrWhiteSpace(subscriptionId) ||
            string.IsNullOrWhiteSpace(resourceGroup) ||
            string.IsNullOrWhiteSpace(name))
        {
            Console.Error.WriteLine("Usage: AvailabilitySetDemo <subscriptionId> <resourceGroup> <availabilitySetName> [location]");
            Console.Error.WriteLine("       (or set AZURE_SUBSCRIPTION_ID / AZURE_RESOURCE_GROUP / AVAILABILITY_SET_NAME / AZURE_LOCATION)");
            return 1;
        }

        var client = new AvailabilitySetsClient(new DefaultAzureCredential());

        var parameters = new AvailabilitySet
        {
            Location = location,
            Sku = new Sku { Name = "Aligned" },
            Properties = new AvailabilitySetProperties
            {
                PlatformFaultDomainCount = 2,
                PlatformUpdateDomainCount = 5,
            },
        };

        Console.WriteLine($"Creating availability set '{name}' in '{resourceGroup}' ({location})...");

        try
        {
            Response<AvailabilitySet> response = await client.CreateOrUpdateAsync(
                subscriptionId, resourceGroup, name, parameters);

            var avset = response.Value;
            Console.WriteLine("Success.");
            Console.WriteLine($"  Id:       {avset.Id}");
            Console.WriteLine($"  Name:     {avset.Name}");
            Console.WriteLine($"  Type:     {avset.ResourceType}");
            Console.WriteLine($"  Location: {avset.Location}");
            Console.WriteLine($"  Sku:      {avset.Sku?.Name}");
            Console.WriteLine($"  PUDC:     {avset.Properties?.PlatformUpdateDomainCount}");
            Console.WriteLine($"  PFDC:     {avset.Properties?.PlatformFaultDomainCount}");
            return 0;
        }
        catch (RequestFailedException ex)
        {
            Console.Error.WriteLine($"Request failed: {ex.Status} {ex.ErrorCode}");
            Console.Error.WriteLine(ex.Message);
            return ex.Status;
        }
    }

    private static string Get(string[] args, int idx, string envVar)
        => args.Length > idx ? args[idx] : Environment.GetEnvironmentVariable(envVar);
}
