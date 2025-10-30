using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Uvc.Net;

public class DeviceHandle : IDisposable
{
    private readonly UvcDeviceHandle _handle;
    private FrameCallback _callback;

    internal DeviceHandle(UvcDeviceHandle deviceHandle)
    {
        _handle = deviceHandle;
    }

    public AutoExposureMode AutoExposure
    {
        get
        {
            NativeMethods.uvc_get_ae_mode(_handle, out var value, RequestCode.GetCurrent);
            return (AutoExposureMode)value;
        }
        set
        {
            var error = NativeMethods.uvc_set_ae_mode(_handle, (byte)value);
            UvcException.ThrowExceptionForUvcError(error);
        }
    }

    public bool AutoExposurePriority
    {
        get
        {
            NativeMethods.uvc_get_ae_priority(_handle, out var value, RequestCode.GetCurrent);
            return value != 0;
        }
        set
        {
            var error = NativeMethods.uvc_set_ae_priority(_handle, value ? (byte)1 : (byte)0);
            UvcException.ThrowExceptionForUvcError(error);
        }
    }

    public uint ExposureTimeAbsolute
    {
        get
        {
            NativeMethods.uvc_get_exposure_abs(_handle, out var value, RequestCode.GetCurrent);
            return value;
        }
        set
        {
            var error = NativeMethods.uvc_set_exposure_abs(_handle, value);
            UvcException.ThrowExceptionForUvcError(error);
        }
    }

    public int ExposureTimeRelative
    {
        get
        {
            NativeMethods.uvc_get_exposure_rel(_handle, out var value, RequestCode.GetCurrent);
            return value;
        }
        set
        {
            var error = NativeMethods.uvc_set_exposure_rel(_handle, (sbyte)value);
            UvcException.ThrowExceptionForUvcError(error);
        }
    }

    public IEnumerable<FormatDescriptor> GetStreamControlFormats()
    {
        var descs = NativeMethods.uvc_get_format_descs(_handle);
        while (descs != IntPtr.Zero)
        {
            // access frame_descs field
            var descriptorSubType = (UvcVsDescriptorSubtype)Marshal.ReadInt32(descs, IntPtr.Size * 3);
            var numFrameDescriptors = Marshal.ReadByte(descs, IntPtr.Size * 3 + 5);
            if (numFrameDescriptors > 0)
            {
                var frameDescs = Marshal.ReadIntPtr(descs, IntPtr.Size * 3 + 29);
                while (frameDescs != IntPtr.Zero)
                {
                    var width = (int)(ushort)Marshal.ReadInt16(frameDescs, IntPtr.Size * 3 + 6);
                    var height = (int)(ushort)Marshal.ReadInt16(frameDescs, IntPtr.Size * 3 + 8);
                    yield return new FormatDescriptor { Width = width, Height = height };

                    // access next field
                    frameDescs = Marshal.ReadIntPtr(frameDescs, IntPtr.Size * 2);
                }
            }

            // access next field
            descs = Marshal.ReadIntPtr(descs, IntPtr.Size * 2);
        }

        yield break;
    }

    public StreamControl GetStreamControlFormatSize(FrameFormat format, int width, int height, int fps)
    {
        GetStreamControlFormatSize(format, width, height, fps, out var control);
        return control;
    }

    public void GetStreamControlFormatSize(FrameFormat format, int width, int height, int fps, out StreamControl control)
    {
        var error = NativeMethods.uvc_get_stream_ctrl_format_size(_handle, out control, format, width, height, fps);
        UvcException.ThrowExceptionForUvcError(error);
    }

    public StreamControl ProbeStreamControl()
    {
        ProbeStreamControl(out var control);
        return control;
    }

    public void ProbeStreamControl(out StreamControl control)
    {
        var error = NativeMethods.uvc_probe_stream_ctrl(_handle, out control);
        UvcException.ThrowExceptionForUvcError(error);
    }

    public void StartStreaming(ref StreamControl control, FrameCallback callback)
    {
        var error = NativeMethods.uvc_start_streaming(_handle, ref control, callback, IntPtr.Zero, 0);
        UvcException.ThrowExceptionForUvcError(error);
        _callback = callback;
    }

    public void StartIsoStreaming(ref StreamControl control, FrameCallback callback)
    {
        var error = NativeMethods.uvc_start_iso_streaming(_handle, ref control, callback, IntPtr.Zero);
        UvcException.ThrowExceptionForUvcError(error);
        _callback = callback;
    }

    public void StopStreaming()
    {
        NativeMethods.uvc_stop_streaming(_handle);
        _callback = null;
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}