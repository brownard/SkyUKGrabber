using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SkyUK
{
  class SkyLogoDownloader
  {
    const string URL_FORMAT = "http://tv.sky.com/logo/{0}/{0}/skychb{1}.png";
    const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
    static readonly char[] REPLACE_CHARS = new[] { '\\', '/', '|', ':', '*', '?', '<', '>' };

    public static bool DownloadLogo(int channelId, string channelName, string downloadDirectory)
    {
      string url = string.Format(URL_FORMAT, 200, channelId);
      Image image = DownloadImage(url);
      if (image == null)
      {
        url = string.Format(URL_FORMAT, 100, channelId);
        image = DownloadImage(url);
        if (image == null)
          return false;
      }

      using (image)
      using (Image centeredImage = CenterImage(image))
      {
        if (centeredImage != null)
          return SaveImage(centeredImage, channelName, downloadDirectory);
        return false;
      }
    }

    public static bool LogoExists(string channelName, string downloadDirectory)
    {
      try
      {
        return File.Exists(GetLogoPath(channelName, downloadDirectory));
      }
      catch
      {
        return false;
      }
    }

    public static bool CheckDownloadDirectory(string downloadDirectory)
    {
      try
      {
        if (!Directory.Exists(downloadDirectory))
          Directory.CreateDirectory(downloadDirectory);
        return true;
      }
      catch
      {
        return false;
      }
    }

    protected static Image DownloadImage(string url)
    {
      try
      {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.UserAgent = USER_AGENT;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
          if (request.HaveResponse && response.StatusCode == HttpStatusCode.OK)
            return Image.FromStream(response.GetResponseStream());
      }
      catch
      {
      }
      return null;
    }

    protected static bool SaveImage(Image image, string channelName, string downloadDirectory)
    {
      string path = GetLogoPath(channelName, downloadDirectory);
      try
      {
        image.Save(path);
        return true;
      }
      catch
      {
        return false;
      }
    }

    protected static Image CenterImage(Image originalImage)
    {
      int maxDimension;
      int x;
      int y;
      if (originalImage.Width >= originalImage.Height)
      {
        maxDimension = originalImage.Width;
        x = 0;
        y = (originalImage.Width - originalImage.Height) / 2;
      }
      else
      {
        maxDimension = originalImage.Height;
        x = (originalImage.Height - originalImage.Width) / 2;
        y = 0;
      }

      Bitmap image = null;
      try
      {
        image = new Bitmap(maxDimension, maxDimension);
        using (Graphics graphics = Graphics.FromImage(image))
        {
          graphics.CompositingMode = CompositingMode.SourceCopy;
          graphics.CompositingQuality = CompositingQuality.HighQuality;
          graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
          graphics.SmoothingMode = SmoothingMode.HighQuality;
          graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
          graphics.DrawImage(originalImage, x, y, originalImage.Width, originalImage.Height);
        }
        return image;
      }
      catch
      {
        if (image != null)
          image.Dispose();
        return null;
      }
    }

    protected static string GetLogoPath(string channelName, string downloadDirectory)
    {
      return Path.Combine(downloadDirectory, GetFilename(channelName));
    }

    protected static string GetFilename(string channelName)
    {
      foreach (char c in REPLACE_CHARS)
        channelName = channelName.Replace(c, '_');
      return channelName + ".png";
    }
  }
}
