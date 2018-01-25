using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ReportBuilderAjax.Web;
using ReportBuilderAjax.Web.Common;

namespace ReportBuilderSL.Ajax
{
    public class LogoManager
    {
        LogDomainService _log = new LogDomainService();
        private string[] sep = { "/" };

        private string getFilePath(string assn, string userId)
        {
            var folder = "CompanyLogo";
            string[] folders = ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL].Split(sep, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL]))
                folder = folders[folders.Length - 1];
            return System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/{0}/{1}_{2}.png", folder, userId, assn));
        }

        public bool Save(LogoFile file, string userId)
        {
            string filePath = getFilePath(file.ClientNumber, userId);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception e)
                {
                    _log.Log(e.Message);
                    return false;
                }

            }

            try
            {
                FileStream FileStream = new FileStream(filePath, FileMode.Create);
                FileStream.Write(file.File, 0, file.File.Length);
                FileStream.Close();
                FileStream.Dispose();
            }
            catch (Exception e)
            {
                _log.Log(e.Message);
                return false;
            }


            return true;
        }

       
        public bool Delete(string assn, string userId)
        {
            string filePath = getFilePath(assn, userId);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception e)
                {
                    _log.Log(e.Message);
                    return false;
                }

            }

            return true;
        }
        
        public bool Exists(string assn, string userId)
        {
            string filePath = getFilePath(assn, userId);

            if (File.Exists(filePath))
            {
                return true;
            }
            else return false;
        }

        public string GetLogoAsString(string assn, string userId)
        {
            return getImageAsString(string.Format("{0}_{1}.png", userId, assn));
        }

        public string GetLogoAsStringByName(string imageName)
        {
            return getImageAsString(string.Format("{0}.png", imageName));
        }

        private string getImageAsString(string imageName)
        {
            var folder = "CompanyLogo";
            string[] folders = ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL].Split(sep, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[Const.LOGO_IMAGE_URL]))
                folder = folders[folders.Length - 1];

            string imagePath = System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/{0}/{1}", folder, imageName));

            string resultString = "";
            if (File.Exists(imagePath))
            {
                Image image = Image.FromFile(imagePath);
                using (MemoryStream ms = new MemoryStream())
                {
                    // Convert Image to byte[]
                    image.Save(ms, ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    resultString = Convert.ToBase64String(imageBytes);
                }
                image.Dispose();
            }
            return resultString;
        }
    }
}