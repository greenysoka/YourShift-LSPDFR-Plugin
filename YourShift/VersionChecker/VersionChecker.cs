using System;
using Rage;
using System.Net;
using System.Runtime;
using ExampleCalloutsSRC;

namespace YourShift.VersionCheckers
{
    public class VersionChecker
    {
        public static bool isUpdateAvailable()
        {
            string curVersion = Settings.CalloutVersion;

            Uri latestVersionUri = new Uri("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=45398&textOnly=1");
            WebClient webClient = new WebClient();
            string receivedData = string.Empty;

            try
            {
                receivedData = webClient.DownloadString("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=45398&textOnly=1").Trim();
            }
            catch (WebException)
            {
                Game.DisplayNotification(String.Format("~g~YourShift~m~~n~~r~Failed to check for an update!~n~~s~Check if you're online!"));

                Game.Console.Print();
                Game.Console.Print("------------------------------------------------------< YOURSHIFT >------------------------------------------------------");
                Game.Console.Print();
                Game.Console.Print(String.Format("Failed to check for an update. Current Version {0}", curVersion));
                Game.Console.Print();
                Game.Console.Print("------------------------------------------------------< YOURSHIFT >------------------------------------------------------");
                Game.Console.Print();
            }
            if (receivedData != Settings.CalloutVersion)
            {
                Game.DisplayNotification(String.Format("~g~YourShift~m~~n~~y~New Update's available!!~n~~n~~c~New Version: {0}~n~Your Version: ~r~{1}", receivedData, curVersion));

                Game.Console.Print();
                Game.Console.Print("------------------------------------------------------< YOURSHIFT >------------------------------------------------------");
                Game.Console.Print();
                Game.Console.Print("A new update is available!");
                Game.Console.Print("Your version:  " + curVersion);
                Game.Console.Print("Updated Version:  " + receivedData);
                Game.Console.Print();
                Game.Console.Print("------------------------------------------------------< YOURSHIFT >------------------------------------------------------");
                Game.Console.Print();
                return true;
            }
            else
            {
                Game.DisplayNotification(String.Format("~g~YourShift~m~~n~~g~The plugin is on the newest version!"));
                return false;
            }
        }
    }
}
