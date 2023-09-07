using System.ComponentModel;
using Saritasa.Tools.PropertyChangedGenerator.Demo.Models;

namespace Saritasa.Tools.PropertyChangedGenerator.Demo;

/// <summary>
/// Application point of entry.
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("App started.");

        try
        {
            var company = new Company();
            company.Employees.Add(new Employee(
                firstname: "Wilfred",
                surname: "Fulton",
                age: 23));
            company.Employees.Add(new Employee(
                firstname: "Nabil",
                surname: "Norman",
                age: 25));

            company.PropertyChanging += Company_PropertyChanging;
            company.PropertyChanged += Company_PropertyChanged;

            company.Name = "Saritasa";
            company.FoundedAt = new DateTime(2005, 6, 15);
        }
        finally
        {
            Console.ReadLine();
        }
    }

    private static void Company_PropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        var company = sender as Company;
        if (company == null)
        {
            return;
        }

        if (e.PropertyName == nameof(Company.Fullname))
        {
            Console.WriteLine("Old company name: {0}", company.Fullname);
        }
    }

    private static void Company_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var company = sender as Company;
        if (company == null)
        {
            return;
        }

        if (e.PropertyName == nameof(Company.Fullname))
        {
            Console.WriteLine("New company name: {0}", company.Fullname);
        }
    }
}
