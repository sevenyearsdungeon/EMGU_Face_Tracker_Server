using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Emgu;

namespace FaceTracking
{
    public static class FaceTrackingServer
    {
        static Task listenerTask = null;
        public static void Begin()
        {
            if (listenerTask == null)
                listenerTask = Task.Run(ListenerThread);
        }

        private static void ListenerThread()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            UdpClient client = new UdpClient(5678);
            while (true)
            {
                while (client.Available > 0)
                {
                    client.Receive(ref endpoint);
                    if (FaceTracker.data == null)
                        client.Send(null, 0, endpoint);
                    else
                        client.Send(FaceTracker.data, FaceTracker.data.Length, endpoint);
                }
            }
        }

    }
}
