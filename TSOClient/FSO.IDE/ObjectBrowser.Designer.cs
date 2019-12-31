﻿namespace FSO.IDE
{
    partial class ObjectBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Accessory Rack - Cheap");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Accessory Rack - Expensive");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Accessory Rack - Moderate");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("accessoryrack", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Puzzle - 2 Person Portal - North");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Puzzle - 2 Person Portal - South");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Puzzle - 2 Person Portal - Tunnel");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Puzzle - 2 Person Portal", new System.Windows.Forms.TreeNode[] {
            treeNode5,
            treeNode6,
            treeNode7});
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("2 Person Portal Controller");
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("2personpuzzle", new System.Windows.Forms.TreeNode[] {
            treeNode8,
            treeNode9});
            this.ObjectSearch = new System.Windows.Forms.TextBox();
            this.ObjectTree = new System.Windows.Forms.TreeView();
            this.ObjNameLabel = new System.Windows.Forms.Label();
            this.ObjDescLabel = new System.Windows.Forms.Label();
            this.SearchButton = new System.Windows.Forms.Button();
            this.SearchDescribe = new System.Windows.Forms.Label();
            this.ObjMultitileLabel = new System.Windows.Forms.Label();
            this.ObjThumbnail = new FSO.IDE.Common.ObjThumbnailControl();
            this.SuspendLayout();
            // 
            // ObjectSearch
            // 
            this.ObjectSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ObjectSearch.Location = new System.Drawing.Point(12, 13);
            this.ObjectSearch.Name = "ObjectSearch";
            this.ObjectSearch.Size = new System.Drawing.Size(210, 20);
            this.ObjectSearch.TabIndex = 7;
            this.ObjectSearch.TextChanged += new System.EventHandler(this.ObjectSearch_TextChanged);
            this.ObjectSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ObjectSearch_KeyDown);
            // 
            // ObjectTree
            // 
            this.ObjectTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ObjectTree.FullRowSelect = true;
            this.ObjectTree.HideSelection = false;
            this.ObjectTree.HotTracking = true;
            this.ObjectTree.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.ObjectTree.Indent = 15;
            this.ObjectTree.ItemHeight = 16;
            this.ObjectTree.Location = new System.Drawing.Point(12, 39);
            this.ObjectTree.Name = "ObjectTree";
            treeNode1.Name = "Node2";
            treeNode1.Text = "Accessory Rack - Cheap";
            treeNode2.Name = "Node3";
            treeNode2.Text = "Accessory Rack - Expensive";
            treeNode3.Name = "Node4";
            treeNode3.Text = "Accessory Rack - Moderate";
            treeNode4.Name = "Node0";
            treeNode4.Text = "accessoryrack";
            treeNode5.Name = "Node7";
            treeNode5.Text = "Puzzle - 2 Person Portal - North";
            treeNode6.Name = "Node8";
            treeNode6.Text = "Puzzle - 2 Person Portal - South";
            treeNode7.Name = "Node9";
            treeNode7.Text = "Puzzle - 2 Person Portal - Tunnel";
            treeNode8.Name = "Node6";
            treeNode8.Text = "Puzzle - 2 Person Portal";
            treeNode9.Name = "Node10";
            treeNode9.Text = "2 Person Portal Controller";
            treeNode10.Name = "Node5";
            treeNode10.Text = "2personpuzzle";
            this.ObjectTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode4,
            treeNode10});
            this.ObjectTree.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ObjectTree.ShowRootLines = false;
            this.ObjectTree.Size = new System.Drawing.Size(272, 315);
            this.ObjectTree.TabIndex = 9;
            this.ObjectTree.TabStop = false;
            this.ObjectTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ObjectTree_AfterSelect);
            // 
            // ObjNameLabel
            // 
            this.ObjNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ObjNameLabel.AutoEllipsis = true;
            this.ObjNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ObjNameLabel.Location = new System.Drawing.Point(294, 204);
            this.ObjNameLabel.Name = "ObjNameLabel";
            this.ObjNameLabel.Size = new System.Drawing.Size(186, 17);
            this.ObjNameLabel.TabIndex = 12;
            this.ObjNameLabel.Text = "Accessory Rack - Cheap";
            this.ObjNameLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ObjDescLabel
            // 
            this.ObjDescLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ObjDescLabel.Location = new System.Drawing.Point(294, 222);
            this.ObjDescLabel.Name = "ObjDescLabel";
            this.ObjDescLabel.Size = new System.Drawing.Size(186, 17);
            this.ObjDescLabel.TabIndex = 14;
            this.ObjDescLabel.Text = "§2000 - Job Object";
            this.ObjDescLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // SearchButton
            // 
            this.SearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchButton.Location = new System.Drawing.Point(228, 11);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(56, 23);
            this.SearchButton.TabIndex = 15;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // SearchDescribe
            // 
            this.SearchDescribe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SearchDescribe.Location = new System.Drawing.Point(12, 357);
            this.SearchDescribe.Name = "SearchDescribe";
            this.SearchDescribe.Size = new System.Drawing.Size(234, 23);
            this.SearchDescribe.TabIndex = 16;
            this.SearchDescribe.Text = "Showing all objects.";
            // 
            // ObjMultitileLabel
            // 
            this.ObjMultitileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ObjMultitileLabel.Location = new System.Drawing.Point(294, 237);
            this.ObjMultitileLabel.Name = "ObjMultitileLabel";
            this.ObjMultitileLabel.Size = new System.Drawing.Size(186, 17);
            this.ObjMultitileLabel.TabIndex = 17;
            this.ObjMultitileLabel.Text = "Multitile Master Object";
            this.ObjMultitileLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ObjThumbnail
            // 
            this.ObjThumbnail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ObjThumbnail.Location = new System.Drawing.Point(294, 13);
            this.ObjThumbnail.Name = "ObjThumbnail";
            this.ObjThumbnail.Size = new System.Drawing.Size(186, 186);
            this.ObjThumbnail.TabIndex = 19;
            // 
            // ObjectBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ObjThumbnail);
            this.Controls.Add(this.ObjMultitileLabel);
            this.Controls.Add(this.SearchDescribe);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.ObjDescLabel);
            this.Controls.Add(this.ObjNameLabel);
            this.Controls.Add(this.ObjectSearch);
            this.Controls.Add(this.ObjectTree);
            this.Name = "ObjectBrowser";
            this.Size = new System.Drawing.Size(492, 382);
            this.Load += new System.EventHandler(this.ObjectBrowser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ObjectSearch;
        private System.Windows.Forms.TreeView ObjectTree;
        private System.Windows.Forms.Label ObjNameLabel;
        private System.Windows.Forms.Label ObjDescLabel;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.Label SearchDescribe;
        private System.Windows.Forms.Label ObjMultitileLabel;
        private Common.ObjThumbnailControl ObjThumbnail;
    }
}