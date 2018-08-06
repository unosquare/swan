namespace Unosquare.Swan.Formatters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class HumanizeJson
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly int _indent;
        private readonly string _indentStr;
        private readonly object _obj;

        public HumanizeJson(object obj, int indent)
        {
            if (obj == null)
            {
                return;
            }

            _indent = indent;
            _indentStr = new string(' ', indent * 4);
            _obj = obj;

            ParseObject();
        }

        public string GetResult() => _builder == null ? string.Empty : _builder.ToString().TrimEnd();

        private void ParseObject()
        {
            switch (_obj)
            {
                case Dictionary<string, object> dictionary:
                    AppendDictionary(dictionary);
                    break;
                case List<object> list:
                    AppendList(list);
                    break;
                default:
                    AppendString();
                    break;
            }
        }

        private void AppendDictionary(Dictionary<string, object> objects)
        {
            foreach (var kvp in objects)
            {
                if (kvp.Value == null) continue;

                var writeOutput = false;

                switch (kvp.Value)
                {
                    case Dictionary<string, object> valueDictionary:
                        if (valueDictionary.Count > 0)
                        {
                            writeOutput = true;
                            _builder
                                .Append($"{_indentStr}{kvp.Key,-16}: object")
                                .AppendLine();
                        }

                        break;
                    case List<object> valueList:
                        if (valueList.Count > 0)
                        {
                            writeOutput = true;
                            _builder
                                .Append($"{_indentStr}{kvp.Key,-16}: array[{valueList.Count}]")
                                .AppendLine();
                        }

                        break;
                    default:
                        writeOutput = true;
                        _builder.Append($"{_indentStr}{kvp.Key,-16}: ");
                        break;
                }

                if (writeOutput)
                    _builder.AppendLine(new HumanizeJson(kvp.Value, _indent + 1).GetResult());
            }
        }

        private void AppendList(List<object> objects)
        {
            var index = 0;
            foreach (var value in objects)
            {
                var writeOutput = false;

                switch (value)
                {
                    case Dictionary<string, object> valueDictionary:
                        if (valueDictionary.Count > 0)
                        {
                            writeOutput = true;
                            _builder
                                .Append($"{_indentStr}[{index}]: object")
                                .AppendLine();
                        }

                        break;
                    case List<object> valueList:
                        if (valueList.Count > 0)
                        {
                            writeOutput = true;
                            _builder
                                .Append($"{_indentStr}[{index}]: array[{valueList.Count}]")
                                .AppendLine();
                        }

                        break;
                    default:
                        writeOutput = true;
                        _builder.Append($"{_indentStr}[{index}]: ");
                        break;
                }

                index++;

                if (writeOutput)
                    _builder.AppendLine(new HumanizeJson(value, _indent + 1).GetResult());
            }
        }

        private void AppendString()
        {
            var stringValue = _obj.ToString();

            if (stringValue.Length + _indentStr.Length > 96 || stringValue.IndexOf('\r') >= 0 ||
                stringValue.IndexOf('\n') >= 0)
            {
                _builder.AppendLine();
                var stringLines = stringValue.ToLines().Select(l => l.Trim()).ToArray();

                foreach (var line in stringLines)
                {
                    _builder.AppendLine($"{_indentStr}{line}");
                }
            }
            else
            {
                _builder.Append($"{stringValue}");
            }
        }
    }
}