using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Threading;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace Custom_Data_Grabber
{
  public class PIDbuffer
  {

    public byte[] buffer = new byte[0x1000];
    public int buffer_position;
    public int continuity_counter;
    public bool currently_parsing;
    public int section_length;

    public int table_id;
  }

  public class DataGrabArgs
  {
    public int ChannelID;
    public int Seconds;
    public List<int> Pids;
  }

  public class CustomDataGRabber
  {
    private VirtualCard _card;
    private bool Complete = false;
    private List<int> WantedPIDS = new List<int>();
    private DateTime TimeStamp;
    private int Errorcodesent = 0;

    private List<int> Decoders = new List<int>();

    public void SendComplete(int ErrorCode)
    {
      Errorcodesent = ErrorCode;
      Complete = true;
    }

    public void GrabData(int ChannelID, int Seconds, List<int> Pids)
    {
      if (Decoders.Contains(ChannelID) == false)
      {
        DataGrabArgs paramstouse = new DataGrabArgs();
        paramstouse.ChannelID = ChannelID;
        paramstouse.Seconds = Seconds;
        paramstouse.Pids = Pids;
        Thread thread1 = new Thread(GrabData);
        Decoders.Add(ChannelID);
        thread1.Start(paramstouse);
      }
      else
      {
        if (OnComplete != null)
        {
          OnComplete(true, "Already grabbing on this channel, try again later");
        }
      }
    }

    public void GrabData(object ParamTo)
    {

      DataGrabArgs ParamToUse = (DataGrabArgs)ParamTo;
      int ChannelID = ParamToUse.ChannelID;
      int Seconds = ParamToUse.Seconds;
      List<int> Pids = ParamToUse.Pids;
      string Filename = "Custom_" + ChannelID.ToString() + ".ts";
      Complete = false;
      TvServer server = new TvServer();
      IUser user = new User(Filename, false);
      WantedPIDS = Pids;
      TvResult result = default(TvResult);
      //Used to start and ensure we can tune the channel
      result = server.StartTimeShifting(ref user, ChannelID, out _card);

      if ((result == TvResult.Succeeded))
      {
        Thread.Sleep(2000);
        _card.StopTimeShifting();

        result = server.StartTimeShiftingWithCustom(ref user, ChannelID, out _card, Filename, Pids);

        if ((result == TvResult.Succeeded))
        {
          string NewFile = _card.TimeshiftFolder + "\\" + Filename;
          TimeStamp = DateTime.Now.AddSeconds(Seconds);
          ProcessPackets(NewFile);
          Decoders.Remove(ChannelID);
        }
        else
        {
          Decoders.Remove(ChannelID);
          if (OnComplete != null)
          {
            OnComplete(true, result.ToString());
          }
        }
      }
      else
      {
        Decoders.Remove(ChannelID);
        if (OnComplete != null)
        {
          OnComplete(true, result.ToString());
        }
      }
    }

    public void NewSectionHandler(int pid, int Length, Section section)
    {
      if (OnPacket != null)
      {
        OnPacket(pid, Length, section);
      }
    }


    void lProcessPackets(string filename)
    {
      FileStream Filestream = new FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 8000, true);
      if (Filestream.Length <= 5000)
      {

      }
    }

    const int Min_FILE_SIZE = 5000;

    bool waitForFileSize(FileStream file, int minSize, int timeout)
    {
      int maxTries = timeout / 100;
      int tries = 0;
      while (tries < maxTries)
      {
        if (file.Length - file.Position >= minSize)
          return true;
        Log.Write("Waiting for Custom data file to reach correct size");
        tries++;
        Thread.Sleep(100);
      }
      return false;
    }

    void onProcessPacketError()
    {
      Log.Write("Stream isn't being written to .. Stopping");
      if (OnComplete != null)
      {
        OnComplete(true, "Datastream isn't being written to. Stopping");
      }
      Errorcodesent = 1;
    }

    bool onSync(FileStream fs, BinaryReader br)
    {
      if (!waitForFileSize(fs, 2, 10000))
      {
        onProcessPacketError();
        return false;
      }

      byte[] pida = br.ReadBytes(2);
      int pid = (pida[0] << 8) + pida[1];

      if (!waitForFileSize(fs, 2, 10000))
      {
        onProcessPacketError();
        return false;
      }

      byte[] lengtha = br.ReadBytes(2);

      if (!waitForFileSize(fs, 1, 10000))
      {
        onProcessPacketError();
        return false;
      }

      int sectionnum = Convert.ToInt32(br.ReadByte());
      int length = ((lengtha[0] & 0xf) << 8) + lengtha[1];

      if (!waitForFileSize(fs, length + 5, 10000))
      {
        onProcessPacketError();
        return false;
      }

      Section section = new Section();
      section.Data = br.ReadBytes(length + 5);
      if (section.Data.Length > 0)
      {
        section.table_id = section.Data[0];
        section.section_number = sectionnum;
        section.section_length = length;
        if (OnPacket != null)
        {
          OnPacket(pid, length, section);
        }
      }
      return true;
    }

    bool? getSync(FileStream Filestream, BinaryReader BinaryReader)
    {
      int tries = 0;
      while (tries < 1000)
      {
        if (tries > 1000)
        {
          if (OnComplete != null)
          {
            OnComplete(true, "File Sync could not be recovered");
          }
          return false; // TODO: might not be correct. Was : Exit Do
        }
        if (!waitForFileSize(Filestream, 1, 10000))
        {
          onProcessPacketError();
          return false;
        }
        byte syncByte = BinaryReader.ReadByte();
        if (syncByte != 0xff)
          return null;

        if (!waitForFileSize(Filestream, 1, 10000))
        {
          onProcessPacketError();
          return false;
        }
        syncByte = BinaryReader.ReadByte();
        if (syncByte != 0xaa)
          return null;

        if (!waitForFileSize(Filestream, 1, 10000))
        {
          onProcessPacketError();
          return false;
        }
        syncByte = BinaryReader.ReadByte();
        if (syncByte != 0xff)
          return null;

        return true;
      }
      return null;
    }

    public void ProcessPackets(string Filename)
    {
      // Try
      FileStream Filestream = new FileStream(Filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 8000, true);
      BinaryReader BinaryReader = new BinaryReader(Filestream);

      if (!waitForFileSize(Filestream, Min_FILE_SIZE, 50000))
      {
        Log.Write("Not Enough Data to process");
        if (OnComplete != null)
          OnComplete(true, "Not Enough Data to process");
        return;
      }

      while (!Complete)
      {
        if (!waitForFileSize(Filestream, 3, 10000))
        {
          onProcessPacketError();
          return;
        }

        byte[] sync = BinaryReader.ReadBytes(3);
        if (sync.Length > 2)
        {
          if (Complete)
            break;

          if (sync[0] == 0xff && sync[1] == 0xaa && sync[2] == 0xff)
          {
            if (!onSync(Filestream, BinaryReader))
              return;
          }
          else
          {
            int tries = 0;

            if (Complete)
              break;
            while (tries < 1000)
            {
              bool? result = getSync(Filestream, BinaryReader);
              if (result == true)
              {
                if (onSync(Filestream, BinaryReader))
                  break;
                else
                  return;
              }
              else if (result == false)
              {
                return;
              }
              tries++;
            }

          }
        }
      }

      int a = 0;
      bool wait = false;
      while (Complete == false)
      {
        byte[] sync = new byte[4];
        int syncloop = 0;

        while (!(Filestream.Position + 3 < Filestream.Length))
        {
          syncloop += 1;
          if (wait == false)
          {
            wait = true;
            Log.Write("Waiting for Custom data file to reach correct size");
          }
          wait = false;
          Thread.Sleep(1000);
          if (syncloop > 10)
          {
            goto error1;
          }
        }

        syncloop = 0;

        sync = BinaryReader.ReadBytes(3);

        if (sync.Length > 2)
        {
          gotsync:
          if (sync[0] == 0xff & sync[1] == 0xaa & sync[2] == 0xff)
          {
            if (Complete)
              break;
            byte[] pida = null;

            while (!(Filestream.Position + 2 < Filestream.Length))
            {
              syncloop += 1;
              if (wait == false)
              {
                wait = true;
                Log.Write("Waiting for Custom data file to reach correct size");
              }
              wait = false;
              Thread.Sleep(1000);
              if (syncloop > 10)
              {
                goto error1;
              }
            }
            syncloop = 0;
            pida = BinaryReader.ReadBytes(2);
            int pid = 0;
            pid = (pida[0] << 8) + pida[1];
            byte[] lengtha = null;
            while (!(Filestream.Position + 2 < Filestream.Length))
            {
              syncloop += 1;
              if (wait == false)
              {
                wait = true;
                Log.Write("Waiting for Custom data file to reach correct size");
              }
              wait = false;
              Thread.Sleep(1000);
              if (syncloop > 10)
              {
                goto error1;
              }
            }
            syncloop = 0;
            lengtha = BinaryReader.ReadBytes(2);
            while (!(Filestream.Position < Filestream.Length))
            {
              syncloop += 1;
              if (wait == false)
              {
                wait = true;
                Log.Write("Waiting for Custom data file to reach correct size");
              }
              wait = false;
              Thread.Sleep(1000);
              if (syncloop > 10)
              {
                goto error1;
              }
            }
            int sectionnum = Convert.ToInt32(BinaryReader.ReadByte());
            int length = 0;
            length = ((lengtha[0] & 0xf) << 8) + lengtha[1];
            Section section = new Section();
            syncloop = 0;
            while (!(Filestream.Position + length + 5 < Filestream.Length))
            {
              syncloop += 1;
              if (wait == false)
              {
                wait = true;
                Log.Write("Waiting for Custom data file to reach correct size");
              }
              wait = false;
              Thread.Sleep(1000);
              if (syncloop > 10)
              {
                goto error1;
              }
            }
            section.Data = BinaryReader.ReadBytes(length + 5);
            if (section.Data.Length > 0)
            {
              section.table_id = section.Data[0];
              section.section_number = sectionnum;
              section.section_length = length;
              if (OnPacket != null)
              {
                OnPacket(pid, length, section);
              }
            }
          }
          else
          {
            Log.Write("Lost Sync to data finding it ...");
            if (syncloop > 1000)
            {
              if (OnComplete != null)
              {
                OnComplete(true, "File Sync could not be recovered");
              }
              break; // TODO: might not be correct. Was : Exit Do
            }
            if (!waitForFileSize(Filestream, 1, 10000))
            {
              onProcessPacketError();
              return;
            }
            byte syncByte = BinaryReader.ReadByte();
            if (syncByte != 0xff)
            {
              //
            }
            if (!waitForFileSize(Filestream, 1, 10000))
            {
              onProcessPacketError();
              return;
            }
            syncByte = BinaryReader.ReadByte();
            if (syncByte != 0xaa)
            {
              //
            }
            if (!waitForFileSize(Filestream, 1, 10000))
            {
              onProcessPacketError();
              return;
            }
            syncByte = BinaryReader.ReadByte();
            if (syncByte != 0xff)
            {
              //
            }



            Log.Write("Lost Sync to data finding it ...");
            syncloop = 0;
            Tryagain:
            if (Complete)
              break; // TODO: might not be correct. Was : Exit Do
            syncloop += 1;
            if (syncloop > 1000)
            {
              if (OnComplete != null)
              {
                OnComplete(true, "File Sync could not be recovered");
              }
              break; // TODO: might not be correct. Was : Exit Do
            }
            syncloop = 0;
            while (!(Filestream.Position < Filestream.Length))
            {
              syncloop += 1;
              if (wait == false)
              {
                wait = true;
                Log.Write("Waiting for Custom data file to reach correct size");
              }
              Thread.Sleep(1000);
              wait = false;
              if (syncloop > 10)
              {
                goto error1;
              }
            }
            byte first = 0;
            byte second = 0;
            byte third = 0;
            first = BinaryReader.ReadByte();
            if (first != 0xff)
              goto Tryagain;
            syncloop = 0;
            while (!(Filestream.Position < Filestream.Length))
            {
              syncloop += 1;
              if (wait == false)
              {
                wait = true;
                Log.Write("Waiting for Custom data file to reach correct size");
              }
              Thread.Sleep(1000);
              wait = false;
              if (syncloop > 10)
              {
                goto error1;
              }
            }
            second = BinaryReader.ReadByte();
            if (second != 0xaa)
              goto Tryagain;
            syncloop = 0;
            while (!(Filestream.Position < Filestream.Length))
            {
              syncloop += 1;
              if (wait == false)
              {
                wait = true;
                Log.Write("Waiting for Custom data file to reach correct size");
              }
              Thread.Sleep(1000);
              wait = false;
              if (syncloop > 10)
              {
                goto error1;
              }
            }
            third = BinaryReader.ReadByte();
            if (third != 0xff)
              goto Tryagain;
            Log.Write("File back in sync continuing grab");
            sync[0] = 0xff;
            sync[1] = 0xaa;
            sync[2] = 0xff;
            goto gotsync;
          }
        }
        if (System.DateTime.Now > TimeStamp)
          break; // TODO: might not be correct. Was : Exit Do
      }
      Log.Write("Stopping Timeshift as Complete was received");
      goto over;
      error1:
      Log.Write("Stream isn't being written to .. Stopping");
      if (OnComplete != null)
      {
        OnComplete(true, "Datastream isn't being written to. Stopping");
      }
      Errorcodesent = 1;
      over:

      Complete = false;
      _card.StopTimeShifting();
      BinaryReader.Close();
      Filestream.Close();
      System.IO.File.Delete(Filename);
      if (Errorcodesent == 0)
      {
        if (OnComplete != null)
        {
          OnComplete(false, "");
        }
      }
    }

    public event OnPacketEventHandler OnPacket;
    public delegate void OnPacketEventHandler(int Pid, int Length, Section Data);
    public event OnCompleteEventHandler OnComplete;
    public delegate void OnCompleteEventHandler(bool Err, string ErrorMessage);

  }

  public class Section
  {

    public int table_id;
    public int table_id_extention;
    public int section_length;
    public int section_number;
    public int last_section_number;
    public int version_number;
    public int section_syntax_indicator;
    public int BufferPos;

    public byte[] Data;

    public void Reset()
    {
      table_id = -1;
      table_id_extention = -1;
      section_length = -1;
      section_number = -1;
      version_number = -1;
      section_syntax_indicator = -1;
      BufferPos = 0;
    }

    public int CalcSectionLength(byte[] tsPacket, int start)
    {

      if (BufferPos < 3)
      {
        byte bHi = 0;
        byte bLo = 0;
        if (BufferPos == 1)
        {
          bHi = tsPacket[start];
          bLo = tsPacket[start + 1];
        }
        else if (BufferPos == 2)
        {
          bHi = Data[1];
          bLo = tsPacket[start];
        }
        section_length = ((bHi & 0xf) << 8) + bLo;
      }
      else
      {
        section_length = ((Data[1] & 0xf) << 8) + Data[2];
      }

      return section_length;

    }

    public bool DecodeHeader()
    {
      if (BufferPos < 8)
        return false;
      table_id = Data[0];
      section_syntax_indicator = (Data[1] >> 7) & 1;
      if ((section_length == -1))
      {
        section_length = ((Data[1] & 0xf) << 8) + Data[2];
      }
      table_id_extention = (Data[3] << 8) + Data[4];
      version_number = (Data[5] >> 1) & 0x1f;
      section_number = Data[6];
      section_syntax_indicator = (Data[1] >> 7) & 1;
      return true;
    }

    public bool SectionComplete()
    {
      if (!DecodeHeader() & ((BufferPos - 3) > section_length) & (section_length > 0))
      {
        return true;
      }
      if (!DecodeHeader())
      {
        return false;
      }
      return ((BufferPos - 3) >= section_length);
    }


  }
}