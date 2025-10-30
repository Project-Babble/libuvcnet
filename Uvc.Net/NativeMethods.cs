using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Uvc.Net;

internal static class NativeMethods
{
    private const string LibName = "uvc";
    
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_init(out UvcContext pctx, IntPtr usb_ctx);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_exit(IntPtr ctx);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_get_device_list(UvcContext ctx, out IntPtr list);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_free_device_list(IntPtr list, byte unref_devices);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_get_device_descriptor(UvcDevice dev, out IntPtr desc);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_free_device_descriptor(IntPtr desc);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern UvcError uvc_find_device(
        UvcContext ctx,
        out UvcDevice dev,
        int vid,
        int pid,
        string sn);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern UvcError uvc_find_devices(
        UvcContext ctx,
        out IntPtr devs,
        int vid,
        int pid,
        string sn);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_open(UvcDevice dev, out UvcDeviceHandle devh);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_close(IntPtr devh);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_ref_device(UvcDevice dev);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_unref_device(IntPtr dev);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_get_stream_ctrl_format_size(
        UvcDeviceHandle devh,
        out StreamControl ctrl,
        FrameFormat format,
        int width, int height,
        int fps);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr uvc_get_format_descs(UvcDeviceHandle devh);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_probe_stream_ctrl(UvcDeviceHandle devh, out StreamControl ctrl);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_start_streaming(
        UvcDeviceHandle devh,
        ref StreamControl ctrl,
        FrameCallback cb,
        IntPtr user_ptr,
        byte flags);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_start_iso_streaming(
        UvcDeviceHandle devh,
        ref StreamControl ctrl,
        FrameCallback cb,
        IntPtr user_ptr);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_stop_streaming(UvcDeviceHandle devh);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_stream_open_ctrl(
        UvcDeviceHandle devh,
        out UvcStreamHandle strmh,
        StreamControl ctrl);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_stream_ctrl(UvcStreamHandle strmh, ref StreamControl ctrl);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_stream_stop(UvcStreamHandle strmh);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_stream_close(IntPtr strmh);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int uvc_get_ctrl_len(UvcDeviceHandle devh, byte unit, byte ctrl);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int uvc_get_ctrl(UvcDeviceHandle devh, byte unit, byte ctrl, IntPtr data, int len, RequestCode req_code);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int uvc_set_ctrl(UvcDeviceHandle devh, byte unit, byte ctrl, IntPtr data, int len);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_get_ae_mode(UvcDeviceHandle devh, out byte mode, RequestCode req_code);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_set_ae_mode(UvcDeviceHandle devh, byte mode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_get_ae_priority(UvcDeviceHandle devh, out byte priority, RequestCode req_code);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_set_ae_priority(UvcDeviceHandle devh, byte priority);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_get_exposure_abs(UvcDeviceHandle devh, out uint time, RequestCode req_code);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_set_exposure_abs(UvcDeviceHandle devh, uint time);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_get_exposure_rel(UvcDeviceHandle devh, out sbyte step, RequestCode req_code);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern UvcError uvc_set_exposure_rel(UvcDeviceHandle devh, sbyte step);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr uvc_strerror(UvcError err);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void uvc_print_diag(UvcDeviceHandle devh, IntPtr stream);
}