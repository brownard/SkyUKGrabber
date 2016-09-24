using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyUK
{
  public class SDTInfo
  {
    public int SID { get; set; }

    public string ChannelName { get; set; }

    public string Provider { get; set; }

    public int Category { get; set; }

    public bool IsFTA { get; set; }

    public bool IsRadio { get; set; }

    public bool IsTV { get; set; }

    public bool IsHD { get; set; }

    public bool Is3D { get; set; }
  }
}
