/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * UIEyeMask editor
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-01-18
 * 
 ***************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor class used to edit UI Sprites.
/// </summary>
namespace UnityEditor.UI
{
    [CustomEditor(typeof(UIEyeMask), true)]
    [CanEditMultipleObjects]
    public class UIEyeMaskEditor : GraphicEditor
    {
        SerializedProperty m_Sprite;
        SerializedProperty m_PreserveAspect;
        GUIContent m_SpriteContent;
        UIEyeMask m_mask;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_mask = target as UIEyeMask;

            m_SpriteContent = new GUIContent("Source Image");

            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

            SetShowNativeSize(true);

            m_mask.type = Image.Type.Simple;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();

            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();

            m_mask.maskRage = EditorGUILayout.Slider("Mask Rage", m_mask.maskRage, 0, 1.0f);

            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            bool showNativeSize = m_Sprite.objectReferenceValue != null;
            SetShowNativeSize(showNativeSize, instant);
        }

        protected void SpriteGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Sprite, m_SpriteContent);
        }

        /// <summary>
        /// All graphics have a preview.
        /// </summary>

        public override bool HasPreviewGUI() { return true; }

        /// <summary>
        /// Draw the Image preview.
        /// </summary>

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            Image image = target as Image;
            if (image == null) return;

            Sprite sf = image.sprite;
            if (sf == null) return;

            CustomSpriteDrawUtility.DrawSprite(sf, rect, image.canvasRenderer.GetColor());
        }

        /// <summary>
        /// Info String drawn at the bottom of the Preview
        /// </summary>

        public override string GetInfoString()
        {
            Image image = target as Image;
            Sprite sprite = image.sprite;

            int x = (sprite != null) ? Mathf.RoundToInt(sprite.rect.width) : 0;
            int y = (sprite != null) ? Mathf.RoundToInt(sprite.rect.height) : 0;

            return string.Format("Image Size: {0}x{1}", x, y);
        }
    }
}