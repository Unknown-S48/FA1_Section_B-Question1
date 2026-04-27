// Wian Sandenbergh
// Question 1
/*
- This class is responsible for 
storing resident information
The attributes include 
- name
- address
- account number
- monthly utility usage
*/
using System;
// Class representing a resident with identifying and usage details
public class Resident
{
    public int Id { get; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string AccountNumber { get; set; }
    public double MonthlyUtilityUsage { get; set; } 
    // Static counter to generate unique IDs for residents
    private static int _nextId = 1;
    // Constructor initializes a new resident with provided details
    public Resident(string name, string address, string accountNumber, double monthlyUtilityUsage)
    {
        Id = _nextId++;
        Name = name ?? string.Empty;
        Address = address ?? string.Empty;
        AccountNumber = accountNumber ?? string.Empty;
        MonthlyUtilityUsage = Math.Max(0.0, monthlyUtilityUsage); 
    }
    // Returns a string summary of the resident's details
    public override string ToString()
    {
        return $"[{Id}] {Name} | Address: {Address} | Account: {AccountNumber} | Monthly Usage: {MonthlyUtilityUsage}";
    }
}