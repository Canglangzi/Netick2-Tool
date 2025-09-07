using UnityEngine;
using UnityEditor;
using Netick.Unity;
using System;

using System.Linq;
using CkbEditor;

namespace CockleBurs.GameFrameWork.Editor
{
    [CustomEditor(typeof(ReplicatedObject), true)] 
    [CanEditMultipleObjects]
    public class NetworkObjectInspector : NetickEditor.NetworkObjectEditor
    {
        private int _selectedTab = 0;
	    private readonly string[] _tabNames = { "Settings", "Prediction", "OtherProperties" };
        
        private ReplicatedObject replicatedObject;


        private SerializedProperty PersistentProperty;
        private SerializedProperty AddToNetworkLoop;
        private SerializedProperty PredictionModeProperty;
        private SerializedProperty AoILayer;
        private SerializedProperty BroadPhaseFiltering;
        private SerializedProperty NarrowPhaseFiltering;
        private SerializedProperty UseSAP;
        private const string SandboxTooltip = "The sandbox of this network object.";
        private const string InputSourceTooltip = "The input source of this network object.";
        private const string NetworkIdTooltip = "The network id of this object. It's -1 if the object is not yet added to the network simulation. Which could happen if the object has not yet been received by the client.";
        internal const string PredictionModeTooltip = "Choose whether this object will be resimulated on every client or only the client who's the input source.";
        internal const string AddToNetworkLoopTooltip = "Disable this to make Netick not invoke network loop callbacks such as NetworkFixedUpdate and NetworkRender, on this object.\r\n\r\nThis can be useful for advanced performance optimizations. ";
        internal const string PersistentTooltip = "Enable this to mark this prefab as DontDestroyOnLoad, preventing its instances from being destroyed when changing scenes. Only valid on network prefabs.";
        private const string BroadPhaseFilteringTooltipTooltip = "Choose what set of players are interested in receiving updates to this object. This will only have an effect when Interest Management is enabled. To enable it, go to Netick -> Settings -> Interest Management.";
        private const string NarrowPhaseFilteringTooltip = "Enable this if you want to be able to filter-send this object to specific clients, by explicit control. Don't conflict this with Broad Phase Filter. This will only have an effect when Interest Management and Narrow Phase Filterting are enabled. To enable them, go to Netick -> Settings -> Interest Management.";
        internal const string RelevanceTooltip = "Choose whether this object will be replicated to everyone or only the client who's the input source.";
        private SerializedProperty PrefabIdProperty;
        private SerializedProperty NetworkedBehavioursProperty;
        private SerializedProperty NetickBehavioursProperty;
        private void OnEnable()
        {
            this.replicatedObject = (ReplicatedObject)this.target;
            this.PersistentProperty = this.serializedObject.FindProperty("_isPersistent");
            this.PredictionModeProperty = this.serializedObject.FindProperty("_PredictionMode");
            this.AddToNetworkLoop = this.serializedObject.FindProperty("AddToNetworkLoop");
            this.AoILayer = this.serializedObject.FindProperty("_AoILayer");
            this.BroadPhaseFiltering = this.serializedObject.FindProperty("_BroadPhaseFilter");
            this.NarrowPhaseFiltering = this.serializedObject.FindProperty("_NarrowPhaseFilter");
            this.UseSAP = this.serializedObject.FindProperty("_SpatialPrioritization");
            this.PrefabIdProperty = this.serializedObject.FindProperty("_PrefabId");
            this.NetworkedBehavioursProperty = this.serializedObject.FindProperty("NetworkedBehaviours");
            this.NetickBehavioursProperty = this.serializedObject.FindProperty("NetickBehaviours");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Apply background color to the entire inspector
            Rect backgroundRect = EditorGUILayout.BeginVertical();
            GUI.backgroundColor = new Color(0.0745f, 0.3647f, 0.6275f, 1f); // Background color
            EditorGUI.DrawRect(backgroundRect, GUI.backgroundColor); // Draw background
            GUI.backgroundColor = Color.white; // Reset color

            DrawTitle("NetworkObject");

            // Display script field and help button
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(replicatedObject), typeof(ReplicatedObject), false);
            GUI.enabled = true;
            if (GUILayout.Button(EditorGUIUtility.IconContent("_Help"), GUILayout.Width(25)))
            {
                Application.OpenURL("https://netick.net/docs/2/index.html");
            }
            GUILayout.EndHorizontal();

            // Display critical properties in a box
            DrawCriticalProperties(replicatedObject);

