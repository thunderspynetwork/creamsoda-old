using System;

namespace CreamSoda
{
  public class ProgressEventArgs : EventArgs
  {
    private int m_progress;

    public ProgressEventArgs(long value, long max)
    {
      this.m_progress = (int) Math.Round(value / max * 100.0);
    }

    public int Progress
    {
      get
      {
        return this.m_progress;
      }
      set
      {
        this.m_progress = this.Progress;
      }
    }
  }
}
