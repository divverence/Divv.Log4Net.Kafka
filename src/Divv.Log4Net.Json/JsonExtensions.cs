using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Divv.Log4Net.Json
{
	internal static class JsonExtensions
	{
		public static StringBuilder WriteString(this StringBuilder sb, string name, object value)
        {
			return sb.Append($"\"{name.Replace(':','_')}\":").WriteString(value?.ToString());
		}
		private static StringBuilder WriteString(this StringBuilder sb, string value)
        {
            if (value is null)
                return sb.Append("null");

			sb.Append('\"');

			int runIndex = -1;
			int l = value.Length;
			for (var index = 0; index < l; ++index)
			{
				var c = value[index];

				if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\')// && c != ':' && c!=',')
				{
					if (runIndex == -1)
						runIndex = index;

					continue;
				}

				if (runIndex != -1)
				{
					sb.Append(value, runIndex, index - runIndex);
					runIndex = -1;
				}

				switch (c)
				{
					case '\t': sb.Append("\\t"); break;
					case '\r': sb.Append("\\r"); break;
					case '\n': sb.Append("\\n"); break;
					case '"':
					case '\\': sb.Append('\\'); sb.Append(c); break;
					default:
						sb.Append(c);
						break;
				}
			}

			if (runIndex != -1)
				sb.Append(value, runIndex, value.Length - runIndex);

			return sb.Append('\"');
		}
		public static StringBuilder WriteValueObject(this StringBuilder sb, string name, object value)
		{
			return sb.Append($"\"{name}\":{value}");
		}
		public static StringBuilder WriteMessage(this StringBuilder sb, JsonEvent evt)
		{
			sb.WriteString(nameof(JsonEvent.message), evt.message);

			if (evt.exception != null)
			{
				sb.Append(",")
				  .Append("\"exception\":{")
				  .WriteString(nameof(JsonException.exception_class), evt.exception.exception_class).Append(",")
				  .WriteString(nameof(JsonException.exception_message), evt.exception.exception_message).Append(",")
				  .WriteString(nameof(JsonException.stacktrace), evt.exception.stacktrace)
				  .Append("}");
			}
			return sb;
		}
		public static string ToJson(this JsonEvent evt)
		{
            try
            {
                var logstash = new StringBuilder();
                var comma = ",";
                logstash.Append("{")
                    .WriteValueObject("@version", evt.version).Append(comma)
                    .WriteString("@timestamp", evt.timestamp).Append(comma)
                    //.WriteString(nameof(JsonEvent.source_host), evt.source_host).Append(comma)
                    .WriteString(nameof(JsonEvent.app), evt.app).Append(comma)
                    .WriteString(nameof(JsonEvent.thread_name), evt.thread_name).Append(comma)
                    .WriteString(nameof(JsonEvent.@class), evt.@class).Append(comma)
                    .WriteString(nameof(JsonEvent.method), evt.method).Append(comma)
                    //.WriteString(nameof(JsonEvent.line_number), evt.line_number).Append(comma)
                    .WriteString(nameof(JsonEvent.level), evt.level).Append(comma)
                    .WriteString(nameof(JsonEvent.logger_name), evt.logger_name).Append(comma);

                foreach (var prop in evt.properties)
                    logstash.WriteString(prop.Key, prop.Value).Append(comma);

                if (evt.contextData?.Any() ?? false)
                    foreach (var contextData in evt.contextData)
                        logstash.WriteString(contextData.Key, contextData.Value).Append(comma);

                if (evt.threadProps?.Any() ?? false)
                    foreach (var threadProp in evt.threadProps)
                        logstash.WriteString(threadProp.Key, threadProp.Value).Append(comma);

                if (evt.tags?.Any() ?? false)
                {
                    var innerArray = string.Join(", ", evt.tags.Select(tag => $"\"{tag}\""));
                    logstash.Append($"\"tags\":[{innerArray}]").Append(comma);
                }

                logstash.WriteMessage(evt)
                    .Append("}");

                var json = logstash.ToString();
                //WriteToFile(json);
                return json;
            }
            catch (Exception e)
            {
                var formattableString = $"Exception in ToJson: {e.Message} {e}";
                WriteToFile(formattableString);
                throw;
            }
        }

        public static void WriteToFile( this string json)
        {
            try
            {
                Console.Error.WriteLine(json);
                if (!Directory.Exists("d:\\Logs"))
                    Directory.CreateDirectory("d:\\Logs");
                System.IO.File.AppendAllLines("d:\\Logs\\json.json", new []{json});
            }
            catch{ }
        }
    }
}
