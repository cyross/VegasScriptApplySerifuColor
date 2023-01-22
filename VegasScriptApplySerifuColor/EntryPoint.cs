using ScriptPortal.Vegas;
using System.Windows.Forms;

namespace VegasScriptApplySerifuColor
{
    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
        {
            VegasScriptSettings.Load();
            VegasHelper helper = VegasHelper.Instance(vegas);

            TrackEvents events = helper.GetVideoEvents();
            if (events is null)
            {
                MessageBox.Show("ビデオトラックが選択されていません。");
                return;
            }
            if (events.Count == 0)
            {
                MessageBox.Show("選択したビデオトラック中にイベントが存在していません。");
            }

            helper.ApplyTextColorByActor(events);
        }
    }
}
