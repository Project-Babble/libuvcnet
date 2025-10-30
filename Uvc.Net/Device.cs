using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Uvc.Net;

public class Device : IDisposable
{
    private readonly UvcDevice _handle;

    internal Device(UvcDevice device)
    {
        _handle = device;
        var error = NativeMethods.uvc_get_device_descriptor(_handle, out var descriptor);
        UvcException.ThrowExceptionForUvcError(error);
        try
        {
            VendorId = (ushort)Marshal.ReadInt16(descriptor);
            ProductId = (ushort)Marshal.ReadInt16(descriptor, 2);
            ComplianceLevel = (ushort)Marshal.ReadInt16(descriptor, 4);
            SerialNumber = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(descriptor, 6));
            Manufacturer = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(descriptor, 6 + IntPtr.Size));
            Product = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(descriptor, 6 + IntPtr.Size * 2));
        }
        finally { NativeMethods.uvc_free_device_descriptor(descriptor); }
    }

    public ushort VendorId { get; }

    public ushort ProductId { get; }

    public ushort ComplianceLevel { get; }

    public string SerialNumber { get; }

    public string Manufacturer { get; }

    public string Product { get; }

    public DeviceHandle Open()
    {
        var error = NativeMethods.uvc_open(_handle, out var devh);
        UvcException.ThrowExceptionForUvcError(error);
        return new DeviceHandle(devh);
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}