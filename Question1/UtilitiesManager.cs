//Wian Sandenbergh
//Question 1
/*
- This class processes service requests. 
Its responsibilities include calculating urgency scores for requests 
and generating comprehensive service reports.
*/
using System;
using System.Collections.Generic;
using System.Linq;
//Creating the UtilitiesManger class
public class UtilitiesManager
{
    //Storing the residents and requests privatly in this class
    private readonly List<Resident> _residents = new();
    private readonly List<ServiceRequest> _requests = new();
    //Getting a view of the residents and service requests
    public IEnumerable<Resident> Residents => _residents.AsReadOnly();
    public IEnumerable<ServiceRequest> Requests => _requests.AsReadOnly();
    //Creating a Resident Class and adding all the attributes
    public Resident AddResident(string name, string address, string accountNumber, double monthlyUsage)
    {
        //Creating a var called r and storing all the Residents attributes in it
        var r = new Resident(name, address, accountNumber, monthlyUsage);
        _residents.Add(r);
        return r;
    }
    //Creating a class that gets the resident by their ID
    public Resident? GetResidentById(int id)
    {
        return _residents.FirstOrDefault(r => r.Id == id);
    }
    //Creating a class that adds a service request for a resident to the list
    public ServiceRequest CreateServiceRequest(int residentId, string description, PriorityLevel priority, int severity, double estimatedHours)
    {
        var resident = GetResidentById(residentId);
        //Checking if the input is valid
        if (resident == null) throw new ArgumentException("Resident not found", nameof(residentId));
        //Creating the request var and adding the request attributes
        var req = new ServiceRequest(resident, description, priority, severity, estimatedHours);
        _requests.Add(req);
        return req;
    }
    //Creating a class to return all unresolved service requests ordered by urgency score and creation time
    public IEnumerable<ServiceRequest> GetPendingRequestsOrderedByUrgency()
    {
        foreach (var r in _requests.Where(x => !x.IsResolved))
            r.CalculateUrgencyScore();
        return _requests
            .Where(r => !r.IsResolved)
            .OrderByDescending(r => r.UrgencyScore)
            .ThenBy(r => r.CreatedAt)
            .ToList();
    }
    //This class resolves a service request by ID if it exists
    public bool ResolveRequest(int requestId)
    {
        var req = _requests.FirstOrDefault(r => r.RequestId == requestId);
        if (req == null || req.IsResolved) return false;
        req.MarkResolved();
        return true;
    }
    //This class generates a summary report of residents and service requests with top pending items
    public string SummaryReport()
    {
        //Creating count totals for residents and requests
        var totalResidents = _residents.Count;
        var totalRequests = _requests.Count;
        var pending = _requests.Count(r => !r.IsResolved);
        var resolved = totalRequests - pending;
        //Looping to update urgency score for all unresolved requests
        foreach (var r in _requests.Where(x => !x.IsResolved))
            r.CalculateUrgencyScore();
        //Getting the top 5 pending requests and ordering them by urgency
        var topPending = GetPendingRequestsOrderedByUrgency().Take(5);
        //Adding the Manager Summary to the List
        var lines = new List<string>();
        lines.Add("=== Utilities Manager Summary ===");
        lines.Add($"Total residents: {totalResidents}");
        lines.Add($"Total requests: {totalRequests} (Pending: {pending}, Resolved: {resolved})");
        lines.Add("");
        lines.Add("Top pending requests by urgency:");
        //Adding each top pending request to the report
        foreach (var r in topPending)
        {
            lines.Add(r.ToString());
        }
        //Return the summary as a single string
        return string.Join(Environment.NewLine, lines);
    }
}
