using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.Web.Configuration;

namespace CommonLibrary.Base.Database
{
    public static class ConnectionStringHelper
    {
        // Protect the connectionStrings section.
        public static void ProtectConfiguration()
        {
            //we need to do this check as a web config will not be opened using what's in the else
            try
            {
                System.Configuration.Configuration config = null;
                ConfigurationSection section = null;
                try
                {
                    if (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.Request.PhysicalPath.Equals(string.Empty))
                        config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                    else
                        config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                }
                catch (Exception)
                {
                    section = ConfigurationManager.GetSection("connectionStrings") as ConfigurationSection;
                }


                // Define the Dpapi provider name.
                string provider =
                    "DataProtectionConfigurationProvider";

                ConfigurationSection connStrings = null;
                if (config != null)
                {

                    // Get the section to protect.
                    connStrings =
                    config.ConnectionStrings;
                }
                else
                {
                    connStrings = section;
                }

                if (connStrings != null)
                {
                    if (!connStrings.SectionInformation.IsProtected)
                    {

                        // Protect the section.
                        connStrings.SectionInformation.ProtectSection(provider);

                        connStrings.SectionInformation.ForceSave = true;
                        config.Save(ConfigurationSaveMode.Full);
                    }
                    //do nothing we are already protected i think this is default behavior in 4.0 framework
                }
                else
                    throw new ApplicationException(String.Format("Can't get the section {0}", connStrings.SectionInformation.Name));
            }
            catch (Exception)
            {
                //we probably do not have permission to call protect on the file since we are not running in .exe
            }
        }

        // Unprotect the connectionStrings section.
        private static void UnProtectConfiguration()
        {
            try
            {
                System.Configuration.Configuration config = null;
                ConfigurationSection section = null;
                try
                {
                    if (System.Web.HttpContext.Current != null && !System.Web.HttpContext.Current.Request.PhysicalPath.Equals(string.Empty))
                        config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                    else
                        config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                }
                catch (Exception ex)
                {
                    section = ConfigurationManager.GetSection("connectionStrings") as ConfigurationSection;
                }


                // Get the section to unprotect.
                ConfigurationSection connStrings = null;

                if (config != null)
                {

                    // Get the section to protect.
                    connStrings =
                    config.ConnectionStrings;
                }
                else
                {
                    connStrings = section;
                }
                if (connStrings.SectionInformation.IsProtected)
                {

                    // Unprotect the section.
                    connStrings.SectionInformation.UnprotectSection();

                    connStrings.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Full);
                }
            }
            catch (Exception ex)
            {
                //we probably do not have permission to call protect on the file since we are not running in .exe

            }
        }
           

        public static ConnectionStringsSection GetConnectionStrings()
        {

            Configuration cfg = null;

            System.Web.HttpContext ctx = System.Web.HttpContext.Current;

            //WCF services hosted in IIS...
            VirtualPathExtension p = null;
            try
            {
                p = OperationContext.Current.Host.Extensions.Find<VirtualPathExtension>();
            }
            catch (Exception ex)
            {
            }

            try
            {
                if (ctx != null)
                {
                    cfg = WebConfigurationManager.OpenWebConfiguration(ctx.Request.ApplicationPath);
                }
                else if (p != null)
                {
                    cfg = WebConfigurationManager.OpenWebConfiguration(p.VirtualPath);
                }
                else
                {
                    cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                }
            }
            catch (Exception ex)
            {
            }

            
          

            ConnectionStringsSection connStrings = null;

            if (cfg != null)
            {                
                connStrings =
                cfg.ConnectionStrings;
            }
            
            if(connStrings != null)
                return connStrings;

            //final try at getting the settings:
            connStrings = ConfigurationManager.GetSection("connectionStrings") as ConnectionStringsSection;

            return connStrings;
        }
    }
}