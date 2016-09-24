using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyUK.Sky
{
  public class SkyChannel
  {
    protected Dictionary<string, LCNHolder> _epgChannelNumber = new Dictionary<string, LCNHolder>();
    protected Dictionary<int, SkyEvent> _events = new Dictionary<int, SkyEvent>();

    public bool HasChanged { get; set; }

    public bool IsPopulated { get; set; }

    public bool AddChannelRequired { get; set; }

    public int ChannelID { get; set; }

    public int NID { get; set; }

    public int TID { get; set; }

    public int SID { get; set; }

    public string ChannelName { get; set; }

    public Dictionary<int, SkyEvent> Events
    {
      get { return _events; }
    }

    public int LCNCount
    {
      get { return _epgChannelNumber.Count; }
    }

    public bool AddSkyLCN(LCNHolder LCNHold)
    {
      string key = GetLCNKey(LCNHold.BouquetId, LCNHold.RegionId);
      if (!_epgChannelNumber.ContainsKey(key))
      {
        _epgChannelNumber.Add(key, LCNHold);
        return false;
      }
      return true;
    }

    public LCNHolder GetLCN(int bouquetId, int regionId)
    {
      LCNHolder holder;
      if (_epgChannelNumber.TryGetValue(GetLCNKey(bouquetId, regionId), out holder))
        return holder;
      return null;
    }

    public bool ContainsLCN(int bouquetId, int regionId)
    {
      return _epgChannelNumber.ContainsKey(GetLCNKey(bouquetId, regionId));
    }

    protected static string GetLCNKey(int bouquetId, int regionId)
    {
      return bouquetId + "-" + regionId;
    }
  }
}
