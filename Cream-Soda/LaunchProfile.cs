
namespace CreamSoda
{
  internal class LaunchProfile
  {
    private string m_text = "";
    private string m_Exec = "";
    private string m_Website = "";
    private string m_Params = "";

    public string Text
    {
      get
      {
        return this.m_text;
      }
    }

    public string Exec
    {
      get
      {
        return this.m_Exec;
      }
    }

    public string Website
    {
        get
        {
            return this.m_Website;
        }
    }

    public string Params
    {
      get
      {
        return this.m_Params;
      }
    }

    public LaunchProfile(string Text, string Exec, string Website, string Params)
    {
      this.m_text = Text;
      this.m_Exec = Exec;
      this.m_Website = Website;
      this.m_Params = Params;
    }
  }
}
