using SetupTv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TvControl;
using TvEngine;
using TvLibrary.Log;

namespace SkyUK
{
  public class SkyUKGrabber : ITvServerPlugin
  {
    Settings _settings;
    SkyGrabber _skyGrabber;
    Timer _timer;

    public void Start(IController controller)
    {
      _skyGrabber = new SkyGrabber();
      _skyGrabber.Message += OnMessage;
      _settings = new Settings();
      _settings.IsGrabbing = false;
      _timer = new Timer();
      _timer.Interval = 10000;
      _timer.Elapsed += OnTick;
      _timer.Start();
    }

    public void Stop()
    {
      _timer.Stop();
      _settings = null;
      _skyGrabber = null;
    }

    private void OnMessage(string Text, bool UpdateLast)
    {
      if (!UpdateLast)
        Log.Write("Sky Plugin : " + Text);
    }

    private void OnTick(object sender, ElapsedEventArgs e)
    {
      if (_settings.IsGrabbing || !_settings.AutoUpdate)
        return;

      DateTime Now = DateTime.Now;
      if (_settings.EveryHour)
      {
        if (_settings.LastUpdate.AddHours(_settings.UpdateInterval) < Now)
          _skyGrabber.Grab();
        return;
      }

      if (Now.Hour != _settings.UpdateTime.Hour || _settings.LastUpdate.Date == Now.Date
        || Now.Minute < _settings.UpdateTime.Minute || Now.Minute > _settings.UpdateTime.Minute + 10)
        return;

      switch (Now.DayOfWeek)
      {
        case DayOfWeek.Monday:
          if (_settings.Mon)
            _skyGrabber.Grab();
          break;
        case DayOfWeek.Tuesday:
          if (_settings.Tue)
            _skyGrabber.Grab();
          break;
        case DayOfWeek.Wednesday:
          if (_settings.Wed)
            _skyGrabber.Grab();
          break;
        case DayOfWeek.Thursday:
          if (_settings.Thu)
            _skyGrabber.Grab();
          break;
        case DayOfWeek.Friday:
          if (_settings.Fri)
            _skyGrabber.Grab();
          break;
        case DayOfWeek.Saturday:
          if (_settings.Sat)
            _skyGrabber.Grab();
          break;
        case DayOfWeek.Sunday:
          if (_settings.Sun)
            _skyGrabber.Grab();
          break;
      }
    }

    public string Author
    {
      get { return "DJBlu"; }
    }

    public bool MasterOnly
    {
      get { return true; }
    }

    public string Name
    {
      get { return "Sky UK Grabber"; }
    }

    public SectionSettings Setup
    {
      get { return new Setup(); }
    }

    public string Version
    {
      get { return "1.2.0.27"; }
    }
  }
}
