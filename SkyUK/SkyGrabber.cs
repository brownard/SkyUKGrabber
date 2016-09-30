using Custom_Data_Grabber;
using DirectShowLib.BDA;
using SkyUK.Huffman;
using SkyUK.Sky;
using SkyUK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Interfaces;
using TvService;

namespace SkyUK
{
  public delegate void MessageEventHandler(string message, bool updateLast);

  public class SkyGrabber
  {
    protected const string NETWORK_ID = "SKYUK";    

    Settings Settings = new Settings();
    public CustomDataGRabber Sky;

    List<Card> CardstoMap = new List<Card>();
    public Dictionary<int, SkyChannel> Channels = new Dictionary<int, SkyChannel>();
    public Dictionary<int, SkyBouquet> Bouquets = new Dictionary<int, SkyBouquet>();
    public Dictionary<string, SDTInfo> SDTInfo = new Dictionary<string, SDTInfo>();
    public Dictionary<int, NITSatDescriptor> NITInfo = new Dictionary<int, NITSatDescriptor>();

    int numberBouquetsPopulated = 0;
    int SDTCount = 0;
    string numberSDTPopulated = "";
    bool GotAllSDT = false;
    int numberTIDPopulated = 0;
    bool GotAllTID = false;
    int titlesDecoded = 0;
    int summariesDecoded = 0;

    public Dictionary<int, string> titleDataCarouselStartLookup = new Dictionary<int, string>();
    public List<int> completedTitleDataCarousels = new List<int>();
    public Dictionary<int, string> summaryDataCarouselStartLookup = new Dictionary<int, string>();
    public List<int> completedSummaryDataCarousels = new List<int>();
    public bool NITGot = false;

    TvBusinessLayer _layer = new TvBusinessLayer();
    DateTime start;
    DVBSChannel DVBSChannel;
    public int BouquetIDtoUse;
    public int RegionIDtoUse;
    public bool GrabEPG;

    HuffmanDecoder huffman;
    Dictionary<int, string> _logosToDownload = new Dictionary<int, string>();
    bool _updateLogos;
    string _logoDirectory;
    bool _useThrottle;
    
    public event MessageEventHandler Message;
    protected virtual void OnMessage(string message, bool updateLast)
    {
      var handler = Message;
      if (handler != null)
        handler(message, updateLast);
    }

    public event Action ActivateControls;
    protected virtual void OnActivateControls()
    {
      var handler = ActivateControls;
      if (handler != null)
        handler();
    }

    #region Grabbing

    public void Grab()
    {
      OnMessage("Sky Channel and EPG Grabber initialised", false);
      if (Settings.IsGrabbing == false)
      {
        Settings.IsGrabbing = true;
        Reset();
        Thread back = new Thread(Grabit);

        if (_useThrottle)
            back.Priority = ThreadPriority.Lowest;

        back.Start();
      }
    }

    private void Grabit()
    {
      Sky = new CustomDataGRabber();
      Sky.OnPacket += OnTSPacket;
      Sky.OnComplete += UpdateDataBase;

      List<int> mappedCards = Settings.CardMap;
      if (mappedCards == null | mappedCards.Count == 0)
      {
        OnMessage("No cards are selected for use, please correct this before continuing", false);
        Settings.IsGrabbing = false;
        OnActivateControls();
        return;
      }

      huffman = new HuffmanDecoder(SkyUtils.GetUKHuffmanDictionary());
      OnMessage("Huffman Loaded", false);

      List<int> pidList = new List<int>();
      pidList.Add(0x10);
      pidList.Add(0x11);
      if (Settings.UpdateEPG)
      {
        for (int i = 0; i < 8; i++)
        {
          pidList.Add(0x30 + i);
          pidList.Add(0x40 + i);
        }
      }

      GrabEPG = Settings.UpdateEPG;
      DVBSChannel = new DVBSChannel();

      IList<Channel> channels = _layer.GetChannelsByName("Sky UK Grabber");
      bool create = channels.Count == 0;
      Channel channel = create ? CreateGrabberChannel() : channels[0];

      if (channel == null)
      {
        OnMessage("Channel was lost somewhere, try clicking on Grab data again", false);
        return;
      }

      int id = -1;
      foreach (Card card in Card.ListAll())
      {
        if (RemoteControl.Instance.Type(card.IdCard) == CardType.DvbS)
        {
          id++;
          if (mappedCards.Contains(id))
          {
            CardstoMap.Add(card);
            if (create)
              _layer.MapChannelToCard(card, channel, false);
          }
        }
      }

      _updateLogos = Settings.UpdateLogos;
      _logoDirectory = Settings.LogoDirectory;
      _useThrottle = Settings.UseThrottle;

      OnMessage("Grabbing Data", false);
      int seconds = Settings.GrabTime;
      OnMessage("Grabber set to grab " + seconds + " seconds of data", false);
      Sky.GrabData(channel.IdChannel, seconds, pidList);
    }

    protected Channel CreateGrabberChannel()
    {
      DVBSChannel.BandType = 0;
      DVBSChannel.DisEqc = (DisEqcType)Settings.DiseqC;
      DVBSChannel.FreeToAir = true;
      DVBSChannel.Frequency = Settings.frequency;
      DVBSChannel.SymbolRate = Settings.SymbolRate;
      DVBSChannel.InnerFecRate = BinaryConvolutionCodeRate.RateNotSet;
      DVBSChannel.IsRadio = true;
      DVBSChannel.IsTv = false;
      DVBSChannel.LogicalChannelNumber = 10000;
      DVBSChannel.ModulationType = (ModulationType)(Settings.modulation - 1);
      DVBSChannel.Name = "Sky UK Grabber";
      DVBSChannel.NetworkId = Settings.NID;
      DVBSChannel.Pilot = Pilot.NotSet;
      DVBSChannel.PmtPid = 0;
      DVBSChannel.Polarisation = (Polarisation)(Settings.polarisation - 1);
      DVBSChannel.Provider = "DJBlu";
      DVBSChannel.Rolloff = RollOff.NotSet;
      DVBSChannel.ServiceId = Settings.ServiceID;
      DVBSChannel.TransportId = Settings.TransportID;
      DVBSChannel.SatelliteIndex = 16;
      DVBSChannel.SwitchingFrequency = Settings.SwitchingFrequency;

      Channel channel = _layer.AddNewChannel("Sky UK Grabber", 10000);
      channel.VisibleInGuide = false;
      channel.IsRadio = true;
      channel.IsTv = false;
      channel.Persist();
      _layer.AddTuningDetails(channel, DVBSChannel);
      _layer.AddChannelToRadioGroup(channel, TvConstants.RadioGroupNames.AllChannels);
      OnMessage("Sky UK Grabber channel added to database", false);
      return channel;
    }

    private void Reset()
    {
      Channels.Clear();
      Bouquets.Clear();
      SDTInfo.Clear();
      NITInfo.Clear();
      SDTCount = 0;
      numberBouquetsPopulated = 0;
      titlesDecoded = 0;
      summariesDecoded = 0;
      titleDataCarouselStartLookup.Clear();
      completedTitleDataCarousels.Clear();
      summaryDataCarouselStartLookup.Clear();
      completedSummaryDataCarousels.Clear();
      start = DateTime.Now;
      NITGot = false;
      BouquetIDtoUse = Settings.BouquetID;
      RegionIDtoUse = Settings.RegionID;
      numberSDTPopulated = "";
      GotAllSDT = false;
      numberTIDPopulated = 0;
      GotAllTID = false;
      _logosToDownload.Clear();
    }

    #endregion

    #region Parsed Info Handlers

