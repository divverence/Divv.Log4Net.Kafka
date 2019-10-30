using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using log4net.Core;
using log4net.Layout;
using log4net.Util;

namespace log4net.Kafka
{
	public class LogstashLayout : LayoutSkeleton
	{
		public string App { get; set; }
        public string Tags { get; set; } = "as,bee";
		public LogstashLayout()
		{
			IgnoresException = false;
		}
		public override void ActivateOptions()
		{

		}

		public override void Format(TextWriter writer, LoggingEvent loggingEvent)
		{
			var evt = GetJsonObject(loggingEvent);

			var message = evt.ToJson();

			writer.Write(message);
		}

        private LogstashEvent GetJsonObject(LoggingEvent loggingEvent)
        {
            try
            {
                var loggingEventProperties = loggingEvent.GetProperties() ?? new PropertiesDictionary();
                var obj = new LogstashEvent
                {
                    version = 1,
                    timestamp = loggingEvent.TimeStampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ",
                        CultureInfo.InvariantCulture),
                    app = App,
                    tags = Tags.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries),
                    thread_name = loggingEvent.ThreadName,
                    @class = loggingEvent.LocationInformation?.ClassName,
                    method = loggingEvent.LocationInformation?.MethodName,
                    line_number = loggingEvent.LocationInformation?.LineNumber,
                    level = loggingEvent.Level.ToString(),
                    logger_name = loggingEvent.LoggerName,
                    message = loggingEvent.RenderedMessage,
                    properties = loggingEventProperties.GetKeys().Select(key =>
                        new KeyValuePair<string, string>(key, loggingEventProperties[key]?.ToString())).ToArray()
                };

                if (loggingEvent.ExceptionObject != null)
                {
                    obj.exception = new LogstashException
                    {
                        exception_class = loggingEvent.ExceptionObject.GetType().ToString(),
                        exception_message = loggingEvent.ExceptionObject.Message,
                        stacktrace = loggingEvent.ExceptionObject.StackTrace
                    };
                }

                return obj;
            }
            catch (Exception e)
            {
                $"Exception in GetJson: {e.Message} {e}".WriteToFile();
                throw;
            }
        }
    }

}