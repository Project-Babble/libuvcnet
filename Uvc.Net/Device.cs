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

    [StructLayout(LayoutKind.Sequential)]
    private struct DeviceDescriptor
    {
        public ushort VendorId;
        public ushort ProductId;
        public ushort ComplianceLevel;
        [MarshalAs(UnmanagedType.LPStr)]
        public string SerialNumber;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Manufacturer;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Product;
    }
    internal Device(UvcDevice device)
    {
        _handle = device;
        var error = NativeMethods.uvc_get_device_descriptor(_handle, out var descriptor);
        UvcException.ThrowExceptionForUvcError(error);
        try
        {
            var des = Marshal.PtrToStructure<DeviceDescriptor>(descriptor);
            VendorId = des.VendorId;
            ProductId = des.ProductId;
            ComplianceLevel = des.ComplianceLevel;
            SerialNumber = des.SerialNumber;
            Manufacturer = des.Manufacturer;
            Product = des.Product;
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