    private bool IsEverythingGrabbed()
    {
      if (!AreAllBouquetsPopulated() || !GotAllSDT || !GotAllTID)
        return false;

      if (!GrabEPG)
        return true;

      if (AreAllSummariesPopulated() && AreAllTitlesPopulated())
      {
        OnMessage(string.Format("Everything grabbed: Titles({0}) : Summaries({1})", titlesDecoded, summariesDecoded), false);
        return true;
      }
      return false;
    }

    private SkyBouquet GetBouquet(int bouquetId)
    {
      SkyBouquet returnBouquet;
      if (!Bouquets.TryGetValue(bouquetId, out returnBouquet))
        Bouquets[bouquetId] = returnBouquet = new SkyBouquet();
      return returnBouquet;
    }

    private bool AreAllBouquetsPopulated()
    {
      return Bouquets.Count > 0 && Bouquets.Count == numberBouquetsPopulated;
    }

    private void NotifyBouquetPopulated()
    {
      numberBouquetsPopulated++;
      if (Bouquets.Count == numberBouquetsPopulated)
      {
        OnMessage("Bouquet scan complete.  ", false);
        OnMessage("Found " + Channels.Count + " channels in " + Bouquets.Count + " bouquets, searching SDT Information", false);
      }
    }

    private SkyChannel GetChannel(int ChannelID)
    {
      SkyChannel returnChannel;
      if (!Channels.TryGetValue(ChannelID, out returnChannel))
        Channels[ChannelID] = returnChannel = new SkyChannel() { ChannelID = ChannelID };
      return returnChannel;
    }

    private SDTInfo GetChannelbySID(string SID)
    {
      if (SDTInfo.ContainsKey(SID))
        return SDTInfo[SID];
      return null;
    }

    private void NotifySkyChannelPopulated(int tid, int nid, int sid)
    {
      if (GotAllSDT)
        return;

      string sdtPopulatedString = string.Format("{0}-{1}-{2}", nid, tid, sid);
      if (string.IsNullOrEmpty(numberSDTPopulated))
      {
        numberSDTPopulated = sdtPopulatedString;
      }
      else if (sdtPopulatedString == numberSDTPopulated)
      {
        GotAllSDT = true;
        OnMessage("Got all SDT Info, count: " + SDTInfo.Count, false);
      }
    }

    private void NotifyTIDPopulated(int tid)
    {
      if (GotAllTID)
        return;
      if (numberTIDPopulated == 0)
      {
        numberTIDPopulated = tid;
      }
      else if (tid == numberTIDPopulated)
      {
        GotAllTID = true;
        OnMessage("Got all Network Information", false);
      }
    }

    private bool DoesTidCarryEpgTitleData(int TableID)
    {
      return TableID == 0xa0 | TableID == 0xa1 | TableID == 0xa2 | TableID == 0xa3;
    }

    private bool DoesTidCarryEpgSummaryData(int tableId)
    {
      return tableId == 0xa8 || tableId == 0xa9 || tableId == 0xaa || tableId == 0xab;
    }

    private bool AreAllTitlesPopulated()
    {
      return completedTitleDataCarousels.Count == 8;
    }

    private bool IsTitleDataCarouselOnPidComplete(int pid)
    {
      return completedTitleDataCarousels.Contains(pid);
    }

    private void OnTitleReceived(int pid, string titleChannelEventUnionId)
    {
      if (titleDataCarouselStartLookup.ContainsKey(pid))
      {
        if ((titleDataCarouselStartLookup[pid] == titleChannelEventUnionId))
        {
          completedTitleDataCarousels.Add(pid);
        }
      }
      else
      {
        titleDataCarouselStartLookup.Add(pid, titleChannelEventUnionId);
      }
    }

    private bool AreAllSummariesPopulated()
    {
      return completedSummaryDataCarousels.Count == 8;
    }

    private void OnSummaryReceived(int pid, string summaryChannelEventUnionId)
    {
      string summaryEventId;
      if (summaryDataCarouselStartLookup.TryGetValue(pid, out summaryEventId))
      {
        if (summaryEventId == summaryChannelEventUnionId)
        {
          completedSummaryDataCarousels.Add(pid);
          if (AreAllSummariesPopulated())
            return;
        }
      }
      else
      {
        summaryDataCarouselStartLookup.Add(pid, summaryChannelEventUnionId);
      }
    }

    private bool IsSummaryDataCarouselOnPidComplete(int pid)
    {
      return completedSummaryDataCarousels.Contains(pid);
    }

    private void UpdateChannel(int ChannelId, SkyChannel Channel)
    {
      if (Channels.ContainsKey(ChannelId))
        Channels[ChannelId] = Channel;
    }

    private void UpdateEPGEvent(ref int channelId, int eventId, SkyEvent SkyEvent)
    {
      SkyChannel channel;
      if (Channels.TryGetValue(channelId, out channel) && channel.Events.ContainsKey(eventId))
        channel.Events[eventId] = SkyEvent;
    }

    private SkyEvent GetEpgEvent(long channelId, int eventId)
    {
      SkyChannel channel = GetChannel((int)channelId);
      SkyEvent returnEvent;
      if (!channel.Events.TryGetValue(eventId, out returnEvent))
        channel.Events[eventId] = returnEvent = new SkyEvent();
      return returnEvent;
    }

    #endregion

    #region Database Updating

    private void UpdateDataBase(bool error, string errorMessage)
    {
      if (!error)
      {
        if (Channels.Count < 100)
        {
          OnMessage("Error : Less than 100 channels found, Grabber found : " + Channels.Count, false);
        }
        else
        {
          CreateGroups();
          if (Settings.UpdateChannels)
          {
            OnMessage("Moving/Deleting Old Channels", false);
            DeleteOldChannels();
            OnMessage("Moving/Deleting Old Channels Complete", false);
            OnMessage("Updating/Adding New Channels", false);
            UpdateAddChannels();
            DownloadLogos();
            OnMessage("Updating/Adding New Channels Complete", false);
          }

          if (Settings.UpdateEPG)
          {
            OnMessage("Updating EPG, please wait ... This can take upto 15 mins", false);
            UpdateAddEpg();
          }
          Settings.LastUpdate = DateTime.Now;
          OnMessage("Database Update Complete, took " + (int)(DateTime.Now.Subtract(start).TotalSeconds) + " Seconds", false);
        }
      }
      else
      {
        OnMessage("Error Occured:- " + errorMessage, false);
      }

      OnActivateControls();
      Settings.IsGrabbing = false;
    }

    private void CreateGroups()
    {
      if (!Settings.UseSkyCategories)
        return;
      List<string> categories = Settings.GetConfiguredCategories();
      if (categories.Count == 0)
        return;

      int sortOrder = 1;
      foreach (string category in categories)
      {
        ChannelGroup group = _layer.CreateGroup(category);
        group.SortOrder = sortOrder++;
        group.Persist();
      }
    }

    public void UpdateAddEpg()
    {      
      if (_layer.GetPrograms(DateTime.Now, DateTime.Now.AddDays(1)).Count < 1)
        AddEpg();
      else
        UpdateEpg();
      OnMessage("EPG Update Complete", false);
    }

