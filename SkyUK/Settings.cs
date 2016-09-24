using SkyUK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvDatabase;

namespace SkyUK
{
  public class Settings
  {
    TvBusinessLayer _layer = new TvBusinessLayer();
    Dictionary<int, string> _themes;
    
    public Settings()
    {
      _themes = SkyUtils.GetThemes();
    }

    public string GetSkySetting(string _Setting, object defaultvalue)
    {
      return _layer.GetSetting("OTVC_" + _Setting, defaultvalue.ToString()).Value;
    }

    public void UpdateSetting(string _Setting, string value)
    {
      Setting setting = _layer.GetSetting("OTVC_" + _Setting, "0");
      setting.Value = value.ToString();
      setting.Persist();
    }

    //Properties 
    public int modulation
    {
      get { return Convert.ToInt32(GetSkySetting("modulation", -1)); }
      set { UpdateSetting("modulation", value.ToString()); }
    }

    public int GrabTime
    {
      get { return Convert.ToInt32(GetSkySetting("GrabTime", 60)); }
      set { UpdateSetting("GrabTime", value.ToString()); }
    }

    public int frequency
    {
      get { return Convert.ToInt32(GetSkySetting("frequency", 11778000)); }
      set { UpdateSetting("frequency", value.ToString()); }
    }

    public int SymbolRate
    {
      get { return Convert.ToInt32(GetSkySetting("SymbolRate", 27500)); }
      set { UpdateSetting("SymbolRate", value.ToString()); }
    }

    public int NID
    {
      get { return Convert.ToInt32(GetSkySetting("NID", 2)); }
      set { UpdateSetting("NID", value.ToString()); }
    }

    public int polarisation
    {
      get { return Convert.ToInt32(GetSkySetting("polarisation", 3)); }
      set { UpdateSetting("polarisation", value.ToString()); }
    }

    public int ServiceID
    {
      get { return Convert.ToInt32(GetSkySetting("ServiceID", 4152)); }
      set { UpdateSetting("ServiceID", value.ToString()); }
    }

    public int TransportID
    {
      get { return Convert.ToInt32(GetSkySetting("TransportID", 2004)); }
      set { UpdateSetting("TransportID", value.ToString()); }
    }

    public bool AutoUpdate
    {
      get { return Convert.ToBoolean(GetSkySetting("AutoUpdate", true)); }
      set { UpdateSetting("AutoUpdate", value.ToString()); }
    }

    public bool useExtraInfo
    {
      get { return Convert.ToBoolean(GetSkySetting("useExtraInfo", true)); }
      set { UpdateSetting("useExtraInfo", value.ToString()); }
    }

    public bool UpdateChannels
    {
      get { return Convert.ToBoolean(GetSkySetting("UpdateChannels", true)); }
      set { UpdateSetting("UpdateChannels", value.ToString()); }
    }

    public bool EveryHour
    {
      get { return Convert.ToBoolean(GetSkySetting("EveryHour", true)); }
      set { UpdateSetting("EveryHour", value.ToString()); }
    }

    public bool OnDaysAt
    {
      get { return Convert.ToBoolean(GetSkySetting("OnDaysAt", false)); }
      set { UpdateSetting("OnDaysAt", value.ToString()); }
    }

    //Days
    public bool Mon
    {
      get { return Convert.ToBoolean(GetSkySetting("Mon", true)); }
      set { UpdateSetting("Mon", value.ToString()); }
    }

    public bool Tue
    {
      get { return Convert.ToBoolean(GetSkySetting("Tue", true)); }
      set { UpdateSetting("Tue", value.ToString()); }
    }

    public bool Wed
    {
      get { return Convert.ToBoolean(GetSkySetting("Wed", true)); }
      set { UpdateSetting("Wed", value.ToString()); }
    }

    public bool Thu
    {
      get { return Convert.ToBoolean(GetSkySetting("Thu", true)); }
      set { UpdateSetting("Thu", value.ToString()); }
    }

    public bool Fri
    {
      get { return Convert.ToBoolean(GetSkySetting("Fri", true)); }
      set { UpdateSetting("Fri", value.ToString()); }
    }

    public bool Sat
    {
      get { return Convert.ToBoolean(GetSkySetting("Sat", true)); }
      set { UpdateSetting("Sat", value.ToString()); }
    }

    public bool Sun
    {
      get { return Convert.ToBoolean(GetSkySetting("Sun", true)); }
      set { UpdateSetting("Sun", value.ToString()); }
    }

    public DateTime UpdateTime
    {
      get { return Convert.ToDateTime(GetSkySetting("UpdateTime", DateTime.Now.ToString())); }
      set { UpdateSetting("UpdateTime", value.ToString()); }
    }

    public bool ReplaceSDwithHD
    {
      get { return Convert.ToBoolean(GetSkySetting("ReplaceSDwithHD", true)); }
      set { UpdateSetting("ReplaceSDwithHD", value.ToString()); }
    }

    public bool IgnoreScrambled
    {
      get { return Convert.ToBoolean(GetSkySetting("IgnoreScrambled", false)); }
      set { UpdateSetting("IgnoreScrambled", value.ToString()); }
    }

    public bool UseNotSetModSD
    {
      get { return Convert.ToBoolean(GetSkySetting("UseNotSetModSD", false)); }
      set { UpdateSetting("UseNotSetModSD", value.ToString()); }
    }

    public bool UseNotSetModHD
    {
      get { return Convert.ToBoolean(GetSkySetting("UseNotSetModHD", false)); }
      set { UpdateSetting("UseNotSetModHD", value.ToString()); }
    }

    public bool UpdateEPG
    {
      get { return Convert.ToBoolean(GetSkySetting("UpdateEPG", true)); }
      set { UpdateSetting("UpdateEPG", value.ToString()); }
    }

