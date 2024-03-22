using System.ComponentModel.DataAnnotations;

namespace Saritasa.Tools.PropertyChangedGenerator.Demo.Models;

/// <summary>
/// Employee model.
/// </summary>
internal partial class Employee : EditableModel
{
    [AlsoNotify(nameof(FullName), nameof(Surname))]
    [Property<RequiredAttribute>]
    [Property<MinLengthAttribute>(3)]
    private string firstname;

    [AlsoNotify(nameof(FullName))]
    [Property<RequiredAttribute>]
    private string surname;

    [Accessibility(Setter.Internal)]
    [Property<RequiredAttribute>]
    [Property<RangeAttribute>(0, 100)]
    private int age;

    [Property<RequiredAttribute>]
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

        Attribute attr = new RangeAttribute(0, 100);

        EmployedAt = DateTime.Now;
    }
}