    protected void AddEpg()
    {
      ProgramList programs = new ProgramList();
      bool useExtraInfo = Settings.useExtraInfo;

      foreach (SkyChannel skyChannel in Channels.Values)
      {
        Channel dbChannel = _layer.GetChannelByTuningDetail(skyChannel.NID, skyChannel.TID, skyChannel.SID);
        if (dbChannel == null)
          continue;

        foreach (SkyEvent skyEvent in skyChannel.Events.Values)
        {
          if (skyEvent.EventID == 0 || string.IsNullOrEmpty(skyEvent.Title))
            continue;

          DateTime programStartDay = DVBUtils.DateTimeFromMJD(skyEvent.mjdStart);
          DateTime programStartTime = programStartDay.AddSeconds(skyEvent.StartTime);
          //  Start time is in UTC, need to convert to local time
          programStartTime = programStartTime.ToLocalTime();
          //  Calculate end time
          DateTime programEndTime = programStartTime.AddSeconds(skyEvent.Duration);
          string description = skyEvent.Summary;
          if (useExtraInfo)
            description += " " + skyEvent.DescriptionFlag;
          string theme = Settings.GetTheme(Convert.ToInt32(skyEvent.Category));

          Program program = new Program(dbChannel.IdChannel, programStartTime, programEndTime, skyEvent.Title, description, theme, Program.ProgramState.None, new DateTime(1900, 1, 1), "", "",
          "", "", 0, skyEvent.ParentalCategory, 0, skyEvent.SeriesID.ToString(), skyEvent.seriesTermination);
          programs.Add(program);
        }
      }
      _layer.InsertPrograms(programs, ThreadPriority.Normal);
    }

    protected void UpdateEpg()
    {
      EpgDBUpdater epgUpdater = new EpgDBUpdater(new TVController(), "Sky TV EPG Updater", false);
      List<EpgChannel> channelsToUpdate = new List<EpgChannel>();
      bool useExtraInfo = Settings.useExtraInfo;

      foreach (SkyChannel skyChannel in Channels.Values)
      {
        EpgChannel epgChannel = new EpgChannel();
        DVBBaseChannel baseChannel = new DVBSChannel();
        baseChannel.NetworkId = skyChannel.NID;
        baseChannel.TransportId = skyChannel.TID;
        baseChannel.ServiceId = skyChannel.SID;
        baseChannel.Name = string.Empty;
        epgChannel.Channel = baseChannel;
        foreach (SkyEvent skyEvent in skyChannel.Events.Values)
        {
          if (skyEvent.EventID == 0 || string.IsNullOrEmpty(skyEvent.Title))
            continue;

          DateTime programStartDay = DVBUtils.DateTimeFromMJD(skyEvent.mjdStart);
          DateTime programStartTime = programStartDay.AddSeconds(skyEvent.StartTime);
          //  Start time is in UTC, need to convert to local time
          programStartTime = programStartTime.ToLocalTime();
          //  Calculate end time
          DateTime programEndTime = programStartTime.AddSeconds(skyEvent.Duration);

          string description = skyEvent.Summary;
          if (useExtraInfo)
            description += " " + skyEvent.DescriptionFlag;
          string theme = Settings.GetTheme(Convert.ToInt32(skyEvent.Category));

          EpgLanguageText epgLanguage = new EpgLanguageText("ALL", skyEvent.Title, description, theme, 0, skyEvent.ParentalCategory, -1);
          EpgProgram epgProgram = new EpgProgram(programStartTime, programEndTime);
          epgProgram.Text.Add(epgLanguage);
          epgProgram.SeriesId = skyEvent.SeriesID.ToString();
          epgProgram.SeriesTermination = skyEvent.seriesTermination;
          epgChannel.Programs.Add(epgProgram);
        }

        if (epgChannel.Programs.Count > 0)
          channelsToUpdate.Add(epgChannel);
      }

      OnMessage("", false);
      int currentChannel = 1;
      foreach (EpgChannel epgChannel in channelsToUpdate)
      {
        epgUpdater.UpdateEpgForChannel(epgChannel);
        OnMessage(string.Format("({0}/{1}) Channels Updated", currentChannel++, channelsToUpdate.Count), true);
      }
    }

    #endregion

    #region Channel Updating

    public void UpdateAddChannels()
    {
      try
      {
        int DiseqC = Settings.DiseqC;
        bool UseSkyNumbers = Settings.UseSkyNumbers;
        int SwitchingFrequency = Settings.SwitchingFrequency;
        bool UseSkyRegions = Settings.UseSkyRegions;
        bool UseSkyCategories = Settings.UseSkyCategories;
        bool UseModNotSetSD = Settings.UseNotSetModSD;
        bool UseModNotSetHD = Settings.UseNotSetModHD;
        bool IgnoreScrambled = Settings.IgnoreScrambled;

        int ChannelsAdded = 0;
        OnMessage("", false);

        foreach (SkyChannel ScannedChannel in Channels.Values)
        {
          ChannelsAdded++;
          OnMessage("(" + ChannelsAdded + "/" + Channels.Count + ") Channels sorted", true);

          if (ScannedChannel.ChannelID < 1 || ScannedChannel.NID == 0 || ScannedChannel.TID == 0 || ScannedChannel.SID == 0)
            continue;

          SDTInfo SDT = GetChannelbySID(ScannedChannel.NID + "-" + ScannedChannel.TID + "-" + ScannedChannel.SID);
          if (SDT == null)
            continue;

          if (IgnoreScrambled & SDT.IsFTA)
            continue;

          if (SDT.SID < 1)
            SDT.SID = ScannedChannel.SID;
          if (string.IsNullOrEmpty(SDT.ChannelName))
            SDT.ChannelName = SDT.SID.ToString();
          if (string.IsNullOrEmpty(SDT.Provider))
            SDT.Provider = "BSkyB";

          TuningDetail currentDetail = null;
          Channel checker = _layer.GetChannelbyExternalID(GetExternalId(ScannedChannel.ChannelID));
          if (checker != null)
            currentDetail = checker.ReferringTuningDetail().FirstOrDefault(t => t.ChannelType == 3 & t.NetworkId == 2);

          Channel DBChannel = currentDetail != null ? currentDetail.ReferencedChannel() : null;
          if (currentDetail == null || (DBChannel != null && DBChannel.ExternalId != GetExternalId(ScannedChannel.ChannelID)))
            AddNewChannel(ScannedChannel, SDT, UseSkyNumbers, UseSkyCategories, UseModNotSetSD, UseModNotSetHD, SwitchingFrequency, (DisEqcType)DiseqC);
          else if (DBChannel != null)
            UpdateChannel(currentDetail, DBChannel, ScannedChannel, UseSkyNumbers, UseSkyCategories, UseSkyRegions, UseModNotSetSD, UseModNotSetHD, SwitchingFrequency);
        }
      }
      catch (Exception ex)
      {
        OnMessage("Error updating channels - " + ex.Message, false);
      }
    }

