using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minave.Map
{
	[Serializable]
	public class LayerRatio
	{
		[SerializeField] private List<Key> _keys = new();

		public int NumberKeys => _keys.Count;
		public bool RandomizeColour { get; set; }
		
		/// <summary>
		/// 比率に応じた色を取得
		/// </summary>
		/// <param name="ratio">比率</param>
		public Color Evaluate(float ratio)
		{
			if (_keys.Count == 0)
			{
				Reset();
			}
			
			Key keyLeft = _keys[0];
			Key keyRight = _keys[^1];

			foreach (var key in _keys)
			{
				if (key.Ratio < ratio)
				{
					keyLeft = key;
				}

				if (key.Ratio > ratio)
				{
					keyRight = key;
					break;
				}
			}

			return keyRight.Color;
		}

		/// <summary>
		/// キーを追加
		/// </summary>
		/// <param name="color">色</param>
		/// <param name="ratio">比率</param>
		public int AddKey(Color color, float ratio)
		{
			ratio = (float)Math.Round(Math.Clamp(ratio, 0.00f, 1.00f), 2);
			var newKey = new Key(color, ratio);
			for (int i = 0; i < _keys.Count; i++)
			{
				if (newKey.Ratio < _keys[i].Ratio)
				{
					_keys.Insert(i, newKey);
					return i;
				}
			}

			_keys.Add(newKey);
			return _keys.Count - 1;
		}

		/// <summary>
		/// キーを削除
		/// </summary>
		/// <param name="index">番号</param>
		public void RemoveKey(int index)
		{
			if (_keys.Count >= 2)
			{
				_keys.RemoveAt(index);
			}
		}

		/// <summary>
		/// キーの比率を更新
		/// </summary>
		/// <param name="index">番号</param>
		/// <param name="ratio">比率</param>
		public int UpdateKeyTime(int index, float ratio)
		{
			Color color = _keys[index].Color;
			RemoveKey(index);
			return AddKey(color, ratio);
		}

		/// <summary>
		/// キーの色を更新
		/// </summary>
		/// <param name="index">番号</param>
		/// <param name="color">色</param>
		public void UpdateKeyColor(int index, Color color)
		{
			_keys[index] = new Key(color, _keys[index].Ratio);
		}
		
		/// <summary>
		/// キーを取得
		/// </summary>
		/// <param name="index">番号</param>
		public Key GetKey(int index)
		{
			return _keys[index];
		}

		/// <summary>
		/// テクスチャを取得
		/// </summary>
		/// <param name="width">幅</param>
		public Texture2D GetTexture(int width)
		{
			var texture = new Texture2D(width, 1);
			var colors = new Color[width];
			for (int i = 0; i < width; i++)
			{
				float ratio = (float)i / (width - 1);
				colors[i] = Evaluate(ratio);
			}

			texture.SetPixels(colors);
			texture.Apply();
			return texture;
		}
		
		/// <summary>
		/// 初期化
		/// </summary>
		public void Reset()
		{
			_keys.Clear();
			AddKey(Color.white, 0.5f);
			AddKey(Color.black, 1.0f);
		}

		[Serializable]
		public struct Key
		{
			[SerializeField] private Color _color;
			[SerializeField] private float _ratio;

			public Color Color => _color;
			public float Ratio => _ratio;
			
			public Key(Color colour, float ratio)
			{
				_color = colour;
				_ratio = ratio;
			}
		}
	}
}