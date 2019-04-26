using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace custcontrol
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RichTreeViewItem item = new RichTreeViewItem();
            //item.ItemsChanged += Item_ItemsChanged;

            item.Add("name");
            item.Add("name2");
            item.Remove(1);
            item.Edit(0, "pfaf");
            item.Add("Addeditem");
            item.Edit(1, "newAddedItem");
            
            item.Children.Add(new RichTreeViewItem());
            item.Children[0].Text = "childdd";
            item.Children[1].Text = "3214";
            item.Children[2].Text = "3214";

            treeView1.Nodes.Add("123"); treeView1.Nodes.Add("123"); treeView1.Nodes.Add("123");
            treeView1.Font = new Font("Calibri", 14);
            
            richTreeView2.Nodes.Add("hellow");
            richTreeView2.Nodes.Add(new RichTreeViewItem() { Text = "rofl" });
            richTreeView2.Nodes[0].Add("child1");
            richTreeView2.Nodes[0].Add("child23");
            richTreeView2.Nodes[1].Add("child3");
            richTreeView2.Nodes[0].Children[0].Add("child1");
            richTreeView2.Nodes[0].Children[0].Add("child2");

            richTreeView2.Nodes[0].Children[1].Values = new object[] { 55, 444, "hhhh", "rofl", "hi" };
            richTreeView2.Nodes[1].Values = new object[] { 1, 2, "hello", 475, 222 };
            richTreeView2.Nodes[0].Values = new object[] { 1, 2, "hello", 475, 333 };
            richTreeView2.Nodes[1].Add("child2");

            foreach (RichTreeViewItem it in richTreeView2.Nodes)
                MessageBox.Show(it.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTreeView2.Nodes[0].Children.RemoveAt(0);
        }

        //private void Item_ItemsChanged(object sender, EventArgs e)
        //{
        //    MessageBox.Show((sender as RichTreeViewItem).Text);
        //}
    }
}