            // Tab selection for settings and prediction
            DrawTabButtons();

            // Draw the selected tab
            EditorGUI.BeginChangeCheck();
		    // 3. 在OnInspectorGUI中扩展switch语句
		    switch (_selectedTab)
		    {
			    case 0:
				    DrawSettingsTab();
				    break;
			    case 1:
				    DrawPredictionTab();
				    break;
			    case 2: 
				    DrawOtherPropertiesTab();
				    break;
		    }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }


            EditorGUILayout.EndVertical(); // End vertical layout

        }
        public override bool HasPreviewGUI()
        {
            return true;
        }
        private void DrawTitle(string title)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.currentViewWidth));
            GUILayout.Space(10);
        }

        private void DrawTabButtons()
        {
            Rect tabsRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton);
            float tabWidth = tabsRect.width / _tabNames.Length;

            for (int i = 0; i < _tabNames.Length; i++)
            {
                GUI.backgroundColor = (_selectedTab == i) ? new Color(0.1f, 0.5f, 0.8f, 1f) : new Color(0.0745f, 0.3647f, 0.6275f, 1f);
                Rect tabRect = new Rect(tabsRect.x + (i * tabWidth), tabsRect.y, tabWidth, tabsRect.height);

                EditorGUI.DrawRect(tabRect, GUI.backgroundColor);

                // Draw tab button text
                GUIStyle tabStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                EditorGUI.LabelField(tabRect, _tabNames[i], tabStyle);

                // Handle tab clicks
                if (Event.current.type == EventType.MouseDown && tabRect.Contains(Event.current.mousePosition))
                {
                    _selectedTab = i;
                    Event.current.Use();
                }
            }
        }

        internal static class NetickEditorToggleState
        {
            internal static string SelectedPropertyName = "";
            internal static bool ShowAdvancedNetworkTransform = false;
            internal static bool ShowAdvancedErrorSmoothingNetworkTransform = false;
            internal static bool ShowAdvancedNetworkObject = false;
            internal static bool ShowAdvanced = false;
            internal static bool ShowNetowrkSettings = true;
            internal static bool ShowMiscSettings = true;
            internal static bool ShowGeneralSettings = true;
            internal static bool ShowAdvancedSettings = true;
            internal static bool ShowLagCompSettings = true;
            internal static bool ShowSimulationSettings = true;
            internal static bool ShowSettings = true;
            internal static bool ShowIM = false;
            internal static bool ShowObjectIMSettings = true;
            internal static bool VolumeDebug = false;
        }
        private void DrawSettingsTab()
        {
            DrawBackground(() =>
            {
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

                // 检查是否链接了模拟
                if (Application.isPlaying && (UnityEngine.Object)replicatedObject != (UnityEngine.Object)null &&
                    (UnityEngine.Object)replicatedObject.Sandbox != (UnityEngine.Object)null && replicatedObject.Id < 0)
                {
                    Color color = GUI.color;
                    GUI.color = Color.yellow;
                    CkbEditorProperty.DrawLabel(this.serializedObject.targetObject, "Unlinked", "", "This NetworkObject is not linked to simulation.");
                    GUI.color = color;
                }

                // 禁用组 (只在运行时禁用控件)
                EditorGUI.BeginDisabledGroup(Application.isPlaying);

                // 绘制控件
                CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.AddToNetworkLoop,
                    "Disable this to make Netick not invoke network loop callbacks such as NetworkFixedUpdate and NetworkRender, on this object.\r\n\r\nThis can be useful for advanced performance optimizations.");

                if (this.replicatedObject.IsPrefabObject)
                {
                    CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.PersistentProperty,
                        "Enable this to mark this prefab as DontDestroyOnLoad, preventing its instances from being destroyed when changing scenes. Only valid on network prefabs.");
                }

                // 结束禁用组
                EditorGUI.EndDisabledGroup();

                if (this.replicatedObject.IsPrefabObject)
                {
                    CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.PersistentProperty, PersistentTooltip);
                }
                EditorGUI.EndDisabledGroup();

                // 显示PrefabId
                EditorGUILayout.PropertyField(PrefabIdProperty, new GUIContent("Prefab ID", "The unique identifier for this prefab."));
                // 显示NetworkedBehaviours数组 - Fusion风格
                EditorGUILayout.LabelField("Networked Behaviours", EditorStyles.boldLabel);
                if (replicatedObject.NetworkedBehaviours != null && replicatedObject.NetworkedBehaviours.Length > 0)
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginDisabledGroup(true); // 锁定字段
                    for (int i = 0; i < replicatedObject.NetworkedBehaviours.Length; i++)
                    {
                        var behaviour = replicatedObject.NetworkedBehaviours[i];
                        if (behaviour != null)
                        {
                            EditorGUILayout.ObjectField(
                                $"Behaviour {i} ({behaviour.GetType().Name})",
                                behaviour,
                                typeof(NetworkBehaviour),
                                true);
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"Behaviour {i}: Null");
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("No Networked Behaviours found.", MessageType.Info);
                }
                
                EditorGUILayout.LabelField("Netick Behaviours", EditorStyles.boldLabel);
                if (replicatedObject.NetickBehaviours != null && replicatedObject.NetickBehaviours.Length > 0)
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginDisabledGroup(true); 
                    for (int i = 0; i < replicatedObject.NetickBehaviours.Length; i++)
                    {
                        var behaviour = replicatedObject.NetickBehaviours[i];
                        if (behaviour != null)
                        {
                            EditorGUILayout.ObjectField(
                                $"Behaviour {i} ({behaviour.GetType().Name})",
                                behaviour,
                                typeof(NetickBehaviour),
                                true);
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"Behaviour {i}: Null");
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("No Netick Behaviours found.", MessageType.Info);
                }

                NetickEditorToggleState.ShowObjectIMSettings = EditorGUILayout.BeginFoldoutHeaderGroup(
                    NetickEditorToggleState.ShowObjectIMSettings, "Interest Management", (GUIStyle)null, (Action<Rect>)null, (GUIStyle)null);

                if (NetickEditorToggleState.ShowObjectIMSettings)
                {
                    EditorGUI.BeginDisabledGroup(Application.isPlaying);
                    
                    CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.BroadPhaseFiltering,
                        "Choose what set of players are interested in receiving updates to this object. This will only have an effect when Interest Management is enabled. To enable it, go to Netick -> Settings -> Interest Management.");

                    // 如果选择了特定的兴趣管理层，显示更多选项
                    if (this.BroadPhaseFiltering.enumValueIndex == 1)
                    {
                        CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.AoILayer);
                    }

                    // 绘制狭义过滤器控件
                    CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.NarrowPhaseFiltering,
                        "Enable this if you want to be able to filter-send this object to specific clients, by explicit control. Don't conflict this with Broad Phase Filter. This will only have an effect when Interest Management and Narrow Phase Filtering are enabled. To enable them, go to Netick -> Settings -> Interest Management.");

           
                EditorGUILayout.EndFoldoutHeaderGroup();
                }
                else
                {
               
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
             
                NetickEditorToggleState.ShowAdvancedNetworkObject = EditorGUILayout.BeginFoldoutHeaderGroup(
                    NetickEditorToggleState.ShowAdvancedNetworkObject, "Info", (GUIStyle)null, (Action<Rect>)null, (GUIStyle)null);

                if (NetickEditorToggleState.ShowAdvancedNetworkObject)
                {
                
                    EditorGUILayout.LabelField("Scene Id", this.replicatedObject.GetSceneId().ToString(), Array.Empty<GUILayoutOption>());
                    EditorGUILayout.LabelField("Is Scene Object", this.replicatedObject.IsSceneObject.ToString(), Array.Empty<GUILayoutOption>());
                  
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                else
                {
             
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Horizontal line
            });
        }
	    private void DrawOtherPropertiesTab()
	    {
		    DrawBackground(() =>
		    {
			    EditorGUILayout.LabelField("OtherProperties", EditorStyles.boldLabel);
	    
			    SerializeRemainingProperties();
			    
			    EditorGUILayout.Space();
			    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		    });
	    }
	    private void SerializeRemainingProperties()
	    {
	    
		    SerializedProperty iterator = serializedObject.GetIterator();
		    bool enterChildren = true;
		    
		    string[] excludedProperties = new string[] {
			    "_isPersistent",
			    "_PredictionMode",
			    "AddToNetworkLoop",
			    "_AoILayer",
			    "_BroadPhaseFilter",
			    "_NarrowPhaseFilter",
			    "_SpatialPrioritization",
			    "_PrefabId",
			    "NetworkedBehaviours",
			    "NetickBehaviours",
			    "m_Script" // Unity脚本引用字段
		    };
		    
		    // 开始属性遍历
		    while (iterator.NextVisible(enterChildren))
		    {
			    enterChildren = false;
                
			    if (excludedProperties.Contains(iterator.name))
			    {
				    continue;
			    }
			    if (iterator.name.StartsWith("m_") ||
				    iterator.name.StartsWith("_internal") ||
				    iterator.name.StartsWith("__"))
			    {
				    continue;
			    }
			    EditorGUILayout.PropertyField(iterator, true);
		    }
	    }
        private void DrawPredictionTab()
        {
            DrawBackground(() =>
            {
                EditorGUILayout.LabelField("Prediction", EditorStyles.boldLabel);

                if (this.replicatedObject.IsPrefabObject)
                CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.PersistentProperty, "Enable this to mark this prefab as DontDestroyOnLoad, preventing its instances from being destroyed when changing scenes. Only valid on network prefabs.");
                CkbEditorProperty.DrawProperty(this.serializedObject.targetObject, this.PredictionModeProperty, "Choose whether this object will be resimulated on every client or only the client who's the input source.");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); // Horizontal line
            });
        }
        private void DrawBackground(System.Action drawContent)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.2f); 
            EditorGUI.DrawRect(rect, GUI.backgroundColor); 
            GUI.backgroundColor = Color.white; 

            drawContent(); 

            EditorGUILayout.EndVertical();
        }
        private void DrawCriticalProperties(ReplicatedObject nob)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Critical Properties", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);

            if (nob.Entity != null && nob.InputSource != null)
            {
                EditorGUILayout.LabelField("InputSource", nob.InputSource.ToString());
                EditorGUILayout.LabelField("PlayerID", nob.InputSource.PlayerId.ToString());
                EditorGUILayout.LabelField("InputSource", nob.InterestGroup.ToString());

            }
            else
            {
                EditorGUILayout.LabelField("InputSource", "Null");
                EditorGUILayout.LabelField("PlayerID", "Null");
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }
        public override void OnPreviewSettings()
        {
            GUILayout.Label("Network Object Settings", "preLabel");
            if (GUILayout.Button("Show Description", "preButton"))
            {

            }
        }


        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (replicatedObject != null)
            {
                EditorGUI.DrawRect(r, new Color(0.0745f, 0.3647f, 0.6275f, 1f)); 
                // Set text color and draw network information
                Color originalColor = GUI.contentColor;
                GUI.contentColor = Color.white; // White text for contrast

                EditorGUI.LabelField(new Rect(r.x + 5, r.y + 5, r.width, EditorGUIUtility.singleLineHeight), "Network Information", EditorStyles.boldLabel);

                float yOffset = EditorGUIUtility.singleLineHeight * 1.5f;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float xOffset = r.x + 10f;
                float widthOffset = r.width - 20f;
      
                if (replicatedObject.Entity != null)
                {
                    string[] labels =
                 {
                        $"Object ID: {(replicatedObject.Id.ToString())}",
                        $"Owner ID: {(replicatedObject.InputSource != null ? replicatedObject.InputSource.PlayerId.ToString() : "Null")}",
                        $"Owner: {(replicatedObject.InputSource != null ? replicatedObject.InputSource.ToString() : "Null")}",
                        $"Is Owned: {(replicatedObject.IsOwner.ToString())}",
                        $"Is Client: {(replicatedObject.IsClient.ToString())}",
                        $"Is Server: {(replicatedObject.IsServer.ToString())}",
                        $"Is Resimulating: {(replicatedObject.IsResimulating.ToString())}",
                        $"Is Scene Object: {(replicatedObject.IsSceneObject.ToString())}",
                        $"Is Spawned: {(replicatedObject.IsPrefabObject.ToString())}",
                        $"Is Proxy: {(replicatedObject.IsProxy.ToString())}",
                        $"IsActiveAndEnabled: {(replicatedObject.isActiveAndEnabled.ToString())}",
                        $"HasValidId: {(replicatedObject.HasValidId.ToString())}"
                        };
                    foreach (string label in labels)
                    {
                        EditorGUI.LabelField(new Rect(xOffset, r.y + yOffset, widthOffset, lineHeight), label);
                        yOffset += lineHeight;
                    }
                }


                // Restore original text color
                GUI.contentColor = originalColor;
            }
            else
            {
                EditorGUI.LabelField(r, "No NetworkObject component found");
            }
        }
        public new static void HorizontalLine(Color color, float height, Vector2 margin)
        {
            GUILayout.Space(margin.x);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height, Array.Empty<GUILayoutOption>()), color);
            GUILayout.Space(margin.y);
        }
    }

}