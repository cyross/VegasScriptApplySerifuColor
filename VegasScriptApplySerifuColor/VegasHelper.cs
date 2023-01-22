using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ScriptPortal.Vegas;

namespace VegasScriptApplySerifuColor
{
    /// <summary>
    /// Vegasオブジェクトを操作するヘルパクラス
    /// 本クラスはSingleton
    /// </summary>
    internal partial class VegasHelper
    {
        private static VegasHelper _instance = null;

        internal Vegas Vegas { get; set; }

        internal readonly Timecode BaseTimecode = new Timecode();

        internal static VegasHelper Instance(Vegas vegas)
        {
            if (_instance == null)
            {
                _instance = new VegasHelper(vegas);
            }
            else
            {
                _instance.Vegas = vegas;
            }

            return _instance;
        }

        private VegasHelper(Vegas vegas)
        {
            Vegas = vegas;
        }

        /// <summary>
        /// 現在VEGASが開いているプロジェクトを取得する
        /// </summary>
        internal Project Project
        {
            get
            {
                return Vegas.Project;
            }
        }

        /// <summary>
        /// プロジェクト内で選択しているトラックがあれば、そのトラックのオブジェクトを返す。
        /// なければnullを返す
        /// </summary>
        /// <returns>選択プロジェクトがあればそのTrackオブジェクト、なければnull</returns>
        internal Track SelectedTrack()
        {
            return SelectedTrack(Vegas.Project);
        }

        /// <summary>
        /// プロジェクト内で選択しているトラックがあれば、そのトラックのオブジェクトを返す。
        /// なければnullを返す
        /// </summary>
        /// <param name="project">VEGASが開いているプロジェクト</param>
        /// <returns>選択プロジェクトがあればそのTrackオブジェクト、なければnull</returns>
        internal Track SelectedTrack(Project project)
        {
            foreach(Track track in project.Tracks)
            {
                if (track.Selected)
                {
                    return track;
                }
            }
            return null;
        }

        /// <summary>
        /// プロジェクト内で選択しているトラックがあれば、そのトラックのオブジェクトを返す。
        /// なければnullを返す
        /// </summary>
        /// <returns>選択プロジェクトがあればそのTrackオブジェクト、なければnull</returns>
        internal VideoTrack SelectedVideoTrack()
        {
            return SelectedVideoTrack(Vegas.Project);
        }

        /// <summary>
        /// プロジェクト内で選択しているトラックがあれば、そのトラックのオブジェクトを返す。
        /// なければnullを返す
        /// </summary>
        /// <param name="project">VEGASが開いているプロジェクト</param>
        /// <returns>選択プロジェクトがあればそのTrackオブジェクト、なければnull</returns>
        internal VideoTrack SelectedVideoTrack(Project project)
        {
            Track track = SelectedTrack();
            if (track is null)
            {
                return null;
            }
            return track.IsVideo() ? (VideoTrack)track : null;
        }

        /// <summary>
        /// プロジェクト内で選択しているトラックがあれば、そのトラックのオブジェクトを返す。
        /// なければnullを返す
        /// </summary>
        /// <returns>選択プロジェクトがあればそのTrackオブジェクト、なければnull</returns>
        internal AudioTrack SelectedAudioTrack()
        {
            return SelectedAudioTrack(Vegas.Project);
        }

        /// <summary>
        /// プロジェクト内で選択しているトラックがあれば、そのトラックのオブジェクトを返す。
        /// なければnullを返す
        /// </summary>
        /// <param name="project">VEGASが開いているプロジェクト</param>
        /// <returns>選択プロジェクトがあればそのTrackオブジェクト、なければnull</returns>
        internal AudioTrack SelectedAudioTrack(Project project)
        {
            Track track = SelectedTrack();
            if (track is null)
            {
                return null;
            }
            return track.IsAudio() ? (AudioTrack)track : null;
        }

        /// <summary>
        /// 引数で指定したトラックがビデオトラックかどうかを調べる
        /// </summary>
        /// <param name="track">対象のトラックオブジェクト</param>
        /// <returns>ビデオトラックの場合はTrue、それ以外のときはFalseを返す</returns>
        internal bool IsVideoTrack(Track track)
        {
            return track.IsVideo();
        }

        /// <summary>
        /// 引数で指定したトラックがオーディオトラックかどうかを調べる
        /// </summary>
        /// <param name="track">対象のトラックオブジェクト</param>
        /// <returns>オーディオトラックの場合はTrue、それ以外のときはFalseを返す</returns>
        internal bool IsAudioTrack(Track track)
        {
            return track.IsAudio();
        }

