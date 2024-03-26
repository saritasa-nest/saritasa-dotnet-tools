// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain;

/// <summary>
/// The class is used to get messages to display for the end-user.
/// </summary>
public class DomainErrorDescriber
{
    /// <summary>
    /// The default describer.
    /// </summary>
    public static DomainErrorDescriber Default { get; set; } = new DomainErrorDescriber();

    /// <summary>
    /// Get general error message.
    /// </summary>
    public virtual string Error() => Properties.Strings.Error;

    /// <summary>
    /// Get general server error message.
    /// </summary>
    public virtual string ServerError() => Properties.Strings.ServerError;

    /// <summary>
    /// Get conflict error message.
    /// </summary>
    public virtual string Conflict() => Properties.Strings.Conflict;

    /// <summary>
    /// Get forbidden error message.
    /// </summary>
    public virtual string Forbidden() => Properties.Strings.Forbidden;

    /// <summary>
    /// Get not found error message.
    /// </summary>
    public virtual string NotFound() => Properties.Strings.NotFound;

    /// <summary>
    /// Get not found specified item error message.
    /// </summary>
    public virtual string NotFoundItem(string entityName) =>
        string.Format(Properties.Strings.NotFoundEntity, entityName);

    /// <summary>
    /// Get unauthorized error message.
    /// </summary>
    public virtual string Unauthorized() => Properties.Strings.Unauthorized;

    /// <summary>
    /// Get validation errors message.
    /// </summary>
    public virtual string ValidationErrors() => Properties.Strings.ValidationErrors;

    /// <summary>
    /// Get validation error is required message.
    /// </summary>
    public virtual string ValidationErrorIsEmpty() => Properties.Strings.ValidationErrorIsEmpty;

    /// <summary>
    /// Get cannot find message.
    /// </summary>
    /// <param name="entityName">Entity name.</param>
    public virtual string CannotFind(string entityName) =>
        string.Format(Properties.Strings.CannotFind, entityName);
}