    public int UpdateInterval
    {
      get { return Convert.ToInt32(GetSkySetting("UpdateInterval", 3)); }
      set { UpdateSetting("UpdateInterval", value.ToString()); }
    }

    public int RegionID
    {
      get { return Convert.ToInt32(GetSkySetting("RegionID", 0)); }
      set { UpdateSetting("RegionID", value.ToString()); }
    }

    public int BouquetID
    {
      get
      {
        if (ReplaceSDwithHD)
        {
          return Convert.ToInt32(GetSkySetting("BouquetID", 0)) + 4;
        }
        else
        {
          return Convert.ToInt32(GetSkySetting("BouquetID", 0));
        }
      }
      set { UpdateSetting("BouquetID", value.ToString()); }
    }

    public bool IsLoading
    {
      get { return Convert.ToBoolean(GetSkySetting("IsLoading", true)); }
      set { UpdateSetting("IsLoading", value.ToString()); }
    }

    public List<int> CardMap
    {
      get
      {
        List<int> returnlist = new List<int>();
        string Stringtouse = GetSkySetting("CardMap", "");
        if (Stringtouse.Length > 0)
        {
          if (Stringtouse.Length == 1)
          {
            returnlist.Add(Convert.ToInt32(Stringtouse));
          }
          else
          {
            string[] Array1 = Stringtouse.Split(',');
            if (Array1.Length > 0)
            {
              foreach (string Str__1 in Array1)
              {
                returnlist.Add(Convert.ToInt32(Str__1));
              }
            }
          }
        }
        return returnlist;
      }
      set
      {
        StringBuilder str__2 = new StringBuilder();
        if (value.Count > 0)
        {
          foreach (int Num in value)
          {
            str__2.Append("," + Num.ToString());
          }
          str__2.Remove(0, 1);
        }
        UpdateSetting("CardMap", str__2.ToString());
      }
    }

    public DateTime LastUpdate
    {
      get { return DateTime.Parse(GetSkySetting("LastUpdate", DateTime.Now.ToString())); }
      set { UpdateSetting("LastUpdate", value.ToString()); }
    }

    public bool UseSkyNumbers
    {
      get { return Convert.ToBoolean(GetSkySetting("UseSkyNumbers", true)); }
      set { UpdateSetting("UseSkyNumbers", value.ToString()); }
    }

    public bool UseSkyCategories
    {
      get { return Convert.ToBoolean(GetSkySetting("UseSkyCategories", true)); }
      set { UpdateSetting("UseSkyCategories", value.ToString()); }
    }

    public bool UseSkyRegions
    {
      get { return Convert.ToBoolean(GetSkySetting("UseSkyRegions", true)); }
      set { UpdateSetting("UseSkyRegions", value.ToString()); }
    }

    public bool DeleteOldChannels
    {
      get { return Convert.ToBoolean(GetSkySetting("DeleteOldChannels", true)); }
      set { UpdateSetting("DeleteOldChannels", value.ToString()); }
    }

    public string OldChannelFolder
    {
      get { return GetSkySetting("OldChannelFolder", "Old Sky Channels"); }
      set { UpdateSetting("OldChannelFolder", value); }
    }

    public int RegionIndex
    {
      get { return Convert.ToInt32(GetSkySetting("RegionIndex", 0)); }
      set { UpdateSetting("RegionIndex", value.ToString()); }
    }

    public int CardToUseIndex
    {
      get { return Convert.ToInt32(GetSkySetting("CardToUseIndex", 0)); }
      set { UpdateSetting("CardToUseIndex", value.ToString()); }
    }

    public int DiseqC
    {
      get { return Convert.ToInt32(GetSkySetting("DiseqC", -1)); }
      set { UpdateSetting("DiseqC", value.ToString()); }
    }

    public int SwitchingFrequency
    {
      get { return Convert.ToInt32(GetSkySetting("SwitchingFrequency", 11700000)); }
      set { UpdateSetting("SwitchingFrequency", value.ToString()); }
    }

    public bool IsGrabbing
    {
      get { return Convert.ToBoolean(GetSkySetting("IsGrabbing", false)); }
      set { UpdateSetting("IsGrabbing", value.ToString()); }
    }

    public bool UseThrottle
    {
      get { return Convert.ToBoolean(GetSkySetting("UseThrottle", false)); }
      set { UpdateSetting("UseThrottle", value.ToString()); }
    }

    public bool UpdateLogos
    {
      get { return Convert.ToBoolean(GetSkySetting("UpdateLogos", false)); }
      set { UpdateSetting("UpdateLogos", value.ToString()); }
    }

    public string LogoDirectory
    {
      get { return GetSkySetting("LogoDirectory", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Team MediaPortal\\MediaPortal\\Thumbs\\tv\\logos"); }
      set { UpdateSetting("LogoDirectory", value); }
    }

    public string GetCategory(byte CatByte)
    {
      return GetSkySetting("Cat" + CatByte, CatByte);
    }

    public void SetCategory(byte CatByte, string Name)
    {
      UpdateSetting("Cat" + CatByte, Name);
    }

    public List<string> GetConfiguredCategories()
    {
      List<string> categories = new List<string>();
      for (int i = 1; i <= 20; i++)
        if (GetSkySetting("CatByte" + i, "-1") != "-1")
        {
          string category = GetSkySetting("CatText" + i, string.Empty);
          if (category != string.Empty)
            categories.Add(category);
        }
      return categories;
    }

    public string GetTheme(int id)
    {
      string theme;
      if (_themes.TryGetValue(id, out theme))
        return theme;
      return string.Empty;
    }
  }
}
