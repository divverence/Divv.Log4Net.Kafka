using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using log4net.Core;
using log4net.Layout;
using System.Runtime.Remoting.Messaging;
using JetBrains.Annotations;

namespace Divv.Log4Net.Json
{
    [PublicAPI]
	public sealed class JsonLayout : LayoutSkeleton
	{
		public string App { get; set; }

        private string[] _tags;
        public string Tags
        {
            get => string.Join(",", _tags);
            set => _tags = value?.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        private string[] _callContextVariables;
        public string CallContextVariables
        {
            get => string.Join(",", _callContextVariables);
            set => _callContextVariables = value?.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        public JsonLayout()
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
            #if DEBUG
            Console.WriteLine(message);
#endif

			writer.Write(message);
		}

        private JsonEvent GetJsonObject(LoggingEvent loggingEvent)
        {
            try
            {
                var loggingEventProperties = loggingEvent.GetProperties();
                var tcProps = log4net.ThreadContext.Properties;
                var threadContextProperties = tcProps?.GetKeys()?.Select(key =>
                        new KeyValuePair<string, string>(key, tcProps[key]?.ToString()))
                    .ToArray();


                var obj = new JsonEvent
                {
                    version = 1,
                    timestamp = loggingEvent.TimeStampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ",
                        CultureInfo.InvariantCulture),
                    app = App,
                    tags = _tags,
                    thread_name = loggingEvent.ThreadName,
                    @class = loggingEvent.LocationInformation?.ClassName,
                    method = loggingEvent.LocationInformation?.MethodName,
                    line_number = loggingEvent.LocationInformation?.LineNumber,
                    level = loggingEvent.Level.ToString(),
                    logger_name = loggingEvent.LoggerName,
                    message = loggingEvent.RenderedMessage,
                    properties = loggingEventProperties?.GetKeys()?.Select(key =>
                        new KeyValuePair<string, string>(key, loggingEventProperties[key]?.ToString())).ToArray(),
                    contextData = _callContextVariables?.Select( key =>  new KeyValuePair<string, string>(key, CallContext.LogicalGetData(key)?.ToString())).Where( p => p.Value != null).ToArray(),
                    threadProps = threadContextProperties
                };

                if (loggingEvent.ExceptionObject != null)
                {
                    obj.exception = new JsonException
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
                $"Exception in GetJson: {e.Message} {e} {e.StackTrace}".WriteToFile();
                throw;
            }
        }
    }

}