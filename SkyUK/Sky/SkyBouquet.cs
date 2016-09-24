using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyUK.Sky
{
  public class SkyBouquet
  {
    public byte FirstReceivedSectionNumber { get; set; }

    public bool IsInitialized { get; set; }

    public bool IsPopulated { get; set; }
  }
}