    protected void UpdateChannel(TuningDetail currentDetail, Channel DBChannel, SkyChannel ScannedChannel, bool UseSkyNumbers, bool UseSkyCategories, bool UseSkyRegions, bool UseModNotSetSD, bool UseModNotSetHD, int SwitchingFrequency)
    {
      DVBSChannel checkDVBSChannel = _layer.GetTuningChannel(currentDetail) as DVBSChannel;
      if (checkDVBSChannel == null)
        return;

      SDTInfo checkSDT;
      if (SDTInfo.TryGetValue(ScannedChannel.NID + "-" + ScannedChannel.TID + "-" + ScannedChannel.SID, out checkSDT))
      {
        bool haschanged = false;
        bool deleteepg = false;
        if (DBChannel.DisplayName != checkSDT.ChannelName || currentDetail.Name != checkSDT.ChannelName)
        {
          OnMessage("Channel " + DBChannel.DisplayName + " name changed to " + checkSDT.ChannelName, false);
          DBChannel.DisplayName = checkSDT.ChannelName;
          checkDVBSChannel.Name = checkSDT.ChannelName;
          //Check Channel hasn't become a real channel from a test channel
          if (ScannedChannel.LCNCount > 0 & DBChannel.VisibleInGuide == false)
          {
            DBChannel.VisibleInGuide = true;
            OnMessage("Channel " + DBChannel.DisplayName + " is now part of the EPG making visible " + checkSDT.ChannelName + ".", false);
          }
          haschanged = true;
        }

        if (checkDVBSChannel.Provider != checkSDT.Provider)
        {
          OnMessage("Channel " + DBChannel.DisplayName + " Provider name changed to " + checkSDT.Provider + ".", false);
          OnMessage("", false);
          checkDVBSChannel.Provider = checkSDT.Provider;
          haschanged = true;
        }

        if (currentDetail.TransportId != ScannedChannel.TID)
        {
          OnMessage("Channel : " + DBChannel.DisplayName + " tuning details changed.", false);
          OnMessage("", false);

          NITSatDescriptor NIT;
          if (!NITInfo.TryGetValue(ScannedChannel.TID, out NIT))
            return;

          ModulationType modulationType;
          if ((NIT.IsS2 != 0 && UseModNotSetHD) || (NIT.IsS2 == 0 && UseModNotSetSD))
            modulationType = ModulationType.ModNotSet;
          else
            modulationType = GetModulationType(NIT);

          checkDVBSChannel.BandType = 0;
          checkDVBSChannel.Frequency = NIT.Frequency;
          checkDVBSChannel.SymbolRate = NIT.Symbolrate;
          checkDVBSChannel.InnerFecRate = (BinaryConvolutionCodeRate)NIT.FECInner;
          checkDVBSChannel.Pilot = Pilot.NotSet;
          checkDVBSChannel.ModulationType = modulationType;
          checkDVBSChannel.Rolloff = NIT.IsS2 == 1 ? (RollOff)NIT.RollOff : RollOff.NotSet;
          checkDVBSChannel.PmtPid = 0;
          checkDVBSChannel.Polarisation = (Polarisation)NIT.Polarisation;
          checkDVBSChannel.TransportId = ScannedChannel.TID;
          checkDVBSChannel.SwitchingFrequency = SwitchingFrequency;
          // Option for user to enter
          haschanged = true;
          deleteepg = true;
          OnMessage("Channel : " + DBChannel.DisplayName + " tuning details changed.", false);
          OnMessage("", false);
        }

        if (currentDetail.ServiceId != ScannedChannel.SID)
        {
          checkDVBSChannel.ServiceId = ScannedChannel.SID;
          checkDVBSChannel.PmtPid = 0;
          OnMessage("Channel : " + DBChannel.DisplayName + " serviceID changed.", false);
          OnMessage("", false);
          haschanged = true;
          deleteepg = true;
        }

        if (UseSkyRegions && UseSkyNumbers && ScannedChannel.LCNCount > 0)
        {
          int checkLCN = 10000;
          if (ScannedChannel.ContainsLCN(BouquetIDtoUse, RegionIDtoUse))
            checkLCN = ScannedChannel.GetLCN(BouquetIDtoUse, RegionIDtoUse).SkyNumber;
          else if (ScannedChannel.ContainsLCN(BouquetIDtoUse, 255))
            checkLCN = ScannedChannel.GetLCN(BouquetIDtoUse, 255).SkyNumber;

          if ((currentDetail.ChannelNumber != checkLCN && checkLCN < 1000) || (checkLCN == 10000 && DBChannel.SortOrder != 10000))
          {
            OnMessage("Channel : " + DBChannel.DisplayName + " number has changed from : " + checkDVBSChannel.LogicalChannelNumber + " to : " + checkLCN + ".", false);
            OnMessage("", false);
            DBChannel.RemoveFromAllGroups();
            currentDetail.ChannelNumber = checkLCN;
            checkDVBSChannel.LogicalChannelNumber = checkLCN;
            DBChannel.SortOrder = checkLCN;
            DBChannel.VisibleInGuide = true;
            haschanged = true;
            AddChannelToGroups(DBChannel, checkSDT, checkDVBSChannel, UseSkyCategories);
          }
        }

        bool logoNeedsUpdate;
        if (haschanged)
        {
          DBChannel.Persist();
          TuningDetail tuning = _layer.UpdateTuningDetails(DBChannel, checkDVBSChannel, currentDetail);
          tuning.Persist();
          MapChannelToCards(DBChannel);
          if (deleteepg)
            _layer.RemoveAllPrograms(DBChannel.IdChannel);
          logoNeedsUpdate = _updateLogos && (DBChannel.DisplayName != checkSDT.ChannelName || currentDetail.Name != checkSDT.ChannelName);
        }
        else
        {
          logoNeedsUpdate = _updateLogos && !SkyLogoDownloader.LogoExists(checkSDT.ChannelName, _logoDirectory);
        }

        if (logoNeedsUpdate)
          _logosToDownload[ScannedChannel.ChannelID] = checkSDT.ChannelName;
      }
    }

    protected void AddNewChannel(SkyChannel scannedChannel, SDTInfo sdt, bool useSkyNumbers, bool useSkyCategories, bool useModNotSetSD, bool useModNotSetHD, int switchingFrequency, DisEqcType diseqC)
    {
      NITSatDescriptor nit;
      if (!NITInfo.TryGetValue(scannedChannel.TID, out nit))
      {
        //no nit info
        OnMessage("No NIT found for : " + scannedChannel.SID, false);
        OnMessage("", false);
        return;
      }

      int logicalChannelNumber = 10000;
      bool visibleInGuide = true;

      if (useSkyNumbers && scannedChannel.LCNCount > 0)
      {
        if (scannedChannel.ContainsLCN(BouquetIDtoUse, RegionIDtoUse))
          logicalChannelNumber = scannedChannel.GetLCN(BouquetIDtoUse, RegionIDtoUse).SkyNumber;
        else if (scannedChannel.ContainsLCN(BouquetIDtoUse, byte.MaxValue))
          logicalChannelNumber = scannedChannel.GetLCN(BouquetIDtoUse, byte.MaxValue).SkyNumber;
        if (logicalChannelNumber == 10000)
          visibleInGuide = false;
      }

      ModulationType modulationType;
      if ((nit.IsS2 != 0 & useModNotSetHD) || (nit.IsS2 == 0 & useModNotSetSD))
        modulationType = ModulationType.ModNotSet;
      else
        modulationType = GetModulationType(nit);

      DVBSChannel.BandType = 0;
      DVBSChannel.DisEqc = (DisEqcType)diseqC;
      DVBSChannel.FreeToAir = true;
      DVBSChannel.Frequency = nit.Frequency;
      DVBSChannel.SymbolRate = nit.Symbolrate;
      DVBSChannel.InnerFecRate = (BinaryConvolutionCodeRate)nit.FECInner;
      DVBSChannel.IsRadio = sdt.IsRadio;
      DVBSChannel.IsTv = sdt.IsTV;
      DVBSChannel.FreeToAir = !sdt.IsFTA;
      DVBSChannel.LogicalChannelNumber = logicalChannelNumber;
      DVBSChannel.ModulationType = modulationType;

      DVBSChannel.Name = sdt.ChannelName;
      DVBSChannel.NetworkId = scannedChannel.NID;
      DVBSChannel.Pilot = Pilot.NotSet;
      DVBSChannel.Rolloff = nit.IsS2 == 1 ? (RollOff)nit.RollOff : RollOff.NotSet;

      DVBSChannel.PmtPid = 0;
      DVBSChannel.Polarisation = (Polarisation)nit.Polarisation;
      DVBSChannel.Provider = sdt.Provider;
      DVBSChannel.ServiceId = scannedChannel.SID;
      DVBSChannel.TransportId = scannedChannel.TID;
      DVBSChannel.SwitchingFrequency = switchingFrequency;

      Channel dbChannel = _layer.AddNewChannel(sdt.ChannelName, logicalChannelNumber);
      dbChannel.ChannelNumber = logicalChannelNumber;
      dbChannel.SortOrder = logicalChannelNumber;
      dbChannel.VisibleInGuide = visibleInGuide;
      // Option for user to enter
      dbChannel.IsRadio = sdt.IsRadio;
      dbChannel.IsTv = sdt.IsTV;
      dbChannel.ExternalId = GetExternalId(scannedChannel.ChannelID);
      dbChannel.Persist();

      _layer.AddTuningDetails(dbChannel, DVBSChannel);
      MapChannelToCards(dbChannel);
      AddChannelToGroups(dbChannel, sdt, DVBSChannel, useSkyCategories);

      if (_updateLogos)
        _logosToDownload[scannedChannel.ChannelID] = sdt.ChannelName;
    }

