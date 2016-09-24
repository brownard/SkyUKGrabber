using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyUK.Utils
{
  public static class DVBUtils
  {
    static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime DateTimeFromMJD(long mjd)
    {
      return UNIX_EPOCH.AddSeconds((mjd + 2400000.5 - 2440587.5) * 86400);
    }

    public static string GetDVBString(byte[] data, int offset, int length)
    {
      if (length == 0)
        return string.Empty;

      string isoTable = null;
      int startByte = 0;
      if (data[offset] >= 0x20)
      {
        isoTable = "iso-8859-1";
      }
      else
      {
        switch (data[offset])
        {
          case 0x1:
          case 0x2:
          case 0x3:
          case 0x4:
          case 0x5:
          case 0x6:
          case 0x7:
          case 0x8:
          case 0x9:
          case 0xa:
          case 0xb:
            isoTable = "iso-8859-" + (data[offset] + 4).ToString();
            startByte = 1;
            break; // TODO: might not be correct. Was : Exit Select
          case 0x10:
            if (data[offset + 1] == 0x0)
            {
              if (data[offset + 2] != 0x0 && data[offset + 2] != 0xc)
              {
                isoTable = "iso-8859-" + ((int)data[offset + 2]).ToString();
                startByte = 3;
                break; // TODO: might not be correct. Was : Exit Select
              }
              else
                throw new ArgumentException("Byte 3 is not a valid value");
            }
            else
              throw new ArgumentException("Byte 2 is not a valid value");
          case 0x1f:
            if (data[offset + 1] != 0x1 || data[offset + 1] != 0x2)
              throw new ArgumentException("Custom text specifier is not recognized");
            break;
          default:
            throw new ArgumentException("Byte 1 is not a valid value");
        }
      }

      byte[] editedBytes = new byte[length];
      int editedLength = 0;

      for (int index = startByte; index < length; index++)
      {
        if (data[offset + index] > 0x1f)
        {
          if (data[offset + index] < 0x80 || data[offset + index] > 0x9f)
          {
            editedBytes[editedLength] = data[offset + index];
            editedLength++;
          }
        }
      }

      if (editedLength == 0)
        return string.Empty;

      try
      {
        Encoding sourceEncoding = Encoding.GetEncoding(isoTable) ?? Encoding.GetEncoding("iso-8859-1");
        return sourceEncoding.GetString(editedBytes, 0, editedLength);
      }
      catch (ArgumentException)
      {
        throw;
      }
    }
  }
}
