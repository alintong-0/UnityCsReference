// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEditor.Experimental;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace UnityEditor
{
    internal class AboutWindow : EditorWindow
    {
        const int VersionBoxLineHeight = 13;
        const int VersionBoxHeight = VersionBoxLineHeight * 4;
        static readonly Vector2 WindowSize = new Vector2(573, 545);
        private const string AboutTitle = "";

        [RequiredByNativeCode]
        internal static void ShowAboutWindow()
        {
            var mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
            var aboutRect = EditorGUIUtility.GetCenteredWindowPosition(mainWindowRect, WindowSize);

            AboutWindow w = GetWindowWithRect<AboutWindow>(aboutRect, true, AboutTitle);
            w.position = aboutRect;
            w.minSize = w.maxSize = WindowSize;
            w.m_Parent.window.m_DontSaveToLayout = true;
        }

        bool m_ShowDetailedVersion = false;
        private int m_InternalCodeProgress;


        private VisualElement buildDetailsContainer;
        private Label versionLabel;
        private const string darkClassname = "dark";

        void CreateGUI()
        {
            rootVisualElement.AddToClassList("root-element");


            var layout = EditorResources.Load("UXML/About/AboutWindow.uxml", typeof(UnityEngine.Object)) as VisualTreeAsset;

            string extensionVersion = FormatExtensionVersionString();
            int t = InternalEditorUtility.GetUnityVersionDate();
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string branch = InternalEditorUtility.GetUnityBuildBranch();


            if (layout == null)
            {
                // We display a minimal layout just in case
                rootVisualElement.Add(new Label("About Unity"));
                versionLabel = new Label($"{InternalEditorUtility.GetUnityDisplayVersion()}{extensionVersion}");
                rootVisualElement.Add(versionLabel);
            }
            else
            {
                layout.CloneTree(rootVisualElement);

                versionLabel = rootVisualElement.Q<Label>("version");

                buildDetailsContainer = rootVisualElement.Q("detail-info");
                buildDetailsContainer.AddToClassList("hide-details");

                if (EditorGUIUtility.isProSkin)
                {
                    rootVisualElement.AddToClassList(darkClassname);
                    rootVisualElement.Query<VisualElement>(className: "logo")
                        .ForEach((x) => x.AddToClassList(darkClassname));
                }

                SetLabelValue("product-name", InternalEditorUtility.GetUnityProductName());

                SetLabelValue("build-revision", $"{branch} {InternalEditorUtility.GetUnityBuildHash()}");
                SetLabelValue("build-date", $"{dt.AddSeconds(t):r}");


                SetLabelValue("license-type", InternalEditorUtility.GetLicenseInfoType());
                SetLabelValue("serial-number", InternalEditorUtility.GetLicenseInfoSerial());
                SetLabelValue("unity-copyright", InternalEditorUtility.GetUnityCopyright());
            }

            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            var contextMenu = new ContextualMenuManipulator((evt) =>
            {
                evt.menu.AppendAction("Copy Version Info", (act) => CopyVersionInfoToClipboard());
                evt.menu.AppendAction("Copy License Info", (act) => CopyLicenseInfoToClipboard());
            });

            rootVisualElement.AddManipulator(contextMenu);
            rootVisualElement.focusable = true;

            UpdateVersionLabel();

            rootVisualElement.Focus();
        }

        void SetLabelValue(string labelName, string text)
        {
            var lbl = rootVisualElement.Q<TextElement>(labelName);
            if(lbl != null)
                lbl.text = text.Replace("(c)", "\u00A9");
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            bool altPressed = ((int)evt.modifiers & (int)EventModifiers.Alt) == (int)EventModifiers.Alt;

            UpdateOnAlt(altPressed);

            ListenForSecretCodes(evt.character);

            if (evt.keyCode == KeyCode.C)
            {
                if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
                {
                    if (evt.modifiers == EventModifiers.Command)
                    {
                        CopyVersionInfoToClipboard();
                    }
                }
                else if (evt.modifiers == EventModifiers.Control)
                {
                    CopyVersionInfoToClipboard();
                }
            }
        }

        void OnEnable()
        {
            EditorApplication.modifierKeysChanged += ModifierKeysChanged;
        }

        void OnDisable()
        {
            EditorApplication.modifierKeysChanged -= ModifierKeysChanged;
        }

        void ModifierKeysChanged()
        {
            // because we show the detailed version string when Option (Alt) is pressed
            Repaint();
        }

        public void OnGUI()
        {
            var evt = Event.current;
            UpdateOnAlt(evt.alt);
        }

        private void UpdateOnAlt(bool altPressed)
        {
            if (!m_ShowDetailedVersion && altPressed != m_ShowDetailedVersion)
            {
                m_ShowDetailedVersion |= altPressed;

                UpdateVersionLabel();
            }
        }

        private void CopyVersionInfoToClipboard()
        {
            string extensionVersion = FormatExtensionVersionString();

            if (m_ShowDetailedVersion)
            {
                int t = InternalEditorUtility.GetUnityVersionDate();
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                string branch = InternalEditorUtility.GetUnityBuildBranch();
                Clipboard.stringValue = $"{InternalEditorUtility.GetUnityProductName()}\n" +
                                        $"{InternalEditorUtility.GetUnityDisplayVersionVerbose()}{extensionVersion}\n" +
                                        $"Revision: {branch} {InternalEditorUtility.GetUnityBuildHash()}\n" +
                                        $"Built: {dt.AddSeconds(t):r}";
            }
            else
            {
                Clipboard.stringValue = $"{InternalEditorUtility.GetUnityProductName()}\n" +
                                        $"{InternalEditorUtility.GetUnityDisplayVersion()}{extensionVersion}";
            }
        }

        private void CopyLicenseInfoToClipboard()
        {
            Clipboard.stringValue = InternalEditorUtility.GetLicenseInfo().Replace("(c)", "\u00A9");
        }

        void UpdateVersionLabel()
        {
            if (buildDetailsContainer != null)
            {
                buildDetailsContainer.EnableInClassList("hide-details", !m_ShowDetailedVersion);
                buildDetailsContainer.Query<VisualElement>()
                    .ForEach((x) => x.EnableInClassList("hide-details", !m_ShowDetailedVersion));
            }

            string extensionVersion = FormatExtensionVersionString();

            if (m_ShowDetailedVersion)
            {
                SetLabelValue("version", $"{InternalEditorUtility.GetUnityDisplayVersionVerbose()}{extensionVersion}");
            }
            else
            {
                // The non verbose version should be shorter on public builds
                SetLabelValue("version", $"{InternalEditorUtility.GetUnityDisplayVersion()}{extensionVersion}");
            }

        }

        private void ListenForSecretCodes(char current)
        {
            if (current == '\0')
                return;

            if (SecretCodeHasBeenTyped("internal", current, ref m_InternalCodeProgress))
            {
                ToggleInternalMode();
            }
        }

        private bool SecretCodeHasBeenTyped(string code, char current, ref int characterProgress)
        {
            if (characterProgress < 0 || characterProgress >= code.Length || code[characterProgress] != current)
                characterProgress = 0;

            // Don't use else here. Even if key was mismatch, it should still be recognized as first key of sequence if it matches.
            if (code[characterProgress] == current)
            {
                characterProgress++;

                if (characterProgress >= code.Length)
                {
                    characterProgress = 0;
                    return true;
                }
            }
            return false;
        }

        private void ToggleInternalMode()
        {
            bool enabled = !EditorPrefs.GetBool("DeveloperMode", false);
            EditorPrefs.SetBool("DeveloperMode", enabled);
            ShowNotification(new GUIContent(string.Format(L10n.Tr("Developer Mode {0}"), (enabled ? L10n.Tr("On") : L10n.Tr("Off")))));
            EditorUtility.RequestScriptReload();

            // Repaint all views to show/hide debug repaint indicator
            InternalEditorUtility.RepaintAllViews();
        }

        private string FormatExtensionVersionString()
        {
            string extStr = EditorUserBuildSettings.selectedBuildTargetGroup.ToString();
            string ext = Modules.ModuleManager.GetExtensionVersion(extStr);

            if (!string.IsNullOrEmpty(ext))
                return " [" + extStr + ": " + ext + "]";

            return "";
        }
    }
}
