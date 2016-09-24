using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyUK.Sky
{
  public class SkyEvent
  {
    int _EventID;
    int _StartTime;
    int _duration;
    int _ChannelID;
    string _Title;
    string _Summary;
    string _Category;
    string _ParentalCategory;
    int _SeriesID;
    long _mjdStart;
    int _seriesTermination;
    bool _AD;
    bool _CP;
    bool _HD;
    bool _WS;
    bool _Subs;
    int _SoundType;
    string Flags;
    public void SetFlags(int IntegerNumber)
    {
      _AD = (IntegerNumber & 0x1) != 0;
      _CP = (IntegerNumber & 0x2) != 0;
      _HD = (IntegerNumber & 0x4) != 0;
      _WS = (IntegerNumber & 0x8) != 0;
      _Subs = (IntegerNumber & 0x10) != 0;
      _SoundType = IntegerNumber >> 6;
    }

    public void SetCategory(int Category)
    {
      switch (Category & 0xf)
      {
        case 5:
          _ParentalCategory = "18";
          break;
        case 4:
          _ParentalCategory = "15";
          break;
        case 3:
          _ParentalCategory = "12";
          break;
        case 2:
          _ParentalCategory = "PG";
          break;
        case 1:
          _ParentalCategory = "U";
          break;
        default:
          _ParentalCategory = "";
          break;
      }
    }

    public string ParentalCategory
    {
      get { return _ParentalCategory; }
    }


    public string DescriptionFlag
    {
      get
      {
        Flags = "";
        if (_AD)
        {
          Flags += "[AD]";
        }
        if (_CP)
        {
          if (Flags != "")
            Flags += ",";
          Flags += "[CP]";
        }
        if (_HD)
        {
          if (Flags != "")
            Flags += ",";
          Flags += "[HD]";
        }
        if (_WS)
        {
          if (Flags != "")
            Flags += ",";
          Flags += "[W]";
        }
        if (_Subs)
        {
          if (Flags != "")
            Flags += ",";
          Flags += "[SUB]";
        }

        switch (_SoundType)
        {
          case 1:
            if (Flags != "")
              Flags += ",";
            Flags += "[S]";
            break;
          case 2:
            if (Flags != "")
              Flags += ",";
            Flags += "[DS]";
            break;
          case 3:
            if (Flags != "")
              Flags += ",";
            Flags += "[DD]";
            break;
        }
        return Flags;
      }
    }

    public string Summary
    {
      get { return _Summary; }
      set { _Summary = value; }
    }

    public int EventID
    {
      get { return _EventID; }
      set { _EventID = value; }
    }

    public int StartTime
    {
      get { return _StartTime; }
      set { _StartTime = value; }
    }

    public int Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }

    public int ChannelID
    {
      get { return _ChannelID; }
      set { _ChannelID = value; }
    }

    public string Title
    {
      get { return _Title; }
      set { _Title = value; }
    }

    public string Category
    {
      get { return _Category; }
      set { _Category = value; }
    }

    public int SeriesID
    {
      get { return _SeriesID; }
      set { _SeriesID = value; }
    }

    public long mjdStart
    {
      get { return _mjdStart; }
      set { _mjdStart = value; }
    }

    public int seriesTermination
    {
      get { return _seriesTermination; }
      set { _seriesTermination = value; }
    }

    public SkyEvent()
    {
      _EventID = -1;
      _StartTime = -1;
      _duration = -1;
      _ChannelID = -1;
      _Title = "";
      _Summary = "";
      _Category = "";
      _ParentalCategory = "";
      _SeriesID = 0;
      _mjdStart = 0;
      _seriesTermination = 0;
      _AD = false;
      _CP = false;
      _HD = false;
      _WS = false;
      _Subs = false;
      _SoundType = -1;
      Flags = "";
    }
  }
}
