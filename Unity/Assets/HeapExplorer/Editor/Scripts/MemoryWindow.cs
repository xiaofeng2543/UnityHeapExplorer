using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace HeapExplorer
{
    public class HexView : HeapExplorerView
    {
        HexViewControl m_hexView;
        ArraySegment64<byte> m_segment;

        public string editorPrefsKey
        {
            get;
            set;
        }

        public override void Awake()
        {
            base.Awake();

            editorPrefsKey = "HeapExplorer.HexView";
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_hexView = new HexViewControl();
        }

        public void Clear()
        {
            m_segment = new ArraySegment64<byte>();
        }

        public void Inspect(PackedMemorySnapshot memory, System.UInt64 address, System.UInt64 size)
        {
            m_segment = new ArraySegment64<byte>();
            if (address == 0)
                return;

            var heapIndex = memory.FindHeapOfAddress(address);
            if (heapIndex < 0)
                return;

            var heap = memory.managedHeapSections[heapIndex];
            var segment = new ArraySegment64<byte>(heap.bytes, address - heap.startAddress, size);
            Inspect(memory, address, segment);
        }

        public void Inspect(PackedMemorySnapshot memory, System.UInt64 address, ArraySegment64<byte> segment)
        {
            m_segment = segment;
            m_hexView.Create(window, address, m_segment);
        }

        public override void OnGUI()
        {
            base.OnGUI();

            //DrawTopBar();

            if (m_segment.count == 0)
            {
                EditorGUILayout.HelpBox("Can't inspect memory.", MessageType.Info);
                GUILayoutUtility.GetRect(50, 100000, 50, 100000);
                return;
            }
            else
            {
                m_hexView.OnGUI();
            }

            DrawBottomBar();
        }

        void DrawTopBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginDisabledGroup(m_segment.array == null);
                if (GUILayout.Button("Save...", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    var filePath = EditorUtility.SaveFilePanel("Save as...", "", "", "mem");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate))
                        {
                            stream.Write(m_segment.array, (int)m_segment.offset, (int)m_segment.count);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(1);
                GUILayout.FlexibleSpace();
            }
        }

        void DrawBottomBar()
        {
            //using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            //{
            //    GUILayout.Space(1);
            //    GUILayout.FlexibleSpace();
            //}
        }
    }

    public class MemoryWindow : EditorWindow
    {
        HexViewControl m_hexView = new HexViewControl();
        ArraySegment64<byte> m_segment;

        private void OnEnable()
        {
            this.titleContent = new GUIContent("Memory");
            this.minSize = new Vector2(550, 100);
        }

        static public void Inspect(PackedMemorySnapshot memory, System.UInt64 address, System.UInt64 size)
        {
            var wnd = MemoryWindow.CreateInstance<MemoryWindow>();
            wnd.InspectInternal(memory, address, size);
            wnd.ShowUtility();
        }

        void InspectInternal(PackedMemorySnapshot memory, System.UInt64 address, System.UInt64 size)
        {
            m_segment = new ArraySegment64<byte>();
            if (address == 0)
                return;

            var heapIndex = memory.FindHeapOfAddress(address);
            var heap = memory.managedHeapSections[heapIndex];

            m_segment = new ArraySegment64<byte>(heap.bytes, address - heap.startAddress, size);

            m_hexView.Create(this, address, m_segment);
            Repaint();
        }

        private void OnGUI()
        {
            DrawTopBar();

            if (m_segment.count == 0)
            {
                EditorGUILayout.HelpBox("The memory address to inspect is NULL.", MessageType.Info);
                return;
            }
            else
            {
                m_hexView.OnGUI();
            }

            DrawBottomBar();
        }

        void DrawTopBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginDisabledGroup(m_segment.array == null);
                if (GUILayout.Button("Save...", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    var filePath = EditorUtility.SaveFilePanel("Save as...", "", "", "mem");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate))
                        {
                            stream.Write(m_segment.array, (int)m_segment.offset, (int)m_segment.count);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(1);
                GUILayout.FlexibleSpace();
            }
        }

        void DrawBottomBar()
        {
            //using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            //{
            //    GUILayout.Space(1);
            //    GUILayout.FlexibleSpace();
            //}
        }
    }
}