        internal VideoTrack AddVideoTrack()
        {
            return Vegas.Project.AddVideoTrack();
        }

        internal AudioTrack AddAudioTrack()
        {
            return Vegas.Project.AddAudioTrack();
        }

        /// <summary>
        /// オーディオトラックを作り、指定したディレクトリ内のwavファイルをイベントとして挿入する
        /// オーディオファイルの検知は拡張子のみで、ファイルの中身はチェックしない
        /// 対応するファイルはVegasScriptSettings.SupportedAudioFileで指定されたもの
        /// </summary>
        /// <param name="fileDir">指定したディレクトリ名</param>
        /// <param name="interval">挿入するイベント間の間隔　単位はミリ秒　標準は0.0</param>
        /// <param name="fromStart">トラックの最初から挿入するかどうかを示すフラグ　trueのときは最初から、falseのときは現在のカーソル位置から</param>
        /// <param name="recursive">子ディレクトリのを再帰的にトラックの最初から挿入するかどうかを示すフラグ　trueのときは最初から、falseのときは現在のカーソル位置から</param>
        internal void InseretAudioInTrack(string fileDir, float interval = 0.0f, bool fromStart = false, bool recursive = true)
        {
            AudioTrack audioTrack = AddAudioTrack();
            audioTrack.Selected = true;

            Timecode currentPosition = fromStart ? new Timecode() : Vegas.Cursor;
            Timecode intervalTimecode = new Timecode(interval);

            _InsertAudio(currentPosition, intervalTimecode, fileDir, audioTrack, recursive);
        }

        private Timecode _InsertAudio(Timecode current, Timecode interval, string fileDir, AudioTrack audioTrack, bool recursive)
        {
            if (recursive)
            {
                foreach (string childDir in Directory.GetDirectories(fileDir))
                {
                    current = _InsertAudio(current, interval, childDir, audioTrack, recursive);
                }
            }
            foreach (string filePath in Directory.GetFiles(fileDir))
            {
                if (VegasScriptSettings.SupportedAudioFile.Contains(Path.GetExtension(filePath)))
                {
                    Media audioMedia = new Media(filePath);
                    AudioStream audioStream = audioMedia.GetAudioStreamByIndex(0);

                    AudioEvent audioEvent = audioTrack.AddAudioEvent(current, audioStream.Length);
                    audioEvent.AddTake(audioStream);

                    current += audioStream.Length + interval;
                }
            }
            return current;
        }

        internal TrackEvents GetEvents(Track track)
        {
            return track.Events;
        }

        internal TrackEvents GetVideoEvents()
        {
            VideoTrack selected = SelectedVideoTrack();

            if (selected is null) { return null; }

            return selected.Events;
        }

        internal TrackEvents GetAudioEvents()
        {
            AudioTrack selected = SelectedAudioTrack();

            if (selected is null) { return null; }

            return selected.Events;
        }

        internal Take[] GetFirstTakes(Track track)
        {
            return GetFirstTakes(track.Events);
        }

        internal Take[] GetFirstTakes(TrackEvents events)
        {
            IEnumerable<Take> takes = events.Select(e => e.Takes[0]);
            return takes.ToArray();
        }

        internal Takes GetTakes(TrackEvent trackEvent)
        {
            return trackEvent.Takes;
        }

        internal Take GetFirstTake(TrackEvent trackEvent)
        {
            return trackEvent.Takes[0];
        }

        internal Take[] GetVideoTakes()
        {
            VideoTrack selected = SelectedVideoTrack();

            if (selected is null) { return null; }

            return GetFirstTakes(selected.Events);
        }

        internal Take[] GetAudioTakes()
        {
            AudioTrack selected = SelectedAudioTrack();

            if (selected is null) { return null; }

            return GetFirstTakes(selected.Events);
        }

        internal Media[] GetMediaList(VideoTrack track)
        {
            return GetMediaList(track.Events);
        }

        internal Media[] GetMediaList(AudioTrack track)
        {
            return GetMediaList(track.Events);
        }

        internal Media[] GetVideoMediaList()
        {
            VideoTrack selected = SelectedVideoTrack();

            if (selected is null) { return null; }

            return GetMediaList(selected.Events);
        }

