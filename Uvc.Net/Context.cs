using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Uvc.Net;

public class Context : IDisposable
{
    private readonly UvcContext _handle;

    public Context()
    {
        var error = NativeMethods.uvc_init(out _handle, IntPtr.Zero);
        UvcException.ThrowExceptionForUvcError(error);
    }

    private void OnFrame(ref Frame frame, IntPtr ptr)
    {
        Console.WriteLine(frame.Sequence);
    }

    public Device FindDevice(int vendorId = 0, int productId = 0, string serialNumber = null)
    {
        var error = NativeMethods.uvc_find_device(_handle, out var device, vendorId, productId, serialNumber);
        UvcException.ThrowExceptionForUvcError(error);
        return new Device(device);
    }

    public IEnumerable<Device> FindDevices(int vendorId = 0, int productId = 0, string serialNumber = null)
    {
        var error = NativeMethods.uvc_find_devices(_handle, out var devices, vendorId, productId, serialNumber);
        UvcException.ThrowExceptionForUvcError(error);
        try
        {
            var i = 0;
            IntPtr devh;
            while ((devh = Marshal.ReadIntPtr(devices, IntPtr.Size * i++)) != IntPtr.Zero)
            {
                var device = new UvcDevice(devh);
                yield return new Device(device);
            }
        }
        finally { NativeMethods.uvc_free_device_list(devices, 1); }
    }

    public IEnumerable<Device> GetDevices()
    {
        var error = NativeMethods.uvc_get_device_list(_handle, out var devices);
        UvcException.ThrowExceptionForUvcError(error);
        try
        {
            var i = 0;
            IntPtr devh;
            while ((devh = Marshal.ReadIntPtr(devices, IntPtr.Size * i++)) != IntPtr.Zero)
            {
                var device = new UvcDevice(devh);
                yield return new Device(device);
            }
        }
        finally { NativeMethods.uvc_free_device_list(devices, 1); }
    }

    public void TestDevices()
    {
        var err = NativeMethods.uvc_find_device(_handle, out var device, 0, 0, null);
        UvcException.ThrowExceptionForUvcError(err);

        err = NativeMethods.uvc_open(device, out var devh);
        UvcException.ThrowExceptionForUvcError(err);
        Console.WriteLine(err);

        err = NativeMethods.uvc_get_stream_ctrl_format_size(devh, out var ctrl, FrameFormat.Any, 640, 480, 120);
        Console.WriteLine(ctrl.KeyFrameRate);

        err = NativeMethods.uvc_start_streaming(devh, ref ctrl, OnFrame, IntPtr.Zero, 0);
        Console.ReadLine();

        NativeMethods.uvc_stop_streaming(devh);

        NativeMethods.uvc_print_diag(devh, IntPtr.Zero);

        devh.Close();
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}