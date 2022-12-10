using System;

namespace MyAdventOfCode.Common.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class AliasAttribute : Attribute
{
    public string[] Aliases { get; set; }

    public AliasAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }
}