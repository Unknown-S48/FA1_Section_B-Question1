//Wian Sandenbergh
//Question 1
/*
- The system prompts the user to enter the number of residents and gathers each resident's details
- Next, the user is asked to specify the number of service requests, after which the system collects information 
    for each request. This includes associating the request with a specific resident, capturing the request type, 
    priotity level (ranging from 1 to 5), severity level (ranging from 1 to 10), and estimated resolution hours. 
- The application displays a queue of pending service requests, eac accompanied by its urgency score. 
- The user can interactively select requests from the queue to process.
- For each processed request, the application generates a report that includes resident details, service request 
    details, and the calculated urgency score. 
- After all requests have been processed, a summary is shown. This summary lists all resolved requests and 
    highlights the request that recieved the highest urgency score.
*/
using System;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        var manager = new UtilitiesManager();

        Console.WriteLine("=== Welcome to Emfuleni Municipality Service Desk ===");
        Console.Write("How many residents do you want to register? ");
        var countStr = Console.ReadLine()?.Trim();
        if (!int.TryParse(countStr, out int count) || count < 0) count = 0;

        for (int i = 0; i < count; i++)
        {
            Console.WriteLine();
            Console.WriteLine($"Enter details for resident #{i + 1}:");
            Console.Write("Name: ");
            var name = Console.ReadLine() ?? "";
            Console.Write("Address: ");
            var address = Console.ReadLine() ?? "";
            Console.Write("Account number: ");
            var account = Console.ReadLine() ?? "";
            Console.Write("Monthly utility usage (kWh or units): ");
            var usageStr = Console.ReadLine() ?? "0";
            if (!double.TryParse(usageStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double usage))
                usage = 0;

            var r = manager.AddResident(name, address, account, usage);
            Console.WriteLine($"Added resident: {r}");
        }

        // Continue with interactive menu
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Emfuleni Municipality Console");
            Console.WriteLine("1) Add resident");
            Console.WriteLine("2) Create service request");
            Console.WriteLine("3) View pending requests (by urgency)");
            Console.WriteLine("4) Resolve a request");
            Console.WriteLine("5) Summary report");
            Console.WriteLine("0) Exit");
            Console.Write("Select option: ");
            var input = Console.ReadLine()?.Trim();

            if (input == "0") break;

            switch (input)
            {
                case "1":
                    AddResidentFlow(manager);
                    break;
                case "2":
                    CreateRequestFlow(manager);
                    break;
                case "3":
                    ViewPendingFlow(manager);
                    break;
                case "4":
                    ResolveFlow(manager);
                    break;
                case "5":
                    Console.WriteLine(manager.SummaryReport());
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }

        Console.WriteLine("Exiting. Goodbye.");
    }

    static void AddResidentFlow(UtilitiesManager manager)
    {
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? "";
        Console.Write("Address: ");
        var address = Console.ReadLine() ?? "";
        Console.Write("Account number: ");
        var account = Console.ReadLine() ?? "";
        Console.Write("Monthly utility usage (kWh or units): ");
        var usageStr = Console.ReadLine() ?? "0";
        if (!double.TryParse(usageStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double usage))
            usage = 0;

        var r = manager.AddResident(name, address, account, usage);
        Console.WriteLine($"Added resident: {r}");
    }

    static void CreateRequestFlow(UtilitiesManager manager)
    {
        Console.WriteLine("Residents:");
        foreach (var r in manager.Residents)
            Console.WriteLine(r.ToString());

        Console.Write("Enter resident ID: ");
        var idStr = Console.ReadLine();
        if (!int.TryParse(idStr, out int residentId))
        {
            Console.WriteLine("Invalid resident id.");
            return;
        }

        var resident = manager.GetResidentById(residentId);
        if (resident == null)
        {
            Console.WriteLine("Resident not found.");
            return;
        }

        Console.Write("Description: ");
        var desc = Console.ReadLine() ?? "";

        Console.Write("Priority (1=Low,2=Medium,3=High): ");
        var pStr = Console.ReadLine();
        PriorityLevel priority = PriorityLevel.Medium;
        if (int.TryParse(pStr, out int pInt) && Enum.IsDefined(typeof(PriorityLevel), pInt))
            priority = (PriorityLevel)pInt;

        Console.Write("Severity (1-10): ");
        var sStr = Console.ReadLine();
        if (!int.TryParse(sStr, out int sev)) sev = 5;
        sev = Math.Clamp(sev, 1, 10);

        Console.Write("Estimated resolution hours: ");
        var eStr = Console.ReadLine();
        if (!double.TryParse(eStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double est)) est = 1.0;

        var req = manager.CreateServiceRequest(residentId, desc, priority, sev, est);
        Console.WriteLine("Created request:");
        Console.WriteLine(req.ToString());
    }

    static void ViewPendingFlow(UtilitiesManager manager)
    {
        var pending = manager.GetPendingRequestsOrderedByUrgency();
        Console.WriteLine("Pending requests (highest urgency first):");
        foreach (var r in pending)
        {
            Console.WriteLine(r.ToString());
        }
    }

    static void ResolveFlow(UtilitiesManager manager)
    {
        Console.Write("Enter request id to resolve: ");
        var s = Console.ReadLine();
        if (!int.TryParse(s, out int id))
        {
            Console.WriteLine("Invalid id.");
            return;
        }

        var success = manager.ResolveRequest(id);
        Console.WriteLine(success ? $"Request {id} marked resolved." : $"Request {id} not found or already resolved.");
    }
}

/*  Code snippet
=== Welcome to Emfuleni Municipality Service Desk ===
How many residents do you want to register? {input}

--- Resident 1 ---
Name: {input}
Address: {input}
Account Number: {input}
Monthly Utility Usage (kWh or liters): {input}

How many service requests do you want to log? {input}

--- Service Request 1 ---
Select resident by number (1 to 1): {input}
Request Type (e.g, Water Outage, Burst Pipe): {input}
Priority Level (1-5): {input}
Severity Level (1-10): {input}
Estimated Resolution Hours: {input}

==== Service Report ====
Resident: Lindeka Mntambo
Service Type: Burst Pipe
Urgency Score: 64
Adjusted Resolution: 9 hours
Household Impact Score: 360,00

==== FINAL MUNICIPAL SUMMARY ====
Highest priority issue: 
Resident: Lindeka Mntambo
Service Type: Burst Pipe
Urgency Score: 64
Adjusted Resolution: 7 hours
Household Impact Score: 360,00

Thank you for using the Emfuleni Municipality Service Desk.
 */