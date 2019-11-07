using System.Collections.Generic;

namespace Divv.Log4Net.Json
{
	/// <summary>
	/// logstash format
	/// </summary>
	public class JsonEvent
	{
		public int version { get; set; }
		public string timestamp { get; set; }
		public string app { get; set; }
		public string source_host { get; set; }
		public string thread_name { get; set; }
		public string @class { get; set; }
		public string method { get; set; }
		public string line_number { get; set; }
		public string level { get; set; }
		public string logger_name { get; set; }
		public string message { get; set; }
		public JsonException exception { get; set; }
        public KeyValuePair<string, string>[] properties { get; set; }
        public string[] tags { get; set; }
        public KeyValuePair<string, string>[] contextData { get; set; }
        public KeyValuePair<string, string>[] threadProps { get; set; }
    }
	public class JsonException
	{
		public string exception_class { get; set; }
		public string exception_message { get; set; }
		public string stacktrace { get; set; }
	}


}
