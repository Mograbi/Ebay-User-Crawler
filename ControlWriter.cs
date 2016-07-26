using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace EbayCrawlerWPF_2
{
    public class MyItem
    {
        public int Id { get; set; }

        public string Comment { get; set; }
    }
    public class ControlWriter : System.IO.TextWriter
    {
        private Control listview;
        private int count = 1;
        public ControlWriter(Control listview)
        {
            this.listview = listview;
        }

        public override void Write(char value)
        {
            (listview as TextBox).Text += value;
        }

        public override void Write(string value)
        {
            //string toWrite = (count + ". " + value + "\n--------------------------\n");
            if (listview != null) {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    (this.listview as ListView).Items.Add(new MyItem { Id = this.count, Comment = value });
                });
                this.count++;
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
