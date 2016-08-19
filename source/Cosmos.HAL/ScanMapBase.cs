﻿using System;
using System.Collections.Generic;

using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public abstract class ScanMapBase
    {
        protected List<KeyMapping> _keys;


        protected abstract void InitKeys();
        
        protected ScanMapBase()
        {
            InitKeys();
        }

        public KeyEvent ConvertScanCode(byte scan2, bool ctrl, bool shift, bool alt, bool num, bool caps, bool scroll)
        {
            KeyEvent keyev = new KeyEvent();
            bool found = false;

            if (scan2 == 0)
            {
                found = true;
                return keyev;
            }

            byte scan = scan2;

            if (alt) keyev.Modifiers |= ConsoleModifiers.Alt;
            if (ctrl) keyev.Modifiers |= ConsoleModifiers.Control;
            if (shift) keyev.Modifiers |= ConsoleModifiers.Shift;

            keyev.Type = (scan & 0x80) != 0 ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make;

            if ((scan & 0x80) != 0)
                scan = (byte)(scan ^ 0x80);

            Debugger.DoSend("Number of keys: ");
            Debugger.DoSendNumber((uint) _keys.Count);

            for (int index = 0; index < _keys.Count; index++)
            {
                KeyMapping t = _keys[index];

                if (t == null)
                {
                    Debugger.DoSend("Key received but item is NULL");
                    continue;
                }
                else if (t.Scancode == scan)
                {
                    found = true;
                    KeyMapping map = t;
                    char key = '\0';

                    if (ctrl)
                        if (alt)
                            key = xor(shift, caps) ? map.ControlAltShift : map.ControlAlt;
                        else
                            key = xor(shift, caps) ? map.ControlShift : map.Control;
                    else if (shift)
                        key = caps ? map.ShiftCaps
                            : num ? map.ShiftNum
                            : map.Shift;
                    else if (caps)
                        key = map.Caps;
                    else if (num)
                        key = map.Num;
                    else
                        key = map.Value;

                    keyev.KeyChar = key;
                    keyev.Key = num ? t.NumLockKey : t.Key;

                    break;
                }
            }

            return found ? keyev : null;
        }

#warning TODO: WHY IS XOR FOR BOOLEANS NOT IMPLEMENTED!?
        internal static bool xor(bool b1, bool b2) => (b1 || b2) && !(b1 && b2);
    }
}
