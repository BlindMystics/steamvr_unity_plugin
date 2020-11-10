//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Notify developers when a new version of the plugin is available.
//
//=============================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine.Networking;

//#if UNITY_2018_3_OR_NEWER
//#pragma warning disable CS0618
//#endif

namespace Valve.VR
{

    /// <summary>
    /// Updated by P3TE to stop warning about using WWW
    /// Updated to use UnityWebRequest, which should hopefully work :P
    /// </summary>
    [InitializeOnLoad]
    public class SteamVR_Update : EditorWindow
    {
        const string currentVersion = "2.1";
        const string versionUrl = "http://media.steampowered.com/apps/steamvr/unitypluginversion.txt";
        const string notesUrl = "http://media.steampowered.com/apps/steamvr/unityplugin-v{0}.txt";
        const string pluginUrl = "http://u3d.as/content/valve-corporation/steam-vr-plugin";
        const string doNotShowKey = "SteamVR.DoNotShow.v{0}";

        static bool gotVersion = false;
        //static WWW wwwVersion, wwwNotes;
        static UnityWebRequest wrVersion, wrNotes;
        static string version, notes;
        static SteamVR_Update window;

        static SteamVR_Update()
        {
            EditorApplication.update += Update;
        }

        private static UnityWebRequest SendGetRequest(string url) {
            UnityWebRequest www = UnityWebRequest.Get(versionUrl);
            www.SendWebRequest();
            return www;
        }

        static void Update()
        {
            if (!gotVersion)
            {

                if (wrVersion == null) {
                    wrVersion = SendGetRequest(versionUrl);
                }
                if (!wrVersion.isDone) {
                    return;
                }
                if (WebRequestSuccess(wrVersion)) {
                    version = GetWebRequestText(wrVersion);
                }

                wrVersion = null;
                gotVersion = true;

                if (ShouldDisplay())
                {
                    var url = string.Format(notesUrl, version);
                    wrNotes = SendGetRequest(url);

                    window = GetWindow<SteamVR_Update>(true);
                    window.minSize = new Vector2(320, 440);
                    //window.title = "SteamVR";
                }
            }

            if (wrNotes != null)
            {
                if (!wrNotes.isDone) {
                    return;
                }

                if (WebRequestSuccess(wrNotes)) {
                    notes = GetWebRequestText(wrNotes);
                }

                wrNotes = null;

                if (notes != "") {
                    window.Repaint();
                }
            }

            EditorApplication.update -= Update;
        }

        static string GetWebRequestText(UnityWebRequest webRequest) {
            byte[] byteArray = webRequest.downloadHandler.data;
            string resultString = System.Text.Encoding.UTF8.GetString(byteArray);
            return resultString;
        }

        static bool WebRequestSuccess(UnityWebRequest webRequest) {
            if (webRequest.isNetworkError) {
                Debug.LogWarning(webRequest.error);
                return false;
            }
            if (webRequest.isHttpError) {
                Debug.LogWarning(webRequest.error);
                return false;
            }
            string resultString = GetWebRequestText(webRequest);
            if (Regex.IsMatch(resultString, "404 not found", RegexOptions.IgnoreCase)) {
                Debug.LogWarning(resultString);
                return false;
            }
            return true;
        }

        //static bool UrlSuccess(WWW www)
        //{
        //    if (!string.IsNullOrEmpty(www.error))
        //        return false;
        //    if (Regex.IsMatch(www.text, "404 not found", RegexOptions.IgnoreCase))
        //        return false;
        //    return true;
        //}

        static bool ShouldDisplay()
        {
            if (string.IsNullOrEmpty(version))
                return false;
            if (version == currentVersion)
                return false;
            if (EditorPrefs.HasKey(string.Format(doNotShowKey, version)))
                return false;

            // parse to see if newer (e.g. 1.0.4 vs 1.0.3)
            var versionSplit = version.Split('.');
            var currentVersionSplit = currentVersion.Split('.');
            for (int i = 0; i < versionSplit.Length && i < currentVersionSplit.Length; i++)
            {
                int versionValue, currentVersionValue;
                if (int.TryParse(versionSplit[i], out versionValue) &&
                    int.TryParse(currentVersionSplit[i], out currentVersionValue))
                {
                    if (versionValue > currentVersionValue)
                        return true;
                    if (versionValue < currentVersionValue)
                        return false;
                }
            }

            // same up to this point, now differentiate based on number of sub values (e.g. 1.0.4.1 vs 1.0.4)
            if (versionSplit.Length <= currentVersionSplit.Length)
                return false;

            return true;
        }

        Vector2 scrollPosition;
        bool toggleState;

        string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path.Substring(0, path.Length - "Editor".Length) + "Textures/";
        }

        public void OnGUI()
        {
            EditorGUILayout.HelpBox("A new version of the SteamVR plugin is available!", MessageType.Warning);

            var resourcePath = GetResourcePath();
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "logo.png");
            var rect = GUILayoutUtility.GetRect(position.width, 150, GUI.skin.box);
            if (logo)
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Current version: " + currentVersion);
            GUILayout.Label("New version: " + version);

            if (notes != "")
            {
                GUILayout.Label("Release notes:");
                EditorGUILayout.HelpBox(notes, MessageType.Info);
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Get Latest Version"))
            {
                Application.OpenURL(pluginUrl);
            }

            EditorGUI.BeginChangeCheck();
            var doNotShow = GUILayout.Toggle(toggleState, "Do not prompt for this version again.");
            if (EditorGUI.EndChangeCheck())
            {
                toggleState = doNotShow;
                var key = string.Format(doNotShowKey, version);
                if (doNotShow)
                    EditorPrefs.SetBool(key, true);
                else
                    EditorPrefs.DeleteKey(key);
            }
        }
    }
}

//#if UNITY_2018_3_OR_NEWER
//#pragma warning restore CS0618
//#endif