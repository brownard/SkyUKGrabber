using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SetupTv;
using TvLibrary.Log;
using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;
using SkyUK.Utils;

namespace SkyUK
{
  public partial class Setup : SectionSettings
  {
    SkyGrabber Grabber;
    Settings Settings = new Settings();
    Dictionary<int, Region> regions = new Dictionary<int, Region>();
    
    public Setup()
    {
      Grabber = new SkyGrabber();
      Grabber.ActivateControls += active;
      Grabber.Message += OnMessage;
      // This call is required by the designer.
      InitializeComponent();
      // Add any initialization after the InitializeComponent() call.

      LoadSetting();
      //    SaveSettings()
    }

    protected void InvokeIfRequired(Action action)
    {
      if (InvokeRequired)
        Invoke(action);
      else
        action();
    }

    private void AddLog(string Value, bool UpdateLast)
    {
      if (UpdateLast == true)
      {
        listViewStatus.Items[listViewStatus.Items.Count - 1].Text = Value;
      }
      else
      {
        listViewStatus.Items.Add(Value);
      }
      listViewStatus.Items[listViewStatus.Items.Count - 1].EnsureVisible();
    }

    protected void UpdateControlsEnabled(bool enabled)
    {
        Panel2.Enabled = enabled;
        skyUKContainer.Enabled = enabled;
        MpGroupBox1.Enabled = enabled;
        MpGroupBox2.Enabled = enabled;
        Panel3.Enabled = enabled;
        Button1.Enabled = enabled;
    }

    private void active()
    {
      try
      {
        InvokeIfRequired(() =>
        {
          UpdateControlsEnabled(true);
        });
      }
      catch
      {
      }
    }

    private void Button1_Click(System.Object sender, System.EventArgs e)
    {
      if (!Settings.IsGrabbing)
      {
        try
        {
          InvokeIfRequired(() =>
          {
            listViewStatus.Items.Clear();
            UpdateControlsEnabled(false);
          });
        }
        catch
        {
        }
        Grabber.Grab();
      }
    }

    private void OnMessage(string Message, bool UpdateLast)
    {
      try
      {
        InvokeIfRequired(() => AddLog(Message, UpdateLast));
        if (!UpdateLast == false)
          Log.Write("Sky Plugin : " + Message);
      }
      catch
      {
      }
    }

    private void Button4_Click(object sender, EventArgs e)
    {
      try
      {
        if (CatByte1.Text != "" & CatByte1.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte1.Text), CatText1.Text);
      }
      catch
      {
        CatByte1.Text = "";
      }

      try
      {
        if (CatByte2.Text != "" & CatByte2.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte2.Text), CatText2.Text);
      }
      catch
      {
        CatByte2.Text = "";
      }

