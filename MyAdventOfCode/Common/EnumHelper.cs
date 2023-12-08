using System;
using System.Collections.Generic;
using System.Reflection;

namespace MyAdventOfCode.Common;

public static class EnumHelper
{
    public static Dictionary<TEnum, TMetaDataAttribute> GetEnumMembersAndAttributes<TEnum, TMetaDataAttribute>()
    where TEnum : Enum
    where TMetaDataAttribute : Attribute
    {
        var results = new Dictionary<TEnum, TMetaDataAttribute>();
        var type = typeof(TEnum);
        var values = (TEnum[])Enum.GetValues(type);
        foreach (var value in values)
        {
            var member = type.GetField(value.ToString());
            if (member == null) continue;

            var attribute = member.GetCustomAttribute<TMetaDataAttribute>();
            if (attribute == null) continue;

            results.Add(value, attribute);
        }

        return results;
    }
}