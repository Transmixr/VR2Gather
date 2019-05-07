﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UnityEngine;

public class PCSUBReader : PCBaseReader { 

    string url;
    int streamNumber;
    bool failed;
    IntPtr subHandle;
    IntPtr decoder;
    byte[] currentBuffer;
    IntPtr currentBufferPtr;

    public PCSUBReader(string _url, int _streamNumber) {
        failed = true;
        url = _url;
        streamNumber = _streamNumber;

        bool ok = setup_sub_environment();
        if (!ok) {
            Debug.LogError("setup_sub_environment failed");
            return;
        }

        subHandle = signals_unity_bridge_pinvoke.sub_create("source_from_sub");
        if (subHandle == IntPtr.Zero) {
            Debug.LogError("sub_create failed");
            return;
        }

        ok = signals_unity_bridge_pinvoke.sub_play(subHandle, url);
        if (!ok) {
            Debug.LogError("sub_play failed for " + url);
            return;
        }
        decoder = API_cwipc_codec.cwipc_new_decoder();
        if (decoder == IntPtr.Zero) {
            Debug.LogError("Cannot create PCSUBReader");
            return;
        }

        failed = false;
    }

    internal bool setup_sub_environment()
    {
        signals_unity_bridge_pinvoke.SetPaths();

        IntPtr hMod = API_kernel.GetModuleHandle("signals-unity-bridge");
        if (hMod == IntPtr.Zero)
        {
            Debug.LogError("Cannot get handle on signals-unity-bridge, GetModuleHandle returned NULL.");
            return false;
        }
        StringBuilder modPath = new StringBuilder(255);
        int rv = API_kernel.GetModuleFileName(hMod, modPath, 255);
        if (rv < 0)
        {
            Debug.LogError("Cannot get filename for signals-unity-bridge, GetModuleFileName returned " + rv);
            //return false;
        }
        string dirName = Path.GetDirectoryName(modPath.ToString());
        Environment.SetEnvironmentVariable("SIGNALS_SMD_PATH", dirName);
        return true;
    }

    public void free()
    {
        if (subHandle != IntPtr.Zero)
        {
            signals_unity_bridge_pinvoke.sub_destroy(subHandle);
            subHandle = IntPtr.Zero;
            failed = true; // Not really failed, but reacts the same (nothing will work anymore)
        }
    }

    public bool eof() {
        return failed;
    }

    public bool available(bool wait) {
        return !failed;
    }

    public PointCloudFrame get() {
        signals_unity_bridge_pinvoke.FrameInfo info = new signals_unity_bridge_pinvoke.FrameInfo();
        if (failed) return null;

        int bytesNeeded = signals_unity_bridge_pinvoke.sub_grab_frame(subHandle, streamNumber, IntPtr.Zero, 0, ref info);
        if (bytesNeeded == 0) {
            return null;
        }

        if (currentBuffer == null || bytesNeeded > currentBuffer.Length)
        {
            Debug.Log("Needs more memory!!");
            currentBuffer = new byte[(int)(bytesNeeded * 1.3f)]; // Reserves 30% more.
            currentBufferPtr = Marshal.UnsafeAddrOfPinnedArrayElement(currentBuffer, 0);
        }

        int bytesRead = signals_unity_bridge_pinvoke.sub_grab_frame(subHandle, streamNumber, currentBufferPtr, bytesNeeded, ref info);
        if (bytesRead != bytesNeeded)
        {
            Debug.LogError("sub_grab_frame returned " + bytesRead + " bytes after promising " + bytesNeeded);
            return null;
        }


        API_cwipc_codec.cwipc_decoder_feed(decoder, currentBufferPtr, bytesNeeded);
        bool ok = API_cwipc_util.cwipc_source_available(decoder, true);
        if (!ok)
        {
            Debug.LogError("cwipc_decoder: no pointcloud available");
            return null;
        }
        var pc = API_cwipc_util.cwipc_source_get(decoder);
        if (pc == null)
        {
            Debug.LogError("cwipc_decoder: did not return a pointcloud");
            return null;
        }


        return new PointCloudFrame(pc);

    }

}