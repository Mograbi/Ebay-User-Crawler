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
    public class ProgressWrapper
    {
        private Control bar;
        private int current = 1;
        private int sum;
        public ProgressWrapper(Control bar, int sum)
        {
            this.bar = bar;
            this.sum = sum;
        }
        public void Advance()
        {
            if (bar != null && current <= sum)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    (this.bar as ProgressBar).Value += (1);
                });
                this.current++;
            }
        }
    }
    public class TextBlockWrapper
    {
        private Control block;
        public TextBlockWrapper(Control textBlock)
        {
            this.block = textBlock;
        }
        public void print(string value)
        {
            if (block != null)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
                {
                    //(block as TextBlock).Text = value;
                });
            }
        }
    }
}
