using LSPD_First_Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Reflection;
using YourShift.VersionCheckers;
using System.Security.Policy;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Engine;
using System.Runtime.InteropServices.ComTypes;
using YourShift.Services;
using YourShift.Models;

[assembly: Rage.Attributes.Plugin("YourShift", Description = "A plugin that shows you how long your shift lasts.", Author = "TheGreenCraft")]

namespace YourShift
{
    public class EntryPoint : Plugin
    {


        public int count = 0;
        public static int shiftstopp = 48;
        public static int notificationInterval = 60;
        public static int lunch = shiftstopp / 2;
        public static int lunchend = (shiftstopp / 2) + 5;
        public static int shiftend = shiftstopp - 5;
        public static int breaktime = 5; 
        public static string language = "EN";
        public static string callsign = "Unknown Callsign";
        public static string message = "1st";
        public static string police = "unknown";
        public static bool error = false;
        public static bool waypointset = true;
        public static bool onduty = false;
        

        private static GameFiber lunchTimer;
        private static TimeSpan lunchDuration = TimeSpan.FromMinutes(breaktime);
        private static DateTime lunchEndTime;

        private static StatisticsService statisticsService = new StatisticsService();
        //DRÜBER
        public static void GetShiftSettings()
        {
            InitializationFile iniFile = new InitializationFile("plugins/LSPDFR/YourShift.ini");

            

            language = iniFile.ReadString("Language", "Language", language);

            callsign = iniFile.ReadString("Callsign", "Callsign", callsign);
            message = iniFile.ReadString("Message", "Message", message);
            police = iniFile.ReadString("Station", "Police_Station", police);
            waypointset = iniFile.ReadBoolean("Station", "Waypoint", waypointset);

            shiftstopp = iniFile.ReadInt32("Configuration", "Shift_Stop", shiftstopp);
            notificationInterval = iniFile.ReadInt32("Configuration", "notificationInterval", notificationInterval);
            lunch = shiftstopp / 2;

            shiftend = shiftstopp - 5;
            breaktime = iniFile.ReadInt32("Configuration", "Breaktime", breaktime);
            lunchend = (shiftstopp / 2) + breaktime;
            lunchDuration = TimeSpan.FromMinutes(breaktime);
        }

        public override void Initialize()

