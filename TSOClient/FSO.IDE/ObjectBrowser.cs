﻿using FSO.IDE.Common;
using FSO.SimAntics;
using FSO.SimAntics.NetPlay.Model.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSO.IDE
{
    public partial class ObjectBrowser : UserControl
    {
        private List<TreeNode> VisibleNodes;
        private Dictionary<TreeNode, ObjectRegistryEntry> SourceNodeToEnt;

        public string SelectedFile;
        public ObjectRegistryEntry SelectedObj;

        public delegate void ObjectSelChange();
        public event ObjectSelChange SelectedChanged;

        private VM vm;

        public ObjectBrowser()
        {
            InitializeComponent();
        }

        public void ObjectsModified()
        {
            ObjectRegistry.Init();
            RefreshTree();
        }

        public void RefreshTree()
        {
            ObjectTree.BeginUpdate();
            VisibleNodes = new List<TreeNode>();

            string[] searchTerms = (ObjectSearch.Text == "") ? null : ObjectSearch.Text.ToLowerInvariant().Split(' ');
            SourceNodeToEnt = new Dictionary<TreeNode, ObjectRegistryEntry>();
            ObjectTree.Nodes.Clear();

            var objects = ObjectRegistry.MastersByFilename;
            lock (objects)
            {
                foreach (var obj in objects)
                {
                    var filename = obj.Key;
                    var masters = obj.Value;
                    var fileNode = new TreeNode(filename);
                    bool fileAdded = false;

                    var nodes = new List<TreeNode>();
                    foreach (var master in masters)
                    {
                        //if the master matches the search, OR at least one child does, it appears.
                        int matches = 0;
                        var node = new TreeNode(master.Name);
                        SourceNodeToEnt.Add(node, master);
                        if (master.SearchMatch(searchTerms)) matches++;
                        foreach (var child in master.Children)
                        {
                            if (child.SearchMatch(searchTerms))
                            {
                                var cnode = new TreeNode(child.Name);
                                SourceNodeToEnt.Add(cnode, child);
                                node.Nodes.Add(cnode);
                                matches++;
                            }
                        }
                        if (matches > 0)
                        {
                            if (!fileAdded)
                            {
                                fileAdded = true;
                                ObjectTree.Nodes.Add(fileNode);
                                VisibleNodes.Add(fileNode);
                            }
                            fileNode.Nodes.Add(node);
                            VisibleNodes.Add(node);
                            foreach (var cnode in node.Nodes) VisibleNodes.Add((TreeNode)cnode);
                        }
                    }
                    fileNode.Expand();
                }
            }
            ObjectTree.EndUpdate();

            SearchDescribe.Text = (ObjectSearch.Text == "") ?
            "Showing all objects. ("+VisibleNodes.Count+" results)" :
            VisibleNodes.Count+" search results for '" + ObjectSearch.Text + "'.";
        }

        private void ObjectBrowser_Load(object sender, EventArgs e)
        {

        }
        private void ObjectTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            var node = ObjectTree.SelectedNode;

            if (node == null)
            {
                ObjNameLabel.Text = "No Object Selected.";
                ObjDescLabel.Text = "";
                ObjMultitileLabel.Text = "";
                SelectedFile = null;
                SelectedObj = null;
            }

            ObjectRegistryEntry entry = null;
            SourceNodeToEnt.TryGetValue(node, out entry);

            if (entry == null)
            {
                //chose a filename
                ObjNameLabel.Text = node.Text+".iff";
                ObjDescLabel.Text = "Object File";
                ObjMultitileLabel.Text = "Contains "+ObjectRegistry.MastersByFilename[node.Text].Count+" master objects.";
                SelectedFile = node.Text;
                SelectedObj = null;
                ObjThumbnail.ShowObject(0);
            }
            else
            {
                ObjNameLabel.Text = entry.Name;
                SelectedFile = entry.Filename;
                SelectedObj = entry;
                ObjThumbnail.ShowObject(entry.GUID);
                ObjDescLabel.Text = "§----";
                if (entry.Group == 0) {
                    ObjMultitileLabel.Text = "Single-tile object.";
                }
                else if (entry.SubIndex < 0)
                {
                    ObjMultitileLabel.Text = "Multitile master object.";
                }
                else
                {
                    ObjMultitileLabel.Text = "Multitile part. ("+(entry.SubIndex>>8)+", "+(entry.SubIndex&0xFF)+")";
                }
            }
            if (SelectedChanged != null) SelectedChanged();
        }

        private void ObjectSearch_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            RefreshTree();
        }

        private void ObjectSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchButton.PerformClick();
                e.SuppressKeyPress = true;
            }
        }
    }
}
