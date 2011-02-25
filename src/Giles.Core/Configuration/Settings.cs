using System;
using System.Configuration;

namespace Giles.Core.Configuration
{

    public class Settings : MarshalByRefObject
    {
        public string MsBuild
        {
            get { return ConfigurationManager.AppSettings["MSBuild"]; }
        }
    }
}