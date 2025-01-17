namespace elan2mqtt
{

    public class Program
    {
        static Main m;
        static void Main(string[] args)
        {
            m = new Main();
            bool stopSignal = false;
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.WriteLine("Unhandled exception: " + eventArgs.ExceptionObject);
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                Console.WriteLine("Received processExit request");
                stopSignal = true;
            };
            
            Console.CancelKeyPress += (_, ea) =>
            {
                stopSignal = ea.Cancel = true;
                Console.WriteLine("Received SIGINT (Ctrl+C)");
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