    protected void DeleteOldChannels()
    {
      bool useRegions = Settings.UseSkyRegions;
      bool deleteOld = Settings.DeleteOldChannels;
      string oldFolder = Settings.OldChannelFolder;
      RegionIDtoUse = Settings.RegionID;

      IList<Channel> channelsToCheck = _layer.Channels;

      foreach (Channel checkChannel in channelsToCheck)
      {
        // Get NID and ChannelID
        string NetworkID;
        int ChannelID;
        if (!TryParseExternalId(checkChannel.ExternalId, out NetworkID, out ChannelID))
          continue;

        if (NetworkID != NETWORK_ID)
          continue;

        //Not a 28e channel
        if (!Channels.ContainsKey(ChannelID))
        {
          RemoveChannel(checkChannel, deleteOld, oldFolder);
          continue;
        }

        if (useRegions)
        {
          //Move Channels that are not in this Bouquet
          SkyChannel ScannedChannel = Channels[ChannelID];
          if (ScannedChannel.ContainsLCN(BouquetIDtoUse, RegionIDtoUse) || ScannedChannel.ContainsLCN(BouquetIDtoUse, 255))
            continue;

          if (checkChannel.IsTv && checkChannel.VisibleInGuide)
          {
            checkChannel.RemoveFromAllGroups();
            checkChannel.VisibleInGuide = false;
            checkChannel.Persist();
            _layer.AddChannelToGroup(checkChannel, TvConstants.TvGroupNames.AllChannels);
            OnMessage("Channel " + checkChannel.DisplayName + " isn't used in this region, moved to all channels.", false);
          }
        }
      }
    }

    protected void RemoveChannel(Channel dbChannel, bool deleteOld, string oldChannelFolder)
    {
      //channel has been deleted
      if (deleteOld)
      {
        dbChannel.Delete();
        OnMessage("Channel " + dbChannel.DisplayName + " no longer exists in the EPG, Deleted.", false);
      }
      else
      {
        dbChannel.RemoveFromAllGroups();
        dbChannel.Persist();
        _layer.AddChannelToGroup(dbChannel, oldChannelFolder);
        OnMessage("Channel " + dbChannel.DisplayName + " no longer exists in the EPG, moved to " + oldChannelFolder + ".", false);
      }
    }

    protected void MapChannelToCards(Channel DBChannel)
    {
      foreach (Card card in CardstoMap)
        _layer.MapChannelToCard(card, DBChannel, false);
    }

    protected void AddChannelToGroups(Channel DBChannel, SDTInfo SDT, DVBSChannel DVBSChannel, bool UseSkyCategories)
    {
      if (DBChannel.IsTv)
      {
        _layer.AddChannelToGroup(DBChannel, TvConstants.TvGroupNames.AllChannels);
        if (DVBSChannel.LogicalChannelNumber >= 1000 || !UseSkyCategories)
          return;
        if (Settings.GetCategory((byte)SDT.Category) != SDT.Category.ToString())
          _layer.AddChannelToGroup(DBChannel, Settings.GetCategory((byte)SDT.Category));
        if (SDT.IsHD)
          _layer.AddChannelToGroup(DBChannel, "HD Channels");
        if (SDT.Is3D)
          _layer.AddChannelToGroup(DBChannel, "3D Channels");
      }
      else if (DBChannel.IsRadio)
        _layer.AddChannelToRadioGroup(DBChannel, TvConstants.RadioGroupNames.AllChannels);
      else
        _layer.AddChannelToGroup(DBChannel, TvConstants.TvGroupNames.AllChannels);
    }

    protected static string GetExternalId(int skyChannelId)
    {
      return string.Format("{0}:{1}", NETWORK_ID, skyChannelId);
    }

    protected static bool TryParseExternalId(string externalId, out string networkId, out int channelId)
    {
      networkId = null;
      channelId = 0;
      if (string.IsNullOrEmpty(externalId))
        return false;

      string[] ids = externalId.Split(':');
      if (ids.Length < 2)
        return false;

      try
      {
        networkId = ids[0];
        channelId = Convert.ToInt32(ids[1]);
        return true;
      }
      catch
      {
        return false;
      }
    }

    protected static ModulationType GetModulationType(NITSatDescriptor nit)
    {
      switch (nit.Modulation)
      {
        case 1:
          return nit.IsS2 != 0 ? ModulationType.ModNbcQpsk : ModulationType.ModQpsk;
        case 2:
          return nit.IsS2 != 0 ? ModulationType.ModNbc8Psk : ModulationType.ModNotDefined;
        default:
          return ModulationType.ModNotDefined;
      }
    }

    protected void DownloadLogos()
    {
      if (!_updateLogos || _logosToDownload.Count == 0)
        return;

      OnMessage("Grabbing/Updating Logos", false);

      if (!SkyLogoDownloader.CheckDownloadDirectory(_logoDirectory))
      {
        OnMessage("Error creating logo download directory", false);
        return;
      }

      OnMessage("", false);
      int total = _logosToDownload.Count;
      int current = 1;
      foreach (var kvp in _logosToDownload)
      {
        SkyLogoDownloader.DownloadLogo(kvp.Key, kvp.Value, _logoDirectory);
        OnMessage(string.Format("{0}/{1} completed", current++, total), true);
      }
    }

    #endregion

