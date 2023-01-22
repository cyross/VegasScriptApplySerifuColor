using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using ScriptPortal.Vegas;
using System.Security;

namespace VegasScriptApplySerifuColor
{
    /// <summary>
    /// フォーム処理を伴うヘルパメソッド群はこちらに実装
    /// </summary>
    internal partial class VegasHelper
    {
        private readonly RichTextBox box = new RichTextBox();

        public string Rtf
        {
            get { return box.Rtf; }
            set { box.Rtf = value; }
        }

        public int GetActorNamePositionFromRtf()
        {
            return box.Find(":");
        }

        public string GetActorNameFromRtf()
        {
            int pos = GetActorNamePositionFromRtf();
            return GetActorNameFromRtf(pos);
        }

        public string GetActorNameFromRtf(int pos)
        {
            if (pos == -1)
            {
                return null;
            }
            return box.Text.Substring(0, pos);
        }

        public string GetActorNameFromRtfWithCut()
        {
            int pos = GetActorNamePositionFromRtf();
            if (pos == -1)
            {
                return null;
            }
            string actor_name = GetActorNameFromRtf(pos);
            box.Select(0, pos + 1);
            box.Cut();
            box.Focus();
            return actor_name;
        }

        internal void SetColorIntoAllRtfText(Color textColor)
        {
            box.SelectAll();
            box.SelectionColor = textColor;
        }

        internal void SetTextColorByActor(string actor_name)
        {
            string actor_name_key = VegasScriptSettings.FormatKey(actor_name);
            Color actor_color = GetTextColorByActor(actor_name_key);
            SetColorIntoAllRtfText(actor_color);
        }

        internal Color GetTextColorByActor(string actor_name)
        {
            string actor_name_key = VegasScriptSettings.FormatKey(actor_name);
            if (!VegasScriptSettings.TextColorByActor.ContainsKey(actor_name_key))
            {
                return Color.White;
            }

            return VegasScriptSettings.TextColorByActor[actor_name_key];
        }

        internal void SetTextRGBAParameter(OFXRGBAParameter param, Color color)
        {
            OFXColor textColor = new OFXColor(
                (double)color.R / 255.0,
                (double)color.G / 255.0,
                (double)color.B / 255.0,
                (double)color.A / 255.0
            );
            param.SetValueAtTime(BaseTimecode, textColor);
        }

        internal void ApplyTextColorByActor(TrackEvents events)
        {
            foreach (TrackEvent e in events)
            {
                Take firstTake = GetFirstTake(e);
                Media media = firstTake.Media;
                OFXStringParameter ofxStringParam = GetOFXStringParameter(media);
                if(ofxStringParam is null) { continue; }
                OFXRGBAParameter ofxRGBAParam = GetTextRGBAParameter(media);
                if(ofxRGBAParam is null) { continue; }

                Rtf = GetOFXParameterString(ofxStringParam);

                string actor_string = GetActorNameFromRtfWithCut();
                Color color = GetTextColorByActor(actor_string);

                SetTextRGBAParameter(ofxRGBAParam, color);
                SetStringIntoOFXParameter(ofxStringParam, Rtf);
            }
        }

        internal void ApplyTextColorByActor()
        {
            TrackEvents events = GetVideoEvents();
            if (events is null)
            {
                return;
            }
            if (events.Count == 0)
            {
                return;
            }
            ApplyTextColorByActor(events);
        }
    }
}
