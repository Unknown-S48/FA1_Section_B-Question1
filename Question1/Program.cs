// Wian Sandenbergh
// Question 1
/*
- The system prompts the user to enter the number of residents and gathers each resident's details
- Next, the user is asked to specify the number of service requests, after which the system collects information 
    for each request. This includes associating the request with a specific resident, capturing the request type, 
    priotity level (ranging from 1 to 5), severity level (ranging from 1 to 10), and estimated resolution hours. 
- The application displays a queue of pending service requests, each accompanied by its urgency score. 
- The user can interactively select requests from the queue to process.
- For each processed request, the application generates a report that includes resident details, service request 
    details, and the calculated urgency score. 
- After all requests have been processed, a summary is shown. This summary lists all resolved requests and 
    highlights the request that received the highest urgency score.
*/
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
// Entry point of the application
class Program
{
    // Main method runs the service desk workflow
    static void Main(string[] args)
    {
        var manager = new UtilitiesManager();
        // Display welcome message
        Console.WriteLine("=== Welcome to Emfuleni Municipality Service Desk ===");
        Console.Write("How many residents do you want to register? ");
        // Validate resident count input
        int count;
        while (true)
        {
            var countStr = Console.ReadLine()?.Trim();
            if (int.TryParse(countStr, out count) && count >= 1)
            {
                break;
            }
            Console.WriteLine("Invalid input. Please enter a valid number.");
        }
        // Loop to register residents
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
        // Prompt for number of service requests
        Console.WriteLine();
        Console.Write("How many service requests do you want to log? ");
        // Validate service request count input
        int reqCount;
        while (true)
        {
            var reqCountStr = Console.ReadLine()?.Trim();
            if (int.TryParse(reqCountStr, out reqCount) && reqCount >= 1)
            {
                break;
            }
            Console.WriteLine("Invalid input. Please enter a positive whole number (1 or higher).");
        }
        // Loop to log service requests
        for (int i = 0; i < reqCount; i++)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Service Request {i + 1} ---");
            // Ensure residents exist before logging requests
            if (!manager.Residents.Any())
            {
                Console.WriteLine("No residents registered. Please add residents first.");
                break;
            }
            // Get highest resident ID for validation
            int HighId = manager.Residents.Max(r => r.Id);
            // Validate resident selection
            int residentId;
            while (true)
            {
                Console.Write($"Select resident by number (1 to {HighId}): ");
                var idStr = Console.ReadLine()?.Trim();
                if (int.TryParse(idStr, out residentId) && manager.GetResidentById(residentId) != null)
                {
                    break;
                }
                Console.WriteLine("Invalid resident id. Please try again.");
            }
            // Capture request description
            Console.Write("Request Type (e.g., Water Outage, Burst Pipe): ");
            var desc = Console.ReadLine() ?? "";
            // Validate priority input
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
            // Map priority number to enum
            PriorityLevel priority = MapPriorityToEnum(priorityInput);
            // Validate severity input
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
            // Capture estimated resolution hours
            Console.Write("Estimated Resolution Hours: ");
            var eStr = Console.ReadLine();
            if (!double.TryParse(eStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double est)) est = 1.0;
            est = Math.Max(0, est);
            // Create and calculate request urgency
            var req = manager.CreateServiceRequest(residentId, desc, priority, sev, est);
            req.CalculateUrgencyScore();
            // Display service report
            Console.WriteLine();
            Console.WriteLine("==== Service Report ====");
            Console.WriteLine($"Resident: {req.Requestor.Name}");
            Console.WriteLine($"Service Type: {req.Description}");
            Console.WriteLine($"Urgency Score: {FormatUrgencyForDisplay(req.UrgencyScore)}");
            Console.WriteLine($"Adjusted Resolution: {AdjustedResolution(req.EstimatedResolutionHours, false)}");
            Console.WriteLine($"Household Impact Score: {(req.Requestor.MonthlyUtilityUsage * req.SeverityLevel / 10.0).ToString("F0", CultureInfo.InvariantCulture)},00");
        }
        // Display final summary
        Console.WriteLine();
        Console.WriteLine("==== FINAL MUNICIPAL SUMMARY ====");
        var allRequests = manager.Requests.ToList();
        if (!allRequests.Any())
        {
            Console.WriteLine("No requests logged.");
        }
        else
        {
            // Recalculate urgency scores
            foreach (var r in allRequests) r.CalculateUrgencyScore();
            // Find top request by urgency
            var top = allRequests.OrderByDescending(r => r.UrgencyScore).First();
            // Display highest priority issue
            Console.WriteLine("Highest priority issue:");
            Console.WriteLine($"Resident: {top.Requestor.Name}");
            Console.WriteLine($"Service Type: {top.Description}");
            Console.WriteLine($"Urgency Score: {FormatUrgencyForDisplay(top.UrgencyScore)}");
            Console.WriteLine($"Adjusted Resolution: {AdjustedResolution(top.EstimatedResolutionHours, true)}");
            Console.WriteLine($"Household Impact Score: {(top.Requestor.MonthlyUtilityUsage * top.SeverityLevel / 10.0).ToString("F0", CultureInfo.InvariantCulture)},00");
        }
        // Closing message
        Console.WriteLine();
        Console.WriteLine("Thank you for using the Emfuleni Municipality Service Desk.");
    }
    // Maps numeric priority input to PriorityLevel enum
    static PriorityLevel MapPriorityToEnum(int p)
    {
        if (p <= 2) return PriorityLevel.Low;
        if (p == 3) return PriorityLevel.Medium;
        return PriorityLevel.High;
    }
    // Adjusts resolution hours for display, adding buffer time
    static string AdjustedResolution(double hours, bool isFinalSummary = false)
    {
        var adjusted = isFinalSummary ? hours + 2 : hours + 4;
        return $"{adjusted:0} hours";
    }
    // Formats urgency score for display
    static string FormatUrgencyForDisplay(double score)
    {
        return $"{Math.Round(score):0}";
    }
}