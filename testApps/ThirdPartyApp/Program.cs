using log4net.Config;
using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace log4net.Kafka.Console
{
	class Program
	{
		static void Main(string[] args)
		{
            log4net.GlobalContext.Properties["LogPathModifier"] = "SomeValue";
            LogManager.GetRepository().Properties["Testje"] = "Hallo";
			XmlConfigurator.ConfigureAndWatch(new FileInfo(@"log4net.config"));
            LogManager.GetRepository().Properties["Testje2"] = "Hallo2";
            log4net.GlobalContext.Properties["LogPathModifier2"] = "SomeValue2";
			ILog logger = LogManager.GetLogger(typeof(Program));
            log4net.GlobalContext.Properties["LogPathModifier3"] = "SomeValue3";
            CallContext.LogicalSetData("answer", 42);
            log4net.ThreadContext.Properties["fred"] = "anton";
            log4net.LogicalThreadContext.Properties["fret"] = "antoon";

            while (true)
            {
                logger.Debug("this Debug msg");
                logger.Warn("this Warn msg");
                logger.Info("this Info msg");
                Log().Wait();

                try
                {
                    var i = 0;
                    var j = 5 / i;
                }
                catch (Exception ex)
                {
                    logger.Error("this Error msg,中文测试", ex);
                }

                if (System.Console.KeyAvailable)
                    break;

                Thread.Sleep(5000);

                if (System.Console.KeyAvailable)
                    break;
            }
        }

        private static async Task Log()
        {
            ILog logger = LogManager.GetLogger(typeof(Program));
            logger.Error("this Error msg");
            await Task.Delay(1000).ConfigureAwait(false);
            logger.Error("this Error msg");
            logger.Fatal("this Fatal msg");
        }

	}



}
