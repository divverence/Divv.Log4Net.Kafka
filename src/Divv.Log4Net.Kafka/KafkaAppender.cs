using log4net.Appender;
using System;
using log4net.Core;
using System.IO;
using Confluent.Kafka;
using JetBrains.Annotations;

namespace Divv.Log4Net.Kafka
{
    [PublicAPI]
	public class KafkaAppender : AppenderSkeleton
	{
		private IProducer<string, string> _producer;

		public KafkaSettings KafkaSettings { get; set; }

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			Start();
		}

		private void Start()
		{
			try
			{
				if (KafkaSettings == null) throw new LogException("KafkaSettings is missing");

				if (KafkaSettings.Brokers == null || KafkaSettings.Brokers.Count == 0) throw new LogException("Broker is not found");

				if (_producer == null)
				{
                    var producerConfig = new ProducerConfig
                    {
                        BootstrapServers = string.Join(",",KafkaSettings.Brokers),
                        Acks = Acks.None,
                    };
                    _producer = new ProducerBuilder<string, string>(producerConfig)
                        .SetErrorHandler(OnProduceError)
                        .Build();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Error("Could not create producer", ex);
			}
		}

        private void OnProduceError(IProducer<string, string> _, Error error)
        {
            ErrorHandler.Error($"Could not publish message to Kafka: {error}");
        }

        private void Stop()
		{
			try
			{
                _producer?.Flush(TimeSpan.FromSeconds(30));
                _producer?.Dispose();
                _producer = null;
            }
			catch (Exception ex)
			{
				ErrorHandler.Error("Error while stopping producer", ex);
			}
		}

		private string GetTopic(LoggingEvent loggingEvent)
		{
            if (KafkaSettings.Topic == null)
                return $"{loggingEvent.LoggerName}.{loggingEvent.Level.Name}";

            using var sw = new StringWriter();
            KafkaSettings.Topic.Format(sw, loggingEvent);
            return sw.ToString();
        }

		private string GetMessage(LoggingEvent loggingEvent)
		{
            using var sr = new StringWriter();
            Layout.Format(sr, loggingEvent);

            if (Layout.IgnoresException && loggingEvent.ExceptionObject != null)
                sr.Write(loggingEvent.GetExceptionString());

            return sr.ToString();
        }

		protected override void Append(LoggingEvent loggingEvent)
		{
			var message = GetMessage(loggingEvent);
			var topic = GetTopic(loggingEvent);
			_producer.Produce(topic, 
                    new Message<string, string>
                    {
                        Key = "log4net",
                        Timestamp = new Timestamp(loggingEvent.TimeStampUtc),
                        Value = message
                    });
		}

        protected override void OnClose()
		{
			base.OnClose();
			Stop();
		}
	}
}