      try
      {
        if (CatByte3.Text != "" & CatByte3.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte3.Text), CatText3.Text);
      }
      catch
      {
        CatByte3.Text = "";
      }
      try
      {
        if (CatByte4.Text != "" & CatByte4.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte4.Text), CatText4.Text);
      }
      catch
      {
        CatByte4.Text = "";
      }
      try
      {
        if (CatByte5.Text != "" & CatByte5.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte5.Text), CatText5.Text);
      }
      catch
      {
        CatByte5.Text = "";
      }
      try
      {
        if (CatByte6.Text != "" & CatByte6.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte6.Text), CatText6.Text);
      }
      catch
      {
        CatByte6.Text = "";
      }
      try
      {
        if (CatByte7.Text != "" & CatByte7.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte7.Text), CatText7.Text);
      }
      catch
      {
        CatByte7.Text = "";
      }
      try
      {
        if (CatByte8.Text != "" & CatByte8.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte8.Text), CatText8.Text);
      }
      catch
      {
        CatByte8.Text = "";
      }
      try
      {
        if (CatByte9.Text != "" & CatByte9.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte9.Text), CatText9.Text);
      }
      catch
      {
        CatByte9.Text = "";
      }
      try
      {
        if (CatByte10.Text != "" & CatByte10.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte10.Text), CatText10.Text);
      }
      catch
      {
        CatByte10.Text = "";
      }
      try
      {
        if (CatByte11.Text != "" & CatByte11.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte11.Text), CatText11.Text);
      }
      catch
      {
        CatByte11.Text = "";
      }
      try
      {
        if (CatByte12.Text != "" & CatByte12.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte12.Text), CatText12.Text);
      }
      catch
      {
        CatByte12.Text = "";
      }
      try
      {
        if (CatByte13.Text != "" & CatByte13.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte13.Text), CatText13.Text);
      }
      catch
      {
        CatByte13.Text = "";
      }
      try
      {
        if (CatByte14.Text != "" & CatByte14.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte14.Text), CatText14.Text);
      }
      catch
      {
        CatByte14.Text = "";
      }
      try
      {
        if (CatByte15.Text != "" & CatByte15.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte15.Text), CatText15.Text);
      }
      catch
      {
        CatByte15.Text = "";
      }
      try
      {
        if (CatByte16.Text != "" & CatByte16.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte16.Text), CatText16.Text);
      }
      catch
      {
        CatByte16.Text = "";
      }
      try
      {
        if (CatByte17.Text != "" & CatByte17.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte17.Text), CatText17.Text);
      }
      catch
      {
        CatByte17.Text = "";
      }
      try
      {
        if (CatByte18.Text != "" & CatByte18.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte18.Text), CatText18.Text);
      }
      catch
      {
        CatByte18.Text = "";
      }
      try
      {
        if (CatByte19.Text != "" & CatByte19.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte19.Text), CatText19.Text);
      }
      catch
      {
        CatByte19.Text = "";
      }
      try
      {
        if (CatByte20.Text != "" & CatByte20.Text != "0")
          Settings.SetCategory(Convert.ToByte(CatByte20.Text), CatText20.Text);
      }
      catch
      {
        CatByte20.Text = "";
      }
      SaveCatSettings();
    }

    private void LoadSetting()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      int id = -1;
      int checker = 0;
      foreach (Card card_1 in Card.ListAll())
      {
        if (RemoteControl.Instance.Type(card_1.IdCard) == CardType.DvbS)
        {
          ChannelMap.Items.Add(card_1.Name);
        }
      }
      Settings.IsLoading = true;
      List<int> listofmap = Settings.CardMap;
      if (listofmap.Count > 0 & ChannelMap.Items.Count > 0)
      {
        foreach (int num in listofmap)
        {
          try
          {
            ChannelMap.SetItemChecked(num, true);
          }
          catch (Exception ex)
          {
          }
        }
      }
      Settings.IsLoading = false;
      layer = null;

      List<string> ttt = new List<string>();
      string[] lines = SkyUtils.REGIONS.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string str in lines)
      {
        ttt.Add(str);
      }

      int trt = -1;
      foreach (string yt in ttt)
      {
        trt += 1;
        string[] split = new string[1];
        string[] Split2 = new string[1];
        split = yt.Split('=');
        SkyUK_Region.Items.Add(split[1]);
        Split2 = split[0].Split('-');
        Region bad = new Region();
        bad.BouquetID = Convert.ToInt32(Split2[0]);
        bad.RegionID = Convert.ToInt32(Split2[1]);
        regions.Add(trt, bad);
      }

      if (Settings.GetSkySetting(CatByte1.Name, CatByte1.Text) == "-1")
      {
        CatByte1.Text = "";
        CatText1.Text = "";
      }
      else
      {
        CatByte1.Text = Settings.GetSkySetting(CatByte1.Name, "-1");
        CatText1.Text = Settings.GetSkySetting(CatText1.Name, CatText1.Text);
      }

      if (Settings.GetSkySetting(CatByte2.Name, CatByte2.Text) == "-1")
      {
        CatByte2.Text = "";
        CatText2.Text = "";
      }
      else
      {
        CatByte2.Text = Settings.GetSkySetting(CatByte2.Name, "-1");
        CatText2.Text = Settings.GetSkySetting(CatText2.Name, CatText2.Text);
      }


      if (Settings.GetSkySetting(CatByte3.Name, CatByte3.Text) == "-1")
      {
        CatByte3.Text = "";
        CatText3.Text = "";
      }
      else
      {
        CatByte3.Text = Settings.GetSkySetting(CatByte3.Name, "-1");
        CatText3.Text = Settings.GetSkySetting(CatText3.Name, CatText3.Text);
      }
      if (Settings.GetSkySetting(CatByte4.Name, CatByte4.Text) == "-1")
      {
        CatByte4.Text = "";
        CatText4.Text = "";
      }
      else
      {
        CatByte4.Text = Settings.GetSkySetting(CatByte4.Name, "-1");
        CatText4.Text = Settings.GetSkySetting(CatText4.Name, CatText4.Text);
      }

      if (Settings.GetSkySetting(CatByte5.Name, CatByte5.Text) == "-1")
      {
        CatByte5.Text = "";
        CatText5.Text = "";
      }
      else
      {
        CatByte5.Text = Settings.GetSkySetting(CatByte5.Name, "-1");
        CatText5.Text = Settings.GetSkySetting(CatText5.Name, CatText5.Text);
      }

      if (Settings.GetSkySetting(CatByte6.Name, CatByte6.Text) == "-1")
      {
        CatByte6.Text = "";
        CatText6.Text = "";
      }
      else
      {
        CatByte6.Text = Settings.GetSkySetting(CatByte6.Name, "-1");
        CatText6.Text = Settings.GetSkySetting(CatText6.Name, CatText6.Text);
      }

      if (Settings.GetSkySetting(CatByte7.Name, CatByte7.Text) == "-1")
      {
        CatByte7.Text = "";
        CatText7.Text = "";
      }
      else
      {
        CatByte7.Text = Settings.GetSkySetting(CatByte7.Name, "-1");
        CatText7.Text = Settings.GetSkySetting(CatText7.Name, CatText7.Text);
      }

      if (Settings.GetSkySetting(CatByte8.Name, CatByte8.Text) == "-1")
      {
        CatByte8.Text = "";
        CatText8.Text = "";
      }
      else
      {
        CatByte8.Text = Settings.GetSkySetting(CatByte8.Name, "-1");
        CatText8.Text = Settings.GetSkySetting(CatText8.Name, CatText8.Text);
      }

      if (Settings.GetSkySetting(CatByte9.Name, CatByte9.Text) == "-1")
      {
        CatByte9.Text = "";
        CatText9.Text = "";
      }
      else
      {
        CatByte9.Text = Settings.GetSkySetting(CatByte9.Name, "-1");
        CatText9.Text = Settings.GetSkySetting(CatText9.Name, CatText9.Text);
      }

      if (Settings.GetSkySetting(CatByte10.Name, CatByte10.Text) == "-1")
      {
        CatByte10.Text = "";
        CatText10.Text = "";
      }
      else
      {
        CatByte10.Text = Settings.GetSkySetting(CatByte10.Name, "-1");
        CatText10.Text = Settings.GetSkySetting(CatText10.Name, CatText10.Text);
      }

      if (Settings.GetSkySetting(CatByte11.Name, CatByte11.Text) == "-1")
      {
        CatByte11.Text = "";
        CatText11.Text = "";
      }
      else
      {
        CatByte11.Text = Settings.GetSkySetting(CatByte11.Name, "-1");
        CatText11.Text = Settings.GetSkySetting(CatText11.Name, CatText11.Text);
      }

      if (Settings.GetSkySetting(CatByte12.Name, CatByte12.Text) == "-1")
      {
        CatByte12.Text = "";
        CatText12.Text = "";
      }
      else
      {
        CatByte12.Text = Settings.GetSkySetting(CatByte12.Name, "-1");
        CatText12.Text = Settings.GetSkySetting(CatText12.Name, CatText12.Text);
      }

      if (Settings.GetSkySetting(CatByte13.Name, CatByte13.Text) == "-1")
      {
        CatByte13.Text = "";
        CatText13.Text = "";
      }
      else
      {
        CatByte13.Text = Settings.GetSkySetting(CatByte13.Name, "-1");
        CatText13.Text = Settings.GetSkySetting(CatText13.Name, CatText13.Text);
      }

      if (Settings.GetSkySetting(CatByte14.Name, CatByte14.Text) == "-1")
      {
        CatByte14.Text = "";
        CatText14.Text = "";
      }
      else
      {
        CatByte14.Text = Settings.GetSkySetting(CatByte14.Name, "-1");
        CatText14.Text = Settings.GetSkySetting(CatText14.Name, CatText14.Text);
      }

      if (Settings.GetSkySetting(CatByte15.Name, CatByte15.Text) == "-1")
      {
        CatByte15.Text = "";
        CatText15.Text = "";
      }
      else
      {
        CatByte15.Text = Settings.GetSkySetting(CatByte15.Name, "-1");
        CatText15.Text = Settings.GetSkySetting(CatText15.Name, CatText15.Text);
      }

      if (Settings.GetSkySetting(CatByte16.Name, CatByte16.Text) == "-1")
      {
        CatByte16.Text = "";
        CatText16.Text = "";
      }
      else
      {
        CatByte16.Text = Settings.GetSkySetting(CatByte16.Name, "-1");
        CatText16.Text = Settings.GetSkySetting(CatText16.Name, CatText16.Text);
      }

      if (Settings.GetSkySetting(CatByte17.Name, CatByte17.Text) == "-1")
      {
        CatByte17.Text = "";
        CatText17.Text = "";
      }
      else
      {
        CatByte17.Text = Settings.GetSkySetting(CatByte17.Name, "-1");
        CatText17.Text = Settings.GetSkySetting(CatText17.Name, CatText17.Text);
      }

      if (Settings.GetSkySetting(CatByte18.Name, CatByte18.Text) == "-1")
      {
        CatByte18.Text = "";
        CatText18.Text = "";
      }
      else
      {
        CatByte18.Text = Settings.GetSkySetting(CatByte18.Name, "-1");
        CatText18.Text = Settings.GetSkySetting(CatText18.Name, CatText18.Text);
      }

      if (Settings.GetSkySetting(CatByte19.Name, CatByte19.Text) == "-1")
      {
        CatByte19.Text = "";
        CatText19.Text = "";
      }
      else
      {
        CatByte19.Text = Settings.GetSkySetting(CatByte19.Name, "-1");
        CatText19.Text = Settings.GetSkySetting(CatText19.Name, CatText19.Text);
      }

      if (Settings.GetSkySetting(CatByte20.Name, CatByte20.Text) == "-1")
      {
        CatByte20.Text = "";
        CatText20.Text = "";
      }
      else
      {
        CatByte20.Text = Settings.GetSkySetting(CatByte20.Name, "-1");
        CatText20.Text = Settings.GetSkySetting(CatText20.Name, CatText20.Text);
      }
      TextBox6.Text = Settings.frequency.ToString();
      chk_AutoUpdate.Checked = Settings.UpdateChannels;
      chk_SkyNumbers.Checked = Settings.UseSkyNumbers;
      chk_SkyCategories.Checked = Settings.UseSkyCategories;
      chk_SkyRegions.Checked = Settings.UseSkyRegions;
      chk_DeleteOld.Checked = Settings.DeleteOldChannels;
      chk_MoveOld.Checked = !Settings.DeleteOldChannels;
      CheckBox1.Checked = Settings.ReplaceSDwithHD;
      CheckBox2.Checked = Settings.UpdateEPG;
      useThrottleCheckBox.Checked = Settings.UseThrottle;
      updateLogosCheckBox.Checked = Settings.UpdateLogos;
      logoDirectoryTextBox.Text = Settings.LogoDirectory;

      //TextBox1.Text = Settings.SwitchingFrequency
      txt_Move_Old_Group.Text = Settings.OldChannelFolder;
      SkyUK_Region.SelectedIndex = Settings.RegionIndex;
      Settings.RegionIndex = SkyUK_Region.SelectedIndex;
      TextBox10.Text = Settings.SymbolRate.ToString();
      TextBox5.Text = Settings.TransportID.ToString();
      TextBox4.Text = Settings.ServiceID.ToString();
      mpDisEqc1.SelectedIndex = Settings.DiseqC;
      if (mpDisEqc1.SelectedIndex == -1)
      {
        mpDisEqc1.SelectedIndex = 0;
        Settings.DiseqC = 0;
      }
      MpComboBox2.SelectedIndex = Settings.polarisation;
      if (MpComboBox2.SelectedIndex == -1)
      {
        MpComboBox2.SelectedIndex = 0;
        Settings.polarisation = 0;
      }
      MpComboBox1.SelectedIndex = Settings.modulation;
      if (MpComboBox1.SelectedIndex == -1)
      {
        MpComboBox1.SelectedIndex = 0;
        Settings.modulation = 0;
      }
      TextBox6.Text = Settings.frequency.ToString();
      CheckBox4.Checked = Settings.AutoUpdate;
      CheckBox5.Checked = Settings.EveryHour;
      CheckBox6.Checked = Settings.OnDaysAt;
      if (Settings.UpdateInterval == 0)
      {
        Settings.UpdateInterval = 1;
      }
      CheckBox3.Checked = Settings.useExtraInfo;
      CheckBox7.Checked = Settings.UseNotSetModSD;
      CheckBox9.Checked = Settings.UseNotSetModHD;
      CheckBox8.Checked = Settings.IgnoreScrambled;

      NumericUpDown2.Value = Settings.GrabTime;
      NumericUpDown1.Value = Settings.UpdateInterval;
      Panel1.Visible = Settings.AutoUpdate;
      DateTimePicker1.Value = Settings.UpdateTime;

      Mon.Checked = Settings.Mon;
      Tue.Checked = Settings.Tue;
      Wed.Checked = Settings.Wed;
      Thu.Checked = Settings.Thu;
      Fri.Checked = Settings.Fri;
      Sat.Checked = Settings.Sat;
      Sun.Checked = Settings.Sun;

      if (CatByte1.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte1.Text), CatText1.Text);
      if (CatByte2.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte2.Text), CatText2.Text);
      if (CatByte3.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte3.Text), CatText3.Text);
      if (CatByte8.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte8.Text), CatText8.Text);
      if (CatByte4.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte4.Text), CatText4.Text);
      if (CatByte5.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte5.Text), CatText5.Text);
      if (CatByte6.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte6.Text), CatText6.Text);
      if (CatByte10.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte10.Text), CatText10.Text);
      if (CatByte7.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte7.Text), CatText7.Text);
      if (CatByte9.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte9.Text), CatText9.Text);
      if (CatByte11.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte11.Text), CatText11.Text);
      if (CatByte12.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte12.Text), CatText12.Text);
      if (CatByte13.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte13.Text), CatText13.Text);
      if (CatByte14.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte14.Text), CatText14.Text);
      if (CatByte15.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte15.Text), CatText15.Text);
      if (CatByte16.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte16.Text), CatText16.Text);
      if (CatByte17.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte17.Text), CatText17.Text);
      if (CatByte18.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte18.Text), CatText18.Text);
      if (CatByte19.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte19.Text), CatText19.Text);
      if (CatByte20.Text != "")
        Settings.SetCategory(Convert.ToByte(CatByte11.Text), CatText20.Text);
    }


    private void SaveCatSettings()
    {
      string Temp;
      string Name;
      string Temo;
      string Namo;

      Temp = CatByte1.Text;
      Name = CatByte1.Name;
      Temo = CatText1.Text;
      Namo = CatText1.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }

      Temp = CatByte2.Text;
      Name = CatByte2.Name;
      Temo = CatText2.Text;
      Namo = CatText2.Name;

      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte3.Text;
      Name = CatByte3.Name;
      Temo = CatText3.Text;
      Namo = CatText3.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }


      Temp = CatByte4.Text;
      Name = CatByte4.Name;
      Temo = CatText4.Text;
      Namo = CatText4.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }

      Temp = CatByte5.Text;
      Name = CatByte5.Name;
      Temo = CatText5.Text;
      Namo = CatText5.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte6.Text;
      Name = CatByte6.Name;
      Temo = CatText6.Text;
      Namo = CatText6.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }


      Temp = CatByte7.Text;
      Name = CatByte7.Name;
      Temo = CatText7.Text;
      Namo = CatText7.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte8.Text;
      Name = CatByte8.Name;
      Temo = CatText8.Text;
      Namo = CatText8.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte9.Text;
      Name = CatByte9.Name;
      Temo = CatText9.Text;
      Namo = CatText9.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte10.Text;
      Name = CatByte10.Name;
      Temo = CatText10.Text;
      Namo = CatText10.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte11.Text;
      Name = CatByte11.Name;
      Temo = CatText11.Text;
      Namo = CatText11.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }

      Temp = CatByte12.Text;
      Name = CatByte12.Name;
      Temo = CatText12.Text;
      Namo = CatText12.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte13.Text;
      Name = CatByte13.Name;
      Temo = CatText13.Text;
      Namo = CatText13.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }

      Temp = CatByte14.Text;
      Name = CatByte14.Name;
      Temo = CatText14.Text;
      Namo = CatText14.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte15.Text;
      Name = CatByte15.Name;
      Temo = CatText15.Text;
      Namo = CatText15.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }

      Temp = CatByte16.Text;
      Name = CatByte16.Name;
      Temo = CatText16.Text;
      Namo = CatText16.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte17.Text;
      Name = CatByte17.Name;
      Temo = CatText17.Text;
      Namo = CatText17.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }

      Temp = CatByte18.Text;
      Name = CatByte18.Name;
      Temo = CatText18.Text;
      Namo = CatText18.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
      Temp = CatByte19.Text;
      Name = CatByte19.Name;
      Temo = CatText19.Text;
      Namo = CatText19.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }

      Temp = CatByte20.Text;
      Name = CatByte20.Name;
      Temo = CatText20.Text;
      Namo = CatText20.Name;
      Settings.UpdateSetting(Namo, Temo);
      if (Temp == "")
      {
        Settings.UpdateSetting(Name, "-1");
      }
      else
      {
        Settings.UpdateSetting(Name, Temp);
      }
    }

    private void SkyUK_Region_SelectedIndexChanged(System.Object sender, System.EventArgs e)
    {
      Region region = regions[SkyUK_Region.SelectedIndex];
      Settings.RegionID = region.RegionID;
      Settings.BouquetID = region.BouquetID;
      Settings.RegionIndex = SkyUK_Region.SelectedIndex;
    }

    private void TextBox10_TextChanged(System.Object sender, System.EventArgs e)
    {
      Settings.SymbolRate = Convert.ToInt32(TextBox10.Text);
    }

    private void TextBox5_TextChanged(System.Object sender, System.EventArgs e)
    {
      Settings.TransportID = Convert.ToInt32(TextBox5.Text);

    }

    private void TextBox4_TextChanged(System.Object sender, System.EventArgs e)
    {
      Settings.ServiceID = Convert.ToInt32(TextBox4.Text);
    }

    private void MpComboBox1_SelectedIndexChanged(System.Object sender, System.EventArgs e)
    {
      Settings.modulation = MpComboBox1.SelectedIndex;
    }

    private void mpDisEqc1_SelectedIndexChanged(System.Object sender, System.EventArgs e)
    {
      Settings.DiseqC = mpDisEqc1.SelectedIndex;
    }

    private void MpComboBox2_SelectedIndexChanged(System.Object sender, System.EventArgs e)
    {
      Settings.polarisation = MpComboBox2.SelectedIndex;
    }

    private void TextBox6_TextChanged(System.Object sender, System.EventArgs e)
    {
      Settings.frequency = Convert.ToInt32(TextBox6.Text);
    }

    private void CheckBox4_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.AutoUpdate = CheckBox4.Checked;
      Panel1.Visible = Settings.AutoUpdate;
    }

    private void CheckBox5_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.EveryHour = CheckBox5.Checked;
      Settings.OnDaysAt = !CheckBox5.Checked;
      CheckBox6.Checked = !CheckBox5.Checked;
      Mon.Enabled = !CheckBox5.Checked;
      Tue.Enabled = !CheckBox5.Checked;
      Wed.Enabled = !CheckBox5.Checked;
      Thu.Enabled = !CheckBox5.Checked;
      Fri.Enabled = !CheckBox5.Checked;
      Sat.Enabled = !CheckBox5.Checked;
      Sun.Enabled = !CheckBox5.Checked;
      DateTimePicker1.Enabled = !CheckBox5.Checked;
      NumericUpDown1.Enabled = CheckBox5.Checked;
    }

    private void CheckBox6_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.OnDaysAt = CheckBox6.Checked;
      Settings.EveryHour = !CheckBox6.Checked;
      CheckBox5.Checked = !CheckBox6.Checked;
      Mon.Enabled = !CheckBox5.Checked;
      Tue.Enabled = !CheckBox5.Checked;
      Wed.Enabled = !CheckBox5.Checked;
      Thu.Enabled = !CheckBox5.Checked;
      Fri.Enabled = !CheckBox5.Checked;
      Sat.Enabled = !CheckBox5.Checked;
      Sun.Enabled = !CheckBox5.Checked;
      DateTimePicker1.Enabled = !CheckBox5.Checked;
      NumericUpDown1.Enabled = CheckBox5.Checked;
    }

    private void Mon_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.Mon = Mon.Checked;
    }

    private void Tue_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.Tue = Tue.Checked;
    }

    private void Wed_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.Wed = Wed.Checked;
    }

    private void Thu_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.Thu = Thu.Checked;
    }

    private void Fri_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.Fri = Fri.Checked;
    }

    private void Sat_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.Sat = Sat.Checked;
    }

    private void Sun_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.Sun = Sun.Checked;
    }

    private void NumericUpDown1_ValueChanged(System.Object sender, System.EventArgs e)
    {
      Settings.UpdateInterval = (int)NumericUpDown1.Value;
    }

    private void DateTimePicker1_ValueChanged(System.Object sender, System.EventArgs e)
    {
      Settings.UpdateTime = DateTimePicker1.Value;
    }

    private void ChannelMap_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
      if (Settings.IsLoading == false)
      {
        List<int> listofmap = new List<int>();
        if (ChannelMap.Items.Count > 0)
        {
          for (int a = 0; a <= ChannelMap.Items.Count - 1; a++)
          {
            try
            {
              //   MsgBox(e.Index & " : " & e.NewValue)
              if (e.Index == a)
              {
                if ((e.NewValue == System.Windows.Forms.CheckState.Checked))
                {
                  listofmap.Add(a);
                }
              }
              else
              {
                if (ChannelMap.GetItemChecked(a))
                {
                  listofmap.Add(a);
                }
              }
            }
            catch (Exception ex)
            {
            }
          }
        }
        Settings.CardMap = listofmap;
      }
    }

    private void NumericUpDown2_ValueChanged(System.Object sender, System.EventArgs e)
    {
      Settings.GrabTime = (int)NumericUpDown2.Value;
    }

    private void txt_Move_Old_Group_TextChanged(System.Object sender, System.EventArgs e)
    {
      Settings.OldChannelFolder = txt_Move_Old_Group.Text;
    }

    private void CheckBox8_CheckedChanged_1(System.Object sender, System.EventArgs e)
    {
      Settings.IgnoreScrambled = CheckBox8.Checked;
    }

    private void CheckBox7_CheckedChanged_1(System.Object sender, System.EventArgs e)
    {
      Settings.UseNotSetModSD = CheckBox7.Checked;
    }

    private void chk_AutoUpdate_CheckedChanged_1(System.Object sender, System.EventArgs e)
    {
      Settings.UpdateChannels = chk_AutoUpdate.Checked;
    }

    private void CheckBox2_CheckedChanged_1(System.Object sender, System.EventArgs e)
    {
      Settings.UpdateEPG = CheckBox2.Checked;
    }

    private void chk_SkyNumbers_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.UseSkyNumbers = chk_SkyNumbers.Checked;
    }

    private void chk_SkyCategories_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.UseSkyCategories = chk_SkyCategories.Checked;
    }

    private void chk_SkyRegions_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.UseSkyRegions = chk_SkyRegions.Checked;
    }

    private void CheckBox1_CheckedChanged_1(System.Object sender, System.EventArgs e)
    {
      Settings.ReplaceSDwithHD = CheckBox1.Checked;
    }

    private void CheckBox3_CheckedChanged_1(System.Object sender, System.EventArgs e)
    {
      Settings.useExtraInfo = CheckBox3.Checked;
    }

    private void chk_DeleteOld_CheckedChanged_1(System.Object sender, System.EventArgs e)
    {
      if (chk_DeleteOld.Checked)
      {
        chk_MoveOld.Checked = false;
        Settings.DeleteOldChannels = true;
      }
    }

    private void chk_MoveOld_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      if (chk_MoveOld.Checked)
      {
        chk_DeleteOld.Checked = false;
        Settings.DeleteOldChannels = false;
      }
    }

    private void CheckBox9_CheckedChanged(System.Object sender, System.EventArgs e)
    {
      Settings.UseNotSetModHD = CheckBox9.Checked;
    }

    private void useThrottleCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      Settings.UseThrottle = useThrottleCheckBox.Checked;
    }

    private void updateLogosCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      Settings.UpdateLogos = updateLogosCheckBox.Checked;
    }

    private void browseLogoDirectoryButton_Click(object sender, EventArgs e)
    {
      logoFolderDialog.SelectedPath = Settings.LogoDirectory;
      logoFolderDialog.ShowNewFolderButton = false;
      logoFolderDialog.Description = "Select Channel Logos Directory";
      DialogResult result = logoFolderDialog.ShowDialog();
      if (result == DialogResult.OK)
      {
        Settings.LogoDirectory = logoFolderDialog.SelectedPath;
        logoDirectoryTextBox.Text = logoFolderDialog.SelectedPath;
      }
    }
  }
}
