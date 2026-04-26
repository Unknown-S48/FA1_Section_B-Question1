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

public class UtilitiesManager
{
    private readonly List<Resident> _residents = new();
    private readonly List<ServiceRequest> _requests = new();

    public IEnumerable<Resident> Residents => _residents.AsReadOnly();
    public IEnumerable<ServiceRequest> Requests => _requests.AsReadOnly();

    public Resident AddResident(string name, string address, string accountNumber, double monthlyUsage)
    {
        var r = new Resident(name, address, accountNumber, monthlyUsage);
        _residents.Add(r);
        return r;
    }

    public Resident? GetResidentById(int id)
    {
        return _residents.FirstOrDefault(r => r.Id == id);
    }

    public ServiceRequest CreateServiceRequest(int residentId, string description, PriorityLevel priority, int severity, double estimatedHours)
    {
        var resident = GetResidentById(residentId);
        if (resident == null) throw new ArgumentException("Resident not found", nameof(residentId));

        var req = new ServiceRequest(resident, description, priority, severity, estimatedHours);
        _requests.Add(req);
        return req;
    }

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

    public bool ResolveRequest(int requestId)
    {
        var req = _requests.FirstOrDefault(r => r.RequestId == requestId);
        if (req == null || req.IsResolved) return false;
        req.MarkResolved();
        return true;
    }

    public string SummaryReport()
    {
        var totalResidents = _residents.Count;
        var totalRequests = _requests.Count;
        var pending = _requests.Count(r => !r.IsResolved);
        var resolved = totalRequests - pending;

        foreach (var r in _requests.Where(x => !x.IsResolved))
            r.CalculateUrgencyScore();

        var topPending = GetPendingRequestsOrderedByUrgency().Take(5);

        var lines = new List<string>();
        lines.Add("=== Utilities Manager Summary ===");
        lines.Add($"Total residents: {totalResidents}");
        lines.Add($"Total requests: {totalRequests} (Pending: {pending}, Resolved: {resolved})");
        lines.Add("");
        lines.Add("Top pending requests by urgency:");
        foreach (var r in topPending)
        {
            lines.Add(r.ToString());
        }

        return string.Join(Environment.NewLine, lines);
    }

    public void SeedSampleData()
    {
        var a = AddResident("Alice Mokoena", "12 Main St", "ACC1001", 320);
        var b = AddResident("Sipho Dlamini", "45 River Rd", "ACC1002", 120);
        var c = AddResident("Thandi Nkosi", "88 Park Ave", "ACC1003", 450);

        CreateServiceRequest(a.Id, "No water supply", PriorityLevel.High, 9, 8);
        CreateServiceRequest(b.Id, "Streetlight out", PriorityLevel.Medium, 4, 2);
        CreateServiceRequest(c.Id, "Leaking sewer", PriorityLevel.High, 8, 12);
    }
}
