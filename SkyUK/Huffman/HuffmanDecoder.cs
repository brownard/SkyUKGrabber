using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyUK.Huffman
{
  public class HuffmanDecoder
  {
    HuffmanTreeNode _root;

    public HuffmanDecoder(string[] dictionary)
    {
      ParseDictionary(dictionary);
    }

    public string Decode(byte[] data, int offset, int length)
    {
      StringBuilder decodedText = new StringBuilder();
      bool codeError = false;
      bool isFound = false;
      int lastIndex = offset;
      byte lastMask = 0;

      HuffmanTreeNode current = _root;
      byte currentByte;
      byte mask;
      for (int i = offset; i < length + offset; i++)
      {
        currentByte = data[i];
        mask = 0x80;
        if (i == offset)
        {
          if ((currentByte & 0x20) == 1)
          {
            //showatend = true;
          }
          mask = 0x20;
          lastIndex = i;
          lastMask = mask;
        }

        while (true)
        {
          if (isFound)
          {
            lastIndex = i;
            lastMask = mask;
            isFound = false;
          }

          if (!codeError)
          {
            HuffmanTreeNode next = (currentByte & mask) == 0 ? current.P0 : current.P1;
            if (next != null)
            {
              current = next;
              if (!string.IsNullOrEmpty(current.Value))
              {
                if (current.Value != "!!!")
                  decodedText.Append(current.Value);
                current = _root;
                isFound = true;
              }
            }
            else
            {
              i = lastIndex;
              currentByte = data[lastIndex];
              mask = lastMask;
              codeError = true;
              continue;
            }
          }

          mask = (byte)(mask >> 1);
          if (mask == 0)
            break;
        }
      }
      return decodedText.ToString();
    }

    protected void ParseDictionary(string[] dictionary)
    {
      _root = new HuffmanTreeNode();
      HuffmanTreeNode current;
      foreach (string line in dictionary)
      {
        int splitIndex = line.LastIndexOf("=");
        string entry = line.Substring(0, splitIndex);
        string path = line.Substring(splitIndex + 1, line.Length - splitIndex - 1);

        current = _root;
        foreach (char c in path)
        {
          if (c == '1')
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
    }
  }
}
