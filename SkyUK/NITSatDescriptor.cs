using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyUK
{
  public class NITSatDescriptor
  {
    public int TID { get; set; }

    public int Frequency { get; set; }

    public int OrbitalPosition { get; set; }

    public int WestEastFlag { get; set; }

    public int Polarisation { get; set; }

    public int Modulation { get; set; }

    public int Symbolrate { get; set; }

    public int FECInner { get; set; }

    public int RollOff { get; set; }

    public int IsS2 { get; set; }

    public string NetworkName { get; set; }
  }
}
