using System;
using System.Threading.Tasks;
using Azure.Identity;
using Playground.ComputeAdhoc;

namespace Playground.AvailabilitySetDemo;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
    }

    private static string Get(string[] args, int idx, string envVar)
        => args.Length > idx ? args[idx] : Environment.GetEnvironmentVariable(envVar);
}
