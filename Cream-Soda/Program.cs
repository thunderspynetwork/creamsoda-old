using System;
using System.Windows.Forms;

namespace CreamSoda
{
  internal static class Program
  {
    [STAThread]
    private static void Main(string[] args)
    {
      MyToolkit.args = args;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new CreamSoda());
    }
  }
}
