using ScriptPortal.Vegas;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VegasScriptHelper;

namespace VegasScriptApplySerifuColor
{
    public class EntryPoint: IEntryPoint
    {
        private SettingForm settingForm = null;

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
                if(settingForm == null){ settingForm = new SettingForm(); }

                settingForm.JimakuTrackDataSource = keyList;
                settingForm.JimakuTrackName = selectedVideoTrack != null ? helper.GetTrackKey(selectedVideoTrack) : keyList.First();
                settingForm.OutlineWidth = VegasScriptSettings.JimakuOutlineWidth;

                if (settingForm.ShowDialog() == DialogResult.Cancel) { return; }

                TrackEvents events = helper.GetVideoEvents(keyValuePairs[settingForm.JimakuTrackName]);
                helper.ApplyTextColorByActor(events, settingForm.OutlineWidth, settingForm.RemovePrefix);
            }
            catch (VegasHelperNoneEventsException)
            {
                MessageBox.Show("選択したビデオトラック中にイベントが存在していません。");
            }
        }
    }
}