    private void OnTitleSectionReceived(int pid, Custom_Data_Grabber.Section section)
    {
      try
      {
        if (IsTitleDataCarouselOnPidComplete(pid))
          return;

        if (!DoesTidCarryEpgTitleData(section.table_id))
          return;

        byte[] buffer = section.Data;
        int totalLength = (((buffer[1] & 0xf) * 256) + buffer[2]) - 2;

        if (section.section_length < 20)
          return;

        int channelId = (buffer[3] * ((int)Math.Pow(2, 8))) + buffer[4];
        long mjdStart = (buffer[8] * ((long)Math.Pow(2, 8))) + buffer[9];
        if ((channelId == 0 | mjdStart == 0))
          return;

        int currentTitleItem = 10;
        int iterationCounter = 0;
        while (currentTitleItem < totalLength)
        {
          if (iterationCounter > 512)
            return;
          iterationCounter++;

          int eventId = (buffer[currentTitleItem + 0] * ((int)Math.Pow(2, 8))) + buffer[currentTitleItem + 1];
          double headerType = (buffer[currentTitleItem + 2] & 0xf0) >> 4;
          int bodyLength = ((buffer[currentTitleItem + 2] & 0xf) * ((int)Math.Pow(2, 8))) + buffer[currentTitleItem + 3];

          string carouselLookupId = channelId.ToString() + ":" + eventId.ToString();
          OnTitleReceived(pid, carouselLookupId);

          if (IsTitleDataCarouselOnPidComplete(pid))
            return;

          SkyEvent epgEvent = GetEpgEvent(channelId, eventId);
          if (epgEvent == null)
            return;

          epgEvent.mjdStart = mjdStart;
          epgEvent.EventID = eventId;

          int headerLength = 4;
          int currentTitleItemBody = currentTitleItem + headerLength;
          int titleDescriptor = buffer[currentTitleItemBody];
          int encodedBufferLength = buffer[currentTitleItemBody + 1] - 7;

          if (titleDescriptor == 0xb5)
          {
            epgEvent.StartTime = (int)(buffer[currentTitleItemBody + 2] * (Math.Pow(2, 9))) | (int)(buffer[currentTitleItemBody + 3] * (Math.Pow(2, 1)));
            epgEvent.Duration = (int)(buffer[currentTitleItemBody + 4] * (Math.Pow(2, 9))) | (int)(buffer[currentTitleItemBody + 5] * (Math.Pow(2, 1)));
            byte themeId = buffer[currentTitleItemBody + 6];
            epgEvent.Category = themeId.ToString();
            epgEvent.SetFlags(buffer[currentTitleItemBody + 7]);
            epgEvent.SetCategory(buffer[currentTitleItemBody + 8]);
            epgEvent.seriesTermination = ((buffer[currentTitleItemBody + 8] & 0x40) >> 6) ^ 0x1;

            if (encodedBufferLength <= 0)
              currentTitleItem += headerLength + bodyLength;

            if (string.IsNullOrEmpty(epgEvent.Title))
            {
              if (currentTitleItemBody + 9 + encodedBufferLength > buffer.Length)
                return;

              // Decode the huffman buffer
              epgEvent.Title = huffman.Decode(buffer, currentTitleItemBody + 9, encodedBufferLength);
              if (!string.IsNullOrEmpty(epgEvent.Title))
                OnTitleDecoded();
            }
            else
            {
              return;
            }

            UpdateEPGEvent(ref channelId, epgEvent.EventID, epgEvent);
          }
          currentTitleItem += (bodyLength + headerLength);
        }

        if (currentTitleItem != totalLength + 1)
          return;
      }
      catch (Exception ex)
      {
        OnMessage(string.Format("Error decoding title - {0}\r\n{1}", ex.Message, ex.StackTrace), false);
      }
    }

    private void ParseNIT(Custom_Data_Grabber.Section Data, int Length)
    {
      try
      {
        if (NITGot)
          return;

        byte[] buffer = Data.Data;
        int sectionSyntaxIndicator = buffer[1] & 0x80;
        int sectionLength = ((buffer[1] & 0xf) * 256) | buffer[2];
        int networkId = (buffer[3] * 256) | buffer[4];
        int versionNumber = (buffer[5] >> 1) & 0x1f;
        int currentNextIndicator = buffer[5] & 1;
        int sectionNumber = buffer[6];
        int lastSectionNumber = buffer[7];
        int networkDescriptorLength = ((buffer[8] & 0xf) * 256) | buffer[9];
        int l1 = networkDescriptorLength;
        int pointer = 10;
        int x = 0;

        while (l1 > 0)
        {
          int indicator = buffer[pointer];
          x = buffer[pointer + 1] + 2;

          //if (indicator == 0x40)
          //{
          //  string netWorkName = Encoding.GetEncoding("iso-8859-1").GetString(buffer, pointer + 2, x - 2);
          //}

          l1 -= x;
          pointer += x;
        }

        pointer = 10 + networkDescriptorLength;
        if (pointer > sectionLength)
          return;

        int transportStreamLoopLength = ((buffer[pointer] & 0xf) * 256) + buffer[pointer + 1];
        l1 = transportStreamLoopLength;
        pointer += 2;

        while (l1 > 0)
        {
          if ((pointer + 2 > sectionLength))
            return;

          int transportStreamId = (buffer[pointer] * 256) + buffer[pointer + 1];
          int originalNetworkId = (buffer[pointer + 2] * 256) + buffer[pointer + 3];
          int transportDescriptorLength = ((buffer[pointer + 4] & 0xf) * 256) + buffer[pointer + 5];
          pointer += 6;
          l1 -= 6;

          int l2 = transportDescriptorLength;
          while (l2 > 0)
          {
            if (pointer + 2 > sectionLength)
              return;

            int indicator = buffer[pointer];
            x = buffer[pointer + 1] + 2;
            // sat
            if (indicator == 0x43)
              DVB_GetSatDelivSys(buffer, pointer, x, originalNetworkId, transportStreamId);

            pointer += x;
            l2 -= x;
            l1 -= x;
          }
        }
      }
      catch (Exception ex)
      {
        OnMessage(string.Format("Error Parsing NIT - {0}\r\n{1}", ex.Message, ex.StackTrace), false);
      }
    }

    private void DVB_GetSatDelivSys(byte[] b, int pointer, int maxLen, int NetworkID, int TransportID)
    {
      if (b[pointer + 0] == 0x43 && maxLen >= 13)
      {
        int descriptor_tag = b[pointer + 0];
        int descriptor_length = b[pointer + 1];

        if ((descriptor_length > 13))
          return;
        NITSatDescriptor satteliteNIT = new NITSatDescriptor();
        satteliteNIT.TID = TransportID;
        satteliteNIT.Frequency = (100000000 * ((b[pointer + 2] >> 4) & 0xf));
        satteliteNIT.Frequency += (10000000 * (b[pointer + 2] & 0xf));
        satteliteNIT.Frequency += (1000000 * ((b[pointer + 3] >> 4) & 0xf));
        satteliteNIT.Frequency += (100000 * (b[pointer + 3] & 0xf));
        satteliteNIT.Frequency += (10000 * ((b[pointer + 4] >> 4) & 0xf));
        satteliteNIT.Frequency += (1000 * (b[pointer + 4] & 0xf));
        satteliteNIT.Frequency += (100 * ((b[pointer + 5] >> 4) & 0xf));
        satteliteNIT.Frequency += (10 * (b[pointer + 5] & 0xf));
        satteliteNIT.OrbitalPosition += (1000 * ((b[pointer + 6] >> 4) & 0xf));
        satteliteNIT.OrbitalPosition += (100 * ((b[pointer + 6] & 0xf)));
        satteliteNIT.OrbitalPosition += (10 * ((b[pointer + 7] >> 4) & 0xf));
        satteliteNIT.OrbitalPosition += (b[pointer + 7] & 0xf);

        satteliteNIT.WestEastFlag = (b[pointer + 8] & 0x80) >> 7;

        int Polarisation = (b[pointer + 8] & 0x60) >> 5;

        satteliteNIT.Polarisation = Polarisation + 1;
        satteliteNIT.IsS2 = (b[pointer + 8] & 0x4) >> 2;
        if (satteliteNIT.IsS2 > 0)
        {
          int rollOff = (b[pointer + 8] & 0x18) >> 3;
          switch (rollOff)
          {
            case 0:
              satteliteNIT.RollOff = 3;
              break;
            case 1:
              satteliteNIT.RollOff = 2;
              break;
            case 2:
              satteliteNIT.RollOff = 1;
              break;
          }
        }
        else
        {
          satteliteNIT.RollOff = -1;
        }

        satteliteNIT.Modulation = (b[pointer + 8] & 0x3);
        satteliteNIT.Symbolrate = (100000 * ((b[pointer + 9] >> 4) & 0xf));
        satteliteNIT.Symbolrate += (10000 * ((b[pointer + 9] & 0xf)));
        satteliteNIT.Symbolrate += (1000 * ((b[pointer + 10] >> 4) & 0xf));
        satteliteNIT.Symbolrate += (100 * ((b[pointer + 10] & 0xf)));
        satteliteNIT.Symbolrate += (10 * ((b[pointer + 11] >> 4) & 0xf));
        satteliteNIT.Symbolrate += (1 * ((b[pointer + 11] & 0xf)));

        int fec = (b[pointer + 12] & 0xf);
        switch (fec)
        {
          case 0:
            fec = 0;
            break;
          case 1:
            fec = 1;
            break;
          case 2:
            fec = 2;
            break;
          case 3:
            fec = 3;
            break;
          case 4:
            fec = 6;
            break;
          case 5:
            fec = 8;
            break;
          case 6:
            fec = 13;
            break;
          case 7:
            fec = 4;
            break;
          case 8:
            fec = 5;
            break;
          case 9:
            fec = 14;
            break;
          default:
            fec = 0;
            break;
        }

        satteliteNIT.FECInner = fec;
        if (!NITInfo.ContainsKey(TransportID))
        {
          NITInfo.Add(TransportID, satteliteNIT);
        }
        else
        {
          if (!GotAllTID)
            OnMessage("Got Network Information, " + NITInfo.Count + " transponders", false);
          GotAllTID = true;
          return;
        }
      }
    }

