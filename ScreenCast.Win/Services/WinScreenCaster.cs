﻿using Remotely.ScreenCast.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotely.ScreenCast.Core.Enums;
using Remotely.ScreenCast.Core.Services;
using Remotely.ScreenCast.Core.Capture;
using Remotely.ScreenCast.Core;
using Remotely.ScreenCast.Core.Models;
using Remotely.Shared.Models;
using Remotely.ScreenCast.Win.Capture;
using Remotely.Shared.Win32;

namespace Remotely.ScreenCast.Win.Services
{
    public class WinScreenCaster : ScreenCasterBase,  IScreenCaster
    {
        public WinScreenCaster(CursorIconWatcher cursorIconWatcher)
        {
            CursorIconWatcher = cursorIconWatcher;
        }

        public CursorIconWatcher CursorIconWatcher { get; }

        public async Task BeginScreenCasting(ScreenCastRequest screenCastRequest)
        {
            if (Win32Interop.GetCurrentDesktop(out var currentDesktopName))
            {
                Logger.Write($"Setting desktop to {currentDesktopName} before screen casting.");
                Win32Interop.SwitchToInputDesktop();
            }
            else
            {
                Logger.Write("Failed to get current desktop before screen casting.");
            }
        
            await Conductor.Current.CasterSocket.SendCursorChange(CursorIconWatcher.GetCurrentCursor(), new List<string>() { screenCastRequest.ViewerID });
            _ = BeginScreenCasting(screenCastRequest.ViewerID, screenCastRequest.RequesterName, GetCapturer());
        }

        private static ICapturer GetCapturer()
        {
            ICapturer capturer;
            try
            {
                if (Conductor.Current.Viewers.Count == 0)
                {
                    capturer = new DXCapture();
                }
                else
                {
                    capturer = new BitBltCapture();
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                capturer = new BitBltCapture();
            }

            return capturer;
        }
    }
}
