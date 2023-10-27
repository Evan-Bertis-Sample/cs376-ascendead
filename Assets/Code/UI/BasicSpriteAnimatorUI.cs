using System.Collections;
using System.Collections.Generic;
using Ascendead.Components;
using UnityEngine.UI;
using UnityEngine;

namespace Ascendead.UI
{
    [RequireComponent(typeof(Image))]
    public class BasicSpriteAnimatorUI : BasicSpriteAnimationPlayer
    {
        [field: SerializeField] private Image _image;
        protected override void SetSprite(Sprite sprite)
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_image == null) throw new System.Exception("BasicSpriteAnimatorUI has no Image component.");
            _image.sprite = sprite;
        }
    }
}