    public void OnTSPacket(int Pid, int Length, Custom_Data_Grabber.Section Data)
    {
      //  Try
      switch (Pid)
      {
        case 0x10:
          //NIT
          if (!GotAllTID)
            ParseNIT(Data, Length);
          break;
        case 0x11:
          //SDT/BAT
          ParseChannels(Data, Length);
          break;
        case 0x30:
        case 0x31:
        case 0x32:
        case 0x33:
        case 0x34:
        case 0x35:
        case 0x36:
        case 0x37:
          //OpenTV Titles
          OnTitleSectionReceived(Pid, Data);
          break;
        case 0x40:
        case 0x41:
        case 0x42:
        case 0x43:
        case 0x44:
        case 0x45:
        case 0x46:
        case 0x47:
          //OpenTV Sumarries
          OnSummarySectionReceived(Pid, Data);
          break;
      }

      if (IsEverythingGrabbed())
      {
        OnMessage("Everything Grabbed", false);
        Sky.SendComplete(0);
      }
    }

    private void OnTitleDecoded()
    {
      titlesDecoded++;
    }

    private void OnSummaryDecoded()
    {
      summariesDecoded++;
    }

    private void ParseSDT(Custom_Data_Grabber.Section data, int length)
    {
      if (GotAllSDT)
        return;

      try
      {
        byte[] section = data.Data;
        int transportId = ((section[3]) * 256) + section[4];
        long originalNetworkId = ((section[8]) * 256) + section[9];
        int len1 = length - 11 - 4;        
        int pointer = 11;
        int x = 0;

        while (len1 > 0)
        {
          int serviceId = (section[pointer] * 256) + section[pointer + 1];
          int eitScheduleFlag = (section[pointer + 2] >> 1) & 1;
          int eitPresentFollowingFlag = section[pointer + 2] & 1;
          int runningStatus = (section[pointer + 3] >> 5) & 7;
          int freeCAMode = (section[pointer + 3] >> 4) & 1;
          int descriptorsLoopLength = ((section[pointer + 3] & 0xf) * 256) + section[pointer + 4];
          int len2 = descriptorsLoopLength;
          pointer += 5;
          len1 -= 5;

          SDTInfo info = new SDTInfo();
          while (len2 > 0)
          {
            int indicator = section[pointer];
            x = section[pointer + 1] + 2;
            switch (indicator)
            {
              case 0x48:
                PopulateSDTInfo(section, pointer, info);
                if (string.IsNullOrEmpty(info.ChannelName))
                  info.ChannelName = "SID " + serviceId;
                info.SID = serviceId;
                info.IsFTA = freeCAMode > 0;
                break;
              case 0xB2:
                info.Category = (section[pointer + 4] & 1) != 1 ? section[pointer + 4] : section[pointer + 5];
                break;
            }

            len2 -= x;
            pointer += x;
            len1 -= x;

            //add sdt info
            string sdtInfoKey = originalNetworkId + "-" + transportId + "-" + serviceId;
            if (!SDTInfo.ContainsKey(sdtInfoKey))
            {
              SDTInfo.Add(sdtInfoKey, info);
              SDTCount++;
            }
            if (!GotAllSDT && AreAllBouquetsPopulated() && SDTCount >= Channels.Count)
            {
              GotAllSDT = true;
              OnMessage("Got All SDT Info, " + SDTInfo.Count + " Channels found", false);
            }
          }
        }
      }
      catch (Exception ex)
      {
        OnMessage(string.Format("Error Parsing SDT - {0}\r\n{1}", ex.Message, ex.StackTrace), false);
      }
    }

    protected void PopulateSDTInfo(byte[] b, int x, SDTInfo info)
    {
      int descriptorTag = b[x + 0];
      int descriptorLength = b[x + 1];

      if (b[x + 2] == 0x2)
      {
        //Radio Channel
        info.IsRadio = true;
        info.IsTV = false;
        info.IsHD = false;
        info.Is3D = false;
      }
      else
      {
        //TV Channel/3D/HD
        info.IsRadio = false;
        info.IsTV = true;
        info.IsHD = b[x + 2] == 0x19 || b[x + 2] == 0x11;
        info.Is3D = b[x + 2] >= 0x80 && b[x + 2] <= 0x84;
      }

      int serviceProviderNameLength = b[x + 3];
      int pointer = 4;
      info.Provider = GetString(b, pointer + x, serviceProviderNameLength);

      pointer += serviceProviderNameLength;
      int serviceNameLength = b[x + pointer];
      pointer += 1;
      info.ChannelName = GetString(b, pointer + x, serviceNameLength);
    }

    private string GetString(byte[] byteData, int offset, int length)
    {
      try
      {
        return DVBUtils.GetDVBString(byteData, offset, length);
      }
      catch (Exception ex)
      {
        OnMessage(string.Format("Invalid DVB text string: {0}", ex.Message), false);
        return string.Empty;
      }
    }

