using OtpNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace XIVLauncherOtp
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Check if there is a secret in the args.
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: XIVLauncherOtp.exe {secret}");
                Console.ReadLine();
                return 1;
            }

            byte[] secretKey;
            try
            {
                secretKey = Base32Encoding.ToBytes(args[0]);
            }
            catch
            {
                Console.WriteLine("Error: Couldn't decode the secret key. Ensure that it is encoded in Base32.");
                Console.ReadLine();
                return 2;
            }

            // Check whether XIVLauncher is already running, and only start it when it isn't.
            Process p;
            var launcherProcs = Process.GetProcessesByName("XIVLauncher");
            if (launcherProcs.Length > 0)
            {
                // Use existing XIVLauncher instance.
                Console.WriteLine("Using existing XIVLauncher process.");
                p = launcherProcs[0];
            }
            else
            {
                // Start XIVLauncher
                string localAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string xivlauncherPath = Path.Combine(localAppdata, "XIVLauncher", "XIVLauncher.exe");

                Console.WriteLine("Starting XIVLauncher...");
                p = Process.Start(new ProcessStartInfo
                {
                    FileName = xivlauncherPath,
                });
            }

            var http = new HttpClient();
            var totp = new Totp(secretKey);

            Console.WriteLine("Spamming XIVLauncher with our TOTP...");
            while (!p.HasExited)
            {
                await http.GetAsync("http://localhost:4646/ffxivlauncher/" + totp.ComputeTotp());
                await Task.Delay(1000);
            }

            Console.WriteLine("XIVLauncher has exited. Assuming that the game has started correctly.");
            return 0;
        }
    }
}
