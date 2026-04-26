//Wian Sandenbergh
//Question 1
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
public class Resident
{
    public int Id { get; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string AccountNumber { get; set; }
    public string MonthlyUtilityUsage { get; set; }

    private static int _nextId = 1;

    public Resident(string name, string address, string accountNumber, string monthlyUtilityUsage)
    {
        Id = _nextId++;
        Name = name;
        Address = address;
        AccountNumber = accountNumber;
        MonthlyUtilityUsage = monthlyUtilityUsage;
    }
    public override string ToString()
    {
        return $"[{Id}] {Name} | Address: {Address} | Account: {AccountNumber} | Monthly Usage: {MonthlyUtilityUsage}";
    }
}
