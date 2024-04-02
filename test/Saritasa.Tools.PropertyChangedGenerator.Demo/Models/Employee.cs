using System.ComponentModel.DataAnnotations;

namespace Saritasa.Tools.PropertyChangedGenerator.Demo.Models;

/// <summary>
/// Employee model.
/// </summary>
internal partial class Employee : EditableModel
{
    [AlsoNotify(nameof(FullName), nameof(Surname))]
    [property: Required]
    [property: MinLength(3)]
    private string firstname;

    [AlsoNotify(nameof(FullName))]
    [property: Required]
    private string surname;

    [Accessibility(Setter.Internal)]
    [property: Required]
    [property: Range(0, 100)]
    private int age;

    [property: Required]
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
