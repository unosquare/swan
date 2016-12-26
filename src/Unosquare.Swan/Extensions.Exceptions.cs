namespace Unosquare.Swan
{
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    partial class Extensions
    {
        /// <summary>
        /// Humanizes the specified exception object so it becomes more easily readable
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static string Humanize(this Exception ex)
        {
            if (ex == null) return string.Empty;
            var jsonText = Json.Serialize(ex, false, "Exception Type", false, null, new[]
            {
                nameof(Exception.Data), nameof(Exception.Source), "TargetSite"
            });

            var jsonData = Json.Deserialize(jsonText) as Dictionary<string, object>;
            if (jsonData == null) return string.Empty;
            var readableException = HumanizeJson(jsonData, 0);
            return readableException;
        }

        /// <summary>
        /// Humanizes the json serialization result.
        /// </summary>
        /// <param name="jsonResult">The json result.</param>
        /// <param name="indent">The indent.</param>
        /// <returns></returns>
        private static string HumanizeJson(object jsonResult, int indent)
        {
            var builder = new StringBuilder();
            var indentStr = new string(' ', indent * 4);
            if (jsonResult == null) return string.Empty;

            var dictionary = jsonResult as Dictionary<string, object>;
            var list = jsonResult as List<object>;

            if (dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    if (kvp.Value == null) continue;
                    builder.Append($"{indentStr}{kvp.Key,-20}: ");

                    var valueDictionary = kvp.Value as Dictionary<string, object>;
                    var valueList = kvp.Value as List<object>;

                    if ((valueDictionary != null && valueDictionary.Count > 0) 
                        || (valueList != null && valueList.Count > 0))
                        builder.AppendLine();

                    builder.AppendLine(HumanizeJson(kvp.Value, indent + 1).TrimEnd());
                }

                return builder.ToString().TrimEnd();
            }

            if (list != null)
            {
                builder.AppendLine();
                foreach (var item in list)
                {
                    if (item == null) continue;
                    builder.AppendLine($"{HumanizeJson(item, indent + 1).TrimEnd()}");
                }

                return builder.ToString().TrimEnd();
            }

            var stringValue = jsonResult.ToString();

            if (stringValue.Length + indentStr.Length > 96 || stringValue.IndexOf('\r') >= 0 || stringValue.IndexOf('\n') >= 0)
            {
                builder.AppendLine();
                var stringLines = stringValue.ToLines().Select(l => l.Trim()).ToArray();
                foreach (var line in stringLines)
                    builder.AppendLine($"{indentStr}{line}");
            }
            else
            {
                builder.Append($"{stringValue}");
            }

            return builder.ToString().TrimEnd();

        }
    }
}
