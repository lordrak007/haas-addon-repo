using elan2mqtt.Model.eLan;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace elan2mqtt
{

    public class Program
    {

        static Main m;
        static void Main(string[] args)
        {
            var log = ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

            m = new Main();
            bool stopSignal = false;
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                log.LogError("Unhandled exception: " + eventArgs.ExceptionObject);
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                log.LogInformation("Received processExit request");
                stopSignal = true;
            };
            
            Console.CancelKeyPress += (_, ea) =>
            {
                stopSignal = ea.Cancel = true;
                log.LogInformation("Received SIGINT (Ctrl+C)");
            };
            // waiting to get stop signal
            while (!stopSignal)
            {
                Thread.Sleep(500);
            }
            // process clean exit
            m?.Exit();
        }
    }
}


