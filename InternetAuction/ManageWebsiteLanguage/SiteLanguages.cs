using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace OnlineAuctionProject.ManageWebsiteLanguage
{
    public class SiteLanguages
    {
        public static List<Language> AvailableLanguages = new List<Language>
        {
            new Language{LanguageName="English",LanguageAbbr="en"},
            new Language{LanguageName="العربية",LanguageAbbr="ar"}
        };

        public static bool IsLanguageAvailable(string language)
        {
            return AvailableLanguages.Where(x => x.LanguageAbbr.Equals(language)).FirstOrDefault() != null ? true : false;
        }

        public static string GetDefaultLanguage()
        {
            return AvailableLanguages[0].LanguageAbbr;
        }

        public static string GetCurrentLanguageCulture()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name.ToString();
        }

        public void SetLanguage(string Language)
        {
            try
            {
                if (!IsLanguageAvailable(Language))
                {
                    Language = GetDefaultLanguage();
                }
                var cultureInfo = new CultureInfo(Language);
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
                HttpCookie languageCookie = new HttpCookie("culture", Language);
                languageCookie.Expires = DateTime.Now.AddYears(1);
                HttpContext.Current.Response.Cookies.Add(languageCookie);
            }
            catch
            {

            }
        }
    }


    public class Language
    {
        public string LanguageName { get; set; }
        public string LanguageAbbr { get; set; }
    }
}