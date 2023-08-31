namespace Saritasa.Tools.SourceGenerator.Demo.Models;

/// <summary>
/// Company model.
/// </summary>
internal partial class Company : EditableModel
{
    [AlsoNotify(nameof(Fullname))]
    private string name;

    [AlsoNotify(nameof(Fullname))]
    private DateTime foundedAt;

    [DoNotNotify]
    private string budget;

    /// <summary>
    /// Company employees.
    /// </summary>
    public IList<Employee> Employees { get; } = new List<Employee>();

    /// <summary>
    /// Company full name.
    /// </summary>
    public string Fullname => $"Inc. {Name}, founded in {FoundedAt}.";
}
