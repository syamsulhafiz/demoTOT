using System;
using System.Collections.Generic;

namespace Demo1.Models;

public partial class UserQuestionLog
{
    public int Id { get; set; }

    public string? Question { get; set; }

    public string? MatchedAnswer { get; set; }

    public DateTime? AskedAt { get; set; }

    public int? Score { get; set; }
}
