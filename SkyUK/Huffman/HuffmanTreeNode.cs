using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyUK.Huffman
{
  public class HuffmanTreeNode
  {
    //the character found in the file.
    public string Value { get; set; }
    //amount of times the character was found in the file.
    //the parent node.
    public HuffmanTreeNode Parent { get; set; }
    //the left leaf.
    public HuffmanTreeNode P1 { get; set; }
    //the right leaf.
    public HuffmanTreeNode P0 { get; set; }

    public void Clear()
    {
      if (P1 != null)
      {
        P1 = null;
        P0 = null;
      }
    }

    public static HuffmanTreeNode CreateTree(string[] dictionary)
    {
      HuffmanTreeNode root = new HuffmanTreeNode();
      HuffmanTreeNode current;
      foreach (string line in dictionary)
      {
        int splitIndex = line.LastIndexOf("=");
        string entry = line.Substring(0, splitIndex);
        string path = line.Substring(splitIndex + 1, line.Length - splitIndex - 1);

        current = root;
        for (int t = 0; t < path.Length; t++)
        {
          if (path[t] == '1')
          {
            if (current.P1 == null)
              current.P1 = new HuffmanTreeNode();
            current = current.P1;
          }
          else
          {
            if (current.P0 == null)
              current.P0 = new HuffmanTreeNode();
            current = current.P0;
          }
        }
        current.Value = entry;
      }
      return root;
    }
  }
}
