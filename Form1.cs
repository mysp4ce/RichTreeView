using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ccontrol
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            richTreeView1.Font = new Font("Arial", 8);
            richTreeView1.Root.Add("hhh");
            richTreeView1.Root.Add("qqq");
            richTreeView1.Root.Add("wwww");
            richTreeView1.Root.Children[0].Add("child1");
            richTreeView1.Root.Children[0].Add("child2");
            richTreeView1.Root.Children[0].Add("child3");
            richTreeView1.Root.Children[1].Add("child1");
            richTreeView1.Root.Children[1].Add("child2");
            richTreeView1.Root.Children[1].Add("child3");
            richTreeView1.Root.Children[0].Children[0].Add("child1");
            richTreeView1.Root.Children[0].Children[0].Children[0].Add("child1");

            richTreeView1.Columns.Add("qwe", 123);
            //richTreeView1.Columns.Add("column1", 50);

            richTreeView1.Root.Children[0].Values = new object[] { "qweqwe", "4321", "zxczxc" };

            richTreeView1.Root.Children[0].Children[1].Values = new object[] { 1234, true, "qwer", null, null, "cxz" };
            richTreeView1.Root.Children[0].Children[2].Values = new object[] { "qwerty", "asd", "zxcvb" };
            richTreeView1.Root.Children[1].Children[1].Values = new object[] { false, false, "qweqwe", 4321, "zzzz" };
            richTreeView1.Root.Children[1].Children[0].Values = new object[] { "qweqwe", "4321", "zxczxc" };
            richTreeView1.Root.Children[0].Children[0].Children[0].Children[0].Values = new object[] { "zzz", new List<string>() { "qewr" }, 123, "ewqewq" };
        }
        
        int i = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            richTreeView1.Root.Children[1].IsHidden = richTreeView1.Root.Children[1].IsHidden ? false : true;
            //richTreeView1.Root.Children[0].Remove(0);
            i++;
            richTreeView1.Columns[i].Width = 100;
        }

        int z = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            z++;
            //richTreeView1.Columns.Add($"column{z.ToString()}", 20);
            MessageBox.Show(richTreeView1.Columns.Count.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < richTreeView1.Columns.Count; i++)
                MessageBox.Show(richTreeView1.Columns[i].Name);
        }
    }
}
