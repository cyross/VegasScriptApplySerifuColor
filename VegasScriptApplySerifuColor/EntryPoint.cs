using ScriptPortal.Vegas;
using VegasScriptHelper;
using System.Windows.Forms;

namespace VegasScriptApplySerifuColor
{
    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
        {
            VegasScriptSettings.Load();
            VegasHelper helper = VegasHelper.Instance(vegas);

            try
            {
                TrackEvents events = helper.GetVideoEvents();

                SettingForm form = new SettingForm();
                form.OutlineWidth = VegasScriptSettings.JimakuOutlineWidth;

                if(form.ShowDialog() == DialogResult.OK)
                {
                    helper.ApplyTextColorByActor(events, form.OutlineWidth, form.RemovePrefix);
                }
            }
            catch(VegasHelperTrackUnselectedException)
            {
                MessageBox.Show("ビデオトラックが選択されていません。");
            }
            catch(VegasHelperNoneEventsException)
            {
                MessageBox.Show("選択したビデオトラック中にイベントが存在していません。");
            }
        }
    }
}
