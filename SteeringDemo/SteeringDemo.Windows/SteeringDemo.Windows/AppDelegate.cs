using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using CocosSharp;

namespace SteeringDemo
{

    static class AppDelegate
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CCApplication application = new CCApplication(false, new CCSize(1024f, 640f));
            application.ApplicationDelegate = new GameAppDelegate();

            application.StartGame();
        }
    }


}

