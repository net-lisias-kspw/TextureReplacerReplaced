﻿/*
 * Copyright © 2017 HaArLiNsH
 * Copyright © 2013-2017 Davorin Učakar, RangeMachine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TextureReplacerReplaced
{
    /// <summary>
    /// The configuration windows in the space center scene
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class TRR_Gui : MonoBehaviour
    {
        //Head_Gui head_Gui = new Head_Gui();

        private bool headGui_IsEnabled = false;

        private Rect headGui_windowRect = new Rect(60, 60, 875, 700);

        private const int WINDOW_ID_HEAD = 107057;

        private bool suitGui_IsEnabled = false;

        private Rect suitGui_windowRect = new Rect(70, 70, 855, 700);

        private const int WINDOW_ID_SUIT = 107058;

        private Vector2 headScroll = Vector2.zero;

        private Vector2 headSettingScroll = Vector2.zero;

        private Head_Set selectedHeadSet = null;

        private Suit_Set selectedsuitSet = null; 

        /// <summary>
        /// icon for the toolbar
        /// </summary>
        private static readonly string APP_ICON_PATH = Util.DIR + "Plugins/AppIcon";

        /// <summary>
        /// The 3 types of reflections : "None", "Static", "Real"
        /// </summary>
        private static readonly string[] REFLECTION_TYPES = { "None", "Static", "Real" };

        /// <summary>
        /// the color of the selected item
        /// </summary>
        private static readonly Color SELECTED_COLOUR = new Color(0.7f, 0.9f, 1.0f);

        /// <summary>
        /// the color for the class
        /// </summary>
        private static readonly Color CLASS_COLOUR = new Color(1.0f, 0.8f, 1.0f);

        /// <summary>
        /// unique ID of the window of the GUI
        /// </summary>
        private const int WINDOW_ID = 107056;

        /// <summary>
        /// Classes from config files.
        /// </summary>
        private readonly List<string> classes = new List<string>();

        /// <summary>
        /// Ui window size
        /// </summary>
        private Rect windowRect = new Rect(Screen.width - 600, 60, 580, 610);

        /// <summary>
        /// vector used for the scroll in the roster area of the GUI
        /// </summary>
        private Vector2 rosterScroll = Vector2.zero;

        /// <summary>
        /// helper for the selected kerbal
        /// </summary>
        private ProtoCrewMember selectedKerbal = null;

        /// <summary>
        /// helper for the selected class
        /// </summary>
        private string selectedClass = null;

        /// <summary>
        /// check to open or close the GUI
        /// </summary>
        private bool isEnabled = false;

        /// <summary>
        /// Application launcher icon.
        /// </summary>
        private Texture2D appIcon = null;

        /// <summary>
        /// Application launcher button.
        /// </summary>
        private ApplicationLauncherButton appButton = null;

        /// <summary>
        /// Checker to see if we use the GUI or not 
        /// <para> used in the @default.cfg file (default = true)</para>
        /// </summary>
        private bool isGuiEnabled = true;

        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The method that populate the GUI window
        /// </summary>
        /// <param name="id"></param>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        private void windowHandler(int id)
        {
            Reflections reflections = Reflections.instance;
            Personaliser personaliser = Personaliser.instance;
            Randomizer randomizer = new Randomizer();

            if (GUI.Button(new Rect(560, 5, 15, 15), "X"))
                appButton.SetFalse();

            GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical(GUILayout.Width(200));

                    // Roster area.
                        rosterScroll = GUILayout.BeginScrollView(rosterScroll);
                            GUILayout.BeginVertical();

                            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
                            {
                                switch (kerbal.rosterStatus)
                                {
                                    case ProtoCrewMember.RosterStatus.Assigned:
                                        GUI.contentColor = Color.cyan;
                                        break;

                                    case ProtoCrewMember.RosterStatus.Dead:
                                        continue;
                                    case ProtoCrewMember.RosterStatus.Missing:
                                        GUI.contentColor = Color.yellow;
                                        break;

                                    default:
                                        GUI.contentColor = Color.white;
                                        break;
                                }

                                if (GUILayout.Button(kerbal.name))
                                {
                                    selectedKerbal = kerbal;
                                    selectedClass = null;
                                }
                            }

                            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Unowned)
                            {
                                switch (kerbal.rosterStatus)
                                {
                                    case ProtoCrewMember.RosterStatus.Dead:
                                        GUI.contentColor = Color.cyan;
                                        break;

                                    default:
                                        continue;
                                }

                                if (GUILayout.Button(kerbal.name))
                                {
                                    selectedKerbal = kerbal;
                                    selectedClass = null;
                                }
                            }

                            GUI.contentColor = Color.white;
                            GUI.color = CLASS_COLOUR;

                            // Class suits.
                            foreach (string clazz in classes)
                            {
                                if (GUILayout.Button(clazz))
                                {
                                    selectedKerbal = null;
                                    selectedClass = clazz;
                                }
                            }

                            GUI.color = Color.white;

                            GUILayout.EndVertical();
                        GUILayout.EndScrollView();

                    if (GUILayout.Button("Reset to Defaults"))
                        personaliser.resetKerbals();

                    GUILayout.EndVertical();

                // Textures.
                Head_Set defaultHead = personaliser.defaulMaleAndFemaleHeads[0];
                Suit_Set defaultSuit = personaliser.defaultSuit;
                KerbalData kerbalData = null;
                Head_Set head = null;
                Suit_Set suit = null;
                int headIndex = -1;           
                int suitIndex = -1;

                if (selectedKerbal != null)
                {
                    kerbalData = personaliser.getKerbalData(selectedKerbal);
                    defaultHead = personaliser.defaulMaleAndFemaleHeads[(int)selectedKerbal.gender];
                                
                    head = personaliser.getKerbalHead(selectedKerbal, kerbalData);
                    suit = personaliser.getKerbalSuit(selectedKerbal, kerbalData);
                
                    headIndex = personaliser.maleAndfemaleHeadsDB_full[(int)selectedKerbal.gender].IndexOf(head);
                    suitIndex = personaliser.KerbalSuitsDB_full.IndexOf(suit);
                }
                else if (selectedClass != null)
                {
                    personaliser.classSuitsDB.TryGetValue(selectedClass, out suit);

                    if (suit != null)
                        suitIndex = personaliser.KerbalSuitsDB_full.IndexOf(suit);
                }

                GUILayout.Space(10);
                    GUILayout.BeginVertical();

                    if (head != null)
                    {
                        GUILayout.Box(head.headTexture[0], GUILayout.Width(200), GUILayout.Height(200));

                        GUILayout.Label(head.name);
                    }

                    if (suit != null)
                    {
                        Texture2D suitTex = suit == defaultSuit && kerbalData != null && kerbalData.isVeteran ?
                                            defaultSuit.get_suit_Iva_Standard_Male(0) : (suit.get_suit_Iva_Standard_Male(0) ?? defaultSuit.get_suit_Iva_Standard_Male(0));
                        Texture2D helmetTex = suit.get_helmet_Iva_Standard_Male(0) ?? defaultSuit.get_helmet_Iva_Standard_Male(0);
                        Texture2D evaSuitTex = suit.get_suit_EvaSpace_Standard_Male(0) ?? defaultSuit.get_suit_EvaSpace_Standard_Male(0);
                        Texture2D evaHelmetTex = suit.get_helmet_EvaSpace_Standard_Male(0) ?? defaultSuit.get_helmet_EvaSpace_Standard_Male(0);

                            GUILayout.BeginHorizontal();
                            GUILayout.Box(suitTex, GUILayout.Width(100), GUILayout.Height(100));
                            GUILayout.Space(10);
                            GUILayout.Box(helmetTex, GUILayout.Width(100), GUILayout.Height(100));
                            GUILayout.EndHorizontal();

                            GUILayout.Space(10);

                            GUILayout.BeginHorizontal();
                            GUILayout.Box(evaSuitTex, GUILayout.Width(100), GUILayout.Height(100));
                            GUILayout.Space(10);
                            GUILayout.Box(evaHelmetTex, GUILayout.Width(100), GUILayout.Height(100));
                            GUILayout.EndHorizontal();

                        GUILayout.Label(suit.name);
                    }

                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(GUILayout.Width(120));

                    if (kerbalData != null)
                    {
                            GUILayout.BeginHorizontal();
                            GUI.enabled = personaliser.KerbalHeadsDB_full.Count != 0;

                            if (GUILayout.Button("<"))
                            {
                                headIndex = headIndex == -1 ? 0 : headIndex;
                                headIndex = (personaliser.maleAndfemaleHeadsDB_full[(int)selectedKerbal.gender].Count + headIndex - 1) % personaliser.maleAndfemaleHeadsDB_full[(int)selectedKerbal.gender].Count;

                                kerbalData.head = personaliser.maleAndfemaleHeadsDB_full[(int)selectedKerbal.gender][headIndex];

                                string value = "";

                                if (personaliser.KerbalAndTheirHeadsDB.TryGetValue(selectedKerbal.name, out value))
                                {
                                    personaliser.KerbalAndTheirHeadsDB[selectedKerbal.name] = kerbalData.head.name;
                                }
                                else
                                {
                                    personaliser.KerbalAndTheirHeadsDB.Add(selectedKerbal.name, kerbalData.head.name);
                                }

                            }
                            if (GUILayout.Button(">"))
                            {
                                headIndex = (headIndex + 1) % personaliser.maleAndfemaleHeadsDB_full[(int)selectedKerbal.gender].Count;

                                kerbalData.head = personaliser.maleAndfemaleHeadsDB_full[(int)selectedKerbal.gender][headIndex];

                                string value = "";

                                if (personaliser.KerbalAndTheirHeadsDB.TryGetValue(selectedKerbal.name, out value) )
                                {
                                    personaliser.KerbalAndTheirHeadsDB[selectedKerbal.name] = kerbalData.head.name;
                                }
                                else
                                {
                                    personaliser.KerbalAndTheirHeadsDB.Add(selectedKerbal.name, kerbalData.head.name);
                                }
                            }

                            GUI.enabled = true;
                            GUILayout.EndHorizontal();

                        GUI.color = kerbalData.head == defaultHead ? SELECTED_COLOUR : Color.white;
                        if (GUILayout.Button("Default"))
                        {
                            kerbalData.head = defaultHead;

                            string value = "";

                            if (personaliser.KerbalAndTheirHeadsDB.TryGetValue(selectedKerbal.name, out value))
                            {
                                personaliser.KerbalAndTheirHeadsDB[selectedKerbal.name] = kerbalData.head.name;
                            }
                            else
                            {
                                personaliser.KerbalAndTheirHeadsDB.Add(selectedKerbal.name, kerbalData.head.name);
                            }
                        }
                

                        GUI.color = kerbalData.head == null ? SELECTED_COLOUR : Color.white;
                        if (GUILayout.Button("Random"))
                        {
                            kerbalData.head = randomizer.randomize((int) selectedKerbal.gender);

                            string value = "";

                            if (personaliser.KerbalAndTheirHeadsDB.TryGetValue(selectedKerbal.name, out value))
                            {
                                personaliser.KerbalAndTheirHeadsDB[selectedKerbal.name] = kerbalData.head.name;
                            }
                            else
                            {
                                personaliser.KerbalAndTheirHeadsDB.Add(selectedKerbal.name, kerbalData.head.name);
                            }
                            //Util.log("{0} use this head set : {1}", selectedKerbal.name, kerbalData.head.headSetName);
                        }
                   

                        GUI.color = Color.white;
                    }

                    if (kerbalData != null || selectedClass != null)
                    {
                        GUILayout.Space(130);

                            GUILayout.BeginHorizontal();
                            GUI.enabled = personaliser.KerbalSuitsDB_full.Count != 0;

                            if (GUILayout.Button("<"))
                            {
                                suitIndex = suitIndex == -1 ? 0 : suitIndex;
                                suitIndex = (personaliser.KerbalSuitsDB_full.Count + suitIndex - 1) % personaliser.KerbalSuitsDB_full.Count;

                                if (kerbalData != null)
                                {
                                    kerbalData.suit = personaliser.KerbalSuitsDB_full[suitIndex];
                                    //kerbalData.cabinSuit = null;
                                }
                                else
                                {
                                    personaliser.classSuitsDB[selectedClass] = personaliser.KerbalSuitsDB_full[suitIndex];
                                }
                            }
                            if (GUILayout.Button(">"))
                            {
                                suitIndex = (suitIndex + 1) % personaliser.KerbalSuitsDB_full.Count;

                                if (kerbalData != null)
                                {
                                    kerbalData.suit = personaliser.KerbalSuitsDB_full[suitIndex];
                                    //kerbalData.cabinSuit = null;
                                }
                                else
                                {
                                    personaliser.classSuitsDB[selectedClass] = personaliser.KerbalSuitsDB_full[suitIndex];
                                }
                            }

                            GUI.enabled = true;
                            GUILayout.EndHorizontal();

                        GUI.color = suit == defaultSuit && (kerbalData == null || kerbalData.suit != null) ?
                          SELECTED_COLOUR : Color.white;

                        if (GUILayout.Button("Default"))
                        {
                            if (kerbalData != null)
                            {
                                kerbalData.suit = defaultSuit;
                                //kerbalData.cabinSuit = null;
                            }
                            else
                            {
                                personaliser.classSuitsDB[selectedClass] = defaultSuit;
                            }
                        }

                        GUI.color = suit == null || (kerbalData != null && kerbalData.suit == null) ? SELECTED_COLOUR : Color.white;
                        if (GUILayout.Button("Unset/Generic"))
                        {
                            if (kerbalData != null)
                            {
                                kerbalData.suit = null;
                                //kerbalData.cabinSuit = null;
                            }
                            else
                            {
                                personaliser.classSuitsDB[selectedClass] = null;
                            }
                        }

                        GUI.color = Color.white;
                    }
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            GUILayout.Space(10);

            personaliser.isHelmetRemovalEnabled = GUILayout.Toggle(
              personaliser.isHelmetRemovalEnabled, "Remove helmets in safe situations");

            personaliser.isAtmSuitEnabled = GUILayout.Toggle(
              personaliser.isAtmSuitEnabled, "Spawn Kerbals in IVA suits when in breathable atmosphere");

            personaliser.isNewSuitStateEnabled = GUILayout.Toggle(
              personaliser.isNewSuitStateEnabled, "Kerbals use another EVA suit when on the ground and with no air");

           


            /*personaliser.isAutomaticSuitSwitchEnabled = GUILayout.Toggle(
              personaliser.isAutomaticSuitSwitchEnabled, "Use the automatic switch system ? (disable the Toggle suit)");*/

            Reflections.Type reflectionType = reflections.reflectionType;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Reflections", GUILayout.Width(120));
                reflectionType = (Reflections.Type)GUILayout.SelectionGrid((int)reflectionType, REFLECTION_TYPES, 3);
                GUILayout.EndHorizontal();

            if (reflectionType != reflections.reflectionType)
                reflections.setReflectionType(reflectionType);

            if (GUILayout.Button("Heads Menu"))
            {
                headGui_IsEnabled = true;
            }

            if (GUILayout.Button("Suits Menu"))
            {
                suitGui_IsEnabled = true;
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }
        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Called when we enable (open) the GUI.
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        private void enable()
        {   
            if (!isEnabled)
            {
                isEnabled = true;
                selectedKerbal = null;
                selectedClass = null;
            }            
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Called when we disable (close) the GUI.
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        private void disable()
        {
            isEnabled = false;
            selectedKerbal = null;
            selectedClass = null;

            rosterScroll = Vector2.zero;
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Add the button to open the GUI in the toolbar when in the Space center scene
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        private void addAppButton()
        {
            if (appButton == null)
            {
                appButton = ApplicationLauncher.Instance.AddModApplication(
                  enable, disable, null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER, appIcon);
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Remove the button to open the GUI in the toolbar when not in the Space center scene
        /// </summary>
        /// <param name="scenes"></param>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        private void removeAppButton(GameScenes scenes)
        {
            if (appButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
                appButton = null;
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Load the configurations at the Awake()
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        public void Awake()
        {
            if (isGuiEnabled)
            {
                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("TextureReplacerReplaced"))
                    Util.parse(node.GetValue("isGUIEnabled"), ref isGuiEnabled);

                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("EXPERIENCE_TRAIT"))
                {
                    string className = node.GetValue("name");
                    if (className != null)
                        classes.AddUnique(className);
                }

                appIcon = GameDatabase.Instance.GetTexture(APP_ICON_PATH, false);
                if (appIcon == null)
                    Util.log("Application icon missing: {0}", APP_ICON_PATH);

                GameEvents.onGUIApplicationLauncherReady.Add(addAppButton);
                GameEvents.onGameSceneLoadRequested.Add(removeAppButton);
            }
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Add the GUI button at the Start()
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        public void Start()
        {
            if (ApplicationLauncher.Ready)
                addAppButton();
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Open the GUI when we push the button
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        public void OnGUI()
        {
            if (isEnabled)
            {
                GUI.skin = HighLogic.Skin;
                windowRect = GUILayout.Window(WINDOW_ID, windowRect, windowHandler, "TextureReplacerReplaced");
                windowRect.x = Math.Max(0, Math.Min(Screen.width - 30, windowRect.x));
                windowRect.y = Math.Max(0, Math.Min(Screen.height - 30, windowRect.y));
            }

            if (headGui_IsEnabled)
            {
                GUI.skin = HighLogic.Skin;
                headGui_windowRect = GUILayout.Window(WINDOW_ID_HEAD, headGui_windowRect, head_WindowHandler, "Heads Menu");
            }

            if (suitGui_IsEnabled)
            {
                GUI.skin = HighLogic.Skin;
                suitGui_windowRect = GUILayout.Window(WINDOW_ID_SUIT, suitGui_windowRect, suit_WindowHandler, "Suits Menu");
            }


        }

        public void head_WindowHandler(int id)
        {
            Reflections reflections = Reflections.instance;
            Personaliser personaliser = Personaliser.instance;
            Color32 color = new Color32(255,255,255,255);
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.padding.bottom = 0;
            style.padding.top = 10;

            string[] level = { "Level 0","Level 1", "Level 2", "Level 3", "Level 4", "Level 5"};

            //GUISkin customSkin = HighLogic.Skin;
           // customSkin.label = style;


            GUILayout.BeginVertical(); // start of the Gui column
            GUILayout.BeginHorizontal(); // start of the Gui row

            if (GUI.Button(new Rect(855, 5, 15, 15), "X"))
            {
                headGui_IsEnabled = false;
                selectedHeadSet = null;
            }
                        
            GUILayout.BeginVertical(GUILayout.Width(200)); // start of head set name column
            headScroll = GUILayout.BeginScrollView(headScroll);
            GUILayout.BeginVertical();
            foreach (Head_Set headSet in personaliser.KerbalHeadsDB_full)
            {
                if (GUILayout.Button(headSet.name))
                {
                    selectedHeadSet = headSet;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (GUILayout.Button("Reset to Defaults"))
                personaliser.resetKerbals();
            GUILayout.EndVertical(); // end of head set name column

            // Textures.            
            Head_Set head = null;

            if (selectedHeadSet != null)
            {
                head = selectedHeadSet;
            }
            GUILayout.Space(10);

            GUILayout.BeginVertical(); // start of the main setting column
            if (head != null)
                GUILayout.Label(head.name);

            GUILayout.BeginHorizontal(); // start of the main setting row

            GUILayout.BeginVertical(GUILayout.Width(430)); // start of the texture + color column

            headSettingScroll = GUILayout.BeginScrollView(headSettingScroll); 
            GUILayout.BeginVertical();

            GUILayout.BeginVertical();// start of the lvl [0] row
            if (head != null)
            {
                GUILayout.Label("Level 0", GUILayout.Width(180), GUILayout.Height(20));
                GUILayout.Space(5);
            }
            GUILayout.BeginHorizontal(); 
            if (head != null)
            {
                GUILayout.Box(head.headTexture[0], GUILayout.Width(200), GUILayout.Height(200));
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUILayout.Height(200));
                    if (head != null)
                    {
                        GUILayout.Label("Left eyeball color", style);
                    }
                    GUILayout.BeginHorizontal(GUILayout.Width(180));
                    if (head != null)
                    {

                        byte GUI_R = head.eyeballColor_Left[0].r;
                        byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                        head.eyeballColor_Left[0].r = GUI_R;
                        GUILayout.Label("R", style);

                        byte GUI_G = head.eyeballColor_Left[0].g;
                        byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                        head.eyeballColor_Left[0].g = GUI_G;
                        GUILayout.Label("G", style);

                        byte GUI_B = head.eyeballColor_Left[0].b;
                        byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                        head.eyeballColor_Left[0].b = GUI_B;
                        GUILayout.Label("B", style);
                    }
                    GUILayout.EndHorizontal();
                    if (head != null)
                    {
                        GUILayout.Label("Right eyeball color", style);
                    }
                    GUILayout.BeginHorizontal(GUILayout.Width(180));
                    if (head != null)
                    {

                        byte GUI_R = head.eyeballColor_Right[0].r;
                        byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                        head.eyeballColor_Right[0].r = GUI_R;
                        GUILayout.Label("R", style);

                        byte GUI_G = head.eyeballColor_Right[0].g;
                        byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                        head.eyeballColor_Right[0].g = GUI_G;
                        GUILayout.Label("G", style);

                        byte GUI_B = head.eyeballColor_Right[0].b;
                        byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                        head.eyeballColor_Right[0].b = GUI_B;
                        GUILayout.Label("B", style);
                    }
                    GUILayout.EndHorizontal();
                    if (head != null)
                    {
                        GUILayout.Label("Left pupil color", style);
                    }
                    GUILayout.BeginHorizontal(GUILayout.Width(180));
                    if (head != null)
                    {

                        byte GUI_R = head.pupilColor_Left[0].r;
                        byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                        head.pupilColor_Left[0].r = GUI_R;
                        GUILayout.Label("R", style);

                        byte GUI_G = head.pupilColor_Left[0].g;
                        byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                        head.pupilColor_Left[0].g = GUI_G;
                        GUILayout.Label("G", style);

                        byte GUI_B = head.pupilColor_Left[0].b;
                        byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                        head.pupilColor_Left[0].b = GUI_B;
                        GUILayout.Label("B", style);
                    }
                    GUILayout.EndHorizontal();
                    if (head != null)
                    {
                        GUILayout.Label("Right pupil color", style);
                    }
                    GUILayout.BeginHorizontal(GUILayout.Width(180));
                    if (head != null)
                    {

                        byte GUI_R = head.pupilColor_Right[0].r;
                        byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                        head.pupilColor_Right[0].r = GUI_R;
                        GUILayout.Label("R", style);

                        byte GUI_G = head.pupilColor_Right[0].g;
                        byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                        head.pupilColor_Right[0].g = GUI_G;
                        GUILayout.Label("G", style);

                        byte GUI_B = head.pupilColor_Right[0].b;
                        byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                        head.pupilColor_Right[0].b = GUI_B;
                        GUILayout.Label("B", style);
                    }
                    GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();// end of the lvl [0] row

            GUILayout.BeginVertical();// start of the lvl [1] row
            if (head != null)
            {
                GUILayout.Label("Level 1", GUILayout.Width(180), GUILayout.Height(20));
                GUILayout.Space(5);
            }
            GUILayout.BeginHorizontal();
            if (head != null)
            {
                GUILayout.Box(head.headTexture[1], GUILayout.Width(200), GUILayout.Height(200));
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUILayout.Height(200));
                if (head != null)
                {
                    GUILayout.Label("Left eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Left[1].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Left[1].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Left[1].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Left[1].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Left[1].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Left[1].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Right[1].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Right[1].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Right[1].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Right[1].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Right[1].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Right[1].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Left pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Left[1].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Left[1].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Left[1].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Left[1].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Left[1].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Left[1].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Right[1].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Right[1].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Right[1].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Right[1].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Right[1].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Right[1].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();// end of the lvl [1] row

            GUILayout.BeginVertical();// start of the lvl [2] row
            if (head != null)
            {
                GUILayout.Label("Level 2", GUILayout.Width(180), GUILayout.Height(20));
                GUILayout.Space(5);
            }
            GUILayout.BeginHorizontal();
            if (head != null)
            {
                GUILayout.Box(head.headTexture[2], GUILayout.Width(200), GUILayout.Height(200));
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUILayout.Height(200));
                if (head != null)
                {
                    GUILayout.Label("Left eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Left[2].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Left[2].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Left[2].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Left[2].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Left[2].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Left[2].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Right[2].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Right[2].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Right[2].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Right[2].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Right[2].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Right[2].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Left pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Left[2].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Left[2].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Left[2].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Left[2].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Left[2].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Left[2].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Right[2].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Right[2].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Right[2].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Right[2].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Right[2].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Right[2].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();// end of the lvl [2] row

            GUILayout.BeginVertical();// start of the lvl [3] row
            if (head != null)
            {
                GUILayout.Label("Level 3", GUILayout.Width(180), GUILayout.Height(20));
                GUILayout.Space(5);
            }
            GUILayout.BeginHorizontal();
            if (head != null)
            {
                GUILayout.Box(head.headTexture[3], GUILayout.Width(200), GUILayout.Height(200));
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUILayout.Height(200));
                if (head != null)
                {
                    GUILayout.Label("Left eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Left[3].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Left[3].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Left[3].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Left[3].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Left[3].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Left[3].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Right[3].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Right[3].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Right[3].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Right[3].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Right[3].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Right[3].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Left pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Left[3].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Left[3].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Left[3].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Left[3].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Left[3].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Left[3].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Right[3].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Right[3].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Right[3].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Right[3].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Right[3].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Right[3].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();// end of the lvl [3] row

            GUILayout.BeginVertical();// start of the lvl [4] row
            if (head != null)
            {
                GUILayout.Label("Level 4", GUILayout.Width(180), GUILayout.Height(20));
                GUILayout.Space(5);
            }
            GUILayout.BeginHorizontal();
            if (head != null)
            {
                GUILayout.Box(head.headTexture[4], GUILayout.Width(200), GUILayout.Height(200));
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUILayout.Height(200));
                if (head != null)
                {
                    GUILayout.Label("Left eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Left[4].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Left[4].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Left[4].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Left[4].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Left[4].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Left[4].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Right[4].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Right[4].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Right[4].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Right[4].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Right[4].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Right[4].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Left pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Left[4].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Left[4].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Left[4].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Left[4].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Left[4].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Left[4].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Right[4].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Right[4].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Right[4].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Right[4].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Right[4].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Right[4].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();// end of the lvl [4] row

            GUILayout.BeginVertical();// start of the lvl [5] row
            if (head != null)
            {
                GUILayout.Label("Level 5", GUILayout.Width(180), GUILayout.Height(20));
                GUILayout.Space(5);
            }
            GUILayout.BeginHorizontal();
            if (head != null)
            {
                GUILayout.Box(head.headTexture[5], GUILayout.Width(200), GUILayout.Height(200));
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUILayout.Height(200));
                if (head != null)
                {
                    GUILayout.Label("Left eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Left[5].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Left[5].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Left[5].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Left[5].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Left[5].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Left[5].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right eyeball color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.eyeballColor_Right[5].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.eyeballColor_Right[5].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.eyeballColor_Right[5].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.eyeballColor_Right[5].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.eyeballColor_Right[5].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.eyeballColor_Right[5].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Left pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Left[5].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Left[5].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Left[5].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Left[5].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Left[5].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Left[5].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                if (head != null)
                {
                    GUILayout.Label("Right pupil color", style);
                }
                GUILayout.BeginHorizontal(GUILayout.Width(180));
                if (head != null)
                {

                    byte GUI_R = head.pupilColor_Right[5].r;
                    byte.TryParse(GUILayout.TextField(GUI_R.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_R);
                    head.pupilColor_Right[5].r = GUI_R;
                    GUILayout.Label("R", style);

                    byte GUI_G = head.pupilColor_Right[5].g;
                    byte.TryParse(GUILayout.TextField(GUI_G.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_G);
                    head.pupilColor_Right[5].g = GUI_G;
                    GUILayout.Label("G", style);

                    byte GUI_B = head.pupilColor_Right[5].b;
                    byte.TryParse(GUILayout.TextField(GUI_B.ToString(), 3, GUILayout.MaxWidth(40)), out GUI_B);
                    head.pupilColor_Right[5].b = GUI_B;
                    GUILayout.Label("B", style);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();// end of the lvl [5] row

            GUILayout.Space(10);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical(); // end of the texture + color column

            GUILayout.BeginVertical(); // start of the setting column
            if (head != null)
            {
                head.isExclusive = GUILayout.Toggle(
                    head.isExclusive, "Is this head exclusive ?");

                GUILayout.Space(20);

                GUILayout.Label("Level to start hiding element");
                GUILayout.Label("enter 6 or more to don't hide");               

            }
            

            GUILayout.BeginHorizontal();
            if (head != null)
            {
                int GUI_lvlToHide_Eye_Left = head.lvlToHide_Eye_Left;
                int.TryParse(GUILayout.TextField(GUI_lvlToHide_Eye_Left.ToString(), 2, GUILayout.MaxWidth(30)), out GUI_lvlToHide_Eye_Left);

                head.lvlToHide_Eye_Left = GUI_lvlToHide_Eye_Left;
                GUILayout.Label("Left eyeball", style);
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (head != null)
            {
                int GUI_lvlToHide_Eye_Right = head.lvlToHide_Eye_Right;
                int.TryParse(GUILayout.TextField(GUI_lvlToHide_Eye_Right.ToString(), 2, GUILayout.MaxWidth(30)), out GUI_lvlToHide_Eye_Right);

                head.lvlToHide_Eye_Right = GUI_lvlToHide_Eye_Right;
                GUILayout.Label("Right eyeball", style);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (head != null)
            {
                int GUI_lvlToHide_Pupil_Left = head.lvlToHide_Pupil_Left;
                int.TryParse(GUILayout.TextField(GUI_lvlToHide_Pupil_Left.ToString(), 2, GUILayout.MaxWidth(30)), out GUI_lvlToHide_Pupil_Left);

                head.lvlToHide_Pupil_Left = GUI_lvlToHide_Pupil_Left;
                GUILayout.Label("Left pupil", style);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (head != null)
            {
                int GUI_lvlToHide_Pupil_Right = head.lvlToHide_Pupil_Right;
                int.TryParse(GUILayout.TextField(GUI_lvlToHide_Pupil_Right.ToString(), 2, GUILayout.MaxWidth(30)), out GUI_lvlToHide_Pupil_Right);

                head.lvlToHide_Pupil_Right = GUI_lvlToHide_Pupil_Right;
                GUILayout.Label("Right pupil", style);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (head != null)
            {
                int GUI_lvlToHide_TeethUp = head.lvlToHide_TeethUp;
                int.TryParse(GUILayout.TextField(GUI_lvlToHide_TeethUp.ToString(), 2, GUILayout.MaxWidth(30)), out GUI_lvlToHide_TeethUp);

                head.lvlToHide_TeethUp = GUI_lvlToHide_TeethUp;
                GUILayout.Label("Up teeth", style);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (head != null)
            {
                int GUI_lvlToHide_TeethDown = head.lvlToHide_TeethDown;
                int.TryParse(GUILayout.TextField(GUI_lvlToHide_TeethDown.ToString(), 2, GUILayout.MaxWidth(30)), out GUI_lvlToHide_TeethDown);

                head.lvlToHide_TeethDown = GUI_lvlToHide_TeethDown;
                GUILayout.Label("Down teeth", style);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (head != null)
            {
                int GUI_lvlToHide_Ponytail = head.lvlToHide_Ponytail;
                int.TryParse(GUILayout.TextField(GUI_lvlToHide_Ponytail.ToString(), 2, GUILayout.MaxWidth(30)), out GUI_lvlToHide_Ponytail);

                head.lvlToHide_Ponytail = GUI_lvlToHide_Ponytail;
                GUILayout.Label("Ponytail", style);
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Default"))
                Util.log("Clicked Button");
            GUILayout.EndVertical(); // end of the setting column

            GUILayout.EndHorizontal(); // end of the main setting row

            GUILayout.EndVertical(); // end of the main setting column

            GUILayout.EndHorizontal();// end of the Gui row
            GUILayout.EndVertical();// end of the Gui column
            

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        public void suit_WindowHandler(int id)
        {
            Reflections reflections = Reflections.instance;
            Personaliser personaliser = Personaliser.instance;

            if (GUI.Button(new Rect(560, 5, 15, 15), "X"))
                suitGui_IsEnabled = false;

            if (GUILayout.Button("I am pouet"))
                Util.log("Clicked Button");

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }


        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Cleaning at OnDestroy()
        /// </summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(addAppButton);
            GameEvents.onGameSceneLoadRequested.Remove(removeAppButton);
        }
    }
}