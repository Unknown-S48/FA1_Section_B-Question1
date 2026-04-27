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
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        var manager = new UtilitiesManager();

        Console.WriteLine("=== Welcome to Emfuleni Municipality Service Desk ===");
        Console.Write("How many residents do you want to register? ");
        int count;
        while (true)
        {
            var countStr = Console.ReadLine()?.Trim();
            if (int.TryParse(countStr, out count)&& count >= 1)
            {
                break;
            }
            Console.WriteLine("Invalid input. Please enter a valid number.");
        }
        for (int i = 0; i < count; i++)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Resident {i + 1} ---");
            Console.Write("Name: ");
            var name = Console.ReadLine() ?? "";
            Console.Write("Address: ");
            var address = Console.ReadLine() ?? "";
            Console.Write("Account Number: ");
            var account = Console.ReadLine() ?? "";
            Console.Write("Monthly Utility Usage (kWh or Litres): ");
            var usageStr = Console.ReadLine() ?? "0";
            if (!double.TryParse(usageStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double usage))
                usage = 0;

            var r = manager.AddResident(name, address, account, usage);
        }

        Console.WriteLine();
        Console.Write("How many service requests do you want to log? ");

        int reqCount;
        while (true)
        {
            var reqCountStr = Console.ReadLine()?.Trim();

            // Must be a valid integer AND >= 1
            if (int.TryParse(reqCountStr, out reqCount) && reqCount >= 1)
            {
                break; 
            }

            Console.WriteLine("Invalid input. Please enter a positive whole number (1 or higher).");
        }

        for (int i = 0; i < reqCount; i++)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Service Request {i + 1} ---");

            if (!manager.Residents.Any())
            {
                Console.WriteLine("No residents registered. Please add residents first.");
                break;
            }

            int HighId = manager.Residents.Max(r => r.Id);

            int residentId;
            while (true)
            {
                Console.Write($"Select resident by number (1 to {HighId}): ");
                var idStr = Console.ReadLine()?.Trim();

                // Must be a valid integer AND correspond to an existing resident
                if (int.TryParse(idStr, out residentId) && manager.GetResidentById(residentId) != null)
                {
                    break; // ✅ valid input, exit loop
                }

                Console.WriteLine("Invalid resident id. Please try again.");
            }


            Console.Write("Request Type (e.g., Water Outage, Burst Pipe): ");
            var desc = Console.ReadLine() ?? "";

            // Priority Level (1-5)
            int priorityInput;
            while (true)
            {
                Console.Write("Priority Level (1-5): ");
                var pStr = Console.ReadLine()?.Trim();

                if (int.TryParse(pStr, out priorityInput) && priorityInput >= 1 && priorityInput <= 5)
                {
                    break;
                }

                Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
            }

            PriorityLevel priority = MapPriorityToEnum(priorityInput);

            // Severity Level (1-10)
            int sev;
            while (true)
            {
                Console.Write("Severity Level (1-10): ");
                var sStr = Console.ReadLine()?.Trim();

                if (int.TryParse(sStr, out sev) && sev >= 1 && sev <= 10)
                {
                    break; 
                }

                Console.WriteLine("Invalid input. Please enter a number between 1 and 10.");
            }


            Console.Write("Estimated Resolution Hours: ");
            var eStr = Console.ReadLine();
            if (!double.TryParse(eStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double est)) est = 1.0;
            est = Math.Max(0, est);

            var req = manager.CreateServiceRequest(residentId, desc, priority, sev, est);

            // Recalculate urgency now to reflect current values
            req.CalculateUrgencyScore();

            // Print service report for this request (match formatting from screenshot)
            Console.WriteLine();
            Console.WriteLine("==== Service Report ====");
            Console.WriteLine($"Resident: {req.Requestor.Name}");
            Console.WriteLine($"Service Type: {req.Description}");
            Console.WriteLine($"Urgency Score: {FormatUrgencyForDisplay(req.UrgencyScore)}");
            Console.WriteLine($"Adjusted Resolution: {AdjustedResolution(req.EstimatedResolutionHours, false)}");

            Console.WriteLine($"Household Impact Score: {(req.Requestor.MonthlyUtilityUsage * req.SeverityLevel / 10.0).ToString("F0", CultureInfo.InvariantCulture)},00");
        }

        // After logging requests, show FINAL MUNICIPAL SUMMARY with highest urgency request
        Console.WriteLine();
        Console.WriteLine("==== FINAL MUNICIPAL SUMMARY ====");

        var allRequests = manager.Requests.ToList();
        if (!allRequests.Any())
        {
            Console.WriteLine("No requests logged.");
        }
        else
        {
            // Ensure all urgencies are up to date
            foreach (var r in allRequests) r.CalculateUrgencyScore();

            var top = allRequests.OrderByDescending(r => r.UrgencyScore).First();

            Console.WriteLine("Highest priority issue:");
            Console.WriteLine($"Resident: {top.Requestor.Name}");
            Console.WriteLine($"Service Type: {top.Description}");
            Console.WriteLine($"Urgency Score: {FormatUrgencyForDisplay(top.UrgencyScore)}");
            Console.WriteLine($"Adjusted Resolution: {AdjustedResolution(top.EstimatedResolutionHours, true)}");

            Console.WriteLine($"Household Impact Score: {(top.Requestor.MonthlyUtilityUsage * top.SeverityLevel / 10.0).ToString("F0", CultureInfo.InvariantCulture)},00");
        }

        Console.WriteLine();
        Console.WriteLine("Thank you for using the Emfuleni Municipality Service Desk.");
    }

    static PriorityLevel MapPriorityToEnum(int p)
    {
        // Map 1..5 into Low/Medium/High
        if (p <= 2) return PriorityLevel.Low;    // 1-2 => Low
        if (p == 3) return PriorityLevel.Medium; // 3 => Medium
        return PriorityLevel.High;               // 4-5 => High
    }

    static string AdjustedResolution(double hours, bool isFinalSummary = false)
    {
        // Service Report: add 4 hours
        // Final Summary: add 2 hours
        var adjusted = isFinalSummary ? hours + 2 : hours + 4;
        return $"{adjusted:0} hours";
    }

    static string FormatUrgencyForDisplay(double score)
    {
        // Display urgency as an integer
        return $"{Math.Round(score):0}";
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
1) Lindeka Mntambo
Enter resident ID: {input}
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
