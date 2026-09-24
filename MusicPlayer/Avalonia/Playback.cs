using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ManagedBass;
using ManagedBass.DirectX8;

namespace MusicPlayer
{
    public sealed class Playback
    {
        private static bool _resolverReady;
        private DXParamEQ _eq;
        private bool _eqFailed;
        private readonly Dictionary<int, int> _bands = new Dictionary<int, int>();

        public int Stream { get; private set; }

        public static void PrepareNativeLibrary()
        {
            if (_resolverReady) return;
            _resolverReady = true;
            NativeLibrary.SetDllImportResolver(typeof(Bass).Assembly, ResolveBass);
        }

        public bool Start()
        {
            PrepareNativeLibrary();
            return Bass.Init(-1, 44100, DeviceInitFlags.Default, IntPtr.Zero);
        }

        public string LastError()
        {
            return Bass.LastError.ToString();
        }

        public bool PlayFile(string path)
        {
            FreeStream();
            Stream = Bass.CreateStream(path, 0, 0, BassFlags.Default);
            return StartStream();
        }

        public bool PlayUrl(string url)
        {
            FreeStream();
            Stream = Bass.CreateStream(url, 0, BassFlags.Default, null, IntPtr.Zero);
            return StartStream();
        }

        public double ProbeSeconds(string path)
        {
            var handle = Bass.CreateStream(path, 0, 0, BassFlags.Decode);
            if (handle == 0) return double.NaN;
            var seconds = Bass.ChannelBytes2Seconds(handle, Bass.ChannelGetLength(handle));
            Bass.StreamFree(handle);
            return seconds;
        }

        public void Pause()
        {
            if (Stream != 0) Bass.ChannelPause(Stream);
        }

        public void Resume()
        {
            if (Stream != 0) Bass.ChannelPlay(Stream, false);
        }

        public void Stop()
        {
            if (Stream != 0) Bass.ChannelStop(Stream);
        }

        public void SetVolume(double unit)
        {
            if (Stream != 0)
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Volume, unit);
        }

        public void Seek(double seconds)
        {
            if (Stream == 0) return;
            Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, seconds));
        }

        public double PositionSeconds()
        {
            if (Stream == 0) return 0;
            return Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetPosition(Stream));
        }

        public double LengthSeconds()
        {
            if (Stream == 0) return 0;
            return Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetLength(Stream));
        }

        public void ApplyEq(int band, double centerHz, double gainDb)
        {
            if (Stream == 0 || _eqFailed || !OperatingSystem.IsWindows()) return;
            try
            {
                if (_eq == null)
                    _eq = new DXParamEQ(Stream, 18);
                int index;
                if (!_bands.TryGetValue(band, out index))
                {
                    index = _eq.AddBand(centerHz);
                    _bands[band] = index;
                }
                _eq.UpdateBand(index, gainDb);
            }
            catch (Exception)
            {
                _eqFailed = true;
            }
        }

        public void FreeStream()
        {
            if (_eq != null)
            {
                _eq.Dispose();
                _eq = null;
                _bands.Clear();
            }
            if (Stream != 0)
            {
                Bass.StreamFree(Stream);
                Stream = 0;
            }
        }

        public void Shutdown()
        {
            FreeStream();
            Bass.Free();
        }

        private bool StartStream()
        {
            if (Stream == 0 || !Bass.ChannelPlay(Stream, false))
            {
                FreeStream();
                return false;
            }
            return true;
        }

        private static IntPtr ResolveBass(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (!string.Equals(libraryName, "bass", StringComparison.OrdinalIgnoreCase))
                return IntPtr.Zero;

            var fileName = OperatingSystem.IsWindows() ? "bass.dll" : "libbass.dylib";
            var path = Path.Combine(AppContext.BaseDirectory, fileName);
            return File.Exists(path) ? NativeLibrary.Load(path) : IntPtr.Zero;
        }
    }
}
