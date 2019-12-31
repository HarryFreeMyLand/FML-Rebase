﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using FSO.Client.GameContent;

namespace FSO.Client.Debug
{
    public partial class TSOClientFindAssetSearch : Form
    {
        private Thread thread;

        public TSOClientFindAssetSearch()
        {
            InitializeComponent();
        }


        public void StartSearch(string query)
        {
            lblLooking.Text = "Searching for: " + query;

            thread = new Thread(new ParameterizedThreadStart(DoSearch));
            thread.Start(query);
        }

        private void DoSearch(object queryObj)
        {
            var query = (string)queryObj;
        }
    }
}
