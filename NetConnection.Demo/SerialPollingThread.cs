using System;
using System.Collections.Generic;
using System.Threading;

namespace NetConnection.Demo {
    public class SerialPollingThread {
        private static volatile int NextId;
        private Thread thread;
        private readonly List<SerialDeviceConnection> devices;
        private volatile bool isRunning;
        private volatile bool isPaused;

        public bool IsRunning {
            get => this.isRunning;
            set => this.isRunning = value;
        }

        public bool IsPaused {
            get => this.isPaused;
            set => this.isPaused = value;
        }

        public SerialPollingThread() {
            this.devices = new List<SerialDeviceConnection>();
        }

        public void Add(SerialDeviceConnection connection) {
            lock (this.devices) {
                this.devices.Add(connection);
            }
        }

        /// <summary>
        /// Stars the thread
        /// </summary>
        /// <exception cref="InvalidOperationException">Thread already running</exception>
        public void Start() {
            if (this.isRunning) {
                if (this.thread.ThreadState == ThreadState.Running) {
                    throw new InvalidOperationException("Thread already running");
                }

                // something nasty has happened...
                this.isRunning = false;
                this.thread.Abort();
                this.thread = null;
            }

            if (this.thread == null || this.thread.ThreadState == ThreadState.Stopped) {
                this.thread = new Thread(this.ThreadMain) {
                    Name = "Serial Poll Thread #" + Interlocked.Increment(ref NextId),
                    IsBackground = true
                };
            }

            this.isRunning = true;
            this.thread.Start();
        }

        /// <summary>
        /// Marks the polling thread to gracefully shutdown, optionally allowing to be blocked until it stops
        /// </summary>
        /// <param name="join">True to block until the thread stops</param>
        public void Stop(bool join = false) {
            this.isRunning = false;
            if (join) {
                this.thread?.Join();
            }
        }

        private void ThreadMain() {
            while (this.isRunning) {
                if (this.isPaused) {
                    Thread.Sleep(10);
                    continue;
                }

                bool hasReadData = false;
                lock (this.devices) {
                    foreach (SerialDeviceConnection device in this.devices) {
                        try {
                            hasReadData |= device.ReadNextMessage();
                        }
                        catch (Exception e) {
                            Console.WriteLine(e);
                            device.Disconnect();
                        }
                    }
                }

                if (!hasReadData) {
                    Thread.Sleep(2);
                }
            }
        }
    }
}