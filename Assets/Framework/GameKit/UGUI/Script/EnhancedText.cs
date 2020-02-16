using System.Collections;
using System.Collections.Generic;
using PureMVC.Manager;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/EnhancedText", 10)]
public class EnhancedText : Text
{
    public bool _isUseLocalize = true;
    public string _key;
    public EnhancedFont _customFont;

    protected override void Awake()
    {
        base.Awake();
        if (null != _customFont)
        {
            font = _customFont._useFont;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EnhancedTextManager.OnLocalize += OnLocalize;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EnhancedTextManager.OnLocalize -= OnLocalize;
    }

    public void OnLocalize()
    {
        if (_isUseLocalize)
        {
            text = EnhancedTextManager.Instance.GetLocalizeString(_key);
        }
    }

    //public override string text
    //{
    //    get
    //    {
    //        if (_isUseLocalize)
    //        {
    //            m_Text = EnhancedTextManager.Instance.GetLocalizeString(_key);
    //            return m_Text;
    //        }
    //        else
    //        {
    //            return m_Text;
    //        }
    //    }
    //    set
    //    {
    //        if (string.IsNullOrEmpty(value))
    //        {
    //            if (string.IsNullOrEmpty(m_Text))
    //                return;
    //            m_Text = "";
    //            SetVerticesDirty();
    //        }
    //        else if (m_Text != value)
    //        {
    //            m_Text = value;
    //            SetVerticesDirty();
    //            SetLayoutDirty();
    //        }
    //    }
    //}
}
