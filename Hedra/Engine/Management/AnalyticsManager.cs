/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 08:41 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Management
{
    /// <summary>
    ///     Description of AnalyticsManager.
    /// </summary>
    public static class AnalyticsManager
    {
        private static readonly string Server = "https://hedra-account-system.herokuapp.com/";
        public static float AverageFPS = 0;
        public static float PlayTime;
        public static bool Online;
        private static float PassedTime;
        public static string UserId { get; private set; }

        public static void Load2()
        {
            UserId = CalculateUserHash();
            //Ignore SSL just for Mono support
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://google.com"))
                    {
                        Online = true;
                    }
                }
            }
            catch
            {
                Online = false;
            }

            if (Online)
                RegisterPlayer();

            RoutineManager.StartRoutine(Run);
        }

        private static string CalculateUserHash()
        {
            var identifiers = Environment.OSVersion + Renderer.GetString(StringName.Vendor)
                                                    + Renderer.GetString(StringName.Renderer)
                                                    + Renderer.GetString(StringName.Version)
                                                    + Environment.OSVersion
                                                    + Environment.UserName;


            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                return Encoding.ASCII.GetString(md5Provider.ComputeHash(Encoding.UTF8.GetBytes(identifiers)));
            }
        }

        public static void RegisterPlayer()
        {
            try
            {
                var Request =
                    WebRequest.Create(Server + "request?type=analytics&gversion=" + Program.GameWindow.GameVersion);
                Request.GetResponse().Close();
                Request.Abort();
            }
            catch (Exception e)
            {
                Online = false;
                //Continue as nothing
            }
        }

        private static IEnumerator Run()
        {
            if (Online)
            {
                PlayTime += 0.25f;
                TaskScheduler.Asynchronous(() => SendData());
            }

            while (Online)
            {
                if (PassedTime > 60)
                {
                    TaskScheduler.Asynchronous(SendData);
                    PassedTime = 0;
                }

                PassedTime += Time.IndependentDeltaTime;
                yield return null;
            }
        }

        public static void SendData()
        {
            var sw = new Stopwatch();
            try
            {
                var specs = OSManager.Specs;
                var Request = WebRequest.Create(Server + "request?type=gametime&time=" +
                                                (PlayTime / 60f).ToString().Replace(",", ".") + "&specs=" + specs);
                Request.GetResponse().Close();
                Request.Abort();
                PlayTime = 0;
            }
            catch (Exception e)
            {
                Online = false;
                Log.WriteLine("Failed to send analytics data. Aborting...");
            }
        }

        public static void SendCrashReport(string Ex, int State)
        {
            try
            {
                if (!Online) return;
#if !DEBUG
                /*
                if(File.Exists(AssetManager.AppPath+"log.txt"))
                    Ex = Ex + Environment.NewLine + "----- PROGRAM LOG -----" + Environment.NewLine + Log.Output;
                        //+ Enviroment.NewLine+" --- Graphic Options ---" + Environment.NewLine+ GraphicsOptions.s;
                
                WebRequest Request =
 WebRequest.Create(Server+"request?type=crash&state="+State.ToString()+"&version="+Program.GameWindow.GameVersion+"&ex="+Ex);
                Request.GetResponse().Close();
                Request.Abort();*/
#endif
            }
            catch (Exception e)
            {
                Online = false;
                Log.WriteLine("Whoops! Error when sending log.");
            }
        }
    }
}