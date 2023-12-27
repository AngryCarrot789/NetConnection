using System;
using System.IO.Ports;
using System.Threading;

namespace NetConnection.Demo {
    internal class Program {
        static Program() {

        }

        public struct MsgLEDState {
            public byte ledId;
            public byte state;
        }

        public struct MsgLEDStatusResponce {
            public byte responce; // 1 == OK, 0 == invalid
        }

        public static void Main(string[] args) {
            SerialDeviceConnection port = new SerialDeviceConnection(new SerialPort("COM5", 9600));

            SerialPollingThread thread = new SerialPollingThread();
            thread.Add(port);
            thread.Start();

            MessageRegistry registry = new MessageRegistry();
            FrameworkMessage msg1 = registry.RegisterMessage(1, MsgDirection.FromServerToClient, "SET LED STATE");
            FrameworkMessage msg2 = registry.RegisterMessage(2, MsgDirection.FromClientToServer, "LED STATUS RESPONSE");
            msg2.AddHandler<MsgLEDStatusResponce>((msg, data) => {
                Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss:FFFF")}] LED STATUS: " + (data.responce == 1 ? "ON" : "OFF"));
            });

            port.MessageBus = new MessageBus(registry);
            port.Connect();
            const int delay = 100; // 250
            for (int i = 0; i < 10; i++) {
                port.WriteMessage(msg1.Id, 0, new MsgLEDState() {ledId = 13, state = 1});
                Thread.Sleep(delay);
                port.WriteMessage(msg1.Id, 0, new MsgLEDState() {ledId = 13, state = 0});
                Thread.Sleep(delay);
            }

            Thread.Sleep(100);
            port.Disconnect();
            port.Dispose();
        }
    }
}