        {

            GetShiftSettings();
            if (breaktime > shiftstopp)
            {
                Game.DisplayNotification(String.Format("~h~~r~YOURSHIFT ERROR!"));
                Game.DisplayNotification(String.Format("~o~You have configured the following incorrectly in the ini-file:~n~~n~Breaktime = {0}~n~~n~~c~If you think this is a mistake, please contact me!", breaktime));
                Game.DisplayNotification("~c~YourShift has been stopped");
                error = true;
            }

            if (shiftstopp < 5)
            {
                Game.DisplayNotification(String.Format("~h~~r~YOURSHIFT ERROR!"));
                Game.DisplayNotification(String.Format("~o~You have configured the following incorrectly in the ini-file:~n~~n~~r~Shift_Stop = {0}~n~~n~~c~If you think this is a mistake, please contact me!", shiftstopp));
                Game.DisplayNotification("~c~YourShift has been stopped");
                error = true;
            }
            if (police == "unknown")
            {
                Game.DisplayNotification(String.Format("~h~~r~YOURSHIFT ERROR!"));
                Game.DisplayNotification(String.Format("~o~You have configured the following incorrectly in the ini-file:~n~~n~~r~Police_Station = {0}~n~~n~~c~If you think this is a mistake, please contact me!", police));
                Game.DisplayNotification("~c~YourShift has been stopped");
                error = true;
            }
            if (message != "1st" && message != "2nd" && message != "3rd")
            {
                Game.DisplayNotification(String.Format("~h~~r~YOURSHIFT ERROR!"));
                Game.DisplayNotification(String.Format("~o~You have configured the following incorrectly in the ini-file:~n~~n~~r~Message = {0}~n~~n~~c~If you think this is a mistake, please contact me!", message));
                Game.DisplayNotification("~c~YourShift has been stopped");
                error = true;
            }
            if (language != "EN" && language != "DE")
            {
                Game.DisplayNotification(String.Format("~h~~r~YOURSHIFT ERROR!"));
                Game.DisplayNotification(String.Format("~o~You have configured the following incorrectly in the ini-file:~n~~n~~r~Language = {0}~n~~n~~c~If you think this is a mistake, please contact me!", language));
                Game.DisplayNotification("~c~YourShift has been stopped");
                error = true;
            }



            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;


            if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
            {
                Game.LogTrivial("   ");
                Game.LogTrivial("YOURSHIFT has been loaded...");
                Game.LogTrivial("   ");
                Game.LogTrivial("Your shift begins once you go On-Duty!");
                Game.LogTrivial(String.Format("Your Language: {0}", language));
                Game.LogTrivial(String.Format("Your Callsign: {0}", callsign));
                Game.LogTrivial(String.Format("Your Message Design: {0}", message));
                Game.LogTrivial(String.Format("Police Station: {0}", police));
                Game.LogTrivial(String.Format("Interval: {0}s", notificationInterval));
                Game.LogTrivial(String.Format("Break: {0}min", breaktime));
                Game.LogTrivial(String.Format("Shift: {0}min", shiftstopp));
                Game.LogTrivial("Do you like this plugin? I really appreciate a review on LCPDFR.com!");
                //Game.DisplayNotification(String.Format("Interval: {0}", notificationInterval));
                //Game.DisplayNotification(String.Format("Shiftstopp: {0}", shiftstopp));
                Game.LogTrivial("   ");
            }
            else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
            {
                Game.LogTrivial("   ");
                Game.LogTrivial("YOURSHIFT has been loaded...");
                Game.LogTrivial("   ");
                Game.LogTrivial("Your shift begins once you go On-Duty!");
                Game.LogTrivial(String.Format("Your Language: {0}", language));
                Game.LogTrivial(String.Format("Your Callsign: {0}", callsign));
                Game.LogTrivial(String.Format("Your Message Design: {0}", message));
                Game.LogTrivial(String.Format("Police Station: {0}", police));
                Game.LogTrivial(String.Format("Interval: {0}s", notificationInterval));
                Game.LogTrivial(String.Format("Break: {0}min", breaktime));
                Game.LogTrivial(String.Format("Shift: {0}min", shiftstopp));
                Game.LogTrivial("Do you like this plugin? I really appreciate a review on LCPDFR.com!");
                //Game.DisplayNotification(String.Format("Shiftstopp: {0}", shiftstopp));
                //Game.DisplayNotification(String.Format("Interval: {0}", notificationInterval));
                Game.LogTrivial("   ");
            }

        }

        private void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            onduty = OnDuty;
            if (OnDuty)