        internal Media[] GetAudioMediaList()
        {
            AudioTrack selected = SelectedAudioTrack();

            if (selected is null) { return null; }

            return GetMediaList(selected.Events);
        }

        internal Media[] GetMediaList(TrackEvents events)
        {
            // テイクは考慮しない
            IEnumerable<Media> mediaList = events.Select(e => e.Takes[0].Media);
            return mediaList.ToArray();
        }

        internal OFXStringParameter GetOFXStringParameter(Media media)
        {
            foreach(OFXParameter param in media.Generator.OFXEffect.Parameters)
            {
                if(param.ParameterType == OFXParameterType.String)
                {
                    return (OFXStringParameter)param;
                }
            }
            return null;
        }

        internal OFXStringParameter[] GetOFXStringParameters(Media[] mediaList)
        {
            return mediaList.Select(m => GetOFXStringParameter(m)).ToList().ToArray();
        }

        internal OFXStringParameter[] GetOFXStringParameters(VideoTrack track)
        {
            Media[] mediaList = GetMediaList(track.Events);
            return GetOFXStringParameters(mediaList);
        }

        /// <summary>
        /// 選択したビデオトラックから、メディジェネレータ字幕のパラメータの配列を取得する
        /// ビデオトラックを選択していなければnullを返す
        /// </summary>
        /// <returns>選択したビデオトラックから得られたメディジェネレータ文字列パラメータの配列、もしくはnull</returns>
        internal OFXStringParameter[] GetOFXStringParameters()
        {
            VideoTrack selected = SelectedVideoTrack();

            if (selected is null) { return null; }

            return GetOFXStringParameters(selected);
        }

        public string GetOFXParameterString(OFXStringParameter param)
        {
            return param.GetValueAtTime(BaseTimecode);
        }

        public string GetOFXParameterString(Media media)
        {
            OFXStringParameter param = GetOFXStringParameter(media);
            if (param is null)
            {
                return null;
            }
            return GetOFXParameterString(param);
        }

        internal string[] GetOFXParameterStrings(OFXStringParameter[] parameters)
        {
            return parameters.Select(p => GetOFXParameterString(p)).ToArray();
        }

        internal string[] GetOFXParameterStrings()
        {
            VideoTrack selected = SelectedVideoTrack();

            if (selected is null) { return null; }

            OFXStringParameter[] ofxParams = GetOFXStringParameters(selected);
            return GetOFXParameterStrings(ofxParams);
        }

        internal void SetStringIntoOFXParameter(OFXStringParameter param, string value)
        {
            param.SetValueAtTime(BaseTimecode, value);
        }

        /// <summary>
        /// 文字列の配列の内容を、メディジェネレータOFXの文字列パラメータ配列の各要素に設定する。
        /// 各引数の要素数が同じでないと処理しない。
        /// </summary>
        /// <param name="ofxParams">メディジェネレータOFXの文字列パラメータの配列</param>
        /// <param name="values">設定する文字列（RTF）の配列</param>
        internal void SetStringsIntoOFXParameters(OFXStringParameter[] ofxParams, string[] values)
        {
            if(ofxParams.Length != values.Length) { return; }
            for(int i = 0; i < ofxParams.Length; i++)
            {
                SetStringIntoOFXParameter(ofxParams[i], values[i]);
            }
        }

        internal OFXRGBAParameter GetTextRGBAParameter(Media media)
        {
            foreach (OFXParameter param in media.Generator.OFXEffect.Parameters)
            {
                if (param.ParameterType == OFXParameterType.RGBA &&
                    param.Name == "TextColor")
                {
                    return (OFXRGBAParameter)param;
                }
            }
            return null;
        }

        internal OFXRGBAParameter[] GetTextRGBAParameters(Media[] mediaList)
        {
            return mediaList.Select(m => GetTextRGBAParameter(m)).ToList().ToArray();
        }

        internal OFXRGBAParameter[] GetTextRGBAParameters(VideoTrack track)
        {
            Media[] mediaList = GetMediaList(track.Events);
            return GetTextRGBAParameters(mediaList);
        }

        internal OFXRGBAParameter[] GetTextRGBAParameters()
        {
            VideoTrack selected = SelectedVideoTrack();

            if (selected is null) { return null; }

            return GetTextRGBAParameters(selected);
        }

        internal void SetTextRGBAParameter(OFXRGBAParameter param, OFXColor color)
        {
            param.SetValueAtTime(BaseTimecode, color);
        }
    }
}
