using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting
{
    public class ResourceManifest : IResourceManifestProvider
    {   
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            var manifest = builder.Add();            
            manifest.DefineScript("jQueryUI").SetUrl("ui/jquery-ui.min.js");            
            manifest.DefineScript("jQueryUI_DatePicker").SetUrl("ui/datepicker.min.js", "ui/datepicker.js").SetVersion("1.11.2").SetDependencies("jQueryUI_Core");
            manifest.DefineScript("jQueryUI_TimePicker").SetUrl("ui/jquery-ui-timepicker-addon.min.js").SetDependencies("jQueryUI_Core");
            manifest.DefineScript("jQueryUI_SliderAccess").SetUrl("ui/jquery-ui-sliderAccess.js").SetDependencies("jQueryUI_Core");
            manifest.DefineScript("SheepItPlugin").SetUrl("jquery.sheepItPlugin-1.1.1.js").SetDependencies("jQuery");
            manifest.DefineScript("JQPlotPlugin").SetUrl("jquery.jqplot.min.js");
            manifest.DefineScript("JQPlot_categoryAxisRenderer").SetUrl("Plugins/jqplot.categoryAxisRenderer.min.js");
            manifest.DefineScript("JQPlot_barRenderer").SetUrl("Plugins/jqplot.barRenderer.min.js");
            manifest.DefineScript("JQPlot_canvasAxisTickRenderer").SetUrl("Plugins/jqplot.canvasAxisTickRenderer.min.js");
            manifest.DefineScript("MD5").SetUrl("md5.js");
            manifest.DefineScript("FingerPrint").SetUrl("jquery.browser-fingerprint-1.1.min.js");
            manifest.DefineScript("Toastr").SetUrl("toastr.min.js");

            manifest.DefineStyle("AdvancedPoll_Editor").SetUrl("Styles/AdvancedPoll_Editor.css");
            manifest.DefineStyle("JQPlotStyle").SetUrl("Styles/jquery.jqplot.min.css");            
            manifest.DefineStyle("jQueryUI_DatePicker").SetUrl("Styles/jquery-datetime-editor.css");
            manifest.DefineStyle("jQueryUI_TimePicker").SetUrl("Styles/jquery-ui-timepicker-addon.css");
            manifest.DefineStyle("jQueryUI").SetUrl("Styles/jquery-ui.css");
            manifest.DefineStyle("Toastr").SetUrl("Styles/toastr.min.css");
        }
    }
}