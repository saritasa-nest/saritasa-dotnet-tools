namespace Saritasa.Tools.SourceGenerator.Demo.Models;

/// <summary>
/// Employee model.
/// </summary>
internal partial class Employee : EditableModel
{
    [AlsoNotify(nameof(FullName))]
    private string firstname;

    [AlsoNotify(nameof(FullName))]
    private string surname;

    private int age;

    private DateTime employedAt;

    [DoNotNotify]
    private double salary;

    /// <summary>
    /// Employee full name.
    /// </summary>
    public string FullName => firstname + surname;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="firstname">Employee firstname.</param>
    /// <param name="surname">Employee surname.</param>
    /// <param name="age">Employee age.</param>
    public Employee(string firstname, string surname, int age)
    {
        Firstname = firstname;
        Surname = surname;
        Age = age;

        EmployedAt = DateTime.Now;
    }
}
