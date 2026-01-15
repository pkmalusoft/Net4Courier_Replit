using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Net4Courier.Models
{
    public class GeneralSetupVM :GeneralSetup
    {
        [AllowHtml]
        [Display(Name = "TextDoc")]
        public string TextDoc
        {
            get;
            set;
        }
    }
}