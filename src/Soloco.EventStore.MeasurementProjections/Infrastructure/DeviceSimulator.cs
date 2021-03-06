using System;
using System.Threading;
using System.Threading.Tasks;

using EventStore.ClientAPI;
using Soloco.EventStore.Core.Infrastructure;
using Soloco.EventStore.MeasurementProjections.Events;

namespace Soloco.EventStore.MeasurementProjections.Infrastructure
{
    public class DeviceSimulator
    {
        private static readonly Random Random = new Random();

        private readonly IEventStoreConnection _connection;
        private readonly IConsole _console;

        private volatile bool _running;

        public DeviceSimulator(IEventStoreConnection connection, IConsole console)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (console == null) throw new ArgumentNullException("console");

            _connection = connection;
            _console = console;
        }

        public void ConfigureDevice(int device, string type)
        {
            var deviceName = "Device-" + device;

            AppendConfiguredEvent(deviceName, type).Wait();
        }

        public void Start(int devices, TimeSpan eventInterval)
        {
            _running = true;

            var eventIntervalMilliseconds = (int)eventInterval.TotalMilliseconds;

            for (var device = 0; device < devices; device++)
            {
                var deviceName = "Device-" + device;
                Task.Run(() => StartAsync(deviceName, eventIntervalMilliseconds));
            }
        }

        public void Stop()
        {
            _running = false;
        }

        private async void StartAsync(string deviceName, int eventIntervalMilliseconds)
        {
            var start = DateTime.Now;

            var last = start;
            var eventsPublished = 1;

            while (_running)
            {
                await AppendMeasurementReadEvent(start, Random, deviceName);

                start = start.AddMinutes(60);

                last = Log100Events(eventsPublished, deviceName, last);
                eventsPublished++;

                Thread.Sleep(eventIntervalMilliseconds);
            }
        }

        private DateTime Log100Events(int i, string streamName, DateTime last)
        {
            if (i % 100 != 0) return last;

            _console.Timings(streamName + ": 100 events duration ms " + (DateTime.Now - last).TotalMilliseconds);
            return DateTime.Now;
        }

        private async Task AppendMeasurementReadEvent(DateTime start, Random random, string deviceName)
        {
            var @event = new MeasurementRead(start, random.Next(150, 300) / 10m)
                .AsJsonEvent();

            await _connection.AppendToStreamAsync(deviceName, ExpectedVersion.Any, new[] { @event });
        }

        private Task<WriteResult> AppendConfiguredEvent(string deviceName, string type)
        {
            var @event = new DeviceConfigured(type, deviceName)
                .AsJsonEvent();

            return _connection.AppendToStreamAsync(deviceName, ExpectedVersion.Any, new[] { @event });
        }
    }
}