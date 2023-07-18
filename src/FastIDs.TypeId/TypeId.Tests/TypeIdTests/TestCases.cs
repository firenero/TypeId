using System;

namespace TypeIdTests.TypeIdTests;

public static class TestCases
{
     public static TestCaseData[] NoPrefix => new[]
    {
        new TestCaseData("00000000000000000000000000", new Guid("00000000-0000-0000-0000-000000000000")),
        new TestCaseData("00000000000000000000000001", new Guid("00000000-0000-0000-0000-000000000001")),
        new TestCaseData("0000000000000000000000000a", new Guid("00000000-0000-0000-0000-00000000000a")),
        new TestCaseData("0000000000000000000000000g", new Guid("00000000-0000-0000-0000-000000000010")),
        new TestCaseData("00000000000000000000000010", new Guid("00000000-0000-0000-0000-000000000020")),
        new TestCaseData("7zzzzzzzzzzzzzzzzzzzzzzzzz", new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff")),
        new TestCaseData("00000000000000000000000010", new Guid("00000000-0000-0000-0000-000000000020")),
        new TestCaseData("01h455vb4pex5vsknk084sn02q", new Guid("01890a5d-ac96-774b-bcce-b302099a8057"))
    };

    public static TestCaseData[] WithPrefix => new[]
    {
        new TestCaseData("zeroid_00000000000000000000000000", new Guid("00000000-0000-0000-0000-000000000000"), "zeroid"),
        new TestCaseData("maxid_7zzzzzzzzzzzzzzzzzzzzzzzzz", new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "maxid"),
        new TestCaseData("prefix_0123456789abcdefghjkmnpqrs", new Guid("0110c853-1d09-52d8-d73e-1194e95b5f19"), "prefix"),
        new TestCaseData("type_01h455vb4pex5vsknk084sn02q", new Guid("01890a5d-ac96-774b-bcce-b302099a8057"), "type"),
        new TestCaseData($"{new string('a', 63)}_0123456789abcdefghjkmnpqrs", new Guid("0110c853-1d09-52d8-d73e-1194e95b5f19"), new string('a', 63)),
    };

    public static TestCaseData[] InvalidIds => new TestCaseData[]
    {
        new("PREFIX_00000000000000000000000000") { TestName = "The prefix should be lowercase with no uppercase letters" },
        new("12345_00000000000000000000000000") { TestName = "The prefix can't have numbers, it needs to be alphabetic" },
        new("pre.fix_00000000000000000000000000") { TestName = "The prefix can't have symbols (period), it needs to be alphabetic" },
        new("pre_fix_00000000000000000000000000") { TestName = "The prefix can't have symbols (underscore), it needs to be alphabetic" },
        new("préfix_00000000000000000000000000") { TestName = "The prefix can only have ascii letters" },
        new("  prefix_00000000000000000000000000") { TestName = "The prefix can't have any spaces" },
        new("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijkl_00000000000000000000000000") { TestName = "The prefix can't be 64 characters, it needs to be 63 characters or less" },
        new("_00000000000000000000000000") { TestName = "If the prefix is empty, the separator should not be there" },
        new("_") { TestName = "A separator by itself should not be treated as the empty string" },
        new("prefix_1234567890123456789012345") { TestName = "The suffix can't be 25 characters, it needs to be exactly 26 characters" },
        new("prefix_123456789012345678901234567") { TestName = "The suffix can't be 27 characters, it needs to be exactly 26 characters" },
        new("prefix_1234567890123456789012345 ") { TestName = "The suffix can't have any spaces" },
        new("prefix_0123456789ABCDEFGHJKMNPQRS") { TestName = "The suffix should be lowercase with no uppercase letters" },
        new("prefix_prefix_123456789-123456789-123456") { TestName = "The suffix should be lowercase with no uppercase letters" },
        new("prefix_ooooooiiiiiiuuuuuuulllllll") { TestName = "The suffix should only have letters from the spec's alphabet" },
        new("prefix_i23456789ol23456789oi23456") { TestName = "The suffix should not have any ambiguous characters from the crockford encoding" },
        new("prefix_123456789-0123456789-0123456") { TestName = "The suffix can't ignore hyphens as in the crockford encoding" },
        new("prefix_8zzzzzzzzzzzzzzzzzzzzzzzzz") { TestName = "The suffix should encode at most 128-bits" },
    };
}