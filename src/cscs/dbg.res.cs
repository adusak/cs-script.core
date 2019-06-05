// Having truly embedded resources (*.res) imposes unnecessary constrain on compatibility.
// Thus cscs.exe compiled for .NET4.5 cannot be even started under .NET4.0 while the only
// v4.5 specific operation cscs.exe does is the initializing the resource manager for loading resources.
// Avoiding using resource manager has completely eliminated the constrain.
// Good read: http://stackoverflow.com/questions/13748055/could-not-load-type-system-runtime-compilerservices-extensionattribute-from-as
// class embedded_strings
// {
//     public static string _dbg_source = "using System.Diagnostics;\r\nusing System;\r\nusing System.Collections;\r\nusing System.IO;\r\nusing System.Linq;\r\nusing System.Reflection;\r\nusing System.Text;\r\nusing System.Text.RegularExpressions;\r\n\r\npublic static class dbg_extensions\r\n{\r\n    static public T dump<T>(this T @object, params object[] args)\r\n    {\r\n        dbg.print(@object, args);\r\n        return @object;\r\n    }\r\n\r\n    static public T print<T>(this T @object, params object[] args)\r\n    {\r\n        dbg.print(@object, args);\r\n        return @object;\r\n    }\r\n}\r\n\r\npartial class dbg\r\n{\r\n    public static bool publicOnly = true;\r\n    public static bool propsOnly = false;\r\n    public static int max_items = 25;\r\n    public static int depth = 1;\r\n\r\n    public static void printf(string format, params object[] args)\r\n    {\r\n        try\r\n        {\r\n            print(string.Format(format, args));\r\n        }\r\n        catch { }\r\n    }\r\n\r\n    public static void print(object @object, params object[] args)\r\n    {\r\n        try\r\n        {\r\n            if (args.Length == 0)\r\n            {\r\n                new dbg().WriteObject(@object);\r\n            }\r\n            else\r\n            {\r\n                var sb = new StringBuilder();\r\n\r\n                foreach (var o in new[] { @object }.Concat(args))\r\n                {\r\n                    if (sb.Length > 0)\r\n                        sb.Append(\" \");\r\n                    sb.Append((o ?? \"{null}\").ToString());\r\n                }\r\n                new dbg().writeLine(sb.ToString());\r\n            }\r\n        }\r\n        catch { }\r\n    }\r\n\r\n    //===============================\r\n    int level = 0;\r\n\r\n    string indent = \"  \";\r\n\r\n    void write(object @object = null)\r\n    {\r\n        if (@object != null)\r\n            Console.Out.Write(@object.ToString().ReplaceClrAliaces());\r\n    }\r\n\r\n    void writeLine(object @object = null)\r\n    {\r\n        write(@object);\r\n        Console.Out.WriteLine();\r\n    }\r\n\r\n    string Indent\r\n    {\r\n        get { return new string('0', level).Replace(\"0\", indent); }\r\n    }\r\n\r\n    string DisplayName(IEnumerable obj)\r\n    {\r\n        if (obj is Array)\r\n        {\r\n            var arr = obj as Array;\r\n            return \"{\" + obj + \"} - Length: \" + arr.Length + \" item\" + (arr.Length == 1 ? \"\" : \"s\");\r\n        }\r\n        else if (obj is IList)\r\n        {\r\n            var arr = obj as IList;\r\n            return \"{IList} - Count: \" + arr.Count;\r\n        }\r\n        else\r\n        {\r\n            var count = obj.Cast<object>().Count();\r\n            return \"{IEnumerable} - \" + count + \" item\" + (count == 1 ? \"\" : \"s\");\r\n        }\r\n    }\r\n\r\n    static public string CustomPrimitiveTypes = \"Newtonsoft.Json.Linq.JValue;\";\r\n\r\n    static bool isPrimitive(object obj)\r\n    {\r\n        if (obj == null || obj.GetType().IsPrimitive || obj is decimal || obj is string)\r\n            return true;\r\n        else if (CustomPrimitiveTypes != null)\r\n            return CustomPrimitiveTypes.Split(new char[] { ';' }).Contains(obj.GetType().ToString());\r\n        return false;\r\n    }\r\n\r\n    void WriteObject(object obj)\r\n    {\r\n        var enumerableElement = obj as IEnumerable;\r\n        level++;\r\n        if (isPrimitive(obj))\r\n        {\r\n            writeLine(obj);\r\n        }\r\n        else if (enumerableElement != null)\r\n        {\r\n            writeLine(DisplayName(enumerableElement));\r\n\r\n            int index = 0;\r\n\r\n            foreach (object item in enumerableElement)\r\n            {\r\n                write(Indent);\r\n                if (index > max_items) //need to have some limit\r\n                {\r\n                    writeLine(\"... truncated ...\");\r\n                    break;\r\n                }\r\n                write(\"[\" + (index++) + \"]: \");\r\n                if (level < (depth + 1))\r\n                {\r\n                    level++;\r\n                    WriteValue(item);\r\n                    // WriteObject(item);\r\n                    level--;\r\n                }\r\n                writeLine(\"\");\r\n            }\r\n        }\r\n        else\r\n        {\r\n            writeLine(\"{\" + obj + \"}\");\r\n\r\n            foreach (MemberInfo m in GetMembers(obj))\r\n            {\r\n                write(Indent);\r\n                write(\".\" + m.Name);\r\n                write(\" = \");\r\n\r\n                object value = GetMemberValue(obj, m);\r\n\r\n                if (isPrimitive(value) || (level >= depth))\r\n                {\r\n                    WriteValue(value);\r\n                    writeLine(\"\");\r\n                }\r\n                else\r\n                    WriteObject(value);\r\n            }\r\n        }\r\n        level--;\r\n    }\r\n\r\n    object GetMemberValue(object element, MemberInfo m)\r\n    {\r\n        FieldInfo f = m as FieldInfo;\r\n        PropertyInfo p = m as PropertyInfo;\r\n\r\n        if (f != null || p != null)\r\n        {\r\n            try\r\n            {\r\n                Type t = f != null ? f.FieldType : p.PropertyType;\r\n                return f != null ? f.GetValue(element) : p.GetValue(element, null);\r\n            }\r\n            catch\r\n            {\r\n                return \"{???}\";\r\n            }\r\n        }\r\n        return null;\r\n    }\r\n\r\n    void WriteValue(object o)\r\n    {\r\n        if (o == null)\r\n            write(\"{null}\");\r\n        else if (o is DateTime)\r\n            write(\"{\" + o + \"}\");\r\n        else if (o is ValueType)\r\n            write(o);\r\n        else if (o is string)\r\n            write(\"\\\"\" + o + \"\\\"\");\r\n        else\r\n            write(\"{\" + o.ToString().TrimStart('{').TrimEnd('}') + \"}\");\r\n    }\r\n\r\n    MemberInfo[] GetMembers(object obj)\r\n    {\r\n        Func<MemberInfo, bool> relevant_types = x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property;\r\n\r\n        if (propsOnly)\r\n            relevant_types = x => x.MemberType == MemberTypes.Property;\r\n\r\n        MemberInfo[] members = obj.GetType()\r\n                                      .GetMembers(BindingFlags.Public | BindingFlags.Instance)\r\n                                      .Where(relevant_types)\r\n                                      .OrderBy(x => x.Name)\r\n                                      .ToArray();\r\n\r\n        var private_members = new MemberInfo[0];\r\n\r\n        if (!publicOnly)\r\n            private_members = obj.GetType()\r\n                                      .GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)\r\n                                      .Where(relevant_types)\r\n                                      .OrderBy(x => x.Name)\r\n                                      .OrderBy(x => char.IsLower(x.Name[0]))\r\n                                      .OrderBy(x => x.Name.StartsWith(\"_\"))\r\n                                      .ToArray();\r\n\r\n        var items = members.Concat(private_members);\r\n        return items.ToArray();\r\n    }\r\n}\r\n\r\nstatic class Extension\r\n{\r\n    static public string ReplaceWholeWord(this string text, string pattern, string replacement)\r\n    {\r\n        return Regex.Replace(text, @\"\\b(\" + pattern + @\")\\b\", replacement);\r\n    }\r\n\r\n    static public string ReplaceClrAliaces(this string text, bool hideSystemNamespace = false)\r\n    {\r\n        if (string.IsNullOrEmpty(text))\r\n            return text;\r\n        else\r\n        {\r\n            var retval = text.ReplaceWholeWord(\"System.Object\", \"object\")\r\n                             .ReplaceWholeWord(\"System.Boolean\", \"bool\")\r\n                             .ReplaceWholeWord(\"System.Byte\", \"byte\")\r\n                             .ReplaceWholeWord(\"System.SByte\", \"sbyte\")\r\n                             .ReplaceWholeWord(\"System.Char\", \"char\")\r\n                             .ReplaceWholeWord(\"System.Decimal\", \"decimal\")\r\n                             .ReplaceWholeWord(\"System.Double\", \"double\")\r\n                             .ReplaceWholeWord(\"System.Single\", \"float\")\r\n                             .ReplaceWholeWord(\"System.Int32\", \"int\")\r\n                             .ReplaceWholeWord(\"System.UInt32\", \"uint\")\r\n                             .ReplaceWholeWord(\"System.Int64\", \"long\")\r\n                             .ReplaceWholeWord(\"System.UInt64\", \"ulong\")\r\n                             .ReplaceWholeWord(\"System.Object\", \"object\")\r\n                             .ReplaceWholeWord(\"System.Int16\", \"short\")\r\n                             .ReplaceWholeWord(\"System.UInt16\", \"ushort\")\r\n                             .ReplaceWholeWord(\"System.String\", \"string\")\r\n                             .ReplaceWholeWord(\"System.Void\", \"void\")\r\n                             .ReplaceWholeWord(\"Void\", \"void\");\r\n\r\n            if (hideSystemNamespace && retval.StartsWith(\"System.\"))\r\n            {\r\n                string typeName = retval.Substring(\"System.\".Length);\r\n\r\n                if (!typeName.Contains('.')) // it is not a complex namespace\r\n                    retval = typeName;\r\n            }\r\n\r\n            return retval.Replace(\"`1\", \"<T>\")\r\n                         .Replace(\"`2\", \"<T, T1>\")\r\n                         .Replace(\"`3\", \"<T, T1, T2>\")\r\n                         .Replace(\"`4\", \"<T, T1, T2, T3>\");\r\n        }\r\n    }\r\n\r\n    // for reflecting dynamic objects look at dbg.dynamic.cs\r\n}";
// }