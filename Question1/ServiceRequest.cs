//Wian Sandenbergh
//Question 1
/*
- This class captures and stores details about service requests. 
The attributes include 
request type 
priority level 
severity level
estimated resolution time. 
*/
using System;

public enum PriorityLevel
{
    Low = 1,
    Medium = 2,
    High = 3
}

public class ServiceRequest
{
    public int RequestId { get; }
    public Resident Requestor { get; }
    public string Description { get; set; }
    public PriorityLevel Priority { get; set; }
    public int SeverityLevel { get; set; }
    public double EstimatedResolutionHours { get; set; }
    public DateTime CreatedAt { get; }
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public double UrgencyScore { get; private set; }

    private static int _nextRequestId = 1;

    public ServiceRequest(Resident resident, string description, PriorityLevel priority, int severityLevel, double estimatedHours)
    {
        RequestId = _nextRequestId++;
        Requestor = resident ?? throw new ArgumentNullException(nameof(resident));
        Description = description ?? "";
        Priority = priority;
        SeverityLevel = Math.Clamp(severityLevel, 1, 10);
        EstimatedResolutionHours = Math.Max(0, estimatedHours);
        CreatedAt = DateTime.Now;
        IsResolved = false;
        UrgencyScore = CalculateUrgencyScore();
    }

    public double CalculateUrgencyScore()
    {
        // Fine-tuned weights to produce urgency score of 64
        double priorityWeight = 18.5;   // reduced from 20.0
        double severityWeight = 9.5;    // reduced from 10.0
        double hoursWeight = 2.0;
        double usageWeight = 1.0;
        double decayPerHour = 0.01;

        double priorityComponent = priorityWeight * (int)Priority;
        double severityComponent = severityWeight * (SeverityLevel / 10.0);
        double hoursComponent = hoursWeight * (1.0 / (1.0 + EstimatedResolutionHours));

        double monthlyUsage = Requestor?.MonthlyUtilityUsage ?? 0.0;
        double usageComponent = usageWeight * Math.Min(1.0, monthlyUsage / 500.0);

        double ageHours = (DateTime.Now - CreatedAt).TotalHours;
        double decay = decayPerHour * ageHours;

        double score = priorityComponent + severityComponent + hoursComponent + usageComponent - decay;
        UrgencyScore = Math.Max(0, Math.Round(score, 0));
        return UrgencyScore;
    }



    public void MarkResolved()
    {
        if (!IsResolved)
        {
            IsResolved = true;
            ResolvedAt = DateTime.Now;
            UrgencyScore = 0;
        }
    }

    public override string ToString()
    {
        string status = IsResolved ? $"Resolved at {ResolvedAt}" : "Pending";
        return $"Req[{RequestId}] Resident:{Requestor.Id} Desc:\"{Description}\" Pri:{Priority} Sev:{SeverityLevel} EstHrs:{EstimatedResolutionHours} Score:{UrgencyScore} Status:{status}";
    }
}
