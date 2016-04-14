﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HamustroNClient.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HamustroNClient.ClientTracker tracker = new ClientTracker(
            "http://46.101.208.202:8080",
            "sharedSecretKey",
            "device01",
            "client01",
            "Windows10",
            "1.0",
            "Szisztem",
            "GitHash",
            0);

        public MainPage()
        {
            this.InitializeComponent();

            tracker.GenerateSession();
        }
        
        private async void Send()
        {
            var hosts = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();

            foreach (var aName in hosts)
            {
                if (aName.Type == HostNameType.Ipv4)
                {
                    await Task.Delay(1);
                }
            }

            //await tracker.TrackEvent(eventName.Text, 123u, "params", true);
        }       
    }
}