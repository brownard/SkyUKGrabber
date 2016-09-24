using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyUK
{
  public class LCNHolder
  {
    public LCNHolder(int bouquetId, int regionId, int skyNumber)
    {
      BouquetId = bouquetId;
      RegionId = regionId;
      SkyNumber = skyNumber;
    }

    public int BouquetId { get; set; }

    public int RegionId { get; set; }

    public int SkyNumber { get; set; }
  }
}
