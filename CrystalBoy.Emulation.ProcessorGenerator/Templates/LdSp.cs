__temp16 = (ushort)(bus[pc++] | (bus[pc++] << 8));
bus[__temp16++] = (byte)(sp);
bus[__temp16] = (byte)(sp >> 8);
