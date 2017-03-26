using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDXApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var window = new AppWindow())
                window.Run();
        }
    }
}