                if(error == false)
                {
                    {
                        List<StatisticModel> models = new List<StatisticModel>();
                        models = statisticsService.GetAll();
                        var model = models.First();
                        if (models == null)
                        {
                            var m = new StatisticModel
                            {
                                Rank = Rank.Rooki,
                                Shifts = 69
                            };
                            model = m;
                        }

                        VersionChecker.isUpdateAvailable();

                        int realtime = shiftstopp / 2;

                        Game.LogTrivial("YourShift: Language: " + language);


                        if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                        {

                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                //Datenbank
                                try
                                {
                                    Game.DisplayNotification(String.Format("~b~Dispatch:~m~~n~~c~Your shift now begins. Good luck {3} "+ model.Shifts.ToString() + "~n~~n~~g~Your shift:~n~~s~Length: ~o~{0} hours~n~~s~Break length: ~y~{1} minutes~n~~s~Police Station: ~b~{2}", realtime, breaktime, police, model.Rank.ToString()));
                                } catch (Exception ex)
                                {
                                    Game.LogTrivial("Databank :(" + ex.Message);
                                }
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{2}]~m~~n~~c~Your shift now begins. Good luck!~n~~n~~g~Your shift:~n~~s~Length: ~o~{0} hours~n~~s~Break length: ~y~{1} minutes~n~~s~Police Station: ~b~{3}", realtime, breaktime, callsign, police));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch to {2}: ~m~~n~~c~Your shift now begins. Good luck!~n~~n~~g~Your shift:~n~~s~Length: ~o~{0} hours~n~~s~Break length: ~y~{1} minutes~n~~s~Police Station: ~b~{3}", realtime, breaktime, callsign, police));
                            }

                        }
                        else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                        {

                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~m~~n~~c~Deine Schicht beginnt nun. Viel Glück!~n~~n~~g~Deine Schicht:~n~~s~Länge: ~o~{0} Stunden~n~~s~Pausenlänge: ~y~{1} Minuten~n~~s~Polizeistation: ~b~{2}", realtime, breaktime, police));
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{2}]~m~~n~~c~Deine Schicht beginnt nun. Viel Glück!~n~~n~~g~Deine Schicht:~n~~s~Länge: ~o~{0} Stunden~n~~s~Pausenlänge: ~y~{1} Minuten~n~~s~Polizeistation: ~b~{3}", realtime, breaktime, callsign, police));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch zu {2}: ~m~~n~~c~Deine Schicht beginnt nun. Viel Glück!~n~~n~~g~Deine Schicht:~n~~s~Länge: ~o~{0} Stunden~n~~s~Pausenlänge: ~y~{1} Minuten~n~~s~Polizeistation: ~b~{3}", realtime, breaktime, callsign, police));
                            }

                        }



                        GameFiber.StartNew(ShiftTimer);
                    }
            
            }
        }

        public static void SetWaypoint()
        {
            /*if (waypointset)
            {
                Game.Waypoint(targetCoords.X, targetCoords.Y);
                SpawnBlip.EnableRoute(System.Drawing.Color.Yellow);
            }
            Game.DisplayNotification(String.Format("~g~Du hast Pause!"));
            */

        }


        public static void StartLunchBreak()
        {
            lunchEndTime = DateTime.Now + lunchDuration;

            // Starte den Timer
            lunchTimer = new GameFiber(UpdateLunchTimer);
            lunchTimer.Start();
        }

        private static void UpdateLunchTimer()
        {
            while (true)
            {
                TimeSpan remainingTime = lunchEndTime - DateTime.Now;

                if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                {
                    Game.DisplayNotification(String.Format("~g~You have a break!"));
                }
                else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                {
                    Game.DisplayNotification(String.Format("~g~Du hast Pause!"));
                }
                if (remainingTime <= TimeSpan.Zero)
                {
                    // Die Mittagspause ist vorbei
                    HideLunchMessage();
                    break;
                }

                if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                {
                    string message = $"Shift Break: {remainingTime:mm\\:ss}";
                    Game.DisplaySubtitle(message);
                }
                else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                {
                    string message = $"Schichtpause: {remainingTime:mm\\:ss}";
                    Game.DisplaySubtitle(message);
                }



                GameFiber.Yield();
            }
        }

        public static void HideLunchMessage()
        {
            Game.DisplayNotification("HideLunchMessage()");
            if (lunchTimer != null && lunchTimer.IsAlive)
            {
                lunchTimer.Abort();
            }
        }



        private void ShiftTimer()
        {



            int send_lunch = 0;
            int send_lunchend = 0;
            int send_shiftend = 0;
            int send_shiftstop = 0;
            while (onduty)
            {
                GameFiber.Sleep(1000);

                count++;

                if (Game.IsPaused){
                    Game.DisplayNotification(String.Format("PAUSE HAHAHA {0}", count));
                }

                if (count % (notificationInterval) == 0)
                {
                    int showcount = count / 60;


                    if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                    {

                        if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                        {
                            Game.DisplayNotification(String.Format("~b~Dispatch:~m~~n~~c~~m~ ~n~Current shift time: ~n~~g~{0}~m~ minutes.", count));
                        }
                        else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                        {
                            Game.DisplayNotification(String.Format("~b~[{1}]~m~~n~~c~~m~ ~n~Current shift time: ~n~~g~{0}~m~ minutes.", showcount, callsign));
                        }
                        else
                        {
                            Game.DisplayNotification(String.Format("~b~Dispatch to {1}:~m~~n~~c~~m~ ~n~Current shift time: ~n~~g~{0}~m~ minutes.", showcount, callsign));
                        }

                        //Game.LogTrivial(String.Format("YOURSHIFT > Message for Debug = {0}", count));

                    }
                    else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                        {
                            Game.DisplayNotification(String.Format("~b~Dispatch:~m~~n~Aktuelle Schichtzeit: ~n~~g~{0}~m~ Minuten.", showcount));
                        }
                        else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                        {
                            Game.DisplayNotification(String.Format("~b~[{1}]~m~~n~Aktuelle Schichtzeit: ~n~~g~{0}~m~ Minuten.", showcount, callsign));
                        }
                        else
                        {
                            Game.DisplayNotification(String.Format("~b~Dispatch zu {1}:~m~~n~Aktuelle Schichtzeit: ~n~~g~{0}~m~ Minuten.", showcount, callsign));
                        }
                        //Game.LogTrivial(String.Format("YOURSHIFT > Message for Debug = {0}", count));

                    }

                }






                if (count % (lunch * 60) == 0)
                {




                    if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                    {

                        if (send_lunch == 0)
                        {
                            StartLunchBreak();

                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~n~~y~Break!~n~~n~~c~Eat something and strengthen yourself. Go back on patrol in {0} minutes!", breaktime));
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{1}]~n~~y~Break!~n~~n~~c~Eat something and strengthen yourself. Go back on patrol in {0} minutes!", breaktime, callsign));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch to {1}:~n~~y~Break!~n~~n~~c~Eat something and strengthen yourself. Go back on patrol in {0} minutes!", breaktime, callsign));
                            }
                            Game.LogTrivial(String.Format("YOURSHIFT > Lunch initiated after {0} minutes.", lunch));


                            send_lunch = 1;
                        }
                    }
                    else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                    {

                        if (send_lunch == 0)
                        {
                            StartLunchBreak();

                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~n~~y~Pause!~n~~n~~c~Iss etwas und stärke dich. Geh in {0} Minuten wieder auf Streife!", breaktime));
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{1}]~n~~y~Pause!~n~~n~~c~Iss etwas und stärke dich. Geh in {0} Minuten wieder auf Streife!", breaktime, callsign));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch zu {1}:~n~~y~Pause!~n~~n~~c~Iss etwas und stärke dich. Geh in {0} Minuten wieder auf Streife!", breaktime, callsign));
                            }
                            Game.LogTrivial(String.Format("YOURSHIFT > Lunch initiated after {0} minutes.", lunch));

                            send_lunch = 1;
                        }
                    }

                }





                if (count % (lunchend * 60) == 0)
                {



                    if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                    {
                        if (send_lunchend == 0)
                        {
                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~n~~o~Lunch break over!~n~~c~Come back on patrol. Take good care of yourself!"));
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{0}]~n~~o~Lunch break over!~n~~c~Come back on patrol. Take good care of yourself!", callsign));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch to {0}:~n~~o~Lunch break over!~n~~c~Come back on patrol. Take good care of yourself!", callsign));
                            }
                            Game.LogTrivial(String.Format("YOURSHIFT > Lunch stopped after {0} minutes!", lunchend));

                            send_lunchend = 1;
                        }
                    }
                    else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (send_lunchend == 0)
                        {

                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~n~~o~Mittagspause vorbei!~n~~n~~c~Komm zurück auf Streife. Pass gut auf dich auf!"));
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{0}]~n~~o~Mittagspause vorbei!~n~~n~~c~Komm zurück auf Streife. Pass gut auf dich auf!", callsign));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch zu {0}:~n~~o~Mittagspause vorbei!~n~~n~~c~Komm zurück auf Streife. Pass gut auf dich auf!", callsign));
                            }
                            Game.LogTrivial(String.Format("YOURSHIFT > Lunch stopped after {0} minutes!", lunchend));

                            send_lunchend = 1;
                        }
                    }
                }






                if (count % (shiftend * 60) == 0)
                {


                    if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                    {
                        if (send_shiftend == 0)
                        {

                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~n~~p~Early end of shift!~n~~n~~m~Go ahead and drive to the vicinity of the {0} Police Department. For new ~r~injuries~m~ be sure to go to a ~y~hospital ~m~. Please park the patrol car in a garage so it can be repaired. ~n~Turn in weapons at the department.", police));

                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{0}]~n~~p~Early end of shift!~n~~n~~m~Go ahead and drive to the vicinity of the Mission Row Police Department. For new ~r~injuries~m~ be sure to go to a ~y~hospital ~m~. Please park the patrol car in a garage so it can be repaired. ~n~Turn in weapons at the department.", callsign));

                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch to {0}:~n~~p~Early end of shift!~n~~n~~m~Go ahead and drive to the vicinity of the Mission Row Police Department. For new ~r~injuries~m~ be sure to go to a ~y~hospital ~m~. Please park the patrol car in a garage so it can be repaired. ~n~Turn in weapons at the department.", callsign));

                            }

                            Game.LogTrivial(String.Format("YOURSHIFT > Shiftend is near: {0}", shiftend));
                            send_shiftend = 1;
                        }
                    }
                    else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (send_shiftend == 0) //Fehlerkorrektur für Nachricht 
                        {
                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~n~~p~Bald ist Schichtende!~n~~n~~m~Fahr schonmal in die Nähe des Mission Row Police Departments. Bei neuen ~r~Verletzungen~m~ unbedingt ein ~y~Krankenhaus ~m~aufsuchen. Den Streifenwagen bitte in einer Garage abstellen damit er repariert werden kann. ~n~Waffen im Department abgeben."));

                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{0}]~n~~p~Bald ist Schichtende!~n~~n~~m~Fahr schonmal in die Nähe des Mission Row Police Departments. Bei neuen ~r~Verletzungen~m~ unbedingt ein ~y~Krankenhaus ~m~aufsuchen. Den Streifenwagen bitte in einer Garage abstellen damit er repariert werden kann. ~n~Waffen im Department abgeben.", callsign));

                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch zu {0}:~n~~p~Bald ist Schichtende!~n~~n~~m~Fahr schonmal in die Nähe des Mission Row Police Departments. Bei neuen ~r~Verletzungen~m~ unbedingt ein ~y~Krankenhaus ~m~aufsuchen. Den Streifenwagen bitte in einer Garage abstellen damit er repariert werden kann. ~n~Waffen im Department abgeben.", callsign));

                            }
                            Game.LogTrivial(String.Format("YOURSHIFT > Shift is nearly over after: {0} minutes!", shiftend));
                            send_shiftend = 1;
                        }
                    }


                }

                if (count % (shiftstopp * 60) == 0)
                {


                    if (language.Equals("EN", StringComparison.OrdinalIgnoreCase))
                    {
                        if (send_shiftstop == 0)
                        {
                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~c~~n~Your shift is now over! ~n~~n~~m~Report to the ~g~Mission Row Police Department~m~ and sign out. Get some rest!"));
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{0}]~c~~n~Your shift is now over! ~n~~n~~m~Report to the ~g~Mission Row Police Department~m~ and sign out. Get some rest!", callsign));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch to {0}:~c~~n~Your shift is now over! ~n~~n~~m~Report to the ~g~Mission Row Police Department~m~ and sign out. Get some rest!", callsign));
                            }
                            Game.LogTrivial(String.Format("YOURSHIFT > Shift is over after {0} seconds.", count));

                            send_shiftstop = 1;
                        }
                    }
                    else if (language.Equals("DE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (send_shiftstop == 0)
                        {
                            if (message.Equals("1st", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch:~c~~n~Deine Schicht ist nun vorbei! ~n~~n~~m~Finde dich im ~g~Mission Row Police Department~m~ ein und melde dich ab. Ruhe dich aus!"));
                            }
                            else if (message.Equals("2nd", StringComparison.OrdinalIgnoreCase))
                            {
                                Game.DisplayNotification(String.Format("~b~[{0}]~c~~n~Deine Schicht ist nun vorbei! ~n~~n~~m~Finde dich im ~g~Mission Row Police Department~m~ ein und melde dich ab. Ruhe dich aus!", callsign));
                            }
                            else
                            {
                                Game.DisplayNotification(String.Format("~b~Dispatch zu {0}:~c~~n~Deine Schicht ist nun vorbei! ~n~~n~~m~Finde dich im ~g~Mission Row Police Department~m~ ein und melde dich ab. Ruhe dich aus!", callsign));
                            }
                            Game.LogTrivial(String.Format("YOURSHIFT > Shift is over after {0} seconds.", count));

                            send_shiftstop = 1;
                        }
                    }


                }
                if (count >= (shiftstopp * 60))
                {
                    Game.LogTrivial("YOURSHIFT > All messages have been sent. Your shift is over.");
                    Game.LogTrivial("YOURSHIFT > YourShift has been deactivated!");
                    Game.DisplayNotification("~c~YourShift has been stopped");

                    //Datenbank
                    var model = new StatisticModel
                    {
                        Shifts = 4,
                        Rank = Rank.Officer,
                    };

                    try
                    {
                        statisticsService.Update(model);
                    }
                    catch (Exception ex)
                    {
                        Game.LogTrivial("Databank :(" + ex.Message);
                    }

                    

                    break;
                }
            }
        }


        public override void Finally()
        {
            Game.LogTrivial("YourShift has been stopped. Shift is over.");
            Game.DisplayNotification(String.Format("~g~YourShift~m~~n~~s~Do you like this plugin? I really appreciate a review on LCPDFR.com! ~n~~y~Have a nice day {0}!", callsign));
        }
    }
}
