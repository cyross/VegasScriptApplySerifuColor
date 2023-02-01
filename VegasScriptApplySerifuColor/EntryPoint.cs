using ScriptPortal.Vegas;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VegasScriptHelper;

namespace VegasScriptApplySerifuColor
{
    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
        {
            VegasHelper helper = VegasHelper.Instance(vegas);

            List<VideoTrack> videoTracks = helper.AllVideoTracks.ToList();

            if (videoTracks.Count == 0)
            {
                MessageBox.Show("ビデオトラックがありません。");
                return;
            }

            VideoTrack selectedVideoTrack = helper.SelectedVideoTrack(false);

            Dictionary<string, VideoTrack> keyValuePairs = helper.GetVideoKeyValuePairs(videoTracks);
            List<string> keyList = keyValuePairs.Keys.ToList();

            try
            {
                SettingForm form = new SettingForm()
                {
                    JimakuTrackDataSource = keyList,
                    JimakuTrackName = selectedVideoTrack != null ? helper.GetTrackKey(selectedVideoTrack) : keyList.First(),
                    OutlineWidth = VegasScriptSettings.JimakuOutlineWidth
                };

                if (form.ShowDialog() == DialogResult.Cancel) { return; }

                TrackEvents events = helper.GetVideoEvents(keyValuePairs[form.JimakuTrackName]);
                helper.ApplyTextColorByActor(events, form.OutlineWidth, form.RemovePrefix);
            }
            catch (VegasHelperNoneEventsException)
            {
                MessageBox.Show("選択したビデオトラック中にイベントが存在していません。");
            }
        }
    }
}