    private void ParseChannels(Custom_Data_Grabber.Section Data, int Length)
    {
      //If all bouquets are already fully populated, return
      try
      {
        if (Data.table_id != 0x4a)
        {
          if (!GotAllSDT && AreAllBouquetsPopulated() && (Data.table_id == 0x42 || Data.table_id == 0x46))
            ParseSDT(Data, Length);
          return;
        }

        if (AreAllBouquetsPopulated())
          return;

        byte[] buffer = Data.Data;
        int bouquetId = (buffer[3] * 256) + buffer[4];
        int bouquetDescriptorLength = ((buffer[8] & 0xf) * 256) + buffer[9];

        SkyBouquet skyBouquet = GetBouquet(bouquetId);
        if ((skyBouquet.IsPopulated))
          return;

        // If the bouquet is not initialized, this is the first time we have seen it
        if (!skyBouquet.IsInitialized)
        {
          skyBouquet.FirstReceivedSectionNumber = (byte)Data.section_number;
          skyBouquet.IsInitialized = true;
        }
        else if (Data.section_number == skyBouquet.FirstReceivedSectionNumber)
        {
          skyBouquet.IsPopulated = true;
          NotifyBouquetPopulated();
          return;
        }

        int body = 10 + bouquetDescriptorLength;
        int bouquetPayloadLength = ((buffer[body + 0] & 0xf) * 256) + buffer[body + 1];
        int endOfPacket = body + bouquetPayloadLength + 2;
        int currentTransportGroup = body + 2;

        while (currentTransportGroup < endOfPacket)
        {
          int transportId = (buffer[currentTransportGroup + 0] * 256) + buffer[currentTransportGroup + 1];
          int networkId = (buffer[currentTransportGroup + 2] * 256) + buffer[currentTransportGroup + 3];
          int transportGroupLength = ((buffer[currentTransportGroup + 4] & 0xf) * 256) + buffer[currentTransportGroup + 5];
          int currentTransportDescriptor = currentTransportGroup + 6;
          int endOfTransportGroupDescriptors = currentTransportDescriptor + transportGroupLength;

          while (currentTransportDescriptor < endOfTransportGroupDescriptors)
          {
            byte descriptorType = buffer[currentTransportDescriptor];
            int descriptorLength = buffer[currentTransportDescriptor + 1];
            int currentServiceDescriptor = currentTransportDescriptor + 2;
            int endOfServiceDescriptors = currentServiceDescriptor + descriptorLength - 2;

            if (descriptorType == 0xb1)
            {
              int RegionID = buffer[currentTransportDescriptor + 3];
              while (currentServiceDescriptor < endOfServiceDescriptors)
              {
                int serviceId = (buffer[currentServiceDescriptor + 2] * 256) + buffer[currentServiceDescriptor + 3];
                int channelId = (buffer[currentServiceDescriptor + 5] * 256) + buffer[currentServiceDescriptor + 6];
                int skyChannelNumber = (buffer[currentServiceDescriptor + 7] * 256) + buffer[currentServiceDescriptor + 8];
                SkyChannel skyChannel = GetChannel(channelId);
                LCNHolder SkyLCN = new LCNHolder(bouquetId, RegionID, skyChannelNumber);

                if (!skyChannel.IsPopulated)
                {
                  skyChannel.NID = networkId;
                  skyChannel.TID = transportId;
                  skyChannel.SID = serviceId;
                  skyChannel.ChannelID = channelId;

                  if (skyChannel.AddSkyLCN(SkyLCN))
                    skyChannel.IsPopulated = true;

                  UpdateChannel(skyChannel.ChannelID, skyChannel);
                }
                else
                {
                  skyChannel.AddSkyLCN(SkyLCN);
                  UpdateChannel(skyChannel.ChannelID, skyChannel);
                }
                currentServiceDescriptor += 9;
              }
            }
            currentTransportDescriptor += descriptorLength + 2;
          }
          currentTransportGroup += transportGroupLength + 6;
        }
      }
      catch (Exception ex)
      {
        OnMessage(string.Format("Error Parsing BAT - {0}\r\n{1}", ex.Message, ex.StackTrace), false);
      }
    }

    private void OnSummarySectionReceived(int pid, Custom_Data_Grabber.Section section)
    {
      try
      {
        //	If the summary data carousel is complete for this pid, we can discard the data as we already have it
        if (IsSummaryDataCarouselOnPidComplete(pid))
          return;

        //	Validate table id
        if (!DoesTidCarryEpgSummaryData(section.table_id))
          return;

        byte[] buffer = section.Data;

        //	Total length of summary data (2 less for this length field)
        int totalLength = (((buffer[1] & 0xf) * 256) + buffer[2]) - 2;

        //	If this section is a valid length (14 absolute minimum with 1 blank summary)
        if ((section.section_length < 14))
          return;

        //	Get the channel id that this section's summary data relates to
        int channelId = (buffer[3] * 256) + buffer[4];
        long mjdStartDate = (buffer[8] * 256) + buffer[9];

        //	Check channel id and start date are valid
        if ((channelId == 0 || mjdStartDate == 0))
          return;

        //	Always starts at 10th byte
        int currentSummaryItem = 10;
        int iterationCounter = 0;

        //	Loop while we have more summary data
        while (currentSummaryItem < totalLength)
        {
          if (iterationCounter > 512)
            return;

          iterationCounter++;

          //	Extract event id, header type and body length
          int eventId = (buffer[currentSummaryItem + 0] * 256) | buffer[currentSummaryItem + 1];
          byte headerType = (byte)((buffer[currentSummaryItem + 2] & 0xf0) >> 4);
          int bodyLength = ((buffer[currentSummaryItem + 2] & 0xf) * 256) | buffer[currentSummaryItem + 3];

          //	Build the carousel lookup id
          string carouselLookupId = channelId.ToString() + ":" + eventId.ToString();

          //	Notify the parser that a title has been received
          OnSummaryReceived(pid, carouselLookupId);

          //	If the summary carousel for this pid is now complete, we can return
          if (IsSummaryDataCarouselOnPidComplete(pid))
            return;

          //	Get the epg event we are to populate from the manager
          SkyEvent epgEvent = GetEpgEvent(channelId, eventId);
          //	Check we have the event reference
          if (epgEvent == null)
            return;

          int headerLength;
          //	If this is an extended header (&HF) (7 bytes long)
          if (headerType == 0xf)
            headerLength = 7;
          //Else if normal header (&HB) (4 bytes long)
          else if (headerType == 0xb)
            headerLength = 4;
          // Else other unknown header (not worked them out yet, at least 4 more)
          // Think these are only used for box office and adult channels so not really important
          else
            // Cannot parse the rest of this packet as we dont know the header lengths/format etc
            return;

          //	If body length is less than 3, there is no summary data for this event, move to next
          if (bodyLength < 3)
            currentSummaryItem += (headerLength + bodyLength);

          //	Move to the body of the summary
          int currentSummaryItemBody = currentSummaryItem + headerLength;

          //	Extract summary signature and huffman buffer length
          int summaryDescriptor = buffer[currentSummaryItemBody + 0];
          int encodedBufferLength = buffer[currentSummaryItemBody + 1];

          //	If normal summary item (&HB9)
          if (summaryDescriptor == 0xb9)
          {
            if (string.IsNullOrEmpty(epgEvent.Summary))
            {
              //epgEvent.summary = skyManager.DecodeHuffmanData(&buffer(currentSummaryItemBody + 2), encodedBufferLength)
              if (currentSummaryItemBody + 2 + encodedBufferLength > buffer.Length)
                return;

              // Decode the summary
              epgEvent.Summary = huffman.Decode(buffer, currentSummaryItemBody + 2, encodedBufferLength);

              //	If failed to decode

              //	Notify the manager (for statistics)
              OnSummaryDecoded();
              UpdateEPGEvent(ref channelId, epgEvent.EventID, epgEvent);
              //	Else if (&HBB) - Unknown data item (special box office or adult?)
              //	Seems very rare (1 in every 2000 or so), so not important really
            }
          }
          else if ((summaryDescriptor == 0xbb))
          {
            //	Else other unknown data item, there are a few others that are unknown
          }
          else
          {
            return;
            //skyManager.LogError("CSkyEpgSummaryDecoder::OnSummarySectionReceived() - Error, unrecognised summary descriptor")
          }

          //	Is there any footer information?
          int footerLength = bodyLength - encodedBufferLength - 2;

          if (footerLength >= 4)
          {
            int footerPointer = currentSummaryItemBody + 2 + encodedBufferLength;
            //	Get the descriptor
            int footerDescriptor = buffer[footerPointer + 0];
            //	If series id information (&HC1)
            if (footerDescriptor == 0xc1)
              epgEvent.SeriesID = (buffer[footerPointer + 2] * 256) + (buffer[footerPointer + 3]);
          }
          //	Move to next summary item
          currentSummaryItem += (bodyLength + headerLength);
        }

        //	Check the packet was parsed correctly - seem to get a few of these.  
        //	Seems to be some extra information tagged onto the end of some summary packets (1 in every 2000 or so)
        //	Not worked this out - possibly box office information
        if (currentSummaryItem != (totalLength + 1))
        {
          //skyManager.LogError("CSkyEpgSummaryDecoder::OnSummarySectionReceived() - Warning, summary packet was not parsed correctly - pointer not in expected place")
          return;
        }
      }
      catch (Exception err)
      {
        OnMessage("Error decoding Summary, " + err.Message, false);
      }
    }
  }
}