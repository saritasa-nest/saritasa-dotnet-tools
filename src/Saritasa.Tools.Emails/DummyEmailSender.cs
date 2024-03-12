// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Saritasa.Tools.Emails;

/// <summary>
/// Empty email sender. Can be used for testing.
/// </summary>
public class DummyEmailSender : EmailSender
{
    /// <inheritdoc />
    protected override Task Process(MailMessage message, IDictionary<string, object>? data) => Task.CompletedTask;
}
