using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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

    [StructLayout(LayoutKind.Sequential)]
    private struct UvcFormatDescriptor
    {
        public IntPtr parent;
        public IntPtr prev;
        public IntPtr next;
        public UvcVsDescriptorSubtype bDescriptorSubtype;
        public byte bFormatIndex;
        public byte bNumFrameDescriptors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] guidfourccFormat; 
        //  union {
        //   uint8_t guidFormat[16];
        //   uint8_t fourccFormat[4];
        // };
        public byte bBitsPerPixelbmFlags;
        //union {
        //   /** BPP for uncompressed stream */
        //   uint8_t bBitsPerPixel;
        //   /** Flags for JPEG stream */
        //   uint8_t bmFlags;
        // }; 
        public byte bDefaultFrameIndex; 
        public byte bAspectRatioX; 
        public byte bAspectRatioY; 
        public byte bmInterlaceFlags; 
        public byte bCopyProtect; 
        public byte bVariableSize; 
        public IntPtr frame_descs; 
        public IntPtr still_frame_desc;
        
        public static UvcFormatDescriptor? Create(IntPtr ptr) => ptr == IntPtr.Zero ? null : Marshal.PtrToStructure<UvcFormatDescriptor>(ptr);
        
        public UvcFormatDescriptor? CreatePrevious() => Create(prev);
        public UvcFormatDescriptor? CreateNext() => Create(next);
        public UvcFrameDesc? CreateFrameDescs() => UvcFrameDesc.Create(frame_descs);
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct UvcFrameDesc 
    {
        //TODO: theres a lot more information here thats not used
        public IntPtr parent;
        public IntPtr prev;
        public IntPtr next;
        public UvcVsDescriptorSubtype bDescriptorSubtype;
        public byte bFrameIndex;
        public byte bmCapabilities;
        public ushort wWidth;
        public ushort wHeight;
        public uint dwMinBitRate;
        public uint dwMaxBitRate;
        public uint dwMaxVideoFrameBufferSize;
        public uint dwDefaultFrameInterval;
        public uint dwMinFrameInterval;
        public uint dwMaxFrameInterval;
        public uint dwFrameIntervalStep;
        public byte bFrameIntervalType;
        public uint dwBytesPerLine;
        public IntPtr intervals;
        
        public static UvcFrameDesc? Create(IntPtr ptr) => ptr == IntPtr.Zero ? null : Marshal.PtrToStructure<UvcFrameDesc>(ptr);
        
        public UvcFrameDesc? CreatePrevious() => Create(prev);
        public UvcFrameDesc? CreateNext() => Create(next);
    }
    
    public IEnumerable<FormatDescriptor> GetStreamControlFormats()
    {
        var result = new List<FormatDescriptor>();
        var descs = NativeMethods.uvc_get_format_descs(_handle);
        var current = UvcFormatDescriptor.Create(descs);
        while (current.HasValue)
        {
            var cur = current.Value;
            var frameDesc = cur.CreateFrameDescs();
            while (frameDesc.HasValue)
            {
                result.Add(new FormatDescriptor
                    {
                        Width = frameDesc.Value.wWidth, 
                        Height = frameDesc.Value.wHeight,
                        Fps = (int)(10000000 / frameDesc.Value.dwDefaultFrameInterval)
                    }
                );
                frameDesc = frameDesc.Value.CreateNext();
            }
            current = current.Value.CreateNext();
        }
        return result;
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