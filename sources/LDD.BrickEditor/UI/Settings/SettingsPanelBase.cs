﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LDD.Common.Utilities;
using LDD.BrickEditor.Settings;

namespace LDD.BrickEditor.UI.Settings
{
    public partial class SettingsPanelBase : UserControl
    {
        protected FlagManager FlagManager { get; }

        public bool HasSettingsChanged { get; protected set; }

        [Localizable(true)]
        public string Title
        {
            get => base.Text;
            set => base.Text = value;
        }

        public SettingsPanelBase()
        {
            InitializeComponent();
            FlagManager = new FlagManager();
        }

        protected virtual void FillSettingsCore(AppSettings settings)
        {

        }

        public void FillSettings(AppSettings settings)
        {
            using (FlagManager.UseFlag(nameof(FillSettings)))
                FillSettingsCore(settings);
        }

        public virtual bool SaveSettings()
        {
            return true;
        }

        public virtual bool ValidateSettings()
        {
            return true;
        }

        public virtual void ApplySettings(AppSettings settings)
        {

        }

        //protected virtual void ReloadSettings()
        //{

        //}

        //protected virtual bool IsPendingSave()
        //{
        //    return false;
        //}
    }
}